Imports System.IO

Public Class frmMain
    'Public WithEvents mobjSocketClient As TCPConnection
    Delegate Sub SetTextCallback(ByVal [text] As String)
    Delegate Sub SetTextCallback2()

    Public buttonArray(100) As Button
    Public connectionCounter As Integer
    Public connectionsRequired As Integer
    Public lastRunCount As Integer = 0
    Public lastRunList As String = ""
    Public connectionOrderList As String = ""
    Public ignoreRecentClick As Boolean = False

    Private Sub frmChat_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Try

            sfile = System.Windows.Forms.Application.StartupPath & "\client.ini"

            'take IP from command line
            Dim commandLine As String = Command()

            'connect
            myPortNumber = getINI(sfile, "Settings", "port")
            runAsAdmin = getINI(sfile, "Settings", "runAsAdmin")
            'connect()

            For i As Integer = 1 To 100
                buttonArray(i) = New Button
            Next

            'lab 113
            buttonArray(1) = Button1
            buttonArray(2) = Button2
            buttonArray(3) = Button3
            buttonArray(4) = Button4
            buttonArray(5) = Button5
            buttonArray(6) = Button6
            buttonArray(7) = Button7
            buttonArray(8) = Button8
            buttonArray(9) = Button9
            buttonArray(10) = Button10
            buttonArray(11) = Button11
            buttonArray(12) = Button12
            buttonArray(13) = Button13
            buttonArray(14) = Button14

            'lab 130
            buttonArray(15) = Button15
            buttonArray(16) = Button16
            buttonArray(17) = Button17
            buttonArray(18) = Button18
            buttonArray(19) = Button19
            buttonArray(20) = Button20
            buttonArray(21) = Button21
            buttonArray(22) = Button22
            buttonArray(23) = Button23
            buttonArray(24) = Button24
            buttonArray(25) = Button25
            buttonArray(26) = Button26

            '115
            buttonArray(27) = Button27
            buttonArray(28) = Button28
            buttonArray(29) = Button29
            buttonArray(30) = Button30
            buttonArray(31) = Button31
            buttonArray(32) = Button32
            buttonArray(33) = Button33
            buttonArray(34) = Button34
            buttonArray(35) = Button35
            buttonArray(36) = Button36
            buttonArray(37) = Button37
            buttonArray(38) = Button38
            buttonArray(39) = Button39
            buttonArray(40) = Button40
            buttonArray(41) = Button41
            buttonArray(42) = Button42
            buttonArray(43) = Button43
            buttonArray(44) = Button44
            buttonArray(45) = Button45
            buttonArray(46) = Button46
            buttonArray(47) = Button47
            buttonArray(48) = Button48
            buttonArray(49) = Button49
            buttonArray(50) = Button50

            'monitor room
            buttonArray(51) = Button51
            buttonArray(52) = Button52
            buttonArray(53) = Button53
            buttonArray(54) = Button54
            buttonArray(55) = Button55
            buttonArray(56) = Button56

            Dim tempN As Integer = 1
            For i As Integer = 1 To 14
                buttonArray(i).Text = CStr(tempN)
                tempN += 1
            Next

            tempN = 1
            For i As Integer = 15 To 26
                buttonArray(i).Text = CStr(tempN)
                tempN += 1
            Next

            tempN = 1
            For i As Integer = 27 To 50
                buttonArray(i).Text = CStr(tempN)
                tempN += 1
            Next

            'For i As Integer = 63 To 64
            '    buttonArray(i).BackColor = Color.Empty
            '    buttonArray(i).UseVisualStyleBackColor = True
            'Next

            'setup connections
            For i As Integer = 1 To 50
                connections(i) = New clsConnection(getINI(sfile, "ipsFront", CStr(i)))
                connections(i).myNumber = i
                buttonArray(i).Tag = getINI(sfile, "ipsFront", CStr(i))
                connections(i).room = buttonArray(i).Tag
            Next

            For i As Integer = 51 To 58
                buttonArray(i).Tag = getINI(sfile, "ipsFront", CStr(i))
            Next

            For i As Integer = 1 To 58
                buttonArray(i).BackColor = Color.Empty
                buttonArray(i).UseVisualStyleBackColor = True
            Next

            txtIPAddress.Text = SystemInformation.ComputerName

            loadRecent()

            If Not runAsAdmin Then cmdKillReceiver.Visible = False
            If Not runAsAdmin Then cmdArchiveExperimentsFolder.Visible = False

            selectionCounter = 0

            For i As Integer = 1 To 100
                selectionOrder(i) = 1000
            Next

        Catch ex As Exception
            appEventLog_Write("errorfrmChat_Load :", ex)
        End Try

    End Sub

    Public Sub loadRecent()
        Try
            comboRecent.Items.Clear()

            For i As Integer = 1 To 10
                comboRecent.Items.Add(getINI(sfile, "recent" & i, "location"))
            Next
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Public Sub storeRecent()
        Try
            'shift old down
            For i As Integer = 10 To 2 Step -1
                writeINI(sfile, "recent" & i, "ipAddress", getINI(sfile, "recent" & i - 1, "ipAddress"))
                writeINI(sfile, "recent" & i, "autoConnect", getINI(sfile, "recent" & i - 1, "autoConnect"))
                writeINI(sfile, "recent" & i, "zTree", getINI(sfile, "recent" & i - 1, "zTree"))
                writeINI(sfile, "recent" & i, "clientCount", getINI(sfile, "recent" & i - 1, "clientCount"))
                writeINI(sfile, "recent" & i, "clientList", getINI(sfile, "recent" & i - 1, "clientList"))
                writeINI(sfile, "recent" & i, "location", getINI(sfile, "recent" & i - 1, "location"))
                writeINI(sfile, "recent" & i, "arguments", getINI(sfile, "recent" & i - 1, "arguments"))
                writeINI(sfile, "recent" & i, "launchDelay", getINI(sfile, "recent" & i - 1, "launchDelay"))
                writeINI(sfile, "recent" & i, "launchInOrder", getINI(sfile, "recent" & i - 1, "launchInOrder"))
                writeINI(sfile, "recent" & i, "selectionCounter", getINI(sfile, "recent" & i - 1, "selectionCounter"))
                writeINI(sfile, "recent" & i, "selectionOrder", getINI(sfile, "recent" & i - 1, "selectionOrder"))
            Next

            'store new
            writeINI(sfile, "recent1", "ipAddress", txtIPAddress.Text)
            writeINI(sfile, "recent1", "autoConnect", cbAutoConnect.Checked)
            writeINI(sfile, "recent1", "zTree", cbZtree.Checked)
            writeINI(sfile, "recent1", "clientCount", lastRunCount)
            writeINI(sfile, "recent1", "clientList", lastRunList)
            writeINI(sfile, "recent1", "location", txtLocation.Text)
            writeINI(sfile, "recent1", "arguments", txtArguments.Text)
            writeINI(sfile, "recent1", "launchDelay", nudLaunchDelay.Value)
            writeINI(sfile, "recent1", "launchInOrder", cbLaunchInOrderSelected.Checked)

            writeINI(sfile, "recent1", "selectionCounter", selectionCounter)

            'Dim tempS As String = ""
            'For i As Integer = 1 To 64
            '    tempS &= selectionOrder(i) & ";"
            'Next

            writeINI(sfile, "recent1", "selectionOrder", connectionOrderList)

            'reload
            ignoreRecentClick = True
            loadRecent()

            comboRecent.Text = ""
            ignoreRecentClick = False
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub llESI_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llESI.LinkClicked
        Try
            System.Diagnostics.Process.Start("http://www.chapman.edu/esi/")
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _
          Button15.Click, Button16.Click, Button17.Click, Button18.Click,
          Button19.Click, Button20.Click, Button21.Click, Button22.Click, Button23.Click, Button24.Click, Button25.Click, Button26.Click, Button27.Click, Button28.Click, Button29.Click, Button30.Click,
          Button31.Click, Button32.Click, Button33.Click, Button34.Click, Button35.Click, Button36.Click, Button37.Click, Button38.Click, Button39.Click,
          Button40.Click, Button41.Click, Button42.Click, Button43.Click, Button44.Click, Button45.Click, Button46.Click, Button47.Click, Button48.Click,
          Button49.Click, Button50.Click,
          Button9.Click, Button8.Click, Button7.Click, Button6.Click, Button5.Click, Button4.Click, Button3.Click, Button2.Click, Button1.Click, Button14.Click, Button13.Click, Button12.Click, Button11.Click, Button10.Click,
          Button51.Click, Button52.Click, Button53.Click, Button54.Click, Button55.Click, Button56.Click


        If sender.backcolor <> Color.Yellow Then
            sender.backcolor = Color.Yellow
            selectionCounter += 1

            If cbLaunchInOrderSelected.Checked Then
                For i As Integer = 1 To 50
                    If buttonArray(i) Is DirectCast(sender, Button) Then
                        selectionOrder(i) = selectionCounter
                    End If
                Next
            End If
        Else
            If cbLaunchInOrderSelected.Checked Then
                For i As Integer = 1 To 50
                    If buttonArray(i) Is DirectCast(sender, Button) Then
                        selectionOrder(i) = 1000
                    End If
                Next
            End If

            sender.backcolor = Color.Empty
            sender.UseVisualStyleBackColor = True
        End If



    End Sub

    Public Sub buttonSelected(ByVal index As Integer)
        Try

        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdDeSelectAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdDeSelectAll.Click
        Try
            For i As Integer = 1 To 56
                buttonArray(i).BackColor = Color.Empty
                buttonArray(i).UseVisualStyleBackColor = True
            Next

            For i As Integer = 1 To 100
                selectionOrder(i) = 1000
            Next

            selectionCounter = 0
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Public Sub resetLaunchOrder()
        For i As Integer = 1 To 100
            selectionOrder(i) = 1000
        Next

        selectionCounter = 0

        cbLaunchInOrderSelected.Checked = False
    End Sub

    Private Sub cmdSelectAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelectAll.Click
        Try
            For i As Integer = 1 To 50
                buttonArray(i).BackColor = Color.Yellow
            Next

            For i As Integer = 51 To 56
                buttonArray(i).BackColor = Color.Yellow
            Next

        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdSelectBottom113_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelectBottom113.Click
        Try
            resetLaunchOrder()

            For i As Integer = 1 To 7
                buttonArray(i).BackColor = Color.Yellow
            Next
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdSelectTop113_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelectTop113.Click
        Try
            resetLaunchOrder()

            For i As Integer = 8 To 14
                buttonArray(i).BackColor = Color.Yellow
            Next
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdSelectBottom130_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelectBottom130.Click
        Try
            resetLaunchOrder()

            For i As Integer = 15 To 20
                buttonArray(i).BackColor = Color.Yellow
            Next
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdSelectTop130_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelectTop130.Click
        Try
            resetLaunchOrder()

            For i As Integer = 21 To 26
                buttonArray(i).BackColor = Color.Yellow
            Next
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdSelectBottom115_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelectBottom115.Click
        Try
            resetLaunchOrder()

            For i As Integer = 27 To 32
                buttonArray(i).BackColor = Color.Yellow
            Next
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdSelectBottomMiddle115_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelectBottomMiddle115.Click
        Try
            resetLaunchOrder()

            For i As Integer = 33 To 38
                buttonArray(i).BackColor = Color.Yellow
            Next
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdSelectBottomTop115_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelectBottomTop115.Click
        Try
            resetLaunchOrder()

            For i As Integer = 39 To 44
                buttonArray(i).BackColor = Color.Yellow
            Next
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdSelectTop115_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelectTop115.Click
        Try
            resetLaunchOrder()

            For i As Integer = 45 To 50
                buttonArray(i).BackColor = Color.Yellow
            Next
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdLoad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdLoad.Click
        Try
            'dispaly open file dialog to select file
            OpenFileDialog1.FileName = ""
            OpenFileDialog1.Filter = "All Files (*.*)|*.*"
            'OpenFileDialog1.InitialDirectory = System.Windows.Forms.Application.StartupPath

            OpenFileDialog1.ShowDialog()

            'if filename is not empty then continue with load
            If OpenFileDialog1.FileName = "" Then
                Exit Sub
            End If

            txtLocation.Text = OpenFileDialog1.FileName
            txtArguments.Text = ""
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdStart.Click
        Try
            If Not checkMonitorComputers() Then Exit Sub

            cmdShutdown.Enabled = True
            cmdRestart.Enabled = True
            cmdLogOff.Enabled = True
            cmdStartShell.Enabled = True
            cmdKillShell.Enabled = True
            cmdFileExplorer.Enabled = True

            If txtLocation.Text = "" Then
                MessageBox.Show("Please enter a location.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End If

            cbKillProcessByName.Checked = False

            cmdStart.Enabled = False

            connectionCounter = 0
            connectionsRequired = 0
            launchIncrement = nudAutoIncStart.Value

            For i As Integer = 1 To 50
                If buttonArray(i).BackColor = Color.Yellow Or buttonArray(i).BackColor = Color.LawnGreen Then
                    If connections(i).wskClient.State = WinsockStates.Connected Then
                        connectionCounter += 1
                        buttonArray(i).BackColor = Color.LawnGreen
                        Application.DoEvents()
                    End If

                    connectionsRequired += 1
                End If
            Next

            'store run list
            lastRunList = ""
            connectionOrderList = ""
            lastRunCount = 0

            For i As Integer = 1 To 50
                If buttonArray(i).BackColor = Color.Yellow Or buttonArray(i).BackColor = Color.LawnGreen Then
                    lastRunList &= i & ";"
                    lastRunCount += 1
                End If

                connectionOrderList &= selectionOrder(i) & ";"
            Next

            storeRecent()

            'run software
            If connectionCounter = connectionsRequired Then
                launchSoftware()
            Else
                For i As Integer = 1 To 50
                    If buttonArray(i).BackColor = Color.Yellow Or buttonArray(i).BackColor = Color.LawnGreen Then
                        If connections(i).wskClient.State <> WinsockStates.Connected Then
                            connections(i).wskClient.Connect()
                        End If
                    End If
                Next
            End If

            Timer1.Enabled = True
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Public Sub induvidualLaunch(ByVal index As Integer)
        Try
            If cmdKillReceiver.Enabled = False Then
                connections(index).killReceiver()
            ElseIf cmdShutdown.Enabled = False Then
                connections(index).shutdown()
            ElseIf cmdRestart.Enabled = False Then
                connections(index).restart()
            ElseIf cmdLogOff.Enabled = False Then
                connections(index).logOff()
            ElseIf cmdStartShell.Enabled = False Then
                connections(index).startShell()
            ElseIf cmdKillShell.Enabled = False Then
                connections(index).killShell()
            ElseIf cmdFileExplorer.Enabled = False Then
                connections(index).startExplorer()
            ElseIf cbKillProcessByName.Checked Then
                connections(index).sendKillSoftware()
            Else
                connections(index).sendLaunchSoftware()
                System.Threading.Thread.Sleep(nudLaunchDelay.Value)
            End If
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Public Sub launchSoftware()
        Try

            If cbLaunchInOrderSelected.Checked Then
                Dim go As Boolean = True

                Do While go
                    Dim tempIndex As Integer = -1
                    Dim tempAmount As Integer = 1000

                    For i As Integer = 1 To 50
                        If selectionOrder(i) < tempAmount And buttonArray(i).BackColor = Color.LawnGreen Then
                            tempIndex = i
                            tempAmount = selectionOrder(i)
                        End If

                    Next

                    If tempIndex = -1 Then
                        go = False
                    Else
                        induvidualLaunch(tempIndex)
                        selectionOrder(tempIndex) = 1000

                        'with each launch increment to the next index
                        If cbAutoIncrement.Checked Then
                            launchIncrement += 1
                        End If
                    End If

                    
                Loop

                cbLaunchInOrderSelected.Checked = False
            Else
                For i As Integer = 1 To 50
                    If buttonArray(i).BackColor = Color.LawnGreen Then
                        induvidualLaunch(i)

                        'with each launch increment to the next index
                        If cbAutoIncrement.Checked Then
                            launchIncrement += 1
                        End If
                    End If
                Next
            End If


            cmdStart.Enabled = True
            cmdLoad.Enabled = True
            cmdKillReceiver.Enabled = True
            cmdShutdown.Enabled = True
            cmdRestart.Enabled = True
            cmdLogOff.Enabled = True
            cmdStartShell.Enabled = True
            cmdFileExplorer.Enabled = True
            cmdKillShell.Enabled = True

        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdStop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdStop.Click
        Try
            If Not checkMonitorComputers() Then Exit Sub

            If MessageBox.Show("Kill software on selected computers?", "Stop", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.No Then Exit Sub

            If cbKillProcessByName.Checked Then
                connectToClients()
            Else
                For i As Integer = 1 To 50
                    If buttonArray(i).BackColor = Color.Yellow And connections(i).wskClient.State = WinsockStates.Connected Then

                        connections(i).sendKillSoftware()

                        buttonArray(i).BackColor = Color.Empty
                        buttonArray(i).UseVisualStyleBackColor = True
                    End If
                Next
            End If


        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdIE_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdIE.Click
        Try
            txtLocation.Text = getINI(sfile, "Settings", "iELocation")
            txtArguments.Text = getINI(sfile, "Settings", "iEArguments")
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdFireFox_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdFireFox.Click
        Try
            txtLocation.Text = getINI(sfile, "Settings", "firefoxLocation")
            txtArguments.Text = getINI(sfile, "Settings", "firefoxArguments")
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdCopy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCopy.Click
        Try
            FolderBrowserDialog1.SelectedPath = getINI(sfile, "Settings", "lastCopyFolderLoc")
            If FolderBrowserDialog1.ShowDialog() = Windows.Forms.DialogResult.Cancel Then Exit Sub
            Dim outstr As String = ""
            Dim msgtokens() As String = FolderBrowserDialog1.SelectedPath.Split("\")
            Dim folderName As String = msgtokens(msgtokens.Length - 1)

            frmProgress.Show()

            Dim tempN As Integer = 0
            For i As Integer = 1 To 56
                If buttonArray(i).BackColor = Color.Yellow Then
                    tempN += 1
                End If
            Next

            frmProgress.ProgressBar1.Maximum = tempN
            frmProgress.ProgressBar1.Minimum = 0
            frmProgress.ProgressBar1.Value = 0

            Application.DoEvents()

            For i As Integer = 1 To 56
                If buttonArray(i).BackColor = Color.Yellow Then

                    outstr = "xcopy """ & FolderBrowserDialog1.SelectedPath & """ ""\\" & buttonArray(i).Tag & "\experiments\" & folderName & """ /s /r /i /y /f /u"

                    'Shell(outstr, AppWinStyle.NormalFocus, True)
                    If Not CopyDirectory(FolderBrowserDialog1.SelectedPath,
                          "\\" & buttonArray(i).Tag & "\experiments\" & folderName & "", True) Then

                        MessageBox.Show("Cannot copy to computer: " & buttonArray(i).Tag, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Exit For
                    End If

                    buttonArray(i).BackColor = Color.Empty
                    buttonArray(i).UseVisualStyleBackColor = True

                    frmProgress.ProgressBar1.Value += 1
                    Application.DoEvents()
                End If
            Next

            frmProgress.Close()

            writeINI(sfile, "Settings", "lastCopyFolderLoc", FolderBrowserDialog1.SelectedPath)
        Catch ex As Exception
            MessageBox.Show("Copy Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Public Function CopyDirectory(ByVal SourcePath As String, ByVal DestPath As String, Optional ByVal Overwrite As Boolean = False) As Boolean
        Dim SourceDir As System.IO.DirectoryInfo = New System.IO.DirectoryInfo(SourcePath)
        Dim DestDir As System.IO.DirectoryInfo = New System.IO.DirectoryInfo(DestPath)

        Try
            ' the source directory must exist, otherwise throw an exception
            If SourceDir.Exists Then
                ' if destination SubDir's parent SubDir does not exist throw an exception
                If Not DestDir.Parent.Exists Then
                    Throw New DirectoryNotFoundException _
                        ("Destination directory does not exist: " + DestDir.Parent.FullName)
                End If

                If Not DestDir.Exists Then
                    DestDir.Create()
                End If

                ' copy all the files of the current directory
                Dim ChildFile As FileInfo
                For Each ChildFile In SourceDir.GetFiles()
                    If Overwrite Then
                        ChildFile.CopyTo(Path.Combine(DestDir.FullName, ChildFile.Name), True)
                    Else
                        ' if Overwrite = false, copy the file only if it does not exist
                        ' this is done to avoid an IOException if a file already exists
                        ' this way the other files can be copied anyway...
                        If Not File.Exists(Path.Combine(DestDir.FullName, ChildFile.Name)) Then
                            ChildFile.CopyTo(Path.Combine(DestDir.FullName, ChildFile.Name), False)
                        End If
                    End If
                Next

                ' copy all the sub-directories by recursively calling this same routine
                Dim SubDir As DirectoryInfo
                For Each SubDir In SourceDir.GetDirectories()
                    CopyDirectory(SubDir.FullName, Path.Combine(DestDir.FullName, _
                        SubDir.Name), Overwrite)
                Next

                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
            appEventLog_Write("error :", ex)
        End Try
    End Function

    Private Sub cmdArchiveExperimentsFolder_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdArchiveExperimentsFolder.Click
        Try
            Dim newFolderName As String = InputBox("Enter Archive Fold Name", "Archive")

            If newFolderName = "" Then Exit Sub

            frmProgress.Show()

            Dim tempN As Integer = 0
            For i As Integer = 1 To 56
                If buttonArray(i).BackColor = Color.Yellow Then
                    tempN += 1
                End If
            Next

            frmProgress.ProgressBar1.Maximum = tempN
            frmProgress.ProgressBar1.Minimum = 0
            frmProgress.ProgressBar1.Value = 0

            Application.DoEvents()

            For i As Integer = 1 To 56
                If buttonArray(i).BackColor = Color.Yellow Then

                    Dim SourceDir As System.IO.DirectoryInfo = New System.IO.DirectoryInfo("\\" & buttonArray(i).Tag & "\Experiments")
                    Dim DestDir As System.IO.DirectoryInfo = New System.IO.DirectoryInfo("\\" & buttonArray(i).Tag & "\Experiments\_Archive\" & newFolderName)

                    If Not DestDir.Exists Then
                        DestDir.Create()
                    End If

                    Dim ChildFile As FileInfo
                    For Each ChildFile In SourceDir.GetFiles()
                        ChildFile.MoveTo(Path.Combine(DestDir.FullName, ChildFile.Name))
                    Next

                    ' copy all the sub-directories by recursively calling this same routine
                    Dim SubDir As DirectoryInfo

                    Dim go As Boolean = True
                    Dim archiveAll As Boolean = CBool(getINI(sfile, "Settings", "archiveAll"))

                    For Each SubDir In SourceDir.GetDirectories()
                        If SubDir.FullName <> "\\" & buttonArray(i).Tag & "\Experiments\GEM" And
                        SubDir.FullName <> "\\" & buttonArray(i).Tag & "\Experiments\NewLauncherChapman" And
                        SubDir.FullName <> "\\" & buttonArray(i).Tag & "\Experiments\NewLauncherChapman2" And
                        SubDir.FullName <> "\\" & buttonArray(i).Tag & "\Experiments\_Archive" And
                        SubDir.FullName <> "\\" & buttonArray(i).Tag & "\Experiments\firefox35" Then

                            If IsNumeric(newFolderName) And archiveAll = False Then
                                'If SubDir.CreationTimeUtc.Year <= CInt(newFolderName) Then
                                go = True

                                For Each f In SubDir.GetFiles()
                                    If f.CreationTimeUtc.Year > CInt(newFolderName) Then
                                        go = False
                                        Exit For
                                    End If

                                    For Each subDir2 In SubDir.GetDirectories()

                                        For Each f2 In subDir2.GetFiles()
                                            If f2.CreationTimeUtc.Year > CInt(newFolderName) Then
                                                go = False
                                                Exit For
                                            End If
                                        Next

                                        If Not go Then Exit For
                                    Next

                                    If Not go Then Exit For
                                Next
                                ' Else
                                'go = False
                                ' End If
                            End If

                            If go Then
                                moveDirectory(SubDir.FullName, Path.Combine(DestDir.FullName, SubDir.Name), True)
                                SubDir.Delete(True)
                            End If
                        End If

                    Next


                    buttonArray(i).BackColor = Color.Empty
                    buttonArray(i).UseVisualStyleBackColor = True

                    frmProgress.ProgressBar1.Value += 1
                    Application.DoEvents()
                End If

                frmProgress.Close()
            Next
        Catch ex As Exception
            appEventLog_Write("error: Can't copy ", ex)
        End Try
    End Sub

    Public Function moveDirectory(ByVal SourcePath As String, ByVal DestPath As String, Optional ByVal Overwrite As Boolean = False) As Boolean
        Dim SourceDir As System.IO.DirectoryInfo = New System.IO.DirectoryInfo(SourcePath)
        Dim DestDir As System.IO.DirectoryInfo = New System.IO.DirectoryInfo(DestPath)

        Try
            ' the source directory must exist, otherwise throw an exception
            If SourceDir.Exists Then
                ' if destination SubDir's parent SubDir does not exist throw an exception
                If Not DestDir.Parent.Exists Then
                    Throw New DirectoryNotFoundException _
                        ("Destination directory does not exist: " + DestDir.Parent.FullName)
                End If

                If Not DestDir.Exists Then
                    DestDir.Create()
                End If

                ' copy all the files of the current directory
                Dim ChildFile As FileInfo
                For Each ChildFile In SourceDir.GetFiles()
                    If Overwrite Then
                        ChildFile.MoveTo(Path.Combine(DestDir.FullName, ChildFile.Name))
                    Else
                        ' if Overwrite = false, copy the file only if it does not exist
                        ' this is done to avoid an IOException if a file already exists
                        ' this way the other files can be copied anyway...
                        If Not File.Exists(Path.Combine(DestDir.FullName, ChildFile.Name)) Then
                            ChildFile.MoveTo(Path.Combine(DestDir.FullName, ChildFile.Name))
                        End If
                    End If
                Next

                ' copy all the sub-directories by recursively calling this same routine
                Dim SubDir As DirectoryInfo
                For Each SubDir In SourceDir.GetDirectories()
                    moveDirectory(SubDir.FullName, Path.Combine(DestDir.FullName, _
                        SubDir.Name), Overwrite)
                Next

                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
            appEventLog_Write("error :", ex)
        End Try
    End Function


    Private Sub comboRecent_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles comboRecent.SelectedIndexChanged
        Try
            If ignoreRecentClick Then Exit Sub

            For i As Integer = 1 To 50
                buttonArray(i).BackColor = Color.Empty
                buttonArray(i).UseVisualStyleBackColor = True
            Next

            Dim tempSI As Integer = comboRecent.SelectedIndex + 1

            txtIPAddress.Text = getINI(sfile, "recent" & tempSI, "ipAddress")
            cbAutoConnect.Checked = getINI(sfile, "recent" & tempSI, "autoConnect")
            cbZtree.Checked = getINI(sfile, "recent" & tempSI, "zTree")

            txtLocation.Text = getINI(sfile, "recent" & tempSI, "location")
            txtArguments.Text = getINI(sfile, "recent" & tempSI, "arguments")

            nudLaunchDelay.Value = getINI(sfile, "recent" & tempSI, "launchDelay")
            cbLaunchInOrderSelected.Checked = getINI(sfile, "recent" & tempSI, "launchInOrder")

            Dim tempC As Integer = getINI(sfile, "recent" & tempSI, "clientCount")
            Dim msgtokens() As String = getINI(sfile, "recent" & tempSI, "clientList").Split(";")

            For i As Integer = 1 To tempC
                buttonArray(CInt(msgtokens(i - 1))).BackColor = Color.Yellow
            Next

            selectionCounter = getINI(sfile, "recent" & tempSI, "selectionCounter")

            msgtokens = getINI(sfile, "recent" & tempSI, "selectionOrder").Split(";")
            Dim nextToken As Integer = 0

            For i As Integer = 1 To 50
                selectionOrder(i) = msgtokens(nextToken)
                nextToken += 1
            Next

        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdKillReceiver_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdKillReceiver.Click
        Try
            If MessageBox.Show("Are you sure you want to close the Launch Receiver on the selected computers?", "Close", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) = Windows.Forms.DialogResult.No Then Exit Sub

            cmdKillReceiver.Enabled = False

            connectToClients()
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdShutdown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdShutdown.Click
        Try
            If Not checkMonitorComputers() Then Exit Sub

            If MessageBox.Show("Are you sure you want to Shutdown the selected computers?", "Close", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) = Windows.Forms.DialogResult.No Then Exit Sub

            cmdShutdown.Enabled = False

            connectToClients()
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdRestart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRestart.Click
        Try
            If Not checkMonitorComputers() Then Exit Sub

            If MessageBox.Show("Are you sure you want to Restart the selected computers?", "Close", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) = Windows.Forms.DialogResult.No Then Exit Sub

            cmdRestart.Enabled = False

            connectToClients()
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdLogOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdLogOff.Click
        Try
            If Not checkMonitorComputers() Then Exit Sub

            If MessageBox.Show("Are you sure you want to Log Off the selected computers?", "Close", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) = Windows.Forms.DialogResult.No Then Exit Sub

            cmdLogOff.Enabled = False

            connectToClients()
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Public Sub connectToClients()
        Try
            connectionCounter = 0
            connectionsRequired = 0

            For i As Integer = 1 To 50
                If buttonArray(i).BackColor = Color.Yellow Or buttonArray(i).BackColor = Color.LawnGreen Then
                    If connections(i).wskClient.State = WinsockStates.Connected Then
                        connectionCounter += 1
                        buttonArray(i).BackColor = Color.LawnGreen
                        Application.DoEvents()
                    End If

                    connectionsRequired += 1
                End If
            Next

            If connectionCounter = connectionsRequired Then
                launchSoftware()
            Else
                For i As Integer = 1 To 50
                    If buttonArray(i).BackColor = Color.Yellow Or buttonArray(i).BackColor = Color.LawnGreen Then
                        If connections(i).wskClient.State <> WinsockStates.Connected Then
                            connections(i).wskClient.Connect()
                        End If
                    End If
                Next
            End If

        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Public Function checkMonitorComputers() As Boolean
        Try
            For i As Integer = 51 To 56
                If buttonArray(i).BackColor = Color.Yellow Then
                    MessageBox.Show("This function cannot be used on Monitor Room computers.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return False
                End If
            Next

            Return True
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Function

    Private Sub cmdStartShell_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdStartShell.Click
        Try
            If Not checkMonitorComputers() Then Exit Sub

            If MessageBox.Show("Are you sure you want start the Windows Shell on the selected computers?", "Close", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) = Windows.Forms.DialogResult.No Then Exit Sub

            cmdStartShell.Enabled = False

            connectToClients()
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdKillShell_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdKillShell.Click
        Try
            If Not checkMonitorComputers() Then Exit Sub

            cmdKillShell.Enabled = False

            connectToClients()
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub


    Private Sub cmdStartExplorer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdFileExplorer.Click
        Try
            If Not checkMonitorComputers() Then Exit Sub

            FolderBrowserDialog1.SelectedPath = "c:\experiments"
            If FolderBrowserDialog1.ShowDialog() = Windows.Forms.DialogResult.Cancel Then Exit Sub

            cmdFileExplorer.Enabled = False

            connectToClients()
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Try
            cmdStart.Enabled = True
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdPowerPoint_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPowerPoint.Click
        Try
            'dispaly open file dialog to select file
            OpenFileDialog1.FileName = ""
            OpenFileDialog1.Filter = "Presentations and Shows (*.pptx;*.ppt;*.pptm;*.ppsz*.pps;*ppsm)|*.pptx;*.ppt;*.pptm;*.ppsz*.pps;*ppsm"
            'OpenFileDialog1.InitialDirectory = System.Windows.Forms.Application.StartupPath

            OpenFileDialog1.ShowDialog()

            'if filename is not empty then continue with load
            If OpenFileDialog1.FileName = "" Then
                Exit Sub
            End If

            txtLocation.Text = getINI(sfile, "Settings", "powerpointLocation")
            txtArguments.Text = getINI(sfile, "Settings", "powerpointArguments") & " """ & OpenFileDialog1.FileName & """"
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub RadioButton1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton1.CheckedChanged
        Try
            txtIPAddress.Text = SystemInformation.ComputerName
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub RadioButton2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton2.CheckedChanged
        Try
            txtIPAddress.Text = connections(1).wskClient.LocalIP
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cbZtree_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbZtree.CheckedChanged
        Try
            cbAutoConnect.Checked = True
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub


    Private Sub cbLaunchInOrderSelected_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbLaunchInOrderSelected.CheckedChanged
        Try
            selectionCounter = 0
            cmdDeSelectAll.PerformClick()
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub

    Private Sub cmdChrome_Click(sender As Object, e As EventArgs) Handles cmdChrome.Click
        Try
            txtLocation.Text = getINI(sfile, "Settings", "chromeLocation")
            txtArguments.Text = getINI(sfile, "Settings", "chromeArguments")
        Catch ex As Exception
            appEventLog_Write("error :", ex)
        End Try
    End Sub
End Class
