Public Class frmAdministrator

    Private Sub cmdSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSave.Click
        Dim password As String = encryptString(txtPassword.Text)

        Dim username As String = encryptString(txtUsername.Text)

        writeINI(sfile, "gameSettings", "legacy1", username)
        writeINI(sfile, "gameSettings", "legacy2", password)

        Me.Close()
    End Sub

End Class