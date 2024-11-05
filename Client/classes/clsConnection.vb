Public Class clsConnection
    Public WithEvents wskClient As Winsock
    Public myIPAddress As String
    Public myNumber As Integer
    Public connectionNumber As Integer

    Public room As String

    Public Sub New(ByVal tempIP As String)
        Try
            myIPAddress = tempIP
            setup()
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub wskClient_DataArrival(ByVal sender As Object, ByVal e As WinsockDataArrivalEventArgs) Handles wskClient.DataArrival
        Try
            With frmMain
                Dim buf As String = Nothing
                CType(sender, Winsock).Get(buf)

                Dim msgtokens() As String = buf.Split("#")
                Dim i As Integer

                'appEventLog_Write("data arrival: " & buf)

                For i = 1 To msgtokens.Length - 1
                    takeMessage(msgtokens(i - 1))
                Next
            End With
        Catch ex As Exception
            appEventLog_Write("error wskClient_DataArrival:", ex)
        End Try
    End Sub

    Private Sub wskClient_ErrorReceived(ByVal sender As System.Object, ByVal e As WinsockErrorEventArgs) Handles wskClient.ErrorReceived
        With frmMain
            .buttonArray(myNumber).BackColor = Color.Red
            .cmdStart.Enabled = True
            .cmdLoad.Enabled = True
        End With
    End Sub

    Private Sub wskClient_StateChanged(ByVal sender As Object, ByVal e As WinsockStateChangingEventArgs) Handles wskClient.StateChanged
        Try
            With frmMain

                If e.New_State = WinsockStates.Closed Then
                    .buttonArray(myNumber).BackColor = Color.Red
                    .cmdStart.Enabled = True
                    .cmdLoad.Enabled = True
                ElseIf e.New_State = WinsockStates.Connected Then
                    .buttonArray(myNumber).BackColor = Color.LawnGreen
                End If

                Application.DoEvents()
            End With
        Catch ex As Exception
            appEventLog_Write("error wskClient_StateChanged:", ex)
        End Try

    End Sub

    Public Sub setup()
        Try

            wskClient = New Winsock
            wskClient.BufferSize = 8192
            wskClient.LegacySupport = False
            wskClient.LocalPort = 8080
            wskClient.MaxPendingConnections = 1
            wskClient.Protocol = WinsockProtocols.Tcp
            wskClient.RemotePort = myPortNumber
            wskClient.RemoteServer = myIPAddress
            wskClient.SynchronizingObject = frmMain

            'wskClient.Connect()
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Public Sub takeMessage(ByVal sinstr As String)
        Try
            'take message from server
            'msgtokens(0) has type of message sent, having different types of messages allows you to send different formats for different actions.
            'msgtokens(1) has the semicolon delimited data that is to be parsed and acted upon.  


            Dim msgtokens() As String
            msgtokens = sinstr.Split("|")

            Select Case msgtokens(0) 'case statement to handle each of the different types of messages
                Case "01"

                Case "02"

                Case "03"
                    'setID(msgtokens(1))
                Case "04"
                    frmError.Show()
                    frmError.txtErrors.Text &= room & " : " & msgtokens(1) & vbCrLf
                Case "05"
                    sendIPAddress(msgtokens(1))
                Case "06"

                Case "07"

                Case "08"

                Case "09"

                Case "10"

                Case "11"

                Case "12"

                Case "13"

                Case "14"

                Case "15"

            End Select
        Catch ex As Exception
            appEventLog_Write("error takeMessage:", ex)
        End Try

    End Sub

    Public Sub sendLaunchSoftware()
        Try
            With frmMain
                Dim outstr As String = .txtLocation.Text & ";"
                
                If .cbZtree.Checked Then
                    If .cbAutoConnect.Checked Then
                        outstr &= " /server " & .txtIPAddress.Text
                    End If

                    outstr &= " /language en"
                ElseIf .cbAutoConnect.Checked Then
                    outstr &= .txtIPAddress.Text
                End If

                If .txtArguments.Text <> "" Then

                    If .cbZtree.Checked Or .cbAutoConnect.Checked Then
                        outstr &= " "
                    End If

                    'auto increment if checked
                    If .cbAutoIncrement.Checked Then
                        outstr &= .txtArguments.Text.Replace("[+]", launchIncrement) & ";"
                    Else
                        outstr &= .txtArguments.Text & ";"
                    End If

                Else
                    outstr &= ";"
                End If

                outstr &= .cbRunAsAdmin.Checked & ";"
                outstr &= .txtUsername.Text & ";"
                outstr &= .txtPassword.Text & ";"

                wskClient.Send("01", outstr, connectionNumber)
            End With
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Public Sub sendKillSoftware()
        Try
            With frmMain
                Dim outstr As String = ""

                outstr &= .cbKillProcessByName.Checked & ";"
                outstr &= .txtKillSoftwareName.Text & ";"

                wskClient.Send("02", outstr, connectionNumber)
            End With
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Public Sub killReceiver()
        Try
            With frmMain
                wskClient.Send("04", "", connectionNumber)
            End With
        Catch ex As Exception
            appEventLog_Write("error takeMessage:", ex)
        End Try
    End Sub

    Public Sub shutdown()
        Try
            With frmMain
                wskClient.Send("05", "", connectionNumber)
            End With
        Catch ex As Exception
            appEventLog_Write("error takeMessage:", ex)
        End Try
    End Sub

    Public Sub restart()
        Try
            With frmMain
                wskClient.Send("06", "", connectionNumber)
            End With
        Catch ex As Exception
            appEventLog_Write("error takeMessage:", ex)
        End Try
    End Sub

    Public Sub logOff()
        Try
            With frmMain
                wskClient.Send("07", "", connectionNumber)
            End With
        Catch ex As Exception
            appEventLog_Write("error takeMessage:", ex)
        End Try
    End Sub

    Public Sub startShell()
        Try
            With frmMain
                wskClient.Send("08", "", connectionNumber)
            End With
        Catch ex As Exception
            appEventLog_Write("error takeMessage:", ex)
        End Try
    End Sub

    Public Sub killShell()
        Try
            With frmMain
                wskClient.Send("10", "", connectionNumber)
            End With
        Catch ex As Exception
            appEventLog_Write("error takeMessage:", ex)
        End Try
    End Sub

    Public Sub startExplorer()
        Try
            With frmMain
                wskClient.Send("09", .FolderBrowserDialog1.SelectedPath, connectionNumber)
            End With
        Catch ex As Exception
            appEventLog_Write("error takeMessage:", ex)
        End Try
    End Sub
End Class
