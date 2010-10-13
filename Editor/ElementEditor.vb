Public Class ElementEditor

    Implements ICommandEditor

    Private k_padding As Integer = 8
    Private m_tabs As List(Of ElementEditor)
    Private m_controls As List(Of EditorControl)
    Private m_populating As Boolean
    Private m_data As IEditorData
    Private m_controller As EditorController

    Public Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Implements ICommandEditor.Dirty
    Public Event DataUpdated()

    Public Sub Initialise(ByVal controller As EditorController, ByVal definition As IEditorDefinition)
        m_controller = controller

        InitialiseTabs(definition)
        InitialiseControls(definition.Controls)
    End Sub

    Public Sub InitialiseTabs(ByVal definition As IEditorDefinition)
        If (Not definition.Tabs Is Nothing) AndAlso definition.Tabs.Count > 0 Then
            Dim tabControl As New TabControl

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
                AddHandler tabEditor.DataUpdated, AddressOf Tab_DataUpdated
                m_tabs.Add(tabEditor)
            Next

            tabControl.Parent = Me
            tabControl.Dock = DockStyle.Fill
        End If
    End Sub

    Private Sub Tab_Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs)
        RaiseEvent Dirty(sender, args)
    End Sub

    Private Sub Tab_DataUpdated()
        RaiseEvent DataUpdated()
    End Sub

    Public Sub InitialiseControls(ByVal controller As EditorController, ByVal definition As IEditorTab)
        m_controller = controller

        If (Not definition.Controls Is Nothing) AndAlso definition.Controls.Count > 0 Then
            InitialiseControls(definition.Controls)
        End If
    End Sub

    Private Sub InitialiseControls(ByVal controls As IEnumerable(Of IEditorControl))
        Dim top As Integer = k_padding
        Dim maxCaptionWidth As Integer = 0

        m_controls = New List(Of EditorControl)

        For Each editorControl As IEditorControl In controls
            Dim newControl As New EditorControl
            m_controls.Add(newControl)
            newControl.Parent = Me
            newControl.Controller = m_controller
            newControl.Initialise(editorControl)
            newControl.Caption = editorControl.Caption
            newControl.Control.Top = top
            newControl.Control.Width = Me.Width - k_padding
            newControl.Control.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            AddHandler newControl.Dirty, AddressOf Control_Dirty
            If editorControl.Height.HasValue Then
                newControl.SetControlHeight(editorControl.Height)
            End If

            top = top + newControl.Control.Height + k_padding

            If newControl.CaptionTextWidth > maxCaptionWidth Then
                maxCaptionWidth = newControl.CaptionTextWidth
            End If
        Next

        maxCaptionWidth += k_padding

        For Each ctl As EditorControl In m_controls
            ctl.SetCaptionWidth(maxCaptionWidth)
        Next
    End Sub

    Public Sub Clear()
        ' TO DO: Clear data from all contained controls
    End Sub

    Public Sub Populate(ByVal data As IEditorData)

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

    End Sub

    Public Sub UpdateField(ByVal attribute As String, ByVal newValue As Object, ByVal setFocus As Boolean)
        ' find the control that's currently showing the attribute, and set its value - it's just been updated...

        If Not m_tabs Is Nothing Then
            For Each tab As ElementEditor In m_tabs
                tab.UpdateField(attribute, newValue, setFocus)
            Next
        End If

        If Not m_controls Is Nothing Then
            For Each ctl As EditorControl In m_controls.Where(Function(c) c.AttributeName = attribute)
                ctl.Value = newValue
                If setFocus Then ctl.Focus()
                RaiseEvent DataUpdated()
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

    Private Sub SaveData(ByVal ctl As EditorControl)
        ctl.SaveData(m_data)
    End Sub

    Private Sub Control_Dirty(ByVal sender As EditorControl, ByVal args As DataModifiedEventArgs)
        If Not m_populating Then
            args.Attribute = sender.AttributeName
            RaiseEvent Dirty(sender, args)
        End If
    End Sub

    Public Property Controller As EditorController Implements ICommandEditor.Controller
        Get
            Return m_controller
        End Get
        Set(ByVal value As EditorController)
            m_controller = value
        End Set
    End Property
End Class
