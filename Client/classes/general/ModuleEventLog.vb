
Imports System
Imports System.IO
Imports System.Text

Module ModuleEventLog
    Public eventLog As FileStream
    Dim int As String

    Public Sub AppEventLog_Init()
        Try
            Dim filename As String

            filename = "ClientLog.txt"
            filename = System.Windows.Forms.Application.StartupPath & "\" & filename

            eventLog = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)

            int = "int"
        Catch ex As Exception

        End Try
    End Sub

    Public Sub appEventLog_Write(ByVal text As String)
        Try
            If int = Nothing Then AppEventLog_Init()

            Dim outstr As String

            outstr = text & " (" & Now & ")" & vbCrLf

            Dim info As Byte() = New UTF8Encoding(True).GetBytes(outstr)
            eventLog.Write(info, 0, info.Length)
        Catch ex As Exception

        End Try
    End Sub

    Public Sub appEventLog_Write(ByVal text As String, ByVal err As Exception)
        Try
            If int = Nothing Then AppEventLog_Init()

            Dim outstr As String

            outstr = text & " (" & Now & ")" & vbCrLf

            Dim info As Byte() = New UTF8Encoding(True).GetBytes(outstr)
            eventLog.Write(info, 0, info.Length)

            If err.Message <> "" Then
                outstr = err.Message & vbCrLf
                outstr &= err.StackTrace & vbCrLf

                info = New UTF8Encoding(True).GetBytes(outstr)
                eventLog.Write(info, 0, info.Length)
            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Sub AppEventLog_Close()
        Try
            If eventLog IsNot Nothing Then eventLog.Close()
        Catch ex As Exception

        End Try
    End Sub
End Module
