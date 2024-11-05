
Imports System.Net
Imports System.Net.Sockets
Imports System.Text

Public Class TCPConnection
    ' For TCP connections intercepted by the TcpListener 
    Private tcpConn As TcpClient
    Private m_requestID As Guid = Guid.NewGuid
    Private objClient As TcpClient
    Private arData(1024) As Byte
    Private m_ClientIP As String
    Private m_State As Short
    Private objText As New StringBuilder()

    Public Event Connected(ByVal sender As TCPConnection)
    Public Event Disconnected(ByVal sender As TCPConnection)
    Public Event DataReceived(ByVal sender As TCPConnection, ByVal Data As String, ByVal BytesReceived As Long)
    Public Event SockError(ByVal strMessage As String)

    Public Sub New(ByVal client As TcpClient)
        ' A new instance of the class started meaning a new connection 
        objClient = client

        RaiseEvent Connected(Me)
        'Console.Writeline("Test")
        m_State = 1
        objClient.GetStream.BeginRead(arData, 0, 1024, _
         AddressOf Receive, Nothing)
    End Sub

    Public Sub SendData(ByVal ID As String, ByVal Data As String, ByVal myNumber As String)        ' Server sending to client. String Data 

        Application.DoEvents()

        If objClient.Connected Then
            m_State = 3
            Data = myNumber & "|" & ID & "|" & Data & "|"
            objClient.GetStream().Write(Encoding.ASCII.GetBytes(Data), 0, Encoding.ASCII.GetBytes(Data).Length)
            m_State = 1
        End If

        Application.DoEvents()

    End Sub

    Private Sub Receive(ByVal ar As IAsyncResult)
        ' Start the data receiving process 
        Dim intCount As Integer
        Console.WriteLine("Received")
        Try
            SyncLock objClient.GetStream
                intCount = objClient.GetStream.EndRead(ar)
            End SyncLock
            If intCount < 1 Then
                RaiseEvent Disconnected(Me)
                m_State = 0
                Exit Sub
            End If

            BuildString(arData, 0, intCount)
            SyncLock objClient.GetStream
                objClient.GetStream.BeginRead(arData, 0, 1024, AddressOf Receive, Nothing)
            End SyncLock
        Catch e As Exception
            MsgBox("receive error: " & e.Message, MsgBoxStyle.Exclamation)
            RaiseEvent SockError(e.Message.ToString)
            'RaiseEvent Disconnected(Me)
            m_State = 0
        End Try
    End Sub

    Private Sub BuildString(ByVal Bytes() As Byte, ByVal offset As Integer, ByVal count As Integer)
        Dim intIndex As Integer
        Dim txt As String
        'Console.WriteLine("BuildString")
        m_State = 2

        objText = New StringBuilder()
        For intIndex = offset To offset + count - 1
            txt = ChrW(Bytes(intIndex))
            objText.Append(txt)
        Next
        RaiseEvent DataReceived(Me, objText.ToString, objText.Length)
        m_State = 1
    End Sub

    Public ReadOnly Property requestID() As String
        ' A connections requestID to identify it.
        ' This property returns a Guid. Use this to identify a specific
        ' client when using as as multi-connection server. You can
        ' store the class reference into a collection or hashtable
        ' and use the requestID as the unique key.
        Get
            Return m_requestID.ToString
        End Get
    End Property

    Public ReadOnly Property ClientIP() As String
        ' The ip address of the connected client 
        Get
            Return m_ClientIP
        End Get
    End Property

    Public ReadOnly Property State() As Short
        ' The ip address of the connected client 
        ' Possible values:
        ' 0 - Disconnected
        ' 1 - Connected And Ready
        ' 2 - Receiving
        ' 3 - Sending
        ' 4 - Connecting
        Get
            Return m_State
        End Get
    End Property

    Public Sub Connect(ByVal strDomain As String, ByVal intPort As Integer)
        m_State = 4
        Try
            objClient = New TcpClient(strDomain, intPort)
            RaiseEvent Connected(Me)
            m_State = 1
            objClient.ReceiveBufferSize = 1024
            objClient.NoDelay = True
        Catch e As Exception
            RaiseEvent SockError(e.Message.ToString)
            m_State = 0
        End Try
    End Sub

    Public Sub Disconnect()
        'Me.SendData("", "<DISCONNECT>")
        RaiseEvent Disconnected(Me)
    End Sub
End Class