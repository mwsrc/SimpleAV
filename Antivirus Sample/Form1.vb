Imports System.Security.Cryptography
Imports System.IO
Imports System.Text
Imports System.Net

Public Class Form1

    Public Hashes As New List(Of String) 'List Of MD5 Hashes
    Public Infected_Files As New List(Of String) ' List Of Infected Files

    Dim Exclude_Files As Boolean = True
    Public Excludes_Files As New List(Of String)

    Dim Only_Scan_Above As Boolean = True
    Dim Only_Scan_Above_Number As Integer = 50

    Dim AutoMaticDelete As Boolean = False

    Dim TE As Long = 0

    Private Sub LoadVirusHashes_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles LoadVirusHashes.DoWork

        Try
            Panel2.Enabled = False
            ReportToolStripMenuItem.Enabled = False
            ProgressBar3.Visible = True
            If Directory.Exists(Application.StartupPath & "\Hashes\") Then

                Hashes.Add("44d88612fea8a8f36de82e1278abb02f")
                Hashes.Add("6ce6f415d8475545be5ba114f208b0ff")
                Hashes.Add("e4968ef99266df7c9a1f0637d2389dab")

                For Each F As String In Directory.EnumerateFiles(Application.StartupPath & "\Hashes", "*.hash", SearchOption.TopDirectoryOnly)

                    Dim SR As New StreamReader(F)

                    While SR.EndOfStream = False

                        If SR.ReadLine.Contains("#") = True Or SR.ReadLine = "" Then
                        Else

                            Hashes.Add(SR.ReadLine)

                        End If

                    End While

                    SR.Close()

                Next

            End If

            Label10.Text = Hashes.Count

        Catch ex As Exception

        End Try

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        If File_Scanner.IsBusy = False Then

            If RadioButton1.Checked Then

                File_Scanner.RunWorkerAsync("Full Scan")

            ElseIf RadioButton2.Checked Then

                File_Scanner.RunWorkerAsync("Quick Scan")

            ElseIf RadioButton3.Checked Then

                If FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then

                    File_Scanner.RunWorkerAsync(FolderBrowserDialog1.SelectedPath)

                End If

            End If

        Else
            MsgBox("Still Running")
        End If

    End Sub

    Sub Clean_Scanning()
        Label3.Text = "Scan: ~~"
        Label4.Text = "Name: ~~"
        Label5.Text = "Location: ~~'"
        Label6.Text = "Length: ~~"
        Label7.Text = "Extension: ~~"
        Label8.Text = "Time Elapsed: ~~"
        Label9.Text = "Files Detected: 0"
        Label11.Text = ""
        ProgressBar1.Value = 0
        ProgressBar2.Value = 0
        ProgressBar1.Maximum = 0
        ProgressBar2.Maximum = 0
    End Sub

    Private Sub File_Scanner_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles File_Scanner.DoWork

        On Error Resume Next
        Dim Path_Scan As String = e.Argument.ToString
        Dim Folders As New List(Of DirectoryInfo)
        Dim Files_Detected As New List(Of String)
        Dim Files As New List(Of String)
        Dim Ex_File As Boolean = Exclude_Files
        Dim Ex_Files As New List(Of String)
        Dim Md5 As String
        Dim SW As New Stopwatch
        Ex_Files.AddRange(Excludes_Files)
        Clean_Scanning()
        Button1.Enabled = False
        SW.Start()
        NotifyIcon1.Text = "Scanning: Analyzing..."
        Label11.Text = "Analyzing..."
        If Path_Scan = "Full Scan" Then

            Label3.Text = "Scan: Full Scan"
            For Each Driver As DriveInfo In My.Computer.FileSystem.Drives

                Folders.AddRange(GetFolders(Driver.Name))
                If File_Scanner.CancellationPending = True Then Exit For

            Next

        ElseIf Path_Scan = "Quick Scan" Then
            Label3.Text = "Scan: Quick Scan"
            For Each Directory_Info As String In SPECIALDIRECTORIESSS()

                If Directory.Exists(Directory_Info) Then

                    Folders.AddRange(GetFolders(Directory_Info))

                End If
                If File_Scanner.CancellationPending = True Then Exit For
            Next
        Else

            Label3.Text = "Scan: Custom Scan"
            Folders.AddRange(GetFolders(Path_Scan))

        End If
        Label11.Text = "Scanning"
        ProgressBar2.Value = 0
        ProgressBar2.Maximum = Folders.Count

        For Each Element_Directory As DirectoryInfo In Folders
            Files.Clear()
            Files.AddRange(Directory.EnumerateFiles(Element_Directory.FullName, "*.*", SearchOption.TopDirectoryOnly))
            ProgressBar1.Value = 0
            ProgressBar1.Maximum = Files.Count
            For Each Element_File As String In Files
                NotifyIcon1.Text = "Scanning: " & Math.Round(((ProgressBar2.Value / ProgressBar2.Maximum) * 100), 2) & "%"
                Dim F As FileInfo
                F = My.Computer.FileSystem.GetFileInfo(Element_File)
                If (Ex_File = True AndAlso Ex_Files.Contains(F.Extension) = False) OrElse Ex_File = False Then

                    '------- DESIGNING
                    Label4.Text = "Name: " & F.Name
                    Label5.Text = "Location: " & F.FullName
                    Label6.Text = "Length: " & GetBytes(F.Length)
                    Label7.Text = "Extension: " & GetExtension(F.Extension)
                    Label8.Text = "Time Elapsed: " & SW.Elapsed.Hours & ":" & SW.Elapsed.Minutes & ":" & SW.Elapsed.Seconds
                    Label9.Text = "Files Detected: " & Files_Detected.Count
                    '--------------
                    Md5 = GetMd5(Element_File)
                    If Hashes.Contains(Md5) Then

                        If Not Md5 = "" Then

                            If Infected_Files.Contains(Element_File) = False And Files_Detected.Contains(Element_File) = False Then

                                Files_Detected.Add(Element_File)

                            End If

                        End If

                    End If

                    ProgressBar1.Value += 1
                    If File_Scanner.CancellationPending = True Then Exit For
                End If
            Next

            ProgressBar2.Value += 1
            If File_Scanner.CancellationPending = True Then Exit For
        Next

        SW.Stop()
        For Each EF As String In Files_Detected
            If Infected_Files.Contains(EF) = False Then
                Infected_Files.Add(EF)
            End If
        Next
        Files_Detected.Clear()


    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Directory.Exists(Application.StartupPath & "\Antivirus Config\") = False Then IO.Directory.CreateDirectory(Application.StartupPath & "\Antivirus Config\")
        Dim Str As String = ""
        For Each S As String In Infected_Files
            Str &= S & vbNewLine
        Next
        My.Computer.FileSystem.WriteAllText(Application.StartupPath & "\Antivirus Config\Virus Infected List.txt", Str, False)
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
        LoadVirusHashes.RunWorkerAsync()
        Load_Exclude_Files()
    End Sub

    Public Sub Load_Virus_List()

        If File.Exists(Application.StartupPath & "\Antivirus Config\Virus Infected List.txt") Then

            Dim SR As New StreamReader(Application.StartupPath & "\Antivirus Config\Virus Infected List.txt")
            Dim Str() As String = Split(SR.ReadToEnd, vbNewLine)
            SR.Close()

            For Each S As String In Str
                If Infected_Files.Contains(S) = False Then
                    If File.Exists(S) Then
                        Infected_Files.Add(S)
                    End If
                End If
            Next

        End If

    End Sub

    Public Sub Load_Exclude_Files()
        If File.Exists(Application.StartupPath & "\Antivirus Config\Excluded Extension.txt") Then
            Excludes_Files.Clear()
            Dim SR As New StreamReader(Application.StartupPath & "\Antivirus Config\Excluded Extension.txt")
            While SR.EndOfStream = False
                Excludes_Files.Add(SR.ReadLine)
            End While
            SR.Close()
        Else
            Excludes_Files.Clear()
            Excludes_Files.Add(".txt")
            Excludes_Files.Add(".rtf")
        End If
    End Sub

    Sub FSW(ByVal Path As String)

        On Error Resume Next
        Dim F As FileInfo = My.Computer.FileSystem.GetFileInfo(Path)
        If (Exclude_Files = True AndAlso Excludes_Files.Contains(F.Extension) = False) OrElse Exclude_Files = False Then
            Dim Md5 As String = GetMd5(Path)
            If Hashes.Contains(Md5) Then
                If Not Md5 = "" Then
                    If Infected_Files.Contains(Path) = False Then
                        Infected_Files.Add(Path)
                    End If
                End If
            End If
        End If

    End Sub

    Private Sub FileSystemWatcher1_Changed(ByVal sender As System.Object, ByVal e As System.IO.FileSystemEventArgs) Handles FileSystemWatcher1.Changed
        FSW(e.FullPath)
    End Sub

    Private Sub FileSystemWatcher1_Created(ByVal sender As Object, ByVal e As System.IO.FileSystemEventArgs) Handles FileSystemWatcher1.Created
        FSW(e.FullPath)
    End Sub

    Private Sub FileSystemWatcher1_Renamed(ByVal sender As Object, ByVal e As System.IO.RenamedEventArgs) Handles FileSystemWatcher1.Renamed
        FSW(e.FullPath)
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        If File_Scanner.IsBusy = True Then

            Panel1.Visible = False
            Panel1.Dock = DockStyle.None
            Panel3.Dock = DockStyle.Fill
            Panel3.Visible = True

        Else

            Panel1.Visible = True
            Panel1.Dock = DockStyle.Fill
            Panel3.Dock = DockStyle.None
            Panel3.Visible = False

        End If

        If Me.WindowState = FormWindowState.Minimized Then
            NotifyIcon1.Visible = True
            Me.Visible = False
        Else
            NotifyIcon1.Visible = False
            Me.Visible = True
        End If
        If File_Scanner.IsBusy = False Then NotifyIcon1.Text = "Antivirus Sample"
        If LoadVirusHashes.IsBusy = False Then
            If Infected_Files.Count > 0 Then
                PictureBox1.Image = My.Resources.Warning
                Label2.Text = "Vulnerable"
                If File_Scanner.IsBusy = False Then NotifyIcon1.BalloonTipText = "Your computer is currently in danger."
                Label1.Text = "Your computer is currently in danger."
            ElseIf FileSystemWatcher1.EnableRaisingEvents = False Then
                PictureBox1.Image = My.Resources.Alert
                Label2.Text = "Not Protected"
                If File_Scanner.IsBusy = False Then NotifyIcon1.BalloonTipText = "Your computer is currently not protected."
                Label1.Text = "Your computer is currently not protected."
            Else
                PictureBox1.Image = My.Resources.Protection1
                Label2.Text = "Protected"
                If File_Scanner.IsBusy = False Then NotifyIcon1.BalloonTipText = "Your computer is currently protected."
                Label1.Text = "Your computer is currently protected."
            End If
        Else
            PictureBox1.Image = My.Resources.Protection1
            Label2.Text = "Protected"
            Label1.Text = "Your computer is currently protected."
            If File_Scanner.IsBusy = False Then NotifyIcon1.BalloonTipText = "Your computer is currently protected."
        End If

        If AutoMaticDelete = True And Automatic_Delete_File.IsBusy = False Then Automatic_Delete_File.RunWorkerAsync()
        If LoadVirusHashes.IsBusy = True Then FileSystemWatcher1.EnableRaisingEvents = False
        If FileSystemWatcher1.EnableRaisingEvents = True Then
            OnToolStripMenuItem1.Enabled = False
            OffToolStripMenuItem1.Enabled = True
        Else
            OnToolStripMenuItem1.Enabled = True
            OffToolStripMenuItem1.Enabled = False
        End If

        If Exclude_Files = False Then
            OnToolStripMenuItem.Enabled = True
            OffToolStripMenuItem.Enabled = False
        Else
            OnToolStripMenuItem.Enabled = False
            OffToolStripMenuItem.Enabled = True
        End If

        If AutoMaticDelete = True Then
            OnToolStripMenuItem2.Enabled = False
            OffToolStripMenuItem2.Enabled = True
        Else
            OnToolStripMenuItem2.Enabled = True
            OffToolStripMenuItem2.Enabled = False
        End If

        Label4.Width = (Me.Width - 46) / 2
        Label5.Width = (Me.Width - 46) / 2
        Label6.Width = (Me.Width - 46) / 2
        Label7.Width = (Me.Width - 46) / 2
        Label8.Width = (Me.Width - 46) / 2
        Label9.Width = (Me.Width - 46) / 2
        Label5.Left = 15 + Label5.Width
        Label7.Left = 15 + Label7.Width
        Label9.Left = 15 + Label9.Width
        ToolStripStatusLabel3.Text = "Infected Files: " & Infected_Files.Count

    End Sub

    Private Sub File_Scanner_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles File_Scanner.RunWorkerCompleted
        MsgBox("Scanning Complete")
        Button1.Enabled = True
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If File_Scanner.IsBusy = True Then
            File_Scanner.CancelAsync()
        End If
    End Sub

    Private Sub Automatic_Delete_File_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles Automatic_Delete_File.DoWork

        On Error Resume Next
        For i1 = 0 To 1 Step 0
            If AutoMaticDelete = True Then
                For i = Infected_Files.Count - 1 To 0 Step -1

                    If File.Exists(Infected_Files(i)) Then

                        Kill(Infected_Files(i))

                    End If

                    If File.Exists(Infected_Files(i)) = False Then

                        Infected_Files.RemoveAt(i)

                    End If

                Next
            Else
                Exit For
            End If
        Next

    End Sub

    Private Sub LoadVirusHashes_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles LoadVirusHashes.RunWorkerCompleted

        ProgressBar3.Visible = False
        ReportToolStripMenuItem.Enabled = True
        Panel2.Enabled = True
        Automatic_Delete_File.RunWorkerAsync()
        FileSystemWatcher1.EnableRaisingEvents = True
        Load_Virus_List()

    End Sub

    Private Sub OnToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OnToolStripMenuItem1.Click
        Try
            If LoadVirusHashes.IsBusy = False Then
                FileSystemWatcher1.EnableRaisingEvents = True
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub OffToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OffToolStripMenuItem1.Click
        Try
            FileSystemWatcher1.EnableRaisingEvents = False
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub OnToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OnToolStripMenuItem.Click
        Try
            Exclude_Files = True
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub OffToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OffToolStripMenuItem.Click
        Try
            Exclude_Files = False
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub OnToolStripMenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OnToolStripMenuItem2.Click
        Try
            AutoMaticDelete = True
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub OffToolStripMenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OffToolStripMenuItem2.Click
        Try
            AutoMaticDelete = False
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Dim F As String
        For Each T In ListView1.Items
            F = T.Text
            Kill(F)
            If File.Exists(F) = False Then
                If Infected_Files.Contains(F) Then
                    Infected_Files.RemoveAt(Infected_Files.IndexOf(F))
                End If
            End If
        Next
        ListView1.Items.Clear()
        For Each IFile As String In Infected_Files
            ListView1.Items.Add(IFile)
        Next
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click

        Dim F As String

        For Each T In ListView1.SelectedItems

            F = T.Text
            Kill(F)
            If File.Exists(F) = False Then
                If Infected_Files.Contains(F) Then
                    Infected_Files.RemoveAt(Infected_Files.IndexOf(F))
                End If
            End If

        Next

        ListView1.Items.Clear()
        For Each IFile As String In Infected_Files
            ListView1.Items.Add(IFile)
        Next
    End Sub

    Private Sub ReportToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ReportToolStripMenuItem.Click
        Panel4.Dock = DockStyle.Fill
        Panel4.Visible = True
        Panel1.Visible = False
        Panel3.Visible = False
        ListView1.Items.Clear()
        For Each IFile As String In Infected_Files
            ListView1.Items.Add(IFile)
        Next
    End Sub

    Private Sub HomeToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HomeToolStripMenuItem.Click

        If File_Scanner.IsBusy = True Then

            Panel3.Dock = DockStyle.Fill
            Panel3.Visible = True
            Panel1.Visible = False
            Panel4.Visible = False

        Else

            Panel1.Dock = DockStyle.Fill
            Panel1.Visible = True
            Panel3.Visible = False
            Panel4.Visible = False

        End If

    End Sub

    Private Sub OptionToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OptionToolStripMenuItem.Click
        Form2.ShowDialog()
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        Me.Visible = True
        Me.WindowState = FormWindowState.Normal
    End Sub
End Class
