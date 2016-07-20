Imports System.IO
Imports System.Text
Imports System.Net
Imports System.Xml
Imports System.ComponentModel

Public Class Form1
    Dim titles As String() = {"titles"}
    Dim videourls As String() = {"videourls"}
    Dim audiourls As String() = {"audiourls"}
    Dim progress As Dictionary(Of WebClient, Integer)
    Dim progressbars(0) As Object

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim address As String = TextBox2.Text
        If (address <> "") Then
            Button1.Text = "Populating..."
            ListBox1.Items.Clear()
            Dim client As WebClient = New WebClient()

            Dim reader As StreamReader = New StreamReader(client.OpenRead(address))
            Dim text As String = reader.ReadToEnd
            reader.Close()
            Dim xml As New XmlDocument
            Dim Str As String
            Dim items As XmlNodeList
            Dim item As XmlNodeList
            Dim title As String
            Dim description As String
            Dim enclosure As String
            Dim videourl As String


            xml.LoadXml(text)
            items = xml.GetElementsByTagName("item")
            For i = 0 To items.Count - 1
                item = items(i).ChildNodes
                For k = 0 To item.Count - 1
                    If (item.Item(k).Name = "title") Then
                        title = item.Item(k).InnerText
                    End If
                    If (item.Item(k).Name = "description") Then
                        description = item.Item(k).InnerText
                    End If
                    If (item.Item(k).Name = "enclosure") Then
                        videourl = item.Item(k).Attributes("url").Value
                    End If
                Next

                Dim words As String() = videourl.Split(New Char() {"/"c})
                Dim mp3url As String = words(0)
                For k = 1 To words.Length - 2
                    mp3url = mp3url & "/" & words(k)
                Next
                mp3url = mp3url & "/audio.mp3"

                Array.Resize(titles, titles.Length + 1)
                titles(titles.Length - 1) = title

                Array.Resize(videourls, videourls.Length + 1)
                videourls(videourls.Length - 1) = videourl

                Array.Resize(audiourls, audiourls.Length + 1)
                audiourls(audiourls.Length - 1) = mp3url

                ListBox1.Items.Add(title)
                ListBox1.Sorted = True
            Next
        End If

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim folderDlg As New FolderBrowserDialog
        folderDlg.ShowNewFolderButton = True
        If (folderDlg.ShowDialog() = DialogResult.OK) Then
            Dim savepath = folderDlg.SelectedPath
            Label4.Text = savepath

            Dim root As Environment.SpecialFolder = folderDlg.RootFolder

            Dim downloadlist = ListBox2.Items
            For i = 0 To downloadlist.Count - 1
                Dim title As String = downloadlist(i)
                Dim videourl As String
                Dim audiourl As String
                For k = 0 To titles.Length - 1
                    If (titles(k) = title) Then
                        videourl = videourls(k)
                        audiourl = audiourls(k)
                    End If
                Next

                Dim targetvideofile As String = savepath & "\" & title & ".m4v"
                Dim targetaudiofile As String = savepath & "\" & title & ".mp3"


                If (CheckBox1.Checked = True) Then

                    Dim client As WebClient = New WebClient()
                    progress.Add(client, 0)
                    AddHandler client.DownloadProgressChanged, AddressOf ShowDownloadProgress

                    client.DownloadFileAsync(New Uri(audiourl), targetaudiofile)

                    Dim titlelbl As New Label
                    titlelbl.Width = 275
                    titlelbl.Text = "..." & title.Substring(title.Length - 45)
                    Dim typelbl As New Label
                    typelbl.Text = "MP3"
                    Dim individualprogress As New ProgressBar

                    Array.Resize(progressbars, progressbars.Length + 1)
                    progressbars(progressbars.Length - 1) = individualprogress

                    TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.AutoSize))
                    TableLayoutPanel1.RowCount += 1
                    TableLayoutPanel1.Controls.Add(titlelbl, 0, TableLayoutPanel1.RowCount - 1)
                    TableLayoutPanel1.Controls.Add(typelbl, 1, TableLayoutPanel1.RowCount - 1)
                    TableLayoutPanel1.Controls.Add(individualprogress, 2, TableLayoutPanel1.RowCount - 1)

                End If
                If (CheckBox2.Checked = True) Then

                    Dim client As WebClient = New WebClient()
                    progress.Add(client, 0)
                    AddHandler client.DownloadProgressChanged, AddressOf ShowDownloadProgress

                    client.DownloadFileAsync(New Uri(videourl), targetvideofile)

                    Dim titlelbl As New Label
                    titlelbl.Text = "..." & title.Substring(title.Length - 45)
                    titlelbl.Width = 275
                    Dim typelbl As New Label
                    typelbl.Text = "M4V"
                    Dim individualprogress As New ProgressBar

                    Array.Resize(progressbars, progressbars.Length + 1)
                    progressbars(progressbars.Length - 1) = individualprogress

                    TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.AutoSize))
                    TableLayoutPanel1.RowCount += 1
                    TableLayoutPanel1.Controls.Add(titlelbl, 0, TableLayoutPanel1.RowCount - 1)
                    TableLayoutPanel1.Controls.Add(typelbl, 1, TableLayoutPanel1.RowCount - 1)
                    TableLayoutPanel1.Controls.Add(individualprogress, 2, TableLayoutPanel1.RowCount - 1)

                End If






            Next




        End If



    End Sub

    Private Sub ShowDownloadProgress(ByVal sender As Object, ByVal e As DownloadProgressChangedEventArgs)
        progress(DirectCast(sender, WebClient)) = e.ProgressPercentage
        Dim progressvalues As Integer() = {0}
        Dim total As Integer = 0
        Dim i As Integer = 0
        For Each client In progress.Keys
            total = total + progress(client)

            Array.Resize(progressvalues, progressvalues.Length + 1)
            progressvalues(progressvalues.Length - 1) = progress(client)
        Next

        For i = 1 To progressbars.Count - 1
            progressbars(i).Value = progressvalues(i)
        Next

        ProgressBar1.Value = total \ progress.Count
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        If (ListBox1.SelectedIndex <> -1) Then
            ListBox2.Items.Add(ListBox1.SelectedItem)
            ListBox1.Items.Remove(ListBox1.SelectedItem)
            ListBox1.Sorted = True
            ListBox2.Sorted = True
        End If
    End Sub
    Private Sub ListBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox2.SelectedIndexChanged
        If (ListBox2.SelectedIndex <> -1) Then
            ListBox1.Items.Add(ListBox2.SelectedItem)
            ListBox2.Items.Remove(ListBox2.SelectedItem)
            ListBox1.Sorted = True
            ListBox2.Sorted = True
        End If
    End Sub

    Private Sub ListBox1_SizeChanged(sender As Object, e As EventArgs) Handles ListBox1.SizeChanged
        Button1.Text = "Populate"
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        progress = New Dictionary(Of WebClient, Integer)

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If (Button3.Text = "View Details") Then
            Me.Height = Me.Height + 260
            Button3.Text = "Hide Details"
        Else
            Me.Height = Me.Height - 260
            Button3.Text = "View Details"
        End If
    End Sub

End Class