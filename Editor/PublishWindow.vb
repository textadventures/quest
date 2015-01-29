Imports System.IO

Public Class PublishWindow

    Private m_sourceFilename As String
    Private m_controller As EditorController

    Public Sub New(sourceFilename As String, controller As EditorController)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_sourceFilename = sourceFilename
        m_controller = controller
    End Sub

    Private Sub PublishWindow_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Dim outputFolder As String = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(m_sourceFilename),
            "Output")

        Dim outputFilename As String = System.IO.Path.Combine(
                outputFolder,
                System.IO.Path.GetFileNameWithoutExtension(m_sourceFilename) + ".quest")

        txtFilename.Text = outputFilename
    End Sub

    Private Sub cmdCancel_Click(sender As System.Object, e As System.EventArgs) Handles cmdCancel.Click
        Me.Hide()
    End Sub

    Private Sub cmdBrowse_Click(sender As System.Object, e As System.EventArgs) Handles cmdBrowse.Click
        Dim outputFolder As String = System.IO.Path.GetDirectoryName(txtFilename.Text)

        Try
            System.IO.Directory.CreateDirectory(outputFolder)
        Catch ex As Exception
            ' ignore failure to create the directory when browsing
        End Try

        ctlSaveDialog.InitialDirectory = outputFolder
        ctlSaveDialog.FileName = txtFilename.Text
        Dim result = ctlSaveDialog.ShowDialog()
        If result = DialogResult.OK Then
            txtFilename.Text = ctlSaveDialog.FileName
        End If
    End Sub

    Private Sub cmdSave_Click(sender As System.Object, e As System.EventArgs) Handles cmdSave.Click
        Dim outputFolder As String = System.IO.Path.GetDirectoryName(txtFilename.Text)

        Try
            System.IO.Directory.CreateDirectory(outputFolder)
        Catch ex As Exception
            MsgBox("Failed to create folder: " + ex.Message, MsgBoxStyle.Critical, "Failed to create folder")
            Return
        End Try

        Dim outputFilename As String = txtFilename.Text

        If System.IO.File.Exists(outputFilename) Then
            Dim deleteExisting = MsgBox("Do you want to overwrite the existing .quest file?", MsgBoxStyle.Question Or MsgBoxStyle.YesNoCancel, "Output already exists")

            If deleteExisting = MsgBoxResult.Yes Then
                Try
                    System.IO.File.Delete(outputFilename)
                Catch ex As Exception
                    MsgBox("Unable to delete file: " + ex.Message, MsgBoxStyle.Critical, "Unable to delete file")
                    Return
                End Try
            ElseIf deleteExisting = MsgBoxResult.No Then
                ctlSaveDialog.FileName = outputFilename
                If ctlSaveDialog.ShowDialog() = DialogResult.OK Then
                    outputFilename = ctlSaveDialog.FileName
                Else
                    Return
                End If
            ElseIf deleteExisting = MsgBoxResult.Cancel Then
                Return
            End If
        End If

        Dim result = m_controller.Publish(outputFilename, chkIncludeWalkthrough.Checked)

        If Not result.Valid Then
            EditorControls.PopupEditors.DisplayValidationError(result, String.Empty, "Unable to publish game")
        Else
            ' Show Output folder in a new Explorer window
            Process.Start("explorer.exe", "/n," + outputFolder)

            Try
                ' warn if file size over 20MB
                If New FileInfo(outputFilename).Length > 20 * 1024 * 1024 Then
                    MsgBox("Output file size is over 20MB. Consider reducing the size of any image or sound files, and delete any unused files from the game folder." +
                           Environment.NewLine + Environment.NewLine +
                           "You may not be able to upload this file to textadventures.co.uk.", MsgBoxStyle.Exclamation, "Warning")
                End If
            Catch ex As Exception
                ' ignore
            End Try
        End If

        Me.Hide()
    End Sub
End Class