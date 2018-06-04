Imports TextAdventures.Quest.EditorControls
Imports TextAdventures.Quest.EditorController
Imports TextAdventures.Utility.Language.L

Public Class Editor

    Private WithEvents m_controller As EditorController
    Private m_elementEditors As Dictionary(Of String, WPFElementEditor)
    Private m_currentEditor As WPFElementEditor
    Private m_menu As TextAdventures.Quest.Controls.Menu
    Private m_filename As String
    Private m_currentElement As String
    Private m_codeView As Boolean
    Private m_lastSelection As String
    Private m_currentEditorData As IEditorDataExtendedAttributeInfo
    Private m_unsavedChanges As Boolean
    Private WithEvents m_fileWatcher As System.IO.FileSystemWatcher
    Private m_simpleMode As Boolean
    Private m_editorStyle As EditorStyle = EditorStyle.TextAdventure
    Private m_reloadingFromCodeView As Boolean
    Private m_uiHidden As Boolean
    Private m_splitHelper As TextAdventures.Utility.SplitterHelper

    Public Event AddToRecent(filename As String, name As String)
    Public Event Close()
    Public Event Play(filename As String)
    Public Event Loaded(name As String)
    Public Event NewGame()
    Public Event OpenGame()
    Public Event PlayWalkthrough(filename As String, walkthrough As String, record As Boolean)
    Public Event InitialiseFinished(success As Boolean)

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        BannerVisible = False
        HideUI()
    End Sub

    Public Sub Initialise(ByRef filename As String)
        m_menu.Visible = False
        If Not m_uiHidden Then
            HideUI()
        End If
        m_currentElement = Nothing
        m_currentEditor = Nothing
        m_filename = filename
        If m_controller IsNot Nothing Then
            m_controller.Uninitialise()
        End If
        m_controller = New EditorController()
        m_unsavedChanges = False
        InitialiseEditorControlsList()
        ctlReloadBanner.Visible = False
        m_controller.StartInitialise(filename)
    End Sub

    Private Sub m_controller_InitialiseFinished(sender As Object, e As EditorController.InitialiseResults) Handles m_controller.InitialiseFinished
        BeginInvoke(Sub()
                        If e.Success Then
                            m_controller.UpdateTree()
                            Application.DoEvents()
                            Dim path As String = System.IO.Path.GetDirectoryName(m_filename)
                            Dim filter As String = System.IO.Path.GetFileName(m_filename)
                            m_fileWatcher = New System.IO.FileSystemWatcher(path, "*.aslx")
                            m_fileWatcher.EnableRaisingEvents = True
                            m_simpleMode = False
                            SetUpTree()
                            SetUpToolbar()
                            ctlLoading.UpdateStatus("Loading editors...")
                            SetUpEditors()
                            RaiseEvent AddToRecent(m_filename, m_controller.GameName)
                            EditorStyle = m_controller.EditorStyle
                            SimpleMode = (CInt(TextAdventures.Utility.Registry.GetSetting("Quest", "Settings", "EditorSimpleMode", 0)) = 1)
                            SetWordWrap(CInt(TextAdventures.Utility.Registry.GetSetting("Quest", "Settings", "EditorWordWrap", 0)) = 1)
                            m_splitHelper = New TextAdventures.Utility.SplitterHelper(splitMain, "Quest", "EditorSplitter")
                            m_splitHelper.LoadSplitterPositions()
                            m_menu.Visible = True
                            m_uiHidden = False
                            Me.SuspendLayout()
                            DisplayCodeView(False)
                            ctlLoading.Visible = False
                            splitMain.Visible = True
                            ctlTree.Visible = True
                            ctlToolbar.Visible = True
                            Me.ResumeLayout()
                            ctlTree.SetSelectedItem("game")
                            ctlTree.FocusOnTree()
                            SetWindowTitle()
                            ShowEditor("game")
                        End If

                        If m_reloadingFromCodeView Then
                            If Not e.Success Then
                                ' Couldn't reload the file due to an error, so show code view again
                                m_menu.Visible = True
                                m_uiHidden = False
                                splitMain.Visible = True
                                ctlTree.Visible = True
                                ctlToolbar.Visible = True
                                ctlLoading.Visible = False
                                DisplayCodeView(True)
                            Else
                                ctlTree.TrySetSelectedItem(m_lastSelection)
                            End If
                            m_reloadingFromCodeView = False
                        Else
                            RaiseEvent InitialiseFinished(e.Success)
                        End If

                        ctlLoading.Clear()

                    End Sub)
    End Sub

    Private Sub InitialiseEditorControlsList()
        EditorControls.ElementEditor.InitialiseEditorControls(m_controller)
    End Sub

    Public Sub SetMenu(menu As TextAdventures.Quest.Controls.Menu)
        m_menu = menu
        menu.AddMenuClickHandler("save", AddressOf Save)
        menu.AddMenuClickHandler("saveas", AddressOf SaveAs)
        menu.AddMenuClickHandler("undo", AddressOf Undo)
        menu.AddMenuClickHandler("redo", AddressOf Redo)
        menu.AddMenuClickHandler("addpage", AddressOf AddNewPage)
        menu.AddMenuClickHandler("addobject", AddressOf AddNewObject)
        menu.AddMenuClickHandler("addroom", AddressOf AddNewRoom)
        menu.AddMenuClickHandler("addexit", AddressOf AddNewExit)
        menu.AddMenuClickHandler("addverb", AddressOf AddNewVerb)
        menu.AddMenuClickHandler("addcommand", AddressOf AddNewCommand)
        menu.AddMenuClickHandler("addfunction", AddressOf AddNewFunction)
        menu.AddMenuClickHandler("addtimer", AddressOf AddNewTimer)
        menu.AddMenuClickHandler("addturnscript", AddressOf AddNewTurnScript)
        menu.AddMenuClickHandler("addwalkthrough", AddressOf AddNewWalkthrough)
        menu.AddMenuClickHandler("addlibrary", AddressOf AddNewLibrary)
        menu.AddMenuClickHandler("addimpliedtype", AddressOf AddNewImpliedType)
        menu.AddMenuClickHandler("addtemplate", AddressOf AddNewTemplate)
        menu.AddMenuClickHandler("adddynamictemplate", AddressOf AddNewDynamicTemplate)
        menu.AddMenuClickHandler("adddelegate", AddressOf AddNewDelegate)
        menu.AddMenuClickHandler("addobjecttype", AddressOf AddNewObjectType)
        menu.AddMenuClickHandler("addeditor", AddressOf AddNewEditor)
        menu.AddMenuClickHandler("addjavascript", AddressOf AddNewJavascript)
        menu.AddMenuClickHandler("play", AddressOf PlayGame)
        menu.AddMenuClickHandler("close", AddressOf DoClose)
        menu.AddMenuClickHandler("cut", AddressOf Cut)
        menu.AddMenuClickHandler("copy", AddressOf Copy)
        menu.AddMenuClickHandler("paste", AddressOf Paste)
        menu.AddMenuClickHandler("delete", AddressOf Delete)
        menu.AddMenuClickHandler("publish", AddressOf Publish)
        menu.AddMenuClickHandler("find", AddressOf Find)
        menu.AddMenuClickHandler("replace", AddressOf Replace)
        menu.AddMenuClickHandler("simplemode", AddressOf ToggleSimpleMode)
        menu.AddMenuClickHandler("codeview", AddressOf ToggleCodeView)
        menu.AddMenuClickHandler("wordwrap", AddressOf ToggleWordWrap)
    End Sub

    Private Sub SetUpToolbar()
        ctlToolbar.ResetToolbar()
        ctlToolbar.AddButtonHandler("new", AddressOf CreateNew)
        ctlToolbar.AddButtonHandler("open", AddressOf Open)
        ctlToolbar.AddButtonHandler("save", AddressOf Save)
        ctlToolbar.AddButtonHandler("undo", AddressOf Undo)
        ctlToolbar.AddButtonHandler("redo", AddressOf Redo)
        ctlToolbar.AddButtonHandler("addpage", AddressOf AddNewPage)
        ctlToolbar.AddButtonHandler("addobject", AddressOf AddNewObject)
        ctlToolbar.AddButtonHandler("addroom", AddressOf AddNewRoom)
        ctlToolbar.AddButtonHandler("play", AddressOf PlayGame)
        ctlToolbar.AddButtonHandler("cut", AddressOf Cut)
        ctlToolbar.AddButtonHandler("copy", AddressOf Copy)
        ctlToolbar.AddButtonHandler("paste", AddressOf Paste)
        ctlToolbar.AddButtonHandler("delete", AddressOf Delete)
        ctlToolbar.AddButtonHandler("find", AddressOf Find)
        ctlToolbar.AddButtonHandler("replace", AddressOf Replace)
        ctlToolbar.AddButtonHandler("code", AddressOf ToggleCodeView)
        ctlToolbar.AddButtonHandler("logbug", AddressOf LogBug)
        ctlToolbar.AddButtonHandler("help", AddressOf Help)
    End Sub

    Private Sub SetUpTree()
        ctlTree.SetAvailableFilters(m_controller.AvailableFilters)
        ctlTree.SetCanDragDelegate(AddressOf m_controller.CanMoveElement)
        ctlTree.SetDoDragDelegate(AddressOf m_controller.MoveElement)
        ctlTree.CollapseAdvancedNode()
        ctlTree.ScrollToTop()

        ctlTree.AddMenuClickHandler("addpage", AddressOf AddNewPage)
        ctlTree.AddMenuClickHandler("addobject", AddressOf AddNewObject)
        ctlTree.AddMenuClickHandler("addroom", AddressOf AddNewRoom)
        ctlTree.AddMenuClickHandler("addexit", AddressOf AddNewExit)
        ctlTree.AddMenuClickHandler("addverb", AddressOf AddNewVerb)
        ctlTree.AddMenuClickHandler("addcommand", AddressOf AddNewCommand)
        ctlTree.AddMenuClickHandler("addfunction", AddressOf AddNewFunction)
        ctlTree.AddMenuClickHandler("addtimer", AddressOf AddNewTimer)
        ctlTree.AddMenuClickHandler("addturnscript", AddressOf AddNewTurnScript)
        ctlTree.AddMenuClickHandler("addwalkthrough", AddressOf AddNewWalkthrough)
        ctlTree.AddMenuClickHandler("addlibrary", AddressOf AddNewLibrary)
        ctlTree.AddMenuClickHandler("addimpliedtype", AddressOf AddNewImpliedType)
        ctlTree.AddMenuClickHandler("addtemplate", AddressOf AddNewTemplate)
        ctlTree.AddMenuClickHandler("adddynamictemplate", AddressOf AddNewDynamicTemplate)
        ctlTree.AddMenuClickHandler("adddelegate", AddressOf AddNewDelegate)
        ctlTree.AddMenuClickHandler("addobjecttype", AddressOf AddNewObjectType)
        ctlTree.AddMenuClickHandler("addeditor", AddressOf AddNewEditor)
        ctlTree.AddMenuClickHandler("addjavascript", AddressOf AddNewJavascript)
        ctlTree.AddMenuClickHandler("cut", AddressOf Cut)
        ctlTree.AddMenuClickHandler("copy", AddressOf Copy)
        ctlTree.AddMenuClickHandler("paste", AddressOf Paste)
        ctlTree.AddMenuClickHandler("delete", AddressOf Delete)
    End Sub

    Private Sub SetUpEditors()
        UnloadEditors()
        m_elementEditors = New Dictionary(Of String, WPFElementEditor)

        For Each editor As String In m_controller.GetAllEditorNames()
            Application.DoEvents()
            AddEditor(editor)
        Next
    End Sub

    Private Sub AddEditor(name As String)
        ' Get an EditorDefinition from the EditorController, then pass it in to the ElementEditor so it can initialise its
        ' tabs and subcontrols.
        Dim editor As WPFElementEditor
        editor = New WPFElementEditor
        editor.Initialise(m_controller, m_controller.GetEditorDefinition(name))
        editor.Visible = False
        editor.Parent = pnlContent
        editor.Dock = DockStyle.Fill
        AddHandler editor.Dirty, AddressOf Editor_Dirty
        m_elementEditors.Add(name, editor)
    End Sub

    Private Sub Editor_Dirty(sender As Object, args As DataModifiedEventArgs)
        ctlToolbar.EnableUndo()
        m_unsavedChanges = True
    End Sub

    Private Sub UnloadEditors()
        If m_elementEditors Is Nothing Then Return

        For Each editor As WPFElementEditor In m_elementEditors.Values
            editor.Populate(Nothing)
            editor.Uninitialise()
            editor.Parent = Nothing
            editor.Dispose()
            RemoveHandler editor.Dirty, AddressOf Editor_Dirty
        Next

        m_elementEditors.Clear()
    End Sub

    Private Sub m_controller_AddedNode(sender As Object, e As AddedNodeEventArgs) Handles m_controller.AddedNode
        Dim foreColor As Color = If(e.IsLibraryNode, SystemColors.ControlDarkDark, SystemColors.ControlText)
        ctlTree.AddNode(e.Key, e.Text, e.Parent, foreColor, Nothing, e.Position)
    End Sub

    Private Sub m_controller_RemovedNode(sender As Object, e As RemovedNodeEventArgs) Handles m_controller.RemovedNode
        ctlTree.RemoveNode(e.Key)
    End Sub

    Private Sub m_controller_RenamedNode(sender As Object, e As RenamedNodeEventArgs) Handles m_controller.RenamedNode
        If m_currentElement = e.OldName Then
            m_currentElement = e.NewName
            RefreshCurrentElement()
        End If
        ctlTree.RenameNode(e.OldName, e.NewName)
        ctlToolbar.RenameHistory(e.OldName, e.NewName)
    End Sub

    Private Sub m_controller_RetitledNode(sender As Object, e As RetitledNodeEventArgs) Handles m_controller.RetitledNode
        If (m_currentElement = e.Key) Then
            lblHeader.Text = e.NewTitle
        End If
        ctlTree.RetitleNode(e.Key, e.NewTitle)
        ctlToolbar.RetitleHistory(e.Key, e.NewTitle)
    End Sub

    Private Sub m_controller_BeginTreeUpdate() Handles m_controller.BeginTreeUpdate
        ctlTree.BeginUpdate()
    End Sub

    Private Sub m_controller_ClearTree() Handles m_controller.ClearTree
        ctlTree.Clear()
    End Sub

    Private Sub m_controller_ElementUpdated(sender As Object, e As EditorController.ElementUpdatedEventArgs) Handles m_controller.ElementUpdated
        If e.Element = m_currentElement Then
            m_currentEditor.UpdateField(e.Attribute, e.NewValue, e.IsUndo)
        End If
    End Sub

    Private Sub m_controller_ElementRefreshed(sender As Object, e As EditorController.ElementRefreshedEventArgs) Handles m_controller.ElementRefreshed
        If e.Element = m_currentElement Then
            RefreshCurrentElement()
        End If
    End Sub

    Private Sub RefreshCurrentElement()
        m_currentEditor.Populate(m_controller.GetEditorData(m_currentElement))
    End Sub

    Private Sub m_controller_EndTreeUpdate() Handles m_controller.EndTreeUpdate
        ctlTree.EndUpdate()
    End Sub

    Private Sub m_controller_UndoListUpdated(sender As Object, e As EditorController.UpdateUndoListEventArgs) Handles m_controller.UndoListUpdated
        ctlToolbar.UpdateUndoMenu(e.UndoList)
        If e.UndoList.Count > 0 Then m_unsavedChanges = True
    End Sub

    Private Sub m_controller_RedoListUpdated(sender As Object, e As EditorController.UpdateUndoListEventArgs) Handles m_controller.RedoListUpdated
        ctlToolbar.UpdateRedoMenu(e.UndoList)
        If e.UndoList.Count > 0 Then m_unsavedChanges = True
    End Sub

    Private Sub m_controller_ShowMessage(sender As Object, e As ShowMessageEventArgs) Handles m_controller.ShowMessage
        System.Windows.Forms.MessageBox.Show(e.Message)
    End Sub

    Private Sub ctlTree_FiltersUpdated() Handles ctlTree.FiltersUpdated
        m_controller.UpdateFilterOptions(ctlTree.FilterSettings)
    End Sub

    Private Sub ctlTree_TreeGotFocus() Handles ctlTree.TreeGotFocus
        SetMenuShortcutKeys()
    End Sub

    Private Sub ctlTree_TreeLostFocus() Handles ctlTree.TreeLostFocus
        UnsetMenuShortcutKeys()
    End Sub

    Private Sub ctlTree_SelectionChanged(key As String) Handles ctlTree.SelectionChanged
        ctlToolbar.AddHistory(key, m_controller.GetDisplayName(key))
        ShowEditor(key)
    End Sub

    Private Sub SetMenuShortcutKeys()
        m_menu.SetShortcut("cut", Keys.Control Or Keys.X)
        m_menu.SetShortcut("copy", Keys.Control Or Keys.C)
        m_menu.SetShortcut("paste", Keys.Control Or Keys.V)
        m_menu.SetShortcut("delete", Keys.Delete)
    End Sub

    Private Sub UnsetMenuShortcutKeys()
        m_menu.SetShortcut("cut", Keys.None)
        m_menu.SetShortcut("copy", Keys.None)
        m_menu.SetShortcut("paste", Keys.None)
        m_menu.SetShortcut("delete", Keys.None)
    End Sub

    Private Sub ShowEditor(key As String)

        If m_currentEditor IsNot Nothing Then
            m_currentEditor.SaveData()
        End If

        Dim editorName As String = m_controller.GetElementEditorName(key)
        If editorName Is Nothing Then
            If m_currentEditor IsNot Nothing Then
                m_currentEditor.Visible = False
            End If

            m_currentEditor = Nothing
            BannerVisible = False
        Else
            Dim nextEditor As WPFElementEditor = m_elementEditors(editorName)

            If Not m_currentEditor Is Nothing Then
                If Not m_currentEditor.Equals(nextEditor) Then
                    m_currentEditor.Visible = False
                End If
            End If

            m_currentEditor = nextEditor

            m_currentElement = key
            Dim data As IEditorData = m_controller.GetEditorData(key)
            m_currentEditor.Populate(data)
            nextEditor.Visible = True

            Dim extendedData As IEditorDataExtendedAttributeInfo = TryCast(data, IEditorDataExtendedAttributeInfo)
            If extendedData IsNot Nothing Then
                m_currentEditorData = extendedData
                BannerVisible = extendedData.IsLibraryElement
                Dim filename As String = System.IO.Path.GetFileName(extendedData.Filename)
                ctlBanner.AlertText = String.Format(T("EditorFromLibrary"), filename)
                ctlBanner.ButtonText = T("EditorFromLibraryCopy")
            Else
                BannerVisible = False
            End If
        End If

        lblHeader.Text = m_controller.GetDisplayName(key)
        UpdateClipboardButtons()
    End Sub

    Private Function Save() As Boolean
        If (m_filename.Length = 0) Then
            Return SaveAs()
        Else
            Return Save(m_filename)
        End If
    End Function

    Private Function SaveAs() As Boolean
        ' TO DO: If file is saved to a different folder, we should probably reinitialise as the
        ' available included libraries may have changed.

        ctlSaveFile.FileName = m_filename
        If ctlSaveFile.ShowDialog() = DialogResult.OK Then
            m_filename = ctlSaveFile.FileName
            SetWindowTitle()
            Return Save(m_filename)
        End If
        Return False
    End Function

    Private Function Save(filename As String) As Boolean
        Try
            m_fileWatcher.EnableRaisingEvents = False
            If m_codeView Then
                ctlTextEditor.SaveFile(filename)
            Else
                If Not m_currentEditor Is Nothing Then
                    m_currentEditor.SaveData()
                End If
                System.IO.File.WriteAllText(filename, m_controller.Save())
            End If
            m_controller.Filename = filename
            m_unsavedChanges = False
            Return True
        Catch ex As Exception
            MsgBox(T("EditorUnableToSave") + Environment.NewLine + Environment.NewLine + ex.Message, MsgBoxStyle.Critical)
            Return False
        Finally
            m_fileWatcher.EnableRaisingEvents = True
        End Try
    End Function

    Private Sub ctlToolbar_HistoryClicked(Key As String) Handles ctlToolbar.HistoryClicked
        If ctlTree.TrySetSelectedItemNoEvent(Key) Then
            ShowEditor(Key)
        End If
    End Sub

    Private Sub Undo()
        If m_codeView Then
            ctlTextEditor.Undo()
        Else
            If Not m_currentEditor Is Nothing Then
                Dim thisElement As String = m_currentElement
                m_currentEditor.SaveData()
                m_controller.Undo()
                If thisElement IsNot Nothing Then
                    ctlTree.TrySetSelectedItem(thisElement)
                End If
            End If
        End If
    End Sub

    Private Sub Redo()
        If m_codeView Then
            ctlTextEditor.Redo()
        Else
            If Not m_currentEditor Is Nothing Then
                Dim thisElement As String = m_currentElement
                m_currentEditor.SaveData()
                m_controller.Redo()
                If thisElement IsNot Nothing Then
                    ctlTree.TrySetSelectedItem(thisElement)
                End If
            End If
        End If
    End Sub

    Private Sub ctlToolbar_SaveCurrentEditor() Handles ctlToolbar.SaveCurrentEditor
        If Not m_currentEditor Is Nothing Then
            m_currentEditor.SaveData()
        End If
    End Sub

    Private Sub ctlToolbar_UndoClicked(level As Integer) Handles ctlToolbar.UndoClicked
        m_controller.Undo(level)
    End Sub

    Private Sub ctlToolbar_RedoClicked(level As Integer) Handles ctlToolbar.RedoClicked
        m_controller.Redo(level)
    End Sub

    Private Sub ctlToolbar_UndoEnabled(enabled As Boolean) Handles ctlToolbar.UndoEnabled
        If m_menu Is Nothing Then Return
        m_menu.MenuEnabled("undo") = enabled
    End Sub

    Private Sub ctlToolbar_RedoEnabled(enabled As Boolean) Handles ctlToolbar.RedoEnabled
        If m_menu Is Nothing Then Return
        m_menu.MenuEnabled("redo") = enabled
    End Sub

    Private Function GetParentForCurrentSelection() As String
        If m_controller.GetElementType(ctlTree.SelectedItem) = "object" AndAlso m_controller.GetObjectType(ctlTree.SelectedItem) = "object" Then
            Return ctlTree.SelectedItem
        Else
            Return Nothing
        End If
    End Function

    Private Sub AddNewElement(typeName As String, action As Action(Of String))
        Dim result = PopupEditors.EditString(String.Format(T("EditorNewElement"), typeName), "")
        If result.Cancelled Then Return
        If Not ValidateInput(result.Result).IsValid Then Return

        action(result.Result)
        ctlTree.SetSelectedItem(result.Result)
    End Sub

    Private Sub AddNewObject()
        Dim possibleParents = m_controller.GetPossibleNewObjectParentsForCurrentSelection(ctlTree.SelectedItem)
        Dim result = GetNameAndParent(T("EditorNewObject"), possibleParents, True)

        If result Is Nothing Then Return

        m_controller.CreateNewObject(result.Value.Result, result.Value.ListResult, result.Value.AliasResult)
        ctlTree.SetSelectedItem(result.Value.Result)
    End Sub

    Private Sub AddNewRoom()
        Dim result = PopupEditors.EditString(T("EditorNewRoom"), "")
        If result.Cancelled Then Return
        If Not ValidateInput(result.Result).IsValid Then Return

        m_controller.CreateNewRoom(result.Result, Nothing, Nothing)
        ctlTree.SetSelectedItem(result.Result)
    End Sub

    Private Sub AddNewExit()
        Dim newExit = m_controller.CreateNewExit(GetParentForCurrentSelection())
        ctlTree.SetSelectedItem(newExit)
    End Sub

    Private Sub AddNewVerb()
        Dim newVerb = m_controller.CreateNewVerb(GetParentForCurrentSelection(), True)
        ctlTree.SetSelectedItem(newVerb)
    End Sub

    Private Sub AddNewCommand()
        Dim newCommand = m_controller.CreateNewCommand(GetParentForCurrentSelection())
        ctlTree.SetSelectedItem(newCommand)
    End Sub

    Private Sub AddNewFunction()
        AddNewElement("function", AddressOf m_controller.CreateNewFunction)
    End Sub

    Private Sub AddNewTimer()
        AddNewElement("timer", AddressOf m_controller.CreateNewTimer)
    End Sub

    Private Sub AddNewTurnScript()
        Dim newTurnScript = m_controller.CreateNewTurnScript(GetParentForCurrentSelection())
        ctlTree.SetSelectedItem(newTurnScript)
    End Sub

    Private Sub AddNewWalkthrough()
        Dim possibleParents = m_controller.GetPossibleNewParentsForCurrentSelection(ctlTree.SelectedItem, "walkthrough")
        Dim result = GetNameAndParent(T("EditorNewWalkthrough"), possibleParents, False)

        If result Is Nothing Then Return

        m_controller.CreateNewWalkthrough(result.Value.Result, result.Value.ListResult)
        ctlTree.SetSelectedItem(result.Value.Result)
    End Sub

    Private Sub AddNewLibrary()
        Dim newLibrary = m_controller.CreateNewIncludedLibrary()
        ctlTree.SetSelectedItem(newLibrary)
    End Sub

    Private Sub AddNewTemplate()
        Dim result = PopupEditors.EditString(T("EditorNewTemplate"), "")
        If result.Cancelled Then Return

        If Not ValidateInputTemplateName(result.Result) Then Return

        Dim newTemplate = m_controller.CreateNewTemplate(result.Result)
        ctlTree.SetSelectedItem(newTemplate)
    End Sub

    Private Sub AddNewDynamicTemplate()
        AddNewElement("dynamic template", AddressOf m_controller.CreateNewDynamicTemplate)
    End Sub

    Private Sub AddNewObjectType()
        AddNewElement("object type", AddressOf m_controller.CreateNewType)
    End Sub

    Private Sub AddNewJavascript()
        Dim newJavascript = m_controller.CreateNewJavascript()
        ctlTree.SetSelectedItem(newJavascript)
    End Sub

    ' Intentionally unimplemented, as implied types are not editable in the Editor
    Private Sub AddNewImpliedType()
        Throw New NotImplementedException
    End Sub

    ' Intentionally unimplemented, as editor elements are not editable in the Editor
    Private Sub AddNewEditor()
        Throw New NotImplementedException
    End Sub

    ' Intentionally unimplemented, as delegate elements are not editable in the Editor
    Private Sub AddNewDelegate()
        Throw New NotImplementedException
    End Sub

    Private Sub AddNewPage()
        Dim result = PopupEditors.EditString(T("EditorNewPage"), m_controller.GetUniqueElementName("Page1"))
        If result.Cancelled Then Return
        If Not ValidateInput(result.Result).IsValid Then Return

        m_controller.CreateNewObject(result.Result, Nothing, Nothing)
        ctlTree.SetSelectedItem(result.Result)
    End Sub

    Private Function GetNameAndParent(prompt As String, possibleParents As IEnumerable(Of String), allowAlias As Boolean) As PopupEditors.EditStringResult?
        Dim noParent As String = T("EditorNotParents")

        Dim result As PopupEditors.EditStringResult

        If possibleParents Is Nothing Then
            result = PopupEditors.EditString(prompt, "")
        Else
            Dim parentOptions As New List(Of String)
            parentOptions.Add(noParent)
            parentOptions.AddRange(possibleParents)

            result = PopupEditors.EditStringWithDropdown(prompt, "", T("EditorParent"), parentOptions, parentOptions(1))
        End If

        If result.Cancelled Then Return Nothing
        Dim validateResult = ValidateInput(result.Result, allowAlias)

        If Not validateResult.IsValid Then
            If Not allowAlias Or String.IsNullOrEmpty(validateResult.Alias) Then
                Return Nothing
            Else
                result.Result = validateResult.ElementName
                result.AliasResult = validateResult.Alias
            End If
        End If

        If possibleParents IsNot Nothing And result.ListResult = noParent Then
            result.ListResult = Nothing
        End If

        Return result
    End Function

    Private Class ValidateInputResult
        Public ElementName As String
        Public [Alias] As String
        Public IsValid As Boolean
    End Class

    Private Function ValidateInput(input As String, Optional allowAlias As Boolean = False) As ValidateInputResult
        Dim result As New ValidateInputResult
        Dim validationResult = m_controller.CanAdd(input)
        If validationResult.Valid Then
            result.ElementName = input
            result.IsValid = True
            Return result
        End If

        If allowAlias And Not String.IsNullOrEmpty(validationResult.SuggestedName) Then
            result.ElementName = validationResult.SuggestedName
            result.Alias = input
            result.IsValid = False
            Return result
        End If

        PopupEditors.DisplayValidationError(validationResult, input, T("EditorUnableToAddEmement"))
        result.IsValid = False
        Return result
    End Function

    Private Function ValidateInputTemplateName(input As String) As Boolean
        Dim result = m_controller.CanAddTemplate(input)
        If result.Valid Then Return True

        PopupEditors.DisplayValidationError(result, input, T("EditorUnableToAddTemplate"))
        Return False
    End Function

    Public Function CreateNewGame() As String
        Dim templates As Dictionary(Of String, TemplateData) = EditorController.GetAvailableTemplates()
        Dim newGameWindow As New NewGameWindow
        newGameWindow.SetAvailableTemplates(templates)
        newGameWindow.ShowDialog()

        If newGameWindow.Cancelled Then Return Nothing

        Dim filename = newGameWindow.txtFilename.Text
        Dim folder = System.IO.Path.GetDirectoryName(filename)
        If Not System.IO.Directory.Exists(folder) Then
            System.IO.Directory.CreateDirectory(folder)
        End If

        Dim initialFileText = EditorController.CreateNewGameFile(filename, newGameWindow.GetSelectedTemplate().Filename, newGameWindow.txtGameName.Text)
        IO.File.WriteAllText(filename, initialFileText)

        Return filename
    End Function

    Private Sub PlayGame()
        If Not CheckGameIsSaved(Nothing) Then Return
        RaiseEvent Play(m_filename)
    End Sub

    Private Sub DoClose()
        CloseEditor(True, False)
    End Sub

    Public Function CloseEditor(raiseCloseEvent As Boolean, appIsExiting As Boolean) As Boolean
        If Not CheckGameIsSaved(T("EditorSaveBeforeClosing")) Then Return False

        If raiseCloseEvent Then RaiseEvent Close()

        m_currentElement = Nothing
        m_currentEditor = Nothing
        m_currentEditorData = Nothing
        If m_controller IsNot Nothing Then
            m_controller.Uninitialise()
        End If
        m_controller = Nothing

        If Not appIsExiting Then
            UnloadEditors()
            ctlTree.UnhookDelegates()

            ' uncomment for debugging memory leaks
            'GC.Collect()
            'GC.WaitForPendingFinalizers()
            'GC.Collect()
        End If

        If Not appIsExiting Then
            HideUI()
        End If

        Return True
    End Function

    Private Sub HideUI()
        m_uiHidden = True
        splitMain.Visible = False
        ctlTree.Visible = False
        ctlToolbar.Visible = False
        ctlTextEditor.Visible = False
        ctlLoading.Visible = True
        ctlLoading.BringToFront()
    End Sub

    Private Sub Cut()
        If m_codeView Then
            ctlTextEditor.Cut()
        Else
            m_controller.CutElements({ctlTree.SelectedItem})
            UpdateClipboardButtons()
        End If
    End Sub

    Private Sub Copy()
        If m_codeView Then
            ctlTextEditor.Copy()
        Else
            m_controller.CopyElements({ctlTree.SelectedItem})
            UpdateClipboardButtons()
        End If
    End Sub

    Private Sub Paste()
        If m_codeView Then
            ctlTextEditor.Paste()
        Else
            m_controller.PasteElements(ctlTree.SelectedItem)
        End If
    End Sub

    Private Sub Delete()
        m_controller.DeleteElement(ctlTree.SelectedItem, True)
    End Sub

    Private Sub ToggleCodeView()
        Dim unsavedPrompt = If(m_codeView,
                               T("EditorSaveBeforeLeavingCodeView"),
                               T("EditorSaveBeforeEditingThisGame"))

        If Not CheckGameIsSaved(unsavedPrompt) Then Return

        If Not m_codeView Then
            m_lastSelection = ctlTree.SelectedItem
        End If

        DisplayCodeView(Not m_codeView)

        If m_codeView Then
            ctlTextEditor.LoadFile(m_filename)
            ctlTextEditor.Focus()
            SetWindowTitle()
        Else
            If ctlTextEditor.TextWasSaved Then
                ' file was changed in the text editor, so reload it
                m_reloadingFromCodeView = True
                Initialise(m_filename)
            Else
                SetWindowTitle()
            End If
        End If

        If Not m_reloadingFromCodeView Then
            UpdateClipboardButtons()
        End If
    End Sub

    Private Sub DisplayCodeView(codeView As Boolean)
        m_codeView = codeView
        ctlToolbar.SetToggle("code", codeView)
        ctlTextEditor.Visible = codeView
        splitMain.Visible = Not codeView
        ctlToolbar.CodeView = codeView
        m_menu.MenuVisible("add") = Not codeView
        m_menu.MenuVisible("find") = codeView
        m_menu.MenuVisible("replace") = codeView
        m_menu.MenuVisible("delete") = Not codeView
        m_menu.MenuVisible("cut") = Not codeView
        m_menu.MenuVisible("wordwrap") = codeView
        m_menu.MenuEnabled("simplemode") = Not codeView
        m_menu.MenuChecked("codeview") = codeView
        m_menu.MenuEnabled("publish") = Not codeView
    End Sub

    Public Sub Redisplay()
        DisplayCodeView(m_codeView)
        UpdateClipboardButtons()
    End Sub

    Private Sub ctlTextEditor_UndoRedoEnabledUpdated(undoEnabled As Boolean, redoEnabled As Boolean) Handles ctlTextEditor.UndoRedoEnabledUpdated
        ctlToolbar.UndoButtonEnabled = undoEnabled
        ctlToolbar.RedoButtonEnabled = redoEnabled
        If undoEnabled Or redoEnabled Then m_unsavedChanges = True
    End Sub

    Private Sub m_controller_RequestAddElement(sender As Object, e As RequestAddElementEventArgs) Handles m_controller.RequestAddElement
        Select Case e.ElementType
            Case "object"
                Select Case e.ObjectType
                    Case "object"
                        AddNewObject()
                    Case "exit"
                        AddNewExit()
                    Case "command"
                        If e.Filter = "verb" Then
                            AddNewVerb()
                        Else
                            AddNewCommand()
                        End If
                    Case "turnscript"
                        AddNewTurnScript()
                    Case Else
                        Throw New ArgumentOutOfRangeException
                End Select
            Case "function"
                AddNewFunction()
            Case "timer"
                AddNewTimer()
            Case "walkthrough"
                AddNewWalkthrough()
            Case "include"
                AddNewLibrary()
            Case "implied"
                AddNewImpliedType()
            Case "template"
                AddNewTemplate()
            Case "dynamictemplate"
                AddNewDynamicTemplate()
            Case "delegate"
                AddNewDelegate()
            Case "type"
                AddNewObjectType()
            Case "editor"
                AddNewEditor()
            Case "javascript"
                AddNewJavascript()
            Case Else
                Throw New ArgumentOutOfRangeException
        End Select
    End Sub

    Private Sub m_controller_RequestEdit(sender As Object, e As RequestEditEventArgs) Handles m_controller.RequestEdit
        ctlTree.SetSelectedItem(e.Key)
    End Sub

    Private Property BannerVisible As Boolean
        Get
            Return ctlBanner.Visible
        End Get
        Set(value As Boolean)
            ctlBanner.Visible = value
            pnlHeader.Height = If(value, 41, 18)
        End Set
    End Property

    Private Sub ctlBanner_ButtonClicked() Handles ctlBanner.ButtonClicked
        Dim thisElement As String = m_currentElement
        m_controller.StartTransaction(String.Format(T("EditorCreateLocalCopy"), m_currentElement))
        m_currentEditorData.MakeElementLocal()
        m_controller.EndTransaction()

        ' Changing from library to non-library element (or vice-versa) will move the element in the tree,
        ' so re-select it
        ctlTree.SetSelectedItem(thisElement)
    End Sub

    Private Sub UpdateClipboardButtons()
        Dim canPaste As Boolean = m_codeView OrElse m_controller.CanPaste(ctlTree.SelectedItem)
        m_menu.MenuEnabled("paste") = canPaste
        ctlTree.SetMenuEnabled("paste", canPaste)
        ctlToolbar.CanPaste = canPaste

        Dim canCopy As Boolean = m_codeView OrElse m_controller.CanCopy(ctlTree.SelectedItem)
        m_menu.MenuEnabled("copy") = canCopy
        ctlTree.SetMenuEnabled("copy", canCopy)
        ctlToolbar.CanCopy = canCopy

        Dim canDelete As Boolean = (Not m_codeView) AndAlso m_controller.CanDelete(ctlTree.SelectedItem)
        m_menu.MenuEnabled("delete") = canDelete
        ctlTree.SetMenuEnabled("delete", canDelete)
        ctlToolbar.CanDelete = canDelete

        ' Cut works again. The object is not cut out until it is pasted again. (SoonGames) (prior notification: "Cut" is disabled - see Issue Tracker #1062)
        Dim canCut As Boolean = canCopy And canDelete
        m_menu.MenuEnabled("cut") = canCut
        ctlTree.SetMenuEnabled("cut", canCut)
        ctlToolbar.CanCut = canCut
    End Sub

    Public Sub SetWindowTitle()
        Dim title As String = System.IO.Path.GetFileName(m_filename)
        If m_codeView Then
            title += " [Code]"
        End If
        RaiseEvent Loaded(title)
    End Sub

    Private Sub CreateNew()
        RaiseEvent NewGame()
    End Sub

    Private Sub Open()
        RaiseEvent OpenGame()
    End Sub

    Public Function CheckGameIsSaved(prompt As String) As Boolean
        If (Not m_codeView And m_unsavedChanges) Or (m_codeView And ctlTextEditor.IsModified) Then
            Dim result As MsgBoxResult
            If prompt Is Nothing Then
                result = MsgBoxResult.Yes
            Else
                result = MsgBox(T("EditorUnsavedChanges") + Environment.NewLine + Environment.NewLine + prompt, MsgBoxStyle.YesNoCancel Or MsgBoxStyle.Exclamation)
            End If

            If result = MsgBoxResult.Yes Then
                Dim saveOk As Boolean = Save()
                If Not saveOk Then Return False
            ElseIf result = MsgBoxResult.Cancel Then
                Return False
            End If
        End If

        Return True
    End Function

    Public Sub CancelUnsavedChanges()
        m_unsavedChanges = False
    End Sub

    Private Sub LogBug()
        LaunchURL("https://github.com/textadventures/quest/issues")
    End Sub

    Private Sub Help()
        LaunchURL("http://docs.textadventures.co.uk/quest/")
    End Sub

    Private Sub LaunchURL(url As String)
        Try
            System.Diagnostics.Process.Start(url)
        Catch ex As Exception
            MsgBox(String.Format("Error launching {0}{1}{2}", url, Environment.NewLine + Environment.NewLine, ex.Message), MsgBoxStyle.Critical, "Quest")
        End Try
    End Sub

    Private Sub Publish()
        Dim frmPublish As New PublishWindow(m_filename, m_controller)
        frmPublish.ShowDialog()
    End Sub

    Private Sub m_fileWatcher_Changed(sender As Object, e As System.IO.FileSystemEventArgs) Handles m_fileWatcher.Changed
        BeginInvoke(Sub()
                        ctlReloadBanner.AlertText = String.Format(T("EditorModifiedOutside"), e.Name)
                        ctlReloadBanner.ButtonText = T("EditorReload")
                        ctlReloadBanner.Visible = True
                    End Sub)
    End Sub

    Private Sub ctlReloadBanner_ButtonClicked() Handles ctlReloadBanner.ButtonClicked
        Reload()
    End Sub

    Private Sub Reload()
        Dim lastSelection As String = Nothing
        If Not m_codeView Then
            lastSelection = ctlTree.SelectedItem
        End If

        Initialise(m_filename)

        If lastSelection IsNot Nothing Then
            ctlTree.TrySetSelectedItem(lastSelection)
        End If
    End Sub

    Private Sub Find()
        ctlTextEditor.Find()
    End Sub

    Private Sub Replace()
        ctlTextEditor.Replace()
    End Sub

    Private Sub FindClose()
        ctlTextEditor.FindClose()
    End Sub

    Private Sub m_controller_RequestRunWalkthrough(sender As Object, e As RequestRunWalkthroughEventArgs) Handles m_controller.RequestRunWalkthrough
        If Not CheckGameIsSaved(Nothing) Then Return
        RaiseEvent PlayWalkthrough(m_filename, e.Name, e.Record)
    End Sub

    Public Sub SetRecordedWalkthrough(name As String, steps As List(Of String))
        m_controller.RecordWalkthrough(name, steps)
    End Sub

    Private Sub ToggleSimpleMode()
        SimpleMode = Not m_menu.MenuChecked("simplemode")
    End Sub

    Public Property SimpleMode As Boolean
        Get
            Return m_simpleMode
        End Get
        Set(value As Boolean)
            If (value <> m_simpleMode) Then
                m_simpleMode = value
                m_controller.SimpleMode = value
                m_menu.MenuChecked("simplemode") = m_simpleMode
                ctlToolbar.SimpleMode = value

                SetMenuVisibility()
                SetTreeMenuVisibility()

                For Each editor As WPFElementEditor In m_elementEditors.Values
                    editor.SimpleMode = m_simpleMode
                Next

                TextAdventures.Utility.Registry.SaveSetting("Quest", "Settings", "EditorSimpleMode", If(m_simpleMode, 1, 0))
            End If
        End Set
    End Property

    Private Property EditorStyle As EditorStyle
        Get
            Return m_editorStyle
        End Get
        Set(value As EditorStyle)
            If (value <> m_editorStyle) Then
                m_editorStyle = value
                SetMenuVisibility()
                SetTreeMenuVisibility()
                ctlToolbar.EditorStyle = value
            End If
        End Set
    End Property

    Private Sub SetMenuVisibility()
        m_menu.MenuVisible("addpage") = (EditorStyle = EditorStyle.GameBook)
        m_menu.MenuVisible("addobject") = (EditorStyle = EditorStyle.TextAdventure)
        m_menu.MenuVisible("addroom") = (EditorStyle = EditorStyle.TextAdventure)
        m_menu.MenuVisible("addexit") = (EditorStyle = EditorStyle.TextAdventure)
        m_menu.MenuVisible("addverb") = (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode
        m_menu.MenuVisible("addcommand") = (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode
        m_menu.MenuVisible("addfunction") = Not SimpleMode
        m_menu.MenuVisible("addtimer") = (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode
        m_menu.MenuVisible("addturnscript") = (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode
        m_menu.MenuVisible("addwalkthrough") = (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode
        m_menu.MenuVisible("advanced") = (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode
        m_menu.MenuVisible("codeview") = Not SimpleMode
    End Sub

    Private Sub SetTreeMenuVisibility()
        ctlTree.ShowFilterBar = Not SimpleMode
        ctlTree.SetMenuVisible("addpage", EditorStyle = EditorStyle.GameBook)
        ctlTree.SetMenuVisible("addobject", EditorStyle = EditorStyle.TextAdventure)
        ctlTree.SetMenuVisible("addroom", EditorStyle = EditorStyle.TextAdventure)
        ctlTree.SetMenuVisible("addexit", EditorStyle = EditorStyle.TextAdventure)
        ctlTree.SetMenuVisible("addverb", (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode)
        ctlTree.SetMenuVisible("addcommand", (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode)
        ctlTree.SetMenuVisible("addfunction", Not SimpleMode)
        ctlTree.SetMenuVisible("addtimer", (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode)
        ctlTree.SetMenuVisible("addturnscript", (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode)
        ctlTree.SetMenuVisible("addwalkthrough", (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode)
        ctlTree.SetMenuVisible("addlibrary", Not SimpleMode)
        ctlTree.SetMenuVisible("addtemplate", (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode)
        ctlTree.SetMenuVisible("adddynamictemplate", (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode)
        ctlTree.SetMenuVisible("addobjecttype", (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode)
        ctlTree.SetMenuVisible("addjavascript", Not SimpleMode)
        ctlTree.SetMenuSeparatorVisible("separator1", True)
        ctlTree.SetMenuSeparatorVisible("separator2", True)
        ctlTree.SetMenuSeparatorVisible("separator3", (EditorStyle = EditorStyle.TextAdventure) And Not SimpleMode)
        ctlTree.SetMenuSeparatorVisible("separator4", ((EditorStyle = EditorStyle.GameBook) Or (EditorStyle = EditorStyle.TextAdventure)) And Not SimpleMode)
        ctlTree.SetMenuSeparatorVisible("separator5", ((EditorStyle = EditorStyle.GameBook) Or (EditorStyle = EditorStyle.TextAdventure)) And Not SimpleMode)
    End Sub

    Private Sub m_controller_LoadStatus(sender As Object, e As EditorController.LoadStatusEventArgs) Handles m_controller.LoadStatus
        BeginInvoke(Sub() ctlLoading.UpdateStatus(e.Status))
    End Sub

    Private Sub m_controller_LibrariesUpdated(sender As Object, e As EditorController.LibrariesUpdatedEventArgs) Handles m_controller.LibrariesUpdated
        BeginInvoke(Sub()
                        ctlReloadBanner.AlertText = T("EditorSaveGameAndClickReload")
                        ctlReloadBanner.Visible = True
                    End Sub)
    End Sub

    Private Sub SetWordWrap(turnOn As Boolean)
        m_menu.MenuChecked("wordwrap") = turnOn
        ctlTextEditor.WordWrap = turnOn
        TextAdventures.Utility.Registry.SaveSetting("Quest", "Settings", "EditorWordWrap", If(ctlTextEditor.WordWrap, 1, 0))
    End Sub

    Private Sub ToggleWordWrap()
        SetWordWrap(Not m_menu.MenuChecked("wordwrap"))
    End Sub

    Private Sub ctlTree_Load(sender As Object, e As EventArgs) Handles ctlTree.Load

    End Sub

    Private Sub StatusStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles StatusStrip1.ItemClicked

    End Sub

    Private Sub ctlReloadBanner_Load(sender As Object, e As EventArgs) Handles ctlReloadBanner.Load

    End Sub
End Class
