Public Class frmSetup


    Private Sub frmSetup_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            frmServer.Show()
            Timer1.Enabled = True
            Me.Hide()
        Catch ex As Exception
            appEventLog_Write("error takeIP:", ex)
        End Try
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Try
            frmServer.Show()
        Catch ex As Exception
            appEventLog_Write("error takeIP:", ex)
        End Try
    End Sub
End Class