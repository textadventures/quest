Imports AxeSoftware.Quest.EditorControls

Public Class Editor

    Private WithEvents m_controller As EditorController
    Private m_elementEditors As Dictionary(Of String, WPFElementEditor)
    Private m_currentEditor As WPFElementEditor
    Private m_menu As AxeSoftware.Quest.Controls.Menu
    Private m_filename As String
    Private m_currentElement As String
    Private m_codeView As Boolean
    Private m_lastSelection As String
    Private m_currentEditorData As IEditorDataExtendedAttributeInfo
    Private m_unsavedChanges As Boolean
    Private WithEvents m_fileWatcher As System.IO.FileSystemWatcher

    Public Event AddToRecent(filename As String, name As String)
    Public Event Close()
    Public Event Play(filename As String)
    Public Event Loaded(name As String)
    Public Event NewGame()
    Public Event OpenGame()

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        BannerVisible = False
    End Sub

    Public Function Initialise(ByRef filename As String) As Boolean
        m_currentElement = Nothing
        m_filename = filename
        m_controller = New EditorController()
        m_unsavedChanges = False
        InitialiseEditorControlsList()
        DisplayCodeView(False)
        ctlReloadBanner.Visible = False
        Dim ok As Boolean = m_controller.Initialise(filename)
        If ok Then
            Dim path As String = System.IO.Path.GetDirectoryName(filename)
            Dim filter As String = System.IO.Path.GetFileName(filename)
            m_fileWatcher = New System.IO.FileSystemWatcher(path, filter)
            m_fileWatcher.EnableRaisingEvents = True
            SetUpTree()
            SetUpToolbar()
            SetUpEditors()
            RaiseEvent AddToRecent(filename, m_controller.GameName)
            ctlTree.SetSelectedItem("game")
            ctlTree.FocusOnTree()
            SetWindowTitle()
            ShowEditor("game")
        End If

        Return ok
    End Function

    Private Sub InitialiseEditorControlsList()
        EditorControls.ElementEditor.InitialiseEditorControls(m_controller)
    End Sub

    Public Sub SetMenu(menu As AxeSoftware.Quest.Controls.Menu)
        m_menu = menu
        menu.AddMenuClickHandler("save", AddressOf Save)
        menu.AddMenuClickHandler("saveas", AddressOf SaveAs)
        menu.AddMenuClickHandler("undo", AddressOf Undo)
        menu.AddMenuClickHandler("redo", AddressOf Redo)
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
    End Sub

    Private Sub SetUpToolbar()
        ctlToolbar.ResetToolbar()
        ctlToolbar.AddButtonHandler("new", AddressOf CreateNew)
        ctlToolbar.AddButtonHandler("open", AddressOf Open)
        ctlToolbar.AddButtonHandler("save", AddressOf Save)
        ctlToolbar.AddButtonHandler("undo", AddressOf Undo)
        ctlToolbar.AddButtonHandler("redo", AddressOf Redo)
        ctlToolbar.AddButtonHandler("addobject", AddressOf AddNewObject)
        ctlToolbar.AddButtonHandler("addroom", AddressOf AddNewRoom)
        ctlToolbar.AddButtonHandler("play", AddressOf PlayGame)
        ctlToolbar.AddButtonHandler("cut", AddressOf Cut)
        ctlToolbar.AddButtonHandler("copy", AddressOf Copy)
        ctlToolbar.AddButtonHandler("paste", AddressOf Paste)
        ctlToolbar.AddButtonHandler("delete", AddressOf Delete)
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
    End Sub

    Private Sub SetUpEditors()
        m_elementEditors = New Dictionary(Of String, WPFElementEditor)

        For Each editor As String In m_controller.GetAllEditorNames()
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

    Private Sub m_controller_AddedNode(key As String, text As String, parent As String, isLibraryNode As Boolean, position As Integer?) Handles m_controller.AddedNode
        Dim foreColor As Color = If(isLibraryNode, Color.Gray, Color.Black)
        ctlTree.AddNode(key, text, parent, foreColor, Nothing)
    End Sub

    Private Sub m_controller_RemovedNode(key As String) Handles m_controller.RemovedNode
        ctlTree.RemoveNode(key)
    End Sub

    Private Sub m_controller_RenamedNode(oldName As String, newName As String) Handles m_controller.RenamedNode
        If m_currentElement = oldName Then
            m_currentElement = newName
            RefreshCurrentElement()
        End If
        ctlTree.RenameNode(oldName, newName)
        ctlToolbar.RenameHistory(oldName, newName)
    End Sub

    Private Sub m_controller_RetitledNode(key As String, newTitle As String) Handles m_controller.RetitledNode
        If (m_currentElement = key) Then
            lblHeader.Text = newTitle
        End If
        ctlTree.RetitleNode(key, newTitle)
        ctlToolbar.RetitleHistory(key, newTitle)
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

    Private Sub m_controller_ShowMessage(message As String) Handles m_controller.ShowMessage
        System.Windows.Forms.MessageBox.Show(message)
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
                ctlBanner.AlertText = String.Format("From library {0}", filename)
                ctlBanner.ButtonText = "Copy"
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
            MsgBox("Unable to save the file due to the following error:" + Environment.NewLine + Environment.NewLine + ex.Message, MsgBoxStyle.Critical)
            Return False
        Finally
            m_fileWatcher.EnableRaisingEvents = True
        End Try
    End Function

    Private Sub ctlToolbar_HistoryClicked(Key As String) Handles ctlToolbar.HistoryClicked
        ctlTree.SetSelectedItemNoEvent(Key)
        ShowEditor(Key)
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
        Dim result = PopupEditors.EditString(String.Format("Please enter a name for the new {0}", typeName), "")
        If result.Cancelled Then Return
        If Not ValidateInput(result.Result) Then Return

        action(result.Result)
        ctlTree.SetSelectedItem(result.Result)
    End Sub

    Private Sub AddNewObject()
        Dim possibleParents = m_controller.GetPossibleNewObjectParentsForCurrentSelection(ctlTree.SelectedItem)
        Dim result = GetNameAndParent("Please enter a name for the new object", possibleParents)

        If result Is Nothing Then Return

        m_controller.CreateNewObject(result.Value.Result, result.Value.ListResult)
        ctlTree.SetSelectedItem(result.Value.Result)
    End Sub

    Private Sub AddNewRoom()
        Dim result = PopupEditors.EditString("Please enter a name for the new room", "")
        If result.Cancelled Then Return
        If Not ValidateInput(result.Result) Then Return

        m_controller.CreateNewRoom(result.Result, Nothing)
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
        Dim result = GetNameAndParent("Please enter a name for the new walkthrough", possibleParents)

        If result Is Nothing Then Return

        m_controller.CreateNewWalkthrough(result.Value.Result, result.Value.ListResult)
        ctlTree.SetSelectedItem(result.Value.Result)
    End Sub

    Private Sub AddNewLibrary()
        Dim newLibrary = m_controller.CreateNewIncludedLibrary()
        ctlTree.SetSelectedItem(newLibrary)
    End Sub

    Private Sub AddNewTemplate()
        Dim result = PopupEditors.EditString("Please enter a name for the new template", "")
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

    Private Function GetNameAndParent(prompt As String, possibleParents As IEnumerable(Of String)) As PopupEditors.EditStringResult?
        Const noParent As String = "(none)"

        Dim result As PopupEditors.EditStringResult

        If possibleParents Is Nothing Then
            result = PopupEditors.EditString(prompt, "")
        Else
            Dim parentOptions As New List(Of String)
            parentOptions.Add(noParent)
            parentOptions.AddRange(possibleParents)

            result = PopupEditors.EditStringWithDropdown(prompt, "", "Parent", parentOptions, ctlTree.SelectedItem)
        End If

        If result.Cancelled Then Return Nothing
        If Not ValidateInput(result.Result) Then Return Nothing

        If possibleParents IsNot Nothing And result.ListResult = noParent Then
            result.ListResult = Nothing
        End If

        Return result
    End Function

    Private Function ValidateInput(input As String) As Boolean
        Dim result = m_controller.CanAdd(input)
        If result.Valid Then Return True

        PopupEditors.DisplayValidationError(result, input, "Unable to add element")
        Return False
    End Function

    Private Function ValidateInputTemplateName(input As String) As Boolean
        Dim result = m_controller.CanAddTemplate(input)
        If result.Valid Then Return True

        PopupEditors.DisplayValidationError(result, input, "Unable to add template")
        Return False
    End Function

    Public Function CreateNewGame() As String
        Dim templates As Dictionary(Of String, String) = EditorController.GetAvailableTemplates()
        Dim newGameWindow As New NewGameWindow
        newGameWindow.SetAvailableTemplates(templates)
        newGameWindow.ShowDialog()

        If newGameWindow.Cancelled Then Return Nothing

        Dim filename = newGameWindow.txtFilename.Text
        Dim folder = System.IO.Path.GetDirectoryName(filename)
        If Not System.IO.Directory.Exists(folder) Then
            System.IO.Directory.CreateDirectory(folder)
        End If

        EditorController.CreateNewGameFile(filename, templates(newGameWindow.lstTemplate.Text), newGameWindow.txtGameName.Text)

        Return filename
    End Function

    Private Sub PlayGame()
        If Not CheckGameIsSaved("Do you wish to save your changes before playing this game?") Then Return
        RaiseEvent Play(m_filename)
    End Sub

    Private Sub DoClose()
        CloseEditor(True)
    End Sub

    Public Function CloseEditor(raiseCloseEvent As Boolean) As Boolean
        If Not CheckGameIsSaved("Do you wish to save your changes before closing?") Then Return False

        If raiseCloseEvent Then RaiseEvent Close()

        Return True
    End Function

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
                               "Do you wish to save your changes before leaving the code view?",
                               "Do you wish to save your changes before editing this game in the code view?")

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
                Dim ok As Boolean = Initialise(m_filename)
                If Not ok Then
                    ' Couldn't reload the file due to an error, so show code view again
                    DisplayCodeView(True)
                Else
                    ctlTree.TrySetSelectedItem(m_lastSelection)
                End If
            Else
                SetWindowTitle()
            End If
        End If

        UpdateClipboardButtons()
    End Sub

    Private Sub DisplayCodeView(codeView As Boolean)
        m_codeView = codeView
        ctlToolbar.SetToggle("code", codeView)
        ctlTextEditor.Visible = codeView
        splitMain.Visible = Not codeView
        ctlToolbar.CodeView = codeView
        m_menu.MenuVisible("add") = Not codeView
        m_menu.MenuVisible("find") = codeView
    End Sub

    Public Sub Redisplay()
        DisplayCodeView(m_codeView)
    End Sub

    Private Sub ctlTextEditor_UndoRedoEnabledUpdated(undoEnabled As Boolean, redoEnabled As Boolean) Handles ctlTextEditor.UndoRedoEnabledUpdated
        ctlToolbar.UndoButtonEnabled = undoEnabled
        ctlToolbar.RedoButtonEnabled = redoEnabled
        If undoEnabled Or redoEnabled Then m_unsavedChanges = True
    End Sub

    Private Sub m_controller_RequestAddElement(elementType As String, objectType As String, filter As String) Handles m_controller.RequestAddElement
        Select Case elementType
            Case "object"
                Select Case objectType
                    Case "object"
                        AddNewObject()
                    Case "exit"
                        AddNewExit()
                    Case "command"
                        If filter = "verb" Then
                            AddNewVerb()
                        Else
                            AddNewCommand()
                        End If
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

    Private Sub m_controller_RequestEdit(key As String) Handles m_controller.RequestEdit
        ctlTree.SetSelectedItem(key)
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
        m_controller.StartTransaction(String.Format("Create local copy of '{0}'", m_currentElement))
        m_currentEditorData.MakeElementLocal()
        m_controller.EndTransaction()

        ' Changing from library to non-library element (or vice-versa) will move the element in the tree,
        ' so re-select it
        ctlTree.SetSelectedItem(thisElement)
    End Sub

    Private Sub UpdateClipboardButtons()
        Dim canPaste As Boolean = m_codeView OrElse m_controller.CanPaste(ctlTree.SelectedItem)
        m_menu.MenuEnabled("paste") = canPaste
        ctlToolbar.CanPaste = canPaste

        Dim canCutCopy As Boolean = m_codeView OrElse m_controller.CanCopy(ctlTree.SelectedItem)
        m_menu.MenuEnabled("cut") = canCutCopy
        m_menu.MenuEnabled("copy") = canCutCopy
        ctlToolbar.CanCutCopy = canCutCopy

        Dim canDelete As Boolean = (Not m_codeView) AndAlso m_controller.CanDelete(ctlTree.SelectedItem)
        m_menu.MenuEnabled("delete") = canDelete
        ctlToolbar.CanDelete = canDelete
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
            Dim result = MsgBox("You have unsaved changes." + Environment.NewLine + Environment.NewLine + prompt, MsgBoxStyle.YesNoCancel Or MsgBoxStyle.Exclamation, "Unsaved Changes")
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
        LaunchURL("http://quest.codeplex.com/workitem/list/basic")
    End Sub

    Private Sub Help()
        LaunchURL("http://quest5.net")
    End Sub

    Private Sub LaunchURL(url As String)
        Try
            System.Diagnostics.Process.Start(url)
        Catch ex As Exception
            MsgBox(String.Format("Error launching {0}{1}{2}", url, Environment.NewLine + Environment.NewLine, ex.Message), MsgBoxStyle.Critical, "Quest")
        End Try
    End Sub

    Private Sub Publish()
        Dim outputFolder As String = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(m_filename),
                "Output")

        System.IO.Directory.CreateDirectory(outputFolder)

        Dim outputFilename As String = System.IO.Path.Combine(
                outputFolder,
                System.IO.Path.GetFileNameWithoutExtension(m_filename) + ".quest")

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
                ctlPublishFile.FileName = outputFilename
                If ctlPublishFile.ShowDialog() = DialogResult.OK Then
                    outputFilename = ctlPublishFile.FileName
                Else
                    Return
                End If
            ElseIf deleteExisting = MsgBoxResult.Cancel Then
                Return
            End If
        End If

        Dim result = m_controller.Publish(outputFilename)

        If Not result.Valid Then
            EditorControls.PopupEditors.DisplayValidationError(result, String.Empty, "Unable to publish game")
        Else
            ' Show Output folder in a new Explorer window
            System.Diagnostics.Process.Start("explorer.exe", "/n," + outputFolder)
        End If

    End Sub

    Private Sub m_fileWatcher_Changed(sender As Object, e As System.IO.FileSystemEventArgs) Handles m_fileWatcher.Changed
        BeginInvoke(Sub() ctlReloadBanner.Visible = True)
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
End Class
