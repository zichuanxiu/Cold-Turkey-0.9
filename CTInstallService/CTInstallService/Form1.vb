Imports ServiceTools

Public Class Form

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try
            ServiceInstaller.InstallAndStart("CTService", "CTService", Application.StartupPath & "\CTService.exe")
        Catch ee As Exception
            MsgBox("Error installing Cold Turkey Service:" & vbNewLine & ee.ToString)
        End Try

        End

    End Sub

End Class
