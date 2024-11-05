Imports System
Imports System.ComponentModel
Imports System.Threading
Imports System.Windows.Forms

Public Class frmMain
#Region " Winsock Code "
    Public WithEvents wsk_Col As New WinsockCollection
    Private _users As New UserCollection
    Public WithEvents wskListener As Winsock

    Dim pbAltF4 As Boolean = False
    Public closeProgram As Boolean = False

    Private Sub wskListener_ConnectionRequest(ByVal sender As Object, ByVal e As WinsockClientReceivedEventArgs) Handles wskListener.ConnectionRequest
        Try
            'Log("Connection received from: " & e.ClientIP)
            Dim y As New clsUser
            Dim ID As String = connectionCount + 1

            connectionCount += 1

            _users.Add(y)
            Dim x As New Winsock(Me)
            wsk_Col.Add(x, ID)
            x.Accept(e.Client)


            'Dim found As Boolean = False
            'For i = 1 To playerCount
            '    If playerList(i) IsNot Nothing Then
            '        If playerList(i).myIPAddress = e.ClientIP Then
            '            playerList(i).socketNumber = ID
            '            found = True
            '            Exit For
            '        End If
            '    End If
            'Next

            'If Not found Then
            playerCount += 1
            playerList(playerCount) = New player()
            playerList(playerCount).inumber = playerCount
            playerList(playerCount).socketNumber = ID
            playerList(playerCount).myIPAddress = e.ClientIP

            playerList(playerCount).requsetIP(playerCount)
            'End If

        Catch ex As Exception
            appEventLog_Write("error wskListener_ConnectionRequest:", ex)
        End Try
    End Sub

    Private Sub wskListener_ErrorReceived(ByVal sender As System.Object, ByVal e As WinsockErrorEventArgs) Handles wskListener.ErrorReceived
        Try
            appEventLog_Write("winsock error: " & e.Message)
        Catch ex As Exception
            appEventLog_Write("error wskListener_ErrorReceived:", ex)
        End Try
    End Sub

    Private Sub wskListener_StateChanged(ByVal sender As Object, ByVal e As WinsockStateChangingEventArgs) Handles wskListener.StateChanged

    End Sub



    Private Sub Wsk_DataArrival(ByVal sender As Object, ByVal e As WinsockDataArrivalEventArgs) Handles wsk_Col.DataArrival
        Try
            Dim sender_key As String = wsk_Col.GetKey(sender)
            Dim buf As String = Nothing
            CType(sender, Winsock).Get(buf)

            Dim msgtokens() As String = buf.Split("#")
            Dim i As Integer

            'appEventLog_Write("data arrival: " & buf)

            For i = 1 To msgtokens.Length - 1
                takeMessage(msgtokens(i - 1))
            Next

        Catch ex As Exception
            appEventLog_Write("error Wsk_DataArrival:", ex)
        End Try
    End Sub

    Private Sub Wsk_Disconnected(ByVal sender As Object, ByVal e As System.EventArgs) Handles wsk_Col.Disconnected
        Try
            wsk_Col.Remove(sender)
           
        Catch ex As Exception
            appEventLog_Write("error Wsk_Disconnected:", ex)
        End Try
    End Sub
    Private Sub Wsk_Connected(ByVal sender As Object, ByVal e As System.EventArgs) Handles wsk_Col.Connected

    End Sub

    Private Sub ShutDownServer()
        Try
            GC.Collect()
        Catch ex As Exception
            appEventLog_Write("error ShutDownServer:", ex)
        End Try

    End Sub
#End Region    'communication code

    Dim tempTime As String 'time stamp at start of experiment

    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Try

            'setup communication on load
            wskListener = New Winsock
            wskListener.BufferSize = 8192
            wskListener.LegacySupport = False
            wskListener.LocalPort = portNumber
            wskListener.MaxPendingConnections = 1
            wskListener.Protocol = WinsockProtocols.Tcp
            wskListener.RemotePort = 8080
            wskListener.RemoteServer = "localhost"
            wskListener.SynchronizingObject = Me

            wskListener.Listen()

            playerCount = 0

            Me.Text = "Launch Receiver: " & wskListener.LocalIP & " / " & SystemInformation.ComputerName

        Catch ex As Exception
            appEventLog_Write("error frmSvr_Load:", ex)
        End Try

    End Sub


    Private Sub frmMain_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown
        Try
            If e.Control = True Then
                If CInt(e.KeyValue) = CInt(Keys.K) Then
                    If MessageBox.Show("Close Program?", "Close", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.No Then Exit Sub
                    closeProgram = True
                    Me.Close()
                ElseIf CInt(e.KeyValue) = CInt(Keys.E) Then
                    Shell(System.Windows.Forms.Application.StartupPath & "\2xExplorer\2xExplorer.exe", AppWinStyle.NormalFocus)
                ElseIf CInt(e.KeyValue) = CInt(Keys.S) Then
                    'start or kill explorer

                    Dim p() As Process
                    p = Process.GetProcessesByName("explorer")

                    If p.Count > 0 Then
                        taskKillAction("explorer.exe")
                    Else
                        Shell("explorer.exe")
                    End If

                End If
            ElseIf e.Alt Then
                If CInt(e.KeyValue) = CInt(Keys.A) Then
                    frmAdministrator.Show()
                End If
            End If
        Catch ex As Exception
            appEventLog_Write("error frmSvr_Load:", ex)
        End Try
    End Sub

    Private Sub frmMain_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            If Not closeProgram Then e.Cancel = True

        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub
End Class
