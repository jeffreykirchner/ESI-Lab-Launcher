
Imports System.Threading
Imports System.Net.Sockets
Imports System.Net
Imports System.Collections

Namespace SocketServer

    Public Class clsSocketSvr
        Private mblnRunning As Boolean
        Private marConnections(50) As clsSocketClient
        Private mSocket As TcpListener
        Private marThreads(50) As Thread

        Public Event StatusUpdate(ByVal strStatus As String)
        Public Event ClientConnected(ByVal ID As String)
        Public Event ClientDisconnected(ByVal ID As String)

#Region "Subs"

        Shared Function GetIPAddress() As String
            'Dim Address As System.Net.IPAddress

            'With System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
            '    Address = New System.Net.IPAddress(.AddressList(0).Address)
            'End With

            GetIPAddress = Net.Dns.GetHostEntry(SystemInformation.ComputerName).AddressList(0).ToString
        End Function

        Shared Function GetIPAddress(ByVal strHostName As String) As String
            'Dim Address As System.Net.IPAddress
            'With System.Net.Dns.GetHostEntry(strHostName)
            '    Address = New System.Net.IPAddress(.AddressList(0).Address)
            'End With

            GetIPAddress = Net.Dns.GetHostEntry(strHostName).AddressList(0).ToString
        End Function

        Public Sub New()
            Me.mblnRunning = False
        End Sub

        Public Sub StartServer()
            Dim i As Int16

            'RaiseEvent StatusUpdate("Starting Messaging Server" & vbCrLf)
            Me.mblnRunning = True
            Dim blnSlotFound As Boolean = False

            Dim LocalIP As IPAddress
            LocalIP = IPAddress.Parse(GetIPAddress)

            Dim LocalPort As Integer = 1001

            mSocket = New TcpListener(LocalIP, LocalPort)
            mSocket.Start()

            'RaiseEvent StatusUpdate("Accepting connections on Address " & GetIPAddress() & " Port " & LocalPort.ToString & vbCrLf)
            Dim PoolThread As New Thread(AddressOf PoolManager)
            PoolThread.Start()

            While Me.mblnRunning
                Try
                    Thread.Sleep(100)
                    Dim Temp As Socket = Me.mSocket.AcceptSocket
                    blnSlotFound = False
                    For i = 0 To marConnections.Length - 1
                        If marConnections(i) Is Nothing And Not blnSlotFound Then
                            marConnections(i) = New clsSocketClient(Temp, i)
                            AddHandler marConnections(i).DataReceived, AddressOf HandleIncomingMessage
                            marThreads(i) = New Thread(AddressOf marConnections(i).Listen)
                            marThreads(i).Start()
                            blnSlotFound = True

                            playerCount += 1

                            playerList(playerCount) = New player()
                            playerList(playerCount).inumber = playerCount
                            playerList(playerCount).socketNumber = i

                            Application.DoEvents()

                            Thread.Sleep(500)

                            Application.DoEvents()


                            playerList(playerCount).requsetIP(playerCount)

                            'RaiseEvent StatusUpdate("Connection Accepted from address " & CType(Temp.RemoteEndPoint, IPEndPoint).Address.ToString & vbCrLf)
                            RaiseEvent ClientConnected(marConnections(i).ClientID)
                        End If
                    Next

                Catch ex As Exception
                    'MsgBox(ex.ToString)
                End Try

            End While

        End Sub

        Public Sub StopServer()
            Dim i As Int16

            For i = 0 To marConnections.Length - 1
                If Not marConnections(i) Is Nothing Then
                    If marConnections(i).ClientSocket.Connected Then
                        marConnections(i).ClientSocket.Close()
                    End If
                    marConnections(i) = Nothing
                End If
            Next
            Me.mblnRunning = False

            If Not mSocket Is Nothing Then Me.mSocket.Stop()
            Me.mSocket = Nothing
            'RaiseEvent StatusUpdate("Server Stopped" & vbCrLf)
            GC.Collect()
        End Sub

        Private Sub PoolManager()
            Dim i As Int16

            While Me.mblnRunning
                Thread.Sleep(100)
                For i = 0 To marConnections.Length - 1
                    If Not marConnections(i) Is Nothing Then
                        If Not marConnections(i).ClientSocket.Connected Then
                            marConnections(i) = Nothing
                        End If
                    End If
                Next

            End While

        End Sub

        Sub HandleIncomingMessage(ByVal strMessage As String)
            If InStr(strMessage, "<DISCONNECT>") > 0 Then
                RaiseEvent ClientDisconnected(Mid(strMessage, 8, 1))
                marConnections(CInt(Mid(strMessage, 8, 1))) = Nothing
                Exit Sub
                'Else
                '    BroadcastMessage("02", strMessage, "client")
            End If

            RaiseEvent StatusUpdate(strMessage)
        End Sub

        Sub BroadcastMessage(ByVal messageId As String, ByVal strMessage As String, ByVal source As String)
            Dim i As Int16

            'For i = 0 To marConnections.Length - 1
            '    If Not marConnections(i) Is Nothing Then
            '        If marConnections(i).ClientSocket.Connected Then
            '            Try
            '                marConnections(i).ClientSocket.Send(System.Text.ASCIIEncoding.ASCII.GetBytes(messageId & ";" & strMessage & ";"))

            '                'If source = "server" Then
            '                '    RaiseEvent StatusUpdate(strMessage & vbCrLf)
            '                'Else
            '                '    RaiseEvent StatusUpdate("Server> " & strMessage & vbCrLf)
            '                'End If

            '            Catch ex As Exception

            '            End Try

            '        End If
            '    End If
            'Next

            For i = 1 To playerCount
                Try
                    marConnections(playerList(i).socketNumber).ClientSocket.Send(System.Text.ASCIIEncoding.ASCII.GetBytes(messageId & "|" & strMessage & "|"))
                Catch ex As Exception
                    MsgBox(ex.ToString)
                End Try
            Next

        End Sub

        Sub SendMessage(ByVal messageId As String, ByVal User As Integer, ByVal strMessage As String)
            'Dim i As Int16

            'For i = 0 To marConnections.Length - 1
            '    If Not marConnections(i) Is Nothing Then
            '        If marConnections(i).ClientSocket.Connected Then
            '            If marConnections(i).ClientID = Int(User) Then

            If marConnections(User) Is Nothing Then Exit Sub
            If Not marConnections(User).ClientSocket.Connected Then Exit Sub

            Try
                marConnections(User).ClientSocket.Send(System.Text.ASCIIEncoding.ASCII.GetBytes(messageId & "|" & strMessage & "|"))
                'RaiseEvent StatusUpdate("Server> " & strMessage & vbCrLf)
                'Thread.Sleep(100)
                Application.DoEvents()
                Exit Sub
            Catch ex As Exception

            End Try
            '            End If
            '        End If
            '    End If
            'Next


        End Sub

#End Region



#Region "Properties"

        Public ReadOnly Property ServerRunning() As Boolean
            Get
                Return mblnRunning
            End Get
        End Property

        Public ReadOnly Property ServerSocket() As TcpListener
            Get
                Return mSocket
            End Get
        End Property

#End Region


    End Class

    Public Class clsSocketClient

        Private mSocket As Socket
        Private minIndex As Int16
        Private mstrDataIn As String
        Public Event DataReceived(ByVal strMessage As String)
        Public Event ClientDisconnected(ByVal ID As String)

        Public Sub New(ByVal objSocket As Socket, ByVal Index As Int16)
            mSocket = objSocket
            minIndex = Index
        End Sub

        Public ReadOnly Property ClientID() As Int16
            Get
                Return minIndex
            End Get
        End Property

        Public ReadOnly Property ClientSocket() As Socket
            Get
                Return mSocket
            End Get
        End Property

        Public Sub Listen()
            While Me.mSocket.Connected
                Try
                    Thread.Sleep(100)
                    If Me.mSocket.Available > 0 Then
                        Dim lBuffer(Me.mSocket.Available) As Byte
                        Me.mSocket.Receive(lBuffer)
                        If lBuffer.Length > 0 Then
                            mstrDataIn = System.Text.ASCIIEncoding.ASCII.GetString(lBuffer)
                            RaiseEvent DataReceived(mstrDataIn)
                            Application.DoEvents()
                            mstrDataIn = ""
                        End If
                    End If
                Catch ex As Exception
                    'MsgBox(ex.ToString)
                End Try
            End While
            RaiseEvent ClientDisconnected(minIndex.ToString)
        End Sub
    End Class

End Namespace


