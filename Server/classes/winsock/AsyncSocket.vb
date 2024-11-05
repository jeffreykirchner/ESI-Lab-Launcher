Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Text
Imports System.Collections

''' <summary>
''' A class that encapsulates the raw functions of the Socket.
''' </summary>
Public Class AsyncSocket

    Private m_Monitor As WinsockMonitor
    Private m_sock As Socket
    Private incBufferSize As Integer = 1024
    Private bufferCol As Collection
    Private byteBuffer(incBufferSize) As Byte
    Private m_udp As UdpClient

    Public Sub New(ByVal winsock_monitor As WinsockMonitor, ByVal inc_size As Integer)
        bufferCol = New Collection
        _buff = New ByteBufferCol
        incBufferSize = inc_size
        ReDim byteBuffer(incBufferSize)
        m_Monitor = winsock_monitor
    End Sub
    Public Sub New(ByVal winsock_monitor As WinsockMonitor, ByVal client As Socket, ByVal inc_size As Integer)
        bufferCol = New Collection
        _buff = New ByteBufferCol
        incBufferSize = inc_size
        ReDim byteBuffer(incBufferSize)
        m_Monitor = winsock_monitor
        m_sock = client
        Receive()
    End Sub

    ''' <summary>
    ''' Gets the first object (byte array) in the buffer and removes it.
    ''' </summary>
    Public Function GetData() As Byte()
        If bufferCol.Count = 0 Then Return Nothing
        Dim byt() As Byte = bufferCol.Item(1)
        bufferCol.Remove(1)
        Return byt
    End Function
    ''' <summary>
    ''' Gets a value that contains the number of items in the buffer.
    ''' </summary>
    Public Function BufferCount() As Integer
        Return bufferCol.Count
    End Function

    Public Sub Close()
        Try
            If m_Monitor.State = WinsockStates.Closed Then Exit Sub
            If m_Monitor.State = WinsockStates.Listening Then
                m_Monitor.ClosingBegun()
                If m_Monitor.Protocol = WinsockProtocols.Tcp Then
                    m_sock.Close()
                Else
                    m_udp.Close()
                End If
                m_Monitor.CloseDone()
            ElseIf m_Monitor.State = WinsockStates.Connected Then
                m_Monitor.ClosingBegun()
                m_sock.BeginDisconnect(False, New AsyncCallback(AddressOf CloseCallback), m_sock)
            Else
                m_Monitor.CloseDone()
            End If
        Catch ex As Exception
            m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.Close")
        End Try
    End Sub
    Private Sub CloseCallback(ByVal ar As IAsyncResult)
        Try
            Dim sock As Socket = ar.AsyncState
            sock.EndDisconnect(ar)
            sock.Close()
            m_Monitor.CloseDone()
        Catch ex As Exception
            If TypeOf ex Is SocketException Then
                Dim se As SocketException = ex
                Select Case se.SocketErrorCode
                    Case Is <> SocketError.Success
                        'Close() - could cause infinite loop
                        m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.CloseCallback", se.SocketErrorCode, ex.ToString)
                        Exit Try
                End Select
            End If
            If TypeOf ex Is ObjectDisposedException Then
                Exit Try
            End If
            m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.CloseCallback", , ex.ToString)
        End Try
    End Sub

    Public Sub Listen(ByVal port As Integer, ByVal max_pending As Integer)
        Try
            If m_Monitor.Protocol = WinsockProtocols.Tcp Then
                m_sock = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                Dim ipLocal As New IPEndPoint(IPAddress.Any, port)
                m_sock.Bind(ipLocal)
                m_sock.Listen(max_pending)
                m_Monitor.ListeningStarted()
                m_sock.BeginAccept(New AsyncCallback(AddressOf ListenCallback), m_sock)
            ElseIf m_Monitor.Protocol = WinsockProtocols.Udp Then
                m_udp = New UdpClient(port)
                'm_udp.EnableBroadcast = True
                'm_udp.ExclusiveAddressUse = False
                m_Monitor.ListeningStarted()
                m_udp.BeginReceive(New AsyncCallback(AddressOf ReceiveCallbackUDP), Nothing)
            End If
        Catch ex As Exception
            m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.Listen")
        End Try
    End Sub
    Private Sub ListenCallback(ByVal ar As IAsyncResult)
        Try
            Dim sock As Socket = ar.AsyncState

            Dim client As Socket = sock.EndAccept(ar)

            m_Monitor.ClientReceived(client)
            If m_Monitor.State <> WinsockStates.Listening Then
                sock.Close()
                Exit Sub
            End If
            sock.BeginAccept(New AsyncCallback(AddressOf ListenCallback), sock)
        Catch ex As Exception
            If TypeOf ex Is SocketException Then
                Dim se As SocketException = ex
                Select Case se.SocketErrorCode
                    Case Is <> SocketError.Success
                        Close()
                        m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.ListenCallback", se.SocketErrorCode)
                        Exit Try
                End Select
            ElseIf TypeOf ex Is ObjectDisposedException Then
                Exit Try
            End If
            m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.ListenCallback")
        End Try
    End Sub

    Public Sub Connect(ByVal remIP As IPAddress, ByVal port As Integer)
        Try
            Dim remEP As New IPEndPoint(remIP, port)
            m_sock = New Socket(remIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            If m_Monitor.State <> WinsockStates.HostResolved Then Exit Sub
            m_Monitor.ConnectStarted()
            m_sock.BeginConnect(remEP, New AsyncCallback(AddressOf ConnectCallback), m_sock)
        Catch ex As Exception
            m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.Connect", , ex.ToString)
        End Try
    End Sub
    Private Sub ConnectCallback(ByVal ar As IAsyncResult)
        Try
            If m_Monitor.State <> WinsockStates.Connecting Then Exit Sub
            Dim sock As Socket = ar.AsyncState
            sock.EndConnect(ar)
            If m_Monitor.State <> WinsockStates.Connecting Then Exit Sub
            'start receive listener
            Receive()
            'call the connected event
            m_Monitor.ConnectFinished()
        Catch ex As Exception
            If TypeOf ex Is SocketException Then
                Dim se As SocketException = ex
                Select Case se.SocketErrorCode
                    Case Is <> SocketError.Success
                        Close()
                        m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.ConnectCallback", se.SocketErrorCode)
                        Exit Try
                End Select
            End If
            m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.ConnectCallback")
        End Try
    End Sub

    Private Sub Receive()
        Try
            Dim errorState As SocketError
            m_sock.BeginReceive(byteBuffer, 0, incBufferSize, SocketFlags.None, errorState, New AsyncCallback(AddressOf ReceiveCallback), errorState)
        Catch ex As Exception
            m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.Receive")
        End Try
    End Sub
    ''' <history>
    ''' 06-09-2006 Added ObjectDisposedException to prevent
    '''             Close() from generating an error.
    ''' </history>
    Private Sub ReceiveCallback(ByVal ar As IAsyncResult)
        Try
            Dim errCode As SocketError = ar.AsyncState
            Dim iSize As Integer = m_sock.EndReceive(ar)
            If iSize < 1 Then
                If _buff.Count > 0 Then _buff.Clear()
                Close()
                Exit Sub
            End If
            Dim ipE As IPEndPoint = m_sock.RemoteEndPoint
            ProcessIncoming(byteBuffer, iSize, ipE.Address.ToString, ipE.Port)
            ReDim byteBuffer(incBufferSize)
            m_sock.BeginReceive(byteBuffer, 0, incBufferSize, SocketFlags.None, errCode, New AsyncCallback(AddressOf ReceiveCallback), errCode)
        Catch ex As Exception
            If TypeOf (ex) Is SocketException Then
                Dim se As SocketException = ex
                Select Case se.SocketErrorCode
                    Case Is <> SocketError.Success
                        Close()
                        m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.ReceiveCallback", se.SocketErrorCode)
                        Exit Try
                End Select
            ElseIf TypeOf ex Is ObjectDisposedException Then
                Exit Try
            End If
            m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.ReceiveCallback")
        End Try
    End Sub
    Private Sub ReceiveCallbackUDP(ByVal ar As IAsyncResult)
        Try
            Dim remEP As IPEndPoint = Nothing
            byteBuffer = m_udp.EndReceive(ar, remEP)
            If byteBuffer Is Nothing Then
                If _buff.Count > 0 Then _buff.Clear()
                Exit Sub
            End If
            If byteBuffer.Length < 1 Then
                If _buff.Count > 0 Then _buff.Clear()
                Exit Sub
            End If
            ProcessIncoming(byteBuffer, byteBuffer.Length, remEP.Address.ToString, remEP.Port)
            m_udp.BeginReceive(New AsyncCallback(AddressOf ReceiveCallbackUDP), Nothing)
        Catch ex As Exception
            If TypeOf (ex) Is SocketException Then
                Dim se As SocketException = ex
                Select Case se.SocketErrorCode
                    Case Is <> SocketError.Success
                        m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.ReceiveCallbackUDP", se.SocketErrorCode)
                        Exit Try
                End Select
            ElseIf TypeOf ex Is ObjectDisposedException Then
                Exit Try
            End If
            m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.ReceiveCallbackUDP")
        End Try
    End Sub

    ''' <history>
    ''' 06-12-2006 Added support for buffer size (for send progress)
    ''' </history>
    Public Sub Send(ByVal byt() As Byte)
        Try
            If m_Monitor.Legacy_Support Then
                'don't add separators
                Dim x As New SendState
                x.Bytes = byt
                x.Length = byt.Length
                x.StartIndex = 0
                If UBound(byt) > incBufferSize Then
                    x.SendLength = incBufferSize + 1
                Else
                    x.SendLength = byt.Length
                End If
                If m_Monitor.Protocol = WinsockProtocols.Tcp Then
                    m_sock.BeginSend(byt, 0, x.SendLength, SocketFlags.None, x.ErrCode, New AsyncCallback(AddressOf SendCallback), x)
                ElseIf m_Monitor.Protocol = WinsockProtocols.Udp Then
                    m_sock = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                    Dim ihe As IPHostEntry = Dns.GetHostEntry(m_Monitor.RemoteServer)
                    Dim remEP As New IPEndPoint(ihe.AddressList(0), m_Monitor.RemotePort)
                    m_sock.BeginSendTo(byt, 0, x.SendLength, SocketFlags.None, remEP, New AsyncCallback(AddressOf SendToCallback), x)
                End If
            Else 'add separators
                byt = AppendSize(byt)
                Dim x As New SendState
                x.Bytes = byt
                x.Length = byt.Length
                x.StartIndex = 0
                If UBound(byt) > incBufferSize Then
                    x.SendLength = incBufferSize + 1
                Else
                    x.SendLength = byt.Length
                End If
                m_sock.BeginSend(byt, 0, x.SendLength, SocketFlags.None, x.ErrCode, New AsyncCallback(AddressOf SendCallback), x)
            End If
        Catch ex As Exception
            m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.Send")
        End Try
    End Sub
    Private Sub SendCallback(ByVal ar As IAsyncResult)
        Try
            Dim st As SendState = ar.AsyncState
            Dim errCode As SocketError = st.ErrCode
            st.TotalSent += m_sock.EndSend(ar)
            If st.StartIndex + st.SendLength < UBound(st.Bytes) Then
                st.StartIndex += st.SendLength
                If st.Bytes.Length <= st.StartIndex + st.SendLength + 1 Then
                    st.SendLength = UBound(st.Bytes) - (st.StartIndex - 1)
                End If
                m_Monitor.SendProgress(st.TotalSent, st.Length)
                m_sock.BeginSend(st.Bytes, st.StartIndex, st.SendLength, SocketFlags.None, st.ErrCode, New AsyncCallback(AddressOf SendCallback), st)
            Else
                m_Monitor.SendCompleted(st.TotalSent, st.Length)
            End If
        Catch ex As Exception
            If TypeOf (ex) Is SocketException Then
                Dim se As SocketException = ex
                Select Case se.SocketErrorCode
                    Case Is <> SocketError.Success
                        Close()
                        m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.SendCallback", se.SocketErrorCode)
                        Exit Try
                End Select
            End If
            m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.SendCallback")
        End Try
    End Sub
    Private Sub SendToCallback(ByVal ar As IAsyncResult)
        Try
            Dim st As SendState = ar.AsyncState
            Dim errCode As SocketError = st.ErrCode
            st.TotalSent += m_sock.EndSendTo(ar)
            If st.StartIndex + st.SendLength < UBound(st.Bytes) Then
                st.StartIndex += st.SendLength
                If st.Bytes.Length < st.StartIndex + st.SendLength + 1 Then
                    st.SendLength = UBound(st.Bytes) - (st.StartIndex - 1)
                End If
                m_Monitor.SendProgress(st.TotalSent, st.Length)
                Dim ihe As IPHostEntry = Dns.GetHostEntry(m_Monitor.RemoteServer)
                Dim remEP As New IPEndPoint(ihe.AddressList(0), m_Monitor.RemotePort)
                m_sock.BeginSendTo(st.Bytes, st.StartIndex, st.SendLength, SocketFlags.None, remEP, New AsyncCallback(AddressOf SendToCallback), st)
            Else
                m_Monitor.SendCompleted(st.TotalSent, st.Length)
            End If
        Catch ex As Exception
            If TypeOf (ex) Is SocketException Then
                Dim se As SocketException = ex
                Select Case se.SocketErrorCode
                    Case Is <> SocketError.Success
                        Close()
                        m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.SendToCallback", se.SocketErrorCode)
                        Exit Try
                End Select
            End If
            m_Monitor.ErrorNotify(ex.Message, "AsyncSocket.SendToCallback")
        End Try
    End Sub

    Private Function AppendSize(ByVal byt() As Byte) As Byte()
        Dim arrSize As Integer = UBound(byt)
        Dim strSize() As Byte = Encoding.ASCII.GetBytes(arrSize.ToString)
        Dim fByte As Byte = FreeByte(strSize)
        strSize = EncloseByte(fByte, strSize)
        byt = AppendByte(strSize, byt)
        Return byt
    End Function

    Private _buff As ByteBufferCol
    Private _sizeComing As Integer = -1
    Private _firstDelim As Byte = 0
    ''' <summary>
    ''' Processes raw data that was received from the socket and places it into the appropriate buffer.
    ''' </summary>
    ''' <param name="byt">The raw byte buffer containing the data received from the socket.</param>
    ''' <param name="iSize">The size of the data received from the socket (reported from the EndReceive).</param>
    Private Sub ProcessIncoming(ByVal byt() As Byte, ByVal iSize As Integer, ByVal source_ip As String, ByVal source_port As Integer)
        ' check if we're using legacy support
        If m_Monitor.Legacy_Support Then
            ' legacy support is active just output the data to the buffer
            ' check if we actually received some data
            If iSize > 0 Then
                ' yes we received some data
                If iSize - 1 < UBound(byt) Then
                    ' byte array is larger than it needs to be shrink it
                    ReDim Preserve byt(iSize - 1)
                End If
                ' add the byte array to the buffer queue
                bufferCol.Add(byt)
                ' raise the DataArrival event
                m_Monitor.DataReceived(iSize, source_ip, source_port)
            End If
        Else
            ' legacy support is inactive
            ' first reduce the size of the array to the reported size (fixes trailling zeros)
            If iSize > 0 Then
                ReDim Preserve byt(iSize - 1)
            Else
                'no reported size - no data
                Exit Sub
            End If
            ' first part of the packet contains the size - have we already determined the size?
            If _sizeComing = -1 Then
                ' we still need the size, the first byte is the delimiter
                ' do we have the delimiter?
                If _firstDelim = 0 Then
                    ' we still need the delimiter, get it
                    _firstDelim = byt(0)
                    ' now remove it from the array
                    Dim arT(UBound(byt) - 1) As Byte
                    Array.Copy(byt, 1, arT, 0, byt.Length - 1)
                    ' restart without the delimiter (this part skipped on next run)
                    ProcessIncoming(arT, arT.Length, source_ip, source_port)
                    ' exit
                    Exit Sub
                End If
                ' we have the delimiter, check if the temp buffer contains data
                If _buff.Count > 1 Then
                    ' check buffer contains data, couldn't determine size
                    ' maybe legacy support should be used.

                    ' reset the delimiter
                    _firstDelim = 0
                    ' clear the buffer
                    _buff.Clear()
                    ' throw the exception
                    Throw New Exception("Unable to determine size of incoming packet.  It's possible you may need to use Legacy Support.")
                    ' exit just in case
                    Exit Sub
                End If
                ' check for the next instance of the delimiter
                Dim idx As Integer = Array.IndexOf(byt, _firstDelim)
                ' was the delimiter found
                If idx = -1 Then
                    ' delimiter not found, add the bytes to the temp buffer
                    _buff.Add(byt)
                    ' exit (wait for more data to complete the size)
                    Exit Sub
                End If
                ' delimiter was found, grab the size (part may be in the temp buffer) so combine them
                Dim temp(idx - 1) As Byte
                Array.Copy(byt, 0, temp, 0, idx)
                _buff.Add(temp)
                ' get the combined size
                temp = _buff.Combine()
                ' clear the buffer
                _buff.Clear()
                ' convert the bytes containing the size back to string
                Dim strSize As String = Encoding.ASCII.GetString(temp)
                ' try converting the string back to an integer
                If Not Integer.TryParse(strSize, _sizeComing) Then
                    ' data not an integer, maybe legacy support should be used
                    ' reset the delimiter and the size
                    _firstDelim = 0
                    _sizeComing = -1
                    ' clear the temp buffer
                    _buff.Clear()
                    ' throw the exception
                    Throw New Exception("Unable to determine size of incoming packet.  It's possible you may need to use Legacy Support.")
                    ' exit just in case
                    Exit Sub
                End If
                ' remove the size (and delimiter) from the byte array
                ReDim temp(UBound(byt) - (idx + 1))
                Array.Copy(byt, idx + 1, temp, 0, temp.Length)
                ' is there data to follow?
                If _sizeComing = 0 Then
                    ' no data followed the size
                    ' reset the size and the delimiter
                    _sizeComing = -1
                    _firstDelim = 0
                    ' exit
                    Exit Sub
                End If
                ' restart with the data (we have size so this will be skipped)
                ProcessIncoming(temp, temp.Length, source_ip, source_port)
                ' exit
                Exit Sub
            End If
            ' check to see if the byte array contains the full data
            If _buff.Count = 0 AndAlso UBound(byt) >= _sizeComing Then
                ' the byte array has everything
                Dim tmp(_sizeComing) As Byte
                Dim tBytes As Integer
                ' check if the byte array contains more that what were looking for
                If UBound(byt) > _sizeComing Then
                    ' more that we need - extract what we need
                    Array.Copy(byt, 0, tmp, 0, tmp.Length)
                    ' get the length of our data
                    tBytes = tmp.Length
                    ' add the object to the buffer queue
                    bufferCol.Add(tmp)
                    ' remove the data from the byte array, so only the extra is left
                    ReDim tmp(UBound(byt) - (_sizeComing + 1))
                    Array.Copy(byt, _sizeComing + 1, tmp, 0, tmp.Length)
                    ' reset the size and the delimiter
                    _sizeComing = -1
                    _firstDelim = 0
                    ' raise the DataArrival event
                    m_Monitor.DataReceived(tBytes, source_ip, source_port)
                    ' process the extra data for size/more data
                    ProcessIncoming(tmp, tmp.Length, source_ip, source_port)
                Else
                    ' exactly what we need - get the length
                    tBytes = byt.Length
                    ' add the object to the buffer queue
                    bufferCol.Add(byt)
                    ' reset the size and the delimiter
                    _sizeComing = -1
                    _firstDelim = 0
                    ' raise the DataArrival event
                    m_Monitor.DataReceived(tBytes, source_ip, source_port)
                End If
                ' part of the data is in the temp buffer, check if we have all we need now
            ElseIf _buff.Count > 0 AndAlso UBound(_buff.Combine) + byt.Length >= _sizeComing Then
                ' with the temp buffer, we have everything
                ' add all the current data to the temp buffer
                _buff.Add(byt)
                ' retrieve the combination of everything
                Dim tmp() As Byte = _buff.Combine()
                ' clear the temp buffer
                _buff.Clear()
                Dim tmpF(_sizeComing) As Byte
                Dim tBytes As Integer
                ' do we have more that we need
                If UBound(tmp) > _sizeComing Then
                    ' more that we need
                    ' extract our data
                    Array.Copy(tmp, 0, tmpF, 0, tmpF.Length)
                    ' get the length of our data
                    tBytes = tmpF.Length
                    ' add our data to the buffer queue
                    bufferCol.Add(tmpF)
                    ' remove this data from the byte array to leave us with the extra
                    ReDim tmpF(UBound(tmp) - (_sizeComing + 1))
                    Array.Copy(tmp, _sizeComing + 1, tmpF, 0, tmpF.Length)
                    ' reset the size and the delimiter
                    _sizeComing = -1
                    _firstDelim = 0
                    ' raise the DataArrival event
                    m_Monitor.DataReceived(tBytes, source_ip, source_port)
                    ' process the extra for size/more data
                    ProcessIncoming(tmpF, tmpF.Length, source_ip, source_port)
                Else
                    ' exactly what we need
                    ' get the length of it
                    tBytes = tmp.Length
                    ' add it to the buffer queue
                    bufferCol.Add(tmp)
                    ' reset the size and the delimiter
                    _sizeComing = -1
                    _firstDelim = 0
                    ' raise the DataArrival event
                    m_Monitor.DataReceived(tBytes, source_ip, source_port)
                End If
            Else
                ' don't have the full thing yet
                ' add it to the temp buffer
                _buff.Add(byt)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Determines which byte value was not used in the byte array.
    ''' </summary>
    ''' <param name="byt">The byte array to check.</param>
    Private Function FreeByte(ByVal byt() As Byte) As Byte
        'look for a free byte between 1 and 255
        Dim bc As New ByteCollection
        Dim lowest As Byte = bc.FreeByt(byt)
        Return lowest
    End Function
    ''' <summary>
    ''' Encloses a byte array with another byte.
    ''' </summary>
    ''' <param name="byt">A byte to enclose around a byte array.</param>
    ''' <param name="bytArr">The byte array that needs a byte enclosed around it.</param>
    Private Function EncloseByte(ByVal byt As Byte, ByVal bytArr() As Byte) As Byte()
        Dim orig As Integer = UBound(bytArr)
        Dim newa As Integer = orig + 2
        Dim ar(newa) As Byte
        ar(0) = byt
        Array.Copy(bytArr, 0, ar, 1, bytArr.Length)
        ar(newa) = byt
        Return ar
    End Function
    ''' <summary>
    ''' Combines two byte arrays.
    ''' </summary>
    Private Function AppendByte(ByVal first() As Byte, ByVal sec() As Byte) As Byte()
        Dim orig As Integer = UBound(first) + sec.Length
        Dim ar(orig) As Byte
        Array.Copy(first, 0, ar, 0, first.Length)
        Array.Copy(sec, 0, ar, UBound(first) + 1, sec.Length)
        Return ar
    End Function

    Private Class SendState

        Public Length As Integer
        Public ErrCode As SocketError
        Public Bytes() As Byte
        Public StartIndex As Integer
        Public SendLength As Integer
        Public TotalSent As Integer

    End Class
End Class

Public Class ByteCollection
    Inherits CollectionBase

    Public Sub New()
        For i As Integer = 1 To 255
            List.Add(CByte(i))
        Next
    End Sub

    Public Function FreeByt(ByVal byt() As Byte) As Integer
        For i As Integer = 0 To List.Count - 1
            If Array.IndexOf(byt, CByte(List.Item(i))) = -1 Then
                Return CByte(List.Item(i))
            End If
        Next
        Return 0
    End Function

    Public Function Contains(ByVal value As Byte) As Boolean
        Return List.Contains(value)
    End Function

    Public Sub Remove(ByVal value As Byte)
        If Contains(value) Then List.Remove(value)
    End Sub

    Public Function LowestValue() As Byte
        If List.Count = 0 Then Return CByte(0)
        Dim lowest As Byte = 255
        For i As Integer = 0 To List.Count - 1
            If CByte(List.Item(i)) < lowest Then lowest = List.Item(i)
        Next
        Return lowest
    End Function
End Class

Public Class ByteBufferCol
    Inherits CollectionBase

    Public Sub Add(ByVal byt As Byte)
        List.Add(byt)
    End Sub
    Public Sub Add(ByVal byt() As Byte)
        For i As Integer = 0 To UBound(byt)
            List.Add(byt(i))
        Next
    End Sub

    Public Function Combine() As Byte()
        If List.Count = 0 Then Return Nothing
        Dim ar(List.Count - 1) As Byte
        For i As Integer = 0 To List.Count - 1
            ar(i) = List.Item(i)
        Next
        Return ar
    End Function
End Class