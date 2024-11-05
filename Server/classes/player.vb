
Public Class player
    Public inumber As Integer            'ID number
    Public socketNumber As String        'winsock ID number    
    Public ipAddress As String           'IP address of player's machine 
    Public myIPAddress As String         'IP address of player's machine 

    Public programCount As Integer = 0
    Public programList(1000) As System.Diagnostics.Process

    Public explorerProcess As System.Diagnostics.Process

    Public Sub player()

    End Sub

    Public Sub resetClient()
        Try
            'kill client
            With frmServer
                .wsk_Col.Send("01", socketNumber, "")
            End With
        Catch ex As Exception
            appEventLog_Write("error resetClient:", ex)
        End Try
    End Sub

    Public Sub requsetIP(ByVal count As Integer)
        Try
            'request the client send it's IP address
            Dim outstr As String = ""
            outstr = count & ";"
            outstr &= SystemInformation.ComputerName & ";"
            outstr &= myIPAddress & ";"

            With frmServer
                .wsk_Col.Send("05", socketNumber, outstr)
            End With
        Catch ex As Exception
            appEventLog_Write("error requsetIP:", ex)
        End Try
    End Sub

    Public Sub startProcess(ByVal sinstr As String)
        Try
            Dim msgtokens() As String = sinstr.Split(";")
            Dim nextToken As Integer = 0

            Dim runAsAdmin As Boolean

            programCount += 1
            programList(programCount) = New System.Diagnostics.Process

            Dim pp As New ProcessStartInfo

            pp.FileName = msgtokens(nextToken)
            nextToken += 1

            pp.Arguments = msgtokens(nextToken)
            nextToken += 1

            pp.WindowStyle = ProcessWindowStyle.Normal

            'set the working directory
            Dim tempS As String = ""
            Dim msgtokens2() As String = pp.FileName.Split("\")

            For i As Integer = 1 To msgtokens2.Length - 1
                tempS &= msgtokens2(i - 1) & "\"
            Next
            pp.WorkingDirectory = tempS

            runAsAdmin = msgtokens(nextToken)
            nextToken += 1

            If runAsAdmin Then
                pp.UserName = msgtokens(nextToken)
                nextToken += 1

                Dim tempPassword As String = msgtokens(nextToken)
                nextToken += 1

                Dim ss As Security.SecureString = New Security.SecureString()

                For i As Integer = 1 To tempPassword.Length
                    ss.AppendChar(tempPassword(i - 1))
                Next

                pp.Password = ss
                pp.UseShellExecute = False
            End If

            programList(programCount) = System.Diagnostics.Process.Start(pp)

            'System.Diagnostics.Process.Start(sinstr)
        Catch ex As Exception
            appEventLog_Write("error :", ex)
            frmServer.wsk_Col.Send("04", socketNumber, ex.Message)
        End Try
    End Sub

    Public Sub killProcesses(ByVal sinstr As String)
        Try
            Dim msgtokens() As String = sinstr.Split(";")
            Dim nextToken As Integer = 0

            Dim killSpecificProcess As Boolean = msgtokens(nextToken)
            nextToken += 1

            Dim processToKill As String = msgtokens(nextToken)
            nextToken += 1

            If killSpecificProcess Then
                taskKillAction(processToKill)
            Else
                For i As Integer = 1 To programCount
                    If Not programList(i).HasExited Then
                        programList(i).Kill()
                    End If
                Next

                programCount = 0
            End If


        Catch ex As Exception
            appEventLog_Write("error requsetIP:", ex)
        End Try
    End Sub
End Class
