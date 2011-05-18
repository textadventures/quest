Imports System.IO

<ControlType("file")> _
Public Class FileControl
    Implements IElementEditorControl

    Private m_controller As EditorController
    Private m_attribute As String
    Private m_attributeName As String
    Private m_data As IEditorData
    Private m_fileFilter As String

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements IElementEditorControl.Dirty
    Public Event RequestParentElementEditorSave() Implements IElementEditorControl.RequestParentElementEditorSave

    Public ReadOnly Property AttributeName As String Implements IElementEditorControl.AttributeName
        Get
            Return m_attribute
        End Get
    End Property

    Public ReadOnly Property Control As System.Windows.Forms.Control Implements IElementEditorControl.Control
        Get
            Return Me
        End Get
    End Property

    Public Property Controller As EditorController Implements IElementEditorControl.Controller
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
            ctlDropDown.Controller = value
        End Set
    End Property

    Public ReadOnly Property ExpectedType As System.Type Implements IElementEditorControl.ExpectedType
        Get
            Return GetType(String)
        End Get
    End Property

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        m_controller = controller
        If controlData IsNot Nothing Then
            m_attribute = controlData.Attribute
            m_attributeName = controlData.Caption

            Dim source As String = controlData.GetString("source")
            If source = "libraries" Then source = "*.aslx"
            m_fileFilter = String.Format("{0} ({1})|{1}", controlData.GetString("filefiltername"), source)
        Else
            m_attribute = Nothing
            m_attributeName = Nothing
        End If
        ctlDropDown.Initialise(controller, controlData)
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        m_data = data
        If data IsNot Nothing Then
            cmdBrowse.Enabled = Not data.ReadOnly
        End If
        ctlDropDown.Populate(data)
    End Sub

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save
        ctlDropDown.Save(data)
    End Sub

    Public Property Value As Object Implements IElementEditorControl.Value
        Get
            Return ctlDropDown.Value
        End Get
        Set(value As Object)
            ctlDropDown.Value = value
        End Set
    End Property

    Private Sub cmdBrowse_Click(sender As System.Object, e As System.EventArgs) Handles cmdBrowse.Click
        Dim filename As String

        dlgOpenFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        dlgOpenFile.Multiselect = False
        dlgOpenFile.Filter = m_fileFilter
        dlgOpenFile.FileName = ""
        dlgOpenFile.ShowDialog()
        If dlgOpenFile.FileName.Length > 0 Then
            Dim gameFolder As String = Path.GetDirectoryName(Controller.Filename)
            filename = Path.GetFileName(dlgOpenFile.FileName)
            Try
                File.Copy(dlgOpenFile.FileName, Path.Combine(gameFolder, filename))
            Catch ex As Exception
                MsgBox(String.Format("Unable to copy file. The following error occurred:{0}",
                                      Environment.NewLine + Environment.NewLine + ex.Message),
                                  MsgBoxStyle.Critical,
                                  "Error copying file")
                Return
            End Try

            m_controller.StartTransaction(String.Format("Set filename to '{0}'", filename))
            m_data.SetAttribute(m_attribute, filename)
            m_controller.EndTransaction()

            Populate(m_data)
        End If
    End Sub
End Class
