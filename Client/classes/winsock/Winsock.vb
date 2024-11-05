Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Reflection

''' <summary>
''' Encapsulates the socket functionality into an easy to use VB6 like component.
''' </summary>
''' <author>Chris Kolkman</author>
''' <history>
''' 02-07-2007 Added support for IPv6 vs IPv4.
''' 07-12-2006 Fixed closing to support UDP listener closing.
''' 06-27-2006 Added support for UDP.
''' 06-12-2006 Added support for the SendProgress event.
''' 06-09-2006 Added catches for closing errors.
'''            Added functionality to make the component thread-safe
'''             (calls to a textbox in the ErrorReceived handler won't
'''              error anymore).
'''            Added ability to send/get System.Drawing.Bitmap images.
'''            Modified some property names to be more consistent with other
'''             controls (removed underscores).
'''            Added action list (designer smart tags) support.
''' 06-08-2006 Basic functionality completed (sending/receiving string/files).
'''            Disconnects handled (server doesn't yet detect pulled plug at
'''              client.
'''            Can send and receive up to 2GB of data (in one send command).
''' </history>
<DefaultEvent("ErrorReceived")> _
<DesignerAttribute(GetType(WinsockDesigner), GetType(IDesigner))> _
Public Class Winsock
    Inherits Component

#Region " Private Members "

    Private m_Monitor As WinsockMonitor '           Provides an interface to the socket
    Private m_LocalPort As Integer '                Port to listen on (when listening)
    Private m_maxListenPending As Integer '         Size of the listen queue
    Private m_State As WinsockStates '              Current socket state
    Private m_remoteServer As String '              Server to connect to
    Private m_remotePort As Integer '               Port on server to connect to
    Private m_legacy As Boolean '                   Use Legacy (VB6) support?
    Private m_BufferSize As Integer '               Internal buffer size
    Private m_Protocol As WinsockProtocols '        Network protocol to use
    Private m_IPProtocol As WinsockIPTypes '        IPv6 or IPv4?
    Private _syncObject As System.ComponentModel.ISynchronizeInvoke

#End Region

#Region " Constructor "

    ''' <summary>
    ''' Initializes a new instances of the <see cref="Winsock" /> class.
    ''' </summary>
    ''' <remarks>Only thread safe if placed on a form (synchronizing object gets set automatically).</remarks>
    Public Sub New()
        m_Monitor = New WinsockMonitor(Me)
        m_State = WinsockStates.Closed
        m_LocalPort = 8080
        m_maxListenPending = 1
        m_remoteServer = "localhost"
        m_remotePort = 8080
        m_legacy = False
        m_BufferSize = 8192
        m_Protocol = WinsockProtocols.Tcp
        m_IPProtocol = WinsockIPTypes.IPv4
    End Sub
    ''' <summary>
    ''' Initializes a new instance of the <see cref="Winsock" /> class (thread-safe).
    ''' </summary>
    ''' <param name="Synchronizing_Object">The synchronizing object used to make this thread safe.</param>
    Public Sub New(ByVal Synchronizing_Object As System.ComponentModel.ISynchronizeInvoke)
        Me.New()
        _syncObject = Synchronizing_Object
    End Sub

#End Region

#Region " Events "

    Private Delegate Sub dDataArrival(ByVal sender As Object, ByVal e As WinsockDataArrivalEventArgs)
    Private Delegate Sub dSendComplete(ByVal sender As Object, ByVal e As WinsockSendEventArgs)
    Private Delegate Sub dDisconnected(ByVal sender As Object, ByVal e As System.EventArgs)
    Private Delegate Sub dConnected(ByVal sender As Object, ByVal e As System.EventArgs)
    Private Delegate Sub dConnectionRequest(ByVal sender As Object, ByVal e As WinsockClientReceivedEventArgs)
    Private Delegate Sub dErrorReceived(ByVal sender As Object, ByVal e As WinsockErrorEventArgs)
    Private Delegate Sub dStateChanged(ByVal sender As Object, ByVal e As WinsockStateChangingEventArgs)
    Private Delegate Sub dSendProgress(ByVal sender As Object, ByVal e As WinsockSendEventArgs)

    ''' <summary>
    ''' Occurs when data arrives on the socket.
    ''' </summary>
    ''' <remarks>Raised only after all parts of the data have been collected.</remarks>
    Public Event DataArrival(ByVal sender As Object, ByVal e As WinsockDataArrivalEventArgs)
    ''' <summary>
    ''' Occurs when sending of data is completed.
    ''' </summary>
    Public Event SendComplete(ByVal sender As Object, ByVal e As WinsockSendEventArgs)
    ''' <summary>
    ''' Occurs when disconnected from the remote computer (client and server).
    ''' </summary>
    Public Event Disconnected(ByVal sender As Object, ByVal e As System.EventArgs)
    ''' <summary>
    ''' Occurs when connection is achieved (client and server).
    ''' </summary>
    Public Event Connected(ByVal sender As Object, ByVal e As System.EventArgs)
    ''' <summary>
    ''' Occurs on the server when a client is attempting to connect.
    ''' </summary>
    ''' <remarks>Client registers connected at this point. Server must Accept in order for it to be connected.</remarks>
    Public Event ConnectionRequest(ByVal sender As Object, ByVal e As WinsockClientReceivedEventArgs)
    ''' <summary>
    ''' Occurs when an error is detected in the socket.
    ''' </summary>
    ''' <remarks>May also be raised on disconnected (depending on disconnect circumstance).</remarks>
    Public Event ErrorReceived(ByVal sender As Object, ByVal e As WinsockErrorEventArgs)
    ''' <summary>
    ''' Occurs when the state of the socket changes.
    ''' </summary>
    Public Event StateChanged(ByVal sender As Object, ByVal e As WinsockStateChangingEventArgs)
    ''' <summary>
    ''' Occurs when the send buffer has been sent but not all the data has been sent yet.
    ''' </summary>
    ''' <history>
    ''' 06-12-2006 Added
    ''' </history>
    Public Event SendProgress(ByVal sender As Object, ByVal e As WinsockSendEventArgs)

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets a value indicating the port the <see cref="Winsock" /> control should listen on.
    ''' </summary>
    ''' <remarks>Cannot change while listening.</remarks>
    Public Property LocalPort() As Integer
        Get
            Return m_LocalPort
        End Get
        Set(ByVal value As Integer)
            If State() = WinsockStates.Listening Then
                Throw New Exception("Cannot change the local port while already listening on a port.")
                Exit Property
            End If
            m_LocalPort = value
        End Set
    End Property
    ''' <summary>
    ''' Gets or sets a value that control the length of the maximum length of the pending connections queue.
    ''' </summary>
    ''' <remarks>Cannot change while listening.</remarks>
    Public Property MaxPendingConnections() As Integer
        Get
            Return m_maxListenPending
        End Get
        Set(ByVal value As Integer)
            If State() = WinsockStates.Listening Then
                Throw New Exception("Cannot change the pending connections value while already listening.")
                Exit Property
            End If
            m_maxListenPending = value
        End Set
    End Property
    ''' <summary>
    ''' Gets or sets a value that determines what server to connect to.
    ''' </summary>
    ''' <remarks>Can only change if closed or listening.</remarks>
    Public Property RemoteServer() As String
        Get
            Return m_remoteServer
        End Get
        Set(ByVal value As String)
            If State() <> WinsockStates.Closed AndAlso State() <> WinsockStates.Listening Then
                Throw New Exception("Cannot change the remote address while already connected to a remote server.")
                Exit Property
            End If
            m_remoteServer = value
        End Set
    End Property
    ''' <summary>
    ''' Gets or sets a value that determines which port on the server to connect to.
    ''' </summary>
    ''' <remarks>Can only change if closed or listening.</remarks>
    Public Property RemotePort() As Integer
        Get
            Return m_remotePort
        End Get
        Set(ByVal value As Integer)
            If State() <> WinsockStates.Closed AndAlso State() <> WinsockStates.Listening Then
                Throw New Exception("Cannot change the remote port while already connected to a remote server.")
                Exit Property
            End If
            m_remotePort = value
        End Set
    End Property
    ''' <summary>
    ''' Gets or sets a value indicating if Legacy support should be used or not.
    ''' </summary>
    ''' <remarks>Legacy support is to support VB6 winsock style connections.</remarks>
    Public Property LegacySupport() As Boolean
        Get
            Return m_legacy
        End Get
        Set(ByVal value As Boolean)
            If value = False AndAlso Protocol = WinsockProtocols.Udp Then
                Throw New Exception("Cannot disable legacy support when using UDP.")
                Exit Property
            End If
            m_legacy = value
        End Set
    End Property
    ''' <summary>
    ''' Gets a value indicating whether the buffer has data for retrieval.
    ''' </summary>
    <Browsable(False)> _
    Public ReadOnly Property HasData() As Boolean
        Get
            If m_Monitor.CountBuffer > 0 Then Return True
            Return False
        End Get
    End Property
    ''' <summary>
    ''' Gets or sets a value indicating the interal size of the byte buffers.
    ''' </summary>
    Public Property BufferSize() As Integer
        Get
            Return m_BufferSize
        End Get
        Set(ByVal value As Integer)
            If value < 50 Then value = 50
            m_BufferSize = value
        End Set
    End Property
    ''' <summary>
    ''' Gets or sets the synchronizing object making this control thread-safe.
    ''' </summary>
    <System.ComponentModel.Browsable(False)> _
    Public Property SynchronizingObject() As System.ComponentModel.ISynchronizeInvoke
        Get
            If _syncObject Is Nothing And Me.DesignMode Then
                Dim designer As IDesignerHost = Me.GetService(GetType(IDesignerHost))
                If Not (designer Is Nothing) Then
                    _syncObject = designer.RootComponent
                End If
            End If
            Return _syncObject
        End Get
        Set(ByVal Value As System.ComponentModel.ISynchronizeInvoke)
            If Not Me.DesignMode Then
                If Not (_syncObject Is Nothing) And Not (_syncObject Is Value) Then
                    Throw New Exception("Property cannot be set at run-time")
                Else
                    _syncObject = Value
                End If
            End If
        End Set
    End Property
    ''' <summary>
    ''' Gets or sets the winsock protocol to use when communicating with the remote computer.
    ''' </summary>
    Public Property Protocol() As WinsockProtocols
        Get
            Return m_Protocol
        End Get
        Set(ByVal value As WinsockProtocols)
            If State() <> WinsockStates.Closed Then
                Throw New Exception("Cannot change the protocol while listening or connected to a remote server.")
                Exit Property
            End If
            m_Protocol = value
            If value = WinsockProtocols.Udp Then LegacySupport = True
        End Set
    End Property
    ''' <summary>
    ''' Gets or sets the IP type to use when connecting to the remote computer.
    ''' </summary>
    Public Property IPType() As WinsockIPTypes
        Get
            Return Me.m_IPProtocol
        End Get
        Set(ByVal value As WinsockIPTypes)
            If State() <> WinsockStates.Closed Then
                Throw New Exception("Cannot change the IP type while listening or connected to a remote server.")
                Exit Property
            End If
            Me.m_IPProtocol = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Places a <see cref="Winsock" /> in a listening state.
    ''' </summary>
    Public Sub Listen()
        m_Monitor.BeginListen(m_LocalPort, m_maxListenPending)
    End Sub
    ''' <summary>
    ''' Places a <see cref="Winsock" /> in a listening state.
    ''' </summary>
    ''' <param name="port">The port to begin listening on.</param>
    Public Sub Listen(ByVal port As Integer)
        LocalPort = port
        Listen()
    End Sub
    ''' <summary>
    ''' Closes an open <see cref="Winsock" /> connection.
    ''' </summary>
    Public Sub Close()
        m_Monitor.BeginClose()
    End Sub
    ''' <summary>
    ''' Accepts a client connect as valid and begins to monitor it for incoming data.
    ''' </summary>
    ''' <param name="client">A <see cref="System.Net.Sockets.Socket" /> that represent the client being accepted.</param>
    Public Sub Accept(ByVal client As System.Net.Sockets.Socket)
        m_Monitor.DoAccept(client)
    End Sub
    ''' <summary>
    ''' Establishes a connection to a remote host.
    ''' </summary>
    Public Sub Connect()
        m_Monitor.DoConnect(RemoteServer, RemotePort)
        '    Me.Hide()
        '    frmConnect.Show()
        'End If
    End Sub
    ''' <summary>
    ''' Establishes a connection to a remote host.
    ''' </summary>
    ''' <param name="remoteHostOrIP">A <see cref="System.String" /> containing the Hostname or IP address of the remote host.</param>
    ''' <param name="remote_Port">A value indicated the port on the remote host to connect to.</param>
    Public Sub Connect(ByVal remoteHostOrIP As String, ByVal remote_Port As Integer)
        RemoteServer = remoteHostOrIP
        RemotePort = remote_Port
        Connect()
    End Sub
    ''' <summary>
    ''' Gets the local machine's IP address.
    ''' </summary>
    ''' <remarks>In the case a machine has more than one IP address it retrieves the first one.</remarks>
    Public Function LocalIP() As String
        Dim h As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName)

        For i As Integer = 0 To h.AddressList.Length - 1
            If InStr(h.AddressList.GetValue(i).ToString, ".") Then
                Return CType(h.AddressList.GetValue(i), Net.IPAddress).ToString
            End If
        Next

        Return CType(h.AddressList.GetValue(0), Net.IPAddress).ToString
    End Function
    ''' <summary>
    ''' Gets the state of the <see cref="Winsock" /> control.
    ''' </summary>
    Public Function State() As WinsockStates
        Return m_State
    End Function

#Region " Send "

    ''' <summary>
    ''' Sends a raw byte array to a connected socket on a remote computer.
    ''' </summary>
    ''' <param name="byt">The byte array to send.</param>
    Public Sub Send(ByVal byt() As Byte)
        m_Monitor.Send(byt)
    End Sub
    ''' <summary>
    ''' Sends a string to a connected socket on a remote computer.
    ''' </summary>
    ''' <param name="data">The string to send.</param>
    Public Sub Send(ByVal header As String, ByVal data As String, ByVal myNumber As Integer)
        Dim outstr As String
        outstr = myNumber & "|" & header & "|" & data & "|" & "#"
        m_Monitor.Send(outstr)
    End Sub
    ''' <summary>
    ''' Sends a System.Drawing.Bitmap to a connected socket on a remote computer.
    ''' </summary>
    ''' <param name="data">The image to be transmitted.</param>
    ''' <history>
    ''' 06-09-2006 Added
    ''' </history>
    Public Sub Send(ByVal data As System.Drawing.Bitmap)
        m_Monitor.Send(data)
    End Sub
    ''' <summary>
    ''' Sends a file to a connected socket on a remote computer.
    ''' </summary>
    ''' <param name="filename">The full path of the file you wish to send.</param>
    Public Sub SendFile(ByVal filename As String)
        m_Monitor.SendFile(filename)
    End Sub

#End Region

#Region " Get "

    ''' <summary>
    ''' Gets the next value in the buffer as a string.
    ''' </summary>
    ''' <param name="data">A String that stores the returned data.</param>
    Public Sub [Get](ByRef data As String)
        m_Monitor.GetData(data)
    End Sub
    ''' <summary>
    ''' Gets the next value in the buffer as a System.Drawing.Bitmap
    ''' </summary>
    ''' <param name="data">A System.Drawing.Bitmap that stores the returned image.</param>
    ''' <history>
    ''' 06-09-2006 Added
    ''' </history>
    Public Sub [Get](ByRef data As System.Drawing.Bitmap)
        Dim byt() As Byte = m_Monitor.GetDataFile()
        Dim str As New System.IO.MemoryStream(byt, False)
        data = System.Drawing.Bitmap.FromStream(str)
        str.Close()
    End Sub
    ''' <summary>
    ''' Saves the next value in the buffer as a file.
    ''' </summary>
    ''' <param name="filename">The full path of the file you wish to save to.</param>
    Public Sub GetFile(ByVal filename As String, Optional ByVal append As Boolean = False)
        Dim byt() As Byte = m_Monitor.GetDataFile()
        My.Computer.FileSystem.WriteAllBytes(filename, byt, append)
    End Sub

#End Region

#End Region

#Region " Monitor Hooks "

    ''' <summary>
    ''' Changes the state and raises the StateChanged event (thread safe).
    ''' </summary>
    ''' <param name="new_state">The state that the control is changing to.</param>
    Protected Friend Sub OnStateChanged(ByVal new_state As WinsockStates)
        If m_State <> new_state Then
            Dim x As New WinsockStateChangingEventArgs(m_State, new_state)
            m_State = new_state
            Dim d As New dStateChanged(AddressOf RaiseState)
            If _syncObject IsNot Nothing Then
                If Not closing Then _syncObject.Invoke(d, New Object() {Me, x})
            Else
                RaiseState(Me, x)
            End If
        End If
    End Sub
    ''' <summary>
    ''' Raises the ErrorReceived event (thread safe).
    ''' </summary>
    ''' <param name="msg">A String containing the error message.</param>
    ''' <param name="func">A String containing the function in which the error originated.</param>
    ''' <param name="errCode">The ErrorCode object returned by the socket.</param>
    Protected Friend Sub OnError(ByVal msg As String, Optional ByVal func As String = Nothing, Optional ByVal errCode As System.Net.Sockets.SocketError = Net.Sockets.SocketError.SocketError, Optional ByVal exDetails As String = "")
        Dim x As New WinsockErrorEventArgs(msg, func, errCode, exDetails)
        Dim d As New dErrorReceived(AddressOf RaiseError)
        If _syncObject IsNot Nothing Then
            _syncObject.Invoke(d, New Object() {Me, x})
        Else
            RaiseError(Me, x)
        End If
    End Sub
    ''' <summary>
    ''' Raises the ConnectionRequest event, and closes cancelled connections (thread safe).
    ''' </summary>
    ''' <param name="sock">The new client socket that needs to be accepted.</param>
    Protected Friend Sub OnConnectionRequest(ByVal sock As System.Net.Sockets.Socket)
        Dim x As New WinsockClientReceivedEventArgs(sock)
        Dim d As New dConnectionRequest(AddressOf RaiseConReq)
        If _syncObject IsNot Nothing Then
            _syncObject.Invoke(d, New Object() {Me, x})
        Else
            RaiseConReq(Me, x)
        End If
    End Sub
    ''' <summary>
    ''' Raises the Connected event (thread safe).
    ''' </summary>
    Protected Friend Sub OnConnected()
        OnStateChanged(WinsockStates.Connected)
        Dim d As New dConnected(AddressOf RaiseConnected)
        If _syncObject IsNot Nothing Then
            _syncObject.Invoke(d, New Object() {Me, New System.EventArgs()})
        Else
            RaiseConnected(Me, New System.EventArgs())
        End If
    End Sub
    ''' <summary>
    ''' Raises the Disconnected event (thread safe).
    ''' </summary>
    ''' <remarks></remarks>
    Protected Friend Sub OnDisconnected()
        Dim d As New dDisconnected(AddressOf RaiseDisconnected)
        If _syncObject IsNot Nothing Then
            If Not closing Then _syncObject.Invoke(d, New Object() {Me, New System.EventArgs()})
        Else
            RaiseDisconnected(Me, New System.EventArgs())
        End If
    End Sub
    ''' <summary>
    ''' Raises the SendComplete event (thread safe).
    ''' </summary>
    ''' <param name="bytes_sent">The total bytes sent.</param>
    ''' <param name="bytes_total">The number of bytes that should have been sent.</param>
    Protected Friend Sub OnSendComplete(ByVal bytes_sent As Integer, ByVal bytes_total As Integer)
        Dim x As New WinsockSendEventArgs(bytes_sent, bytes_total)
        Dim d As New dSendComplete(AddressOf RaiseSendComplete)
        If _syncObject IsNot Nothing Then
            _syncObject.Invoke(d, New Object() {Me, x})
        Else
            RaiseSendComplete(Me, x)
        End If
    End Sub
    ''' <summary>
    ''' Raises the SendProgress event (thread safe).
    ''' </summary>
    ''' <param name="bytes_sent">The total bytes sent so far.</param>
    ''' <param name="bytes_total">The number of bytes that need to be sent.</param>
    ''' <history>
    ''' 06-12-2006 Added
    ''' </history>
    Protected Friend Sub OnSendProgress(ByVal bytes_sent As Integer, ByVal bytes_total As Integer)
        Dim x As New WinsockSendEventArgs(bytes_sent, bytes_total)
        Dim d As New dSendProgress(AddressOf RaiseSendProgress)
        If _syncObject IsNot Nothing Then
            _syncObject.Invoke(d, New Object() {Me, x})
        Else
            RaiseSendProgress(Me, x)
        End If
    End Sub
    ''' <summary>
    ''' Raises the DataArrival event (thread safe).
    ''' </summary>
    ''' <param name="totalBytes">The total number of bytes received.</param>
    Protected Friend Sub OnDataArrival(ByVal totalBytes As Integer, ByVal source_ip As String, ByVal source_port As Integer)
        Dim x As New WinsockDataArrivalEventArgs(totalBytes, source_ip, source_port)
        Dim d As New dDataArrival(AddressOf RaiseDataArrival)
        If _syncObject IsNot Nothing Then
            _syncObject.Invoke(d, New Object() {Me, x})
        Else
            RaiseDataArrival(Me, x)
        End If
    End Sub

#Region " Thread safe delegate calls "

    Private Sub RaiseState(ByVal sender As Object, ByVal e As WinsockStateChangingEventArgs)
        RaiseEvent StateChanged(sender, e)
        Application.DoEvents()
    End Sub
    Private Sub RaiseError(ByVal sender As Object, ByVal e As WinsockErrorEventArgs)
        RaiseEvent ErrorReceived(sender, e)
    End Sub
    Private Sub RaiseConReq(ByVal sender As Object, ByVal e As WinsockClientReceivedEventArgs)
        RaiseEvent ConnectionRequest(sender, e)
        If e.Cancel Then
            e.Client.Disconnect(False)
            e.Client.Close()
        End If
    End Sub
    Private Sub RaiseConnected(ByVal sender As Object, ByVal e As System.EventArgs)
        RaiseEvent Connected(sender, e)
    End Sub
    Private Sub RaiseDisconnected(ByVal sender As Object, ByVal e As System.EventArgs)
        RaiseEvent Disconnected(sender, e)
    End Sub
    Private Sub RaiseSendComplete(ByVal sender As Object, ByVal e As WinsockSendEventArgs)
        RaiseEvent SendComplete(sender, e)
    End Sub
    Private Sub RaiseSendProgress(ByVal sender As Object, ByVal e As WinsockSendEventArgs)
        RaiseEvent SendProgress(sender, e)
    End Sub
    Private Sub RaiseDataArrival(ByVal sender As Object, ByVal e As WinsockDataArrivalEventArgs)
        RaiseEvent DataArrival(sender, e)
    End Sub

#End Region

#End Region

End Class
