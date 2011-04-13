Public Class ElementEditor

    Implements ICommandEditor

    Private k_padding As Integer = 8
    Private m_tabs As List(Of ElementEditor)
    Private m_controls As List(Of EditorControl)
    Private m_populating As Boolean
    Private m_data As IEditorData
    Private m_controller As EditorController
    Private m_fullHeight As Integer

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements ICommandEditor.Dirty

    Public Sub Initialise(controller As EditorController, definition As IEditorDefinition)
        m_controller = controller

        InitialiseTabs(definition)
        InitialiseControls(definition.Controls)
    End Sub

    Public Sub InitialiseTabs(definition As IEditorDefinition)
        If (Not definition.Tabs Is Nothing) AndAlso definition.Tabs.Count > 0 Then
            Dim tabControl As New TabControl

            RemoveExistingTabs()
            m_tabs = New List(Of ElementEditor)

            For Each tabDefinition As KeyValuePair(Of String, IEditorTab) In definition.Tabs
                Dim tabPage As TabPage = New TabPage(tabDefinition.Value.Caption)
                Dim tabEditor As New ElementEditor

                tabPage.UseVisualStyleBackColor = True
                tabControl.TabPages.Add(tabPage)
                tabEditor.InitialiseControls(m_controller, tabDefinition.Value)
                tabEditor.Parent = tabPage
                tabEditor.Dock = DockStyle.Fill
                AddHandler tabEditor.Dirty, AddressOf Tab_Dirty
                m_tabs.Add(tabEditor)
            Next

            tabControl.Parent = Me
            tabControl.Dock = DockStyle.Fill
        End If
    End Sub

    Private Sub RemoveExistingTabs()
        If m_tabs Is Nothing Then Return

        For Each tabControl In m_tabs
            tabControl.Parent = Nothing
            tabControl.Visible = False
            RemoveHandler tabControl.Dirty, AddressOf Tab_Dirty
            tabControl.Dispose()
        Next

        m_tabs = Nothing
    End Sub

    Private Sub Tab_Dirty(sender As Object, args As DataModifiedEventArgs)
        RaiseEvent Dirty(sender, args)
    End Sub

    Public Sub InitialiseControls(controller As EditorController, definition As IEditorTab)
        m_controller = controller

        If (Not definition.Controls Is Nothing) AndAlso definition.Controls.Count > 0 Then
            InitialiseControls(definition.Controls)
        End If
    End Sub

    Private Sub InitialiseControls(controls As IEnumerable(Of IEditorControl))
        Dim top As Integer = k_padding
        Dim maxCaptionWidth As Integer = 0

        RemoveExistingControls()
        m_controls = New List(Of EditorControl)

        Me.SuspendLayout()
        For Each editorControl As IEditorControl In controls
            Dim newControl As New EditorControl
            m_controls.Add(newControl)
            newControl.Parent = Me
            newControl.Controller = m_controller
            newControl.Initialise(m_controller, editorControl)
            newControl.Caption = editorControl.Caption
            newControl.Control.Top = top
            newControl.Control.Width = Me.Width - k_padding
            newControl.Control.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            AddHandler newControl.Dirty, AddressOf Control_Dirty
            If editorControl.Height.HasValue Then
                newControl.SetControlHeight(editorControl.Height.Value)
            End If
            If editorControl.Expand Then
                newControl.SetControlHeight(Me.Height - top - k_padding)
                newControl.Anchor = newControl.Anchor Or AnchorStyles.Bottom
            End If

            top = top + newControl.Control.Height + k_padding

            If newControl.CaptionTextWidth > maxCaptionWidth Then
                maxCaptionWidth = newControl.CaptionTextWidth
            End If
        Next
        Me.ResumeLayout()

        m_fullHeight = top - k_padding
        maxCaptionWidth += k_padding

        For Each ctl As EditorControl In m_controls
            ctl.SetCaptionWidth(maxCaptionWidth)
        Next
    End Sub

    Private Sub RemoveExistingControls()
        If m_controls Is Nothing Then Return

        For Each ctl In m_controls
            RemoveHandler ctl.Dirty, AddressOf Control_Dirty
            ctl.Parent = Nothing
            ctl.Visible = False
            ctl.Value = Nothing
            ctl.Dispose()
        Next

        m_controls = Nothing
    End Sub

    Public Sub Clear()
        ' TO DO: Clear data from all contained controls
    End Sub

    Public Sub Populate(data As IEditorData)

        Me.SuspendLayout()
        m_populating = True
        m_data = data

        If Not m_tabs Is Nothing Then
            For Each tab As ElementEditor In m_tabs
                tab.Populate(data)
            Next
        End If

        If Not m_controls Is Nothing Then
            For Each ctl As EditorControl In m_controls
                ctl.Populate(data)
            Next
        End If

        m_populating = False
        Me.ResumeLayout()

    End Sub

    Public Sub UpdateField(attribute As String, newValue As Object, setFocus As Boolean) Implements ICommandEditor.UpdateField
        ' find the control that's currently showing the attribute, and set its value - it's just been updated...

        If Not m_tabs Is Nothing Then
            For Each tab As ElementEditor In m_tabs
                tab.UpdateField(attribute, newValue, setFocus)
            Next
        End If

        If Not m_controls Is Nothing Then
            For Each ctl As EditorControl In m_controls.Where(Function(c) c.AttributeName = attribute)
                ctl.Value = newValue
                ' TO DO: Disabled the setFocus capability at the moment as it can cause some mad things to happen - I think if you
                ' SetFocus during a Leave event then two controls might end up with focus at the same time, and they won't paint
                ' properly.
                'If setFocus Then ctl.Focus()
            Next
        End If
    End Sub

    Public Sub SaveData() Implements ICommandEditor.SaveData

        If Not m_tabs Is Nothing Then
            For Each tab As ElementEditor In m_tabs
                tab.SaveData()
            Next
        End If

        If Not m_controls Is Nothing Then
            For Each ctl As EditorControl In m_controls
                SaveData(ctl)
            Next
        End If

    End Sub

    Private Sub SaveData(ctl As EditorControl)
        ctl.SaveData(m_data)
    End Sub

    Private Sub Control_Dirty(sender As Object, args As DataModifiedEventArgs)
        If Not m_populating Then
            args.Attribute = DirectCast(sender, EditorControl).AttributeName
            RaiseEvent Dirty(sender, args)
        End If
    End Sub

    Public Property Controller As EditorController Implements ICommandEditor.Controller
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
        End Set
    End Property

    Public ReadOnly Property MinHeight As Integer Implements ICommandEditor.MinHeight
        Get
            Return m_fullHeight
        End Get
    End Property
End Class
