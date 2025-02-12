Public Class WPFElementEditor
    Public Event Dirty(sender As Object, args As DataModifiedEventArgs)

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        AddHandler ctlElementEditor.Dirty, AddressOf ElementEditor_Dirty
        AddHandler ctlElementEditor.RequestParentElementEditorSave, AddressOf ElementEditor_RequestParentElementEditorSave
    End Sub

    Public Sub Initialise(controller As EditorController, definition As IEditorDefinition)
        ctlElementEditor.Initialise(controller, definition)
    End Sub

    Public Sub UpdateField(attribute As String, newValue As Object, setFocus As Boolean)
        ctlElementEditor.UpdateField(attribute, setFocus)
    End Sub

    Public Sub Populate(data As IEditorData)
        ctlElementEditor.Populate(data)
    End Sub

    Public Sub SaveData()
        ctlElementEditor.Save()
    End Sub

    Private Sub ElementEditor_Dirty(sender As Object, args As EditorControls.DataModifiedEventArgs)
        RaiseEvent Dirty(sender, New DataModifiedEventArgs(Nothing, args.NewValue))
    End Sub

    Private Sub ElementEditor_RequestParentElementEditorSave()
        SaveData()
    End Sub

    Public Sub Uninitialise()
        ctlElementEditor.Uninitialise()
        RemoveHandler ctlElementEditor.Dirty, AddressOf ElementEditor_Dirty
        RemoveHandler ctlElementEditor.RequestParentElementEditorSave, AddressOf ElementEditor_RequestParentElementEditorSave
    End Sub

    Public Property SimpleMode As Boolean
        Get
            Return ctlElementEditor.SimpleMode
        End Get
        Set(value As Boolean)
            ctlElementEditor.SimpleMode = value
        End Set
    End Property
End Class
