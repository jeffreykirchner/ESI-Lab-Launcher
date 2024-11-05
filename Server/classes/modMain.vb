'Programmed by Jeffrey Kirchner
'kirchner@chapman.edu/jkirchner@gmail.com
'Programmed by Jeffrey Kirchner
'Economic Science Institute, Chapman University 2008-2011 ©

Module modMain
#Region " General Variables "
    Public playerList(1000) As player                  'array of players
    Public playerCount As Integer                    'number of players connected
    Public sfile As String                           'location of intialization file  
    Public checkin As Integer                        'global counter 
    Public connectionCount As Integer                'total number of connections made since server start 
    Public portNumber As Integer                     'port number sockect traffic is operation on 
    Public frmServer As New frmMain                  'main form     
#End Region

    'global variables here
    Public numberOfPeriods As Integer     'number of periods
    Public currentPeriod As Integer       'current period 
    Public restartDesktop As Boolean
    Public killShell As Boolean

    Dim ss As Security.SecureString = New Security.SecureString()
    Dim adminUserName As String
    Dim adminPassword As String


    Public Sub main(ByVal args() As String)
        connectionCount = 0

        AppEventLog_Init()
        appEventLog_Write("Load")

        loadParameters()

        restartDesktopWindowManager()
        If killShell Then taskKillAction("explorer.exe")

        Application.EnableVisualStyles()
        Application.Run(frmServer)

        appEventLog_Write("Exit")
        AppEventLog_Close()
    End Sub

    Public Sub takeIP(ByVal sinstr As String, ByVal index As Integer)
        Try
            playerList(index).ipAddress = sinstr
        Catch ex As Exception
            appEventLog_Write("error takeIP:", ex)
        End Try
    End Sub

    Public Sub takeMessage(ByVal sinstr As String)
        'when a message is received from a client it is parsed here
        'msgtokens(1) has type of message sent, having different types of messages allows you to send different formats for different actions.
        'msgtokens(2) has the semicolon delimited data that is to be parsed and acted upon.  
        'index has the client ID that sent the data.  Client ID is assigned by connection order, indexed from 1.

        Try
            With frmServer
                Dim msgtokens() As String


                msgtokens = sinstr.Split("|")

                Dim index As Integer
                index = msgtokens(0)

                Application.DoEvents()

                Select Case msgtokens(1) 'case statement to handle each of the different types of messages
                    Case "01"
                        playerList(index).startProcess(msgtokens(2))
                    Case "02"
                        playerList(index).killProcesses(msgtokens(2))
                    Case "03"
                        takeIP(msgtokens(2), index)
                    Case "04"
                        .closeProgram = True
                        .Close()
                    Case "05"
                        .closeProgram = True
                        Shell("shutdown -s -f -t 0")
                    Case "06"
                        .closeProgram = True
                        Shell("shutdown -r -f -t 0")
                    Case "07"
                        .closeProgram = True
                        Shell("shutdown -l -t 0")
                    Case "08"
                        Shell("explorer.exe")
                    Case "09"
                        Dim pp As New ProcessStartInfo
                        pp.FileName = System.Windows.Forms.Application.StartupPath & "\2xExplorer\2xExplorer.exe"
                        pp.Arguments = msgtokens(2)
                        pp.WindowStyle = ProcessWindowStyle.Normal

                        playerList(index).explorerProcess = New System.Diagnostics.Process
                        playerList(index).explorerProcess = System.Diagnostics.Process.Start(pp)

                    Case "10"
                        taskKillAction("explorer.exe")
                    Case "11"

                    Case "12"

                    Case "13"


                End Select

                Application.DoEvents()

                'Windows.Forms.Cursor.Position = New Point(Windows.Forms.Cursor.Position.X + 10, Windows.Forms.Cursor.Position.Y + 10)
                wakeUp()

            End With
            'all subs/functions should have an error trap
        Catch ex As Exception
            appEventLog_Write("error takeMessage: " & sinstr & " : ", ex)
        End Try

    End Sub

    Public Sub loadParameters()
        Try
            'load parameters from server.ini

            sfile = System.Windows.Forms.Application.StartupPath & "\server.ini"

            portNumber = getINI(sfile, "gameSettings", "port")
            restartDesktop = getINI(sfile, "gameSettings", "restartDesktop")
            killShell = getINI(sfile, "gameSettings", "killShell")

            adminUserName = decryptString(getINI(sfile, "gameSettings", "legacy1"))
            adminPassword = decryptString(getINI(sfile, "gameSettings", "legacy2"))

            For i As Integer = 1 To adminPassword.Length
                ss.AppendChar(adminPassword(i - 1))
            Next

        Catch ex As Exception
            appEventLog_Write("error loadParameters:", ex)
        End Try
    End Sub

    Public Sub taskKillAction(ByVal processName As String)
        Try
            'If Not killShell Then Exit Sub

            'Dim p() As Process
            'p = System.Diagnostics.Process.GetProcessesByName("explorer")

            'For i As Integer = 1 To p.Length
            '    p(i - 1).Kill()
            'Next

            Shell("taskkill.exe /f /im " & processName & " /u " & adminUserName & " /p " & adminPassword & " /s localhost", _
                   AppWinStyle.NormalFocus, _
                   False)


        Catch ex As Exception
            appEventLog_Write("error loadParameters:", ex)
        End Try
    End Sub

    Public Sub restartDesktopWindowManager()
        Try
            If Not restartDesktop Then Exit Sub

            Dim pp As New ProcessStartInfo

            Dim startDelay As Integer = getINI(sfile, "gameSettings", "startDelay")
            Threading.Thread.Sleep(startDelay)

            pp.FileName = "net.exe"
            pp.Arguments = "stop ""Desktop Window Manager Session Manager"""
            pp.WindowStyle = ProcessWindowStyle.Hidden

            pp.UserName = adminUserName
            pp.Domain = "chapmanedu"

            pp.Password = ss
            pp.UseShellExecute = False

            System.Diagnostics.Process.Start(pp).WaitForExit()

            Application.DoEvents()

            pp = New ProcessStartInfo

            pp.FileName = "C:\Windows\System32\net.exe"
            pp.Arguments = "start ""Desktop Window Manager Session Manager"""
            pp.WindowStyle = ProcessWindowStyle.Hidden

            pp.UserName = adminUserName
            pp.Domain = "chapmanedu"

            pp.Password = ss
            pp.UseShellExecute = False

            System.Diagnostics.Process.Start(pp)
        Catch ex As Exception
            appEventLog_Write("error restartDesktopWindowManager:", ex)
            MessageBox.Show("Error" & ex.Message)
        End Try
    End Sub

    Public Function encryptString(ByVal str As String) As String
        Dim tempR As Integer = rand(9, 1)
        Dim tempS As String = tempR

        For i As Integer = 1 To str.Length
            tempS &= Chr(Asc(str(i - 1)) + tempR)
        Next

        For i As Integer = 1 To tempR
            tempS &= Chr(rand(100, 20))
        Next

        Return tempS
    End Function

    Public Function decryptString(ByVal str As String) As String
        Dim tempR As Integer = str.Substring(0, 1)
        Dim tempS As String = ""

        For i As Integer = 1 To str.Length - 1 - tempR
            tempS &= Chr(Asc(str(i)) - tempR)
        Next
        Return tempS
    End Function
End Module
