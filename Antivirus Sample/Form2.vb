Imports System.IO

Public Class Form2

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        On Error Resume Next
        For Each SI In ListView1.SelectedItems
            SI.Remove()
        Next
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        ListView1.Items.Add("." & TextBox1.Text)
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        On Error Resume Next
        If Directory.Exists(Application.StartupPath & "\Antivirus Config\") = False Then IO.Directory.CreateDirectory(Application.StartupPath & "\Antivirus Config\")
        Dim Str As String = ""
        For Each Element_Items In ListView1.Items
            Str &= Element_Items.Text & vbNewLine
        Next
        File.WriteAllText(Application.StartupPath & "\Antivirus Config\Excluded Extension.txt", Str)
        Form1.Load_Exclude_Files()
        TextBox1.Text = ""
    End Sub

    Private Sub TextBox1_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox1.GotFocus
        If TextBox1.Text = "Extension" Then
            TextBox1.Text = ""
        End If
    End Sub

    Private Sub TextBox1_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox1.LostFocus
        If TextBox1.Text = "" Then
            TextBox1.Text = "Extension"
        End If
    End Sub

    Private Sub Form2_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ListView1.Items.Clear()
        For Each S As String In Form1.Excludes_Files
            ListView1.Items.Add(S)
        Next

    End Sub
End Class