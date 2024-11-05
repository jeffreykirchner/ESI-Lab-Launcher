Imports System.Net
Imports System.Net.Sockets
''' <summary>
''' A "monitor" for the <see cref="Winsock" /> class.
''' Holds all the functionality and calls back to the Winsock class to notify the user.
''' </summary>
''' <history>
''' 
''' </history>
Public Class WinsockMonitor

    Private m_Parent As Winsock
    Private m_Sock As AsyncSocket

    Public Sub New(ByVal wskWinsock As Winsock)
        m_Parent = wskWinsock
        m_Sock = Nothing
    End Sub

    Public ReadOnly Property State() As WinsockStates
        Get
            Return m_Parent.State
        End Get
    End Property
    Public ReadOnly Property Legacy_Support() As Boolean
        Get
            Return m_Parent.LegacySupport
        End Get
    End Property
    Friend ReadOnly Property Protocol() As WinsockProtocols
        Get
            Return m_Parent.Protocol
        End Get
    End Property
    Friend ReadOnly Property RemoteServer() As String
        Get
            Return m_Parent.RemoteServer
        End Get
    End Property
    Friend ReadOnly Property RemotePort() As Integer
        Get
            Return m_Parent.RemotePort
        End Get
    End Property

#Region " Close "

    Public Sub BeginClose()
        If m_Sock IsNot Nothing Then
            m_Sock.Close()
        End If
    End Sub
    Public Sub ClosingBegun()
        m_Parent.OnStateChanged(WinsockStates.Closing)
    End Sub

    Public Sub CloseDone()
        m_Parent.OnStateChanged(WinsockStates.Closed)
        m_Parent.OnDisconnected()
    End Sub

#End Region

#Region " Listen "

    Public Sub BeginListen(ByVal port As Integer, ByVal max_pending As Integer)
        If m_Parent.State = WinsockStates.Listening Then Exit Sub
        If m_Parent.State <> WinsockStates.Closed Then
            Throw New Exception("Cannot begin listening when already connected to a remote source.")
            Exit Sub
        End If
        m_Sock = New AsyncSocket(Me, m_Parent.BufferSize)
        m_Sock.Listen(port, max_pending)
    End Sub
    Public Sub ListeningStarted()
        m_Parent.OnStateChanged(WinsockStates.Listening)
    End Sub
    Public Sub ClientReceived(ByVal client As System.Net.Sockets.Socket)
        m_Parent.OnConnectionRequest(client)
    End Sub

#End Region

#Region " Accept "

    Public Sub DoAccept(ByVal client As System.Net.Sockets.Socket)
        m_Sock = New AsyncSocket(Me, client, m_Parent.BufferSize)
        If m_Sock IsNot Nothing Then m_Parent.OnConnected()
    End Sub

#End Region

#Region " Connect "

    Public Sub DoConnect(ByVal host As String, ByVal port As Integer)
        If State = WinsockStates.Listening Then
            Throw New Exception("Cannot connect to a server when listening for incoming connections.")
            Exit Sub
        End If
        If State <> WinsockStates.Closed Then
            Throw New Exception("Cannot connect to a server when already connected to a remote source.")
            Exit Sub
        End If
        m_Parent.OnStateChanged(WinsockStates.ResolvingHost)
        'Dns.BeginGetHostEntry(host, New AsyncCallback(AddressOf DoConnectCallback), port)
        Try

            Dim resolved As IPHostEntry = Dns.GetHostEntry(host)
            If resolved Is Nothing OrElse resolved.AddressList.Length = 0 Then
                m_Parent.OnStateChanged(WinsockStates.Closed)
                Dim name As String = ""
                If resolved IsNot Nothing Then name = """" & resolved.HostName & """ "
                m_Parent.OnError("Hostname " & name & "could not be resolved.")
                Exit Sub
            End If
            If State <> WinsockStates.ResolvingHost Then Exit Sub
            m_Parent.OnStateChanged(WinsockStates.HostResolved)
            If m_Sock Is Nothing Then m_Sock = New AsyncSocket(Me, m_Parent.BufferSize)
            If State <> WinsockStates.HostResolved Then Exit Sub
            Dim useIPv6 As Boolean = (m_Parent.IPType = WinsockIPTypes.IPv6)
            Dim ranConnect As Boolean = False
            If useIPv6 Then
                For Each ipaddr As System.Net.IPAddress In resolved.AddressList
                    If ipaddr.AddressFamily = AddressFamily.InterNetworkV6 Then
                        ranConnect = True
                        m_Sock.Connect(ipaddr, port)
                        Exit For
                    End If
                Next
            Else
                For Each ipaddr As System.Net.IPAddress In resolved.AddressList
                    If ipaddr.AddressFamily = AddressFamily.InterNetwork Then
                        ranConnect = True
                        m_Sock.Connect(ipaddr, port)
                        Exit For
                    End If
                Next
            End If
            If Not ranConnect Then
                m_Parent.OnStateChanged(WinsockStates.Closed)
                m_Parent.OnError("An address couldn't be found to use - socket did not connect.")
            End If

        Catch

        End Try
    End Sub
    Public Sub DoConnectCallback(ByVal ar As IAsyncResult)
        


    End Sub
    Public Sub ConnectStarted()
        m_Parent.OnStateChanged(WinsockStates.Connecting)
    End Sub
    Public Sub ConnectFinished()
        m_Parent.OnConnected()
    End Sub

#End Region

#Region " Send "

    Public Sub Send(ByVal byt() As Byte)
        If Not CheckSendState() Then Exit Sub
        m_Sock.Send(byt)
    End Sub
    Public Sub Send(ByVal data As String)
        Dim byt() As Byte = System.Text.Encoding.Default.GetBytes(data)
        Send(byt)
    End Sub
    ''' <history>
    ''' 06-09-2006 Added
    ''' </history>
    Public Sub Send(ByVal Data As System.Drawing.Bitmap)
        Dim str As New System.IO.MemoryStream
        Data.Save(str, System.Drawing.Imaging.ImageFormat.Bmp)
        Dim sendBytes(str.Length - 1) As Byte
        str.Position = 0
        str.Read(sendBytes, 0, str.Length)
        str.Close()
        Send(sendBytes)
    End Sub
    Public Sub SendFile(ByVal filename As String)
        If My.Computer.FileSystem.FileExists(filename) Then
            Dim fileBytes() As Byte = My.Computer.FileSystem.ReadAllBytes(filename)
            Send(fileBytes)
        End If
    End Sub
    Private Function CheckSendState() As Boolean
        If m_Parent.State = WinsockStates.Listening Then
            Throw New Exception("Cannot send data while listening for connections.")
            Return False
        End If
        If m_Parent.Protocol = WinsockProtocols.Tcp Then
            If m_Parent.State <> WinsockStates.Connected Then
                Throw New Exception("Cannot send data while not connected.")
                Return False
            End If
        Else
            If m_Sock Is Nothing Then m_Sock = New AsyncSocket(Me, m_Parent.BufferSize)
        End If
        Return True
    End Function
    Public Sub SendCompleted(ByVal bytesSent As Integer, ByVal bytesTotal As Integer)
        m_Parent.OnSendComplete(bytesSent, bytesTotal)
    End Sub
    Public Sub SendProgress(ByVal bytesSent As Integer, ByVal bytesTotal As Integer)
        m_Parent.OnSendProgress(bytesSent, bytesTotal)
    End Sub

#End Region

#Region " Get "

    Public Sub GetData(ByRef data As String)
        Dim dt() As Byte = m_Sock.GetData()
        data = System.Text.Encoding.Default.GetString(dt)
    End Sub
    Public Function GetDataFile() As Byte()
        Dim dt() As Byte = m_Sock.GetData()
        Return dt
    End Function

#End Region

    Public Function CountBuffer() As Integer
        Return m_Sock.BufferCount
    End Function

    Public Sub DataReceived(ByVal totalBytes As Integer, ByVal source_ip As String, ByVal source_port As Integer)
        m_Parent.OnDataArrival(totalBytes, source_ip, source_port)
    End Sub

    Public Sub ErrorNotify(ByVal msg As String, ByVal function_name As String, Optional ByVal errCode As SocketError = SocketError.SocketError, Optional ByVal exDetails As String = "")
        m_Parent.OnError(msg, function_name, errCode, exDetails)
    End Sub
End Class
