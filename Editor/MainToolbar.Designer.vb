Option Strict On
Imports System.Windows.Forms
Imports System.ComponentModel

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainToolbar
    Inherits System.Windows.Forms.UserControl

    'InteropUserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainToolbar))
        Me.ctlToolStrip = New System.Windows.Forms.ToolStrip()
        Me.butNew = New System.Windows.Forms.ToolStripButton()
        Me.butOpen = New System.Windows.Forms.ToolStripButton()
        Me.butSave = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.butCut = New System.Windows.Forms.ToolStripButton()
        Me.butCopy = New System.Windows.Forms.ToolStripButton()
        Me.butPaste = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator4 = New System.Windows.Forms.ToolStripSeparator()
        Me.butUndoSimple = New System.Windows.Forms.ToolStripButton()
        Me.butRedoSimple = New System.Windows.Forms.ToolStripButton()
        Me.butUndo = New System.Windows.Forms.ToolStripSplitButton()
        Me.butRedo = New System.Windows.Forms.ToolStripSplitButton()
        Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator()
        Me.butAddPage = New System.Windows.Forms.ToolStripButton()
        Me.butAddRoom = New System.Windows.Forms.ToolStripButton()
        Me.butAddObject = New System.Windows.Forms.ToolStripButton()
        Me.butDelete = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.butBack = New System.Windows.Forms.ToolStripSplitButton()
        Me.butForward = New System.Windows.Forms.ToolStripSplitButton()
        Me.ToolStripSeparator8 = New System.Windows.Forms.ToolStripSeparator()
        Me.butPlay = New System.Windows.Forms.ToolStripButton()
        Me.butCode = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator5 = New System.Windows.Forms.ToolStripSeparator()
        Me.butHelp = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator6 = New System.Windows.Forms.ToolStripSeparator()
        Me.butLogError = New System.Windows.Forms.ToolStripButton()
        Me.tmrUndoTimer = New System.Windows.Forms.Timer(Me.components)
        Me.ctlToolStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlToolStrip
        '
        Me.ctlToolStrip.Dock = System.Windows.Forms.DockStyle.None
        Me.ctlToolStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.butNew, Me.butOpen, Me.butSave, Me.ToolStripSeparator1, Me.butCut, Me.butCopy, Me.butPaste, Me.ToolStripSeparator4, Me.butUndoSimple, Me.butRedoSimple, Me.butUndo, Me.butRedo, Me.ToolStripSeparator3, Me.butAddPage, Me.butAddRoom, Me.butAddObject, Me.butDelete, Me.ToolStripSeparator2, Me.butBack, Me.butForward, Me.ToolStripSeparator8, Me.butPlay, Me.butCode, Me.ToolStripSeparator5, Me.butHelp, Me.ToolStripSeparator6, Me.butLogError})
        Me.ctlToolStrip.Location = New System.Drawing.Point(0, 0)
        Me.ctlToolStrip.Name = "ctlToolStrip"
        Me.ctlToolStrip.Size = New System.Drawing.Size(886, 25)
        Me.ctlToolStrip.TabIndex = 0
        Me.ctlToolStrip.Text = "ToolStrip1"
        '
        'butNew
        '
        Me.butNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butNew.Image = CType(resources.GetObject("butNew.Image"), System.Drawing.Image)
        Me.butNew.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butNew.Name = "butNew"
        Me.butNew.Size = New System.Drawing.Size(23, 22)
        Me.butNew.Tag = "new"
        Me.butNew.Text = "&New"
        '
        'butOpen
        '
        Me.butOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butOpen.Image = CType(resources.GetObject("butOpen.Image"), System.Drawing.Image)
        Me.butOpen.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butOpen.Name = "butOpen"
        Me.butOpen.Size = New System.Drawing.Size(23, 22)
        Me.butOpen.Tag = "open"
        Me.butOpen.Text = "&Open"
        '
        'butSave
        '
        Me.butSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butSave.Image = CType(resources.GetObject("butSave.Image"), System.Drawing.Image)
        Me.butSave.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butSave.Name = "butSave"
        Me.butSave.Size = New System.Drawing.Size(23, 22)
        Me.butSave.Tag = "save"
        Me.butSave.Text = "&Save"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 25)
        '
        'butCut
        '
        Me.butCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butCut.Image = CType(resources.GetObject("butCut.Image"), System.Drawing.Image)
        Me.butCut.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butCut.Name = "butCut"
        Me.butCut.Size = New System.Drawing.Size(23, 22)
        Me.butCut.Tag = "cut"
        Me.butCut.Text = "C&ut"
        Me.butCut.Visible = False
        '
        'butCopy
        '
        Me.butCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butCopy.Image = CType(resources.GetObject("butCopy.Image"), System.Drawing.Image)
        Me.butCopy.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butCopy.Name = "butCopy"
        Me.butCopy.Size = New System.Drawing.Size(23, 22)
        Me.butCopy.Tag = "copy"
        Me.butCopy.Text = "&Copy"
        '
        'butPaste
        '
        Me.butPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butPaste.Image = CType(resources.GetObject("butPaste.Image"), System.Drawing.Image)
        Me.butPaste.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butPaste.Name = "butPaste"
        Me.butPaste.Size = New System.Drawing.Size(23, 22)
        Me.butPaste.Tag = "paste"
        Me.butPaste.Text = "&Paste"
        '
        'ToolStripSeparator4
        '
        Me.ToolStripSeparator4.Name = "ToolStripSeparator4"
        Me.ToolStripSeparator4.Size = New System.Drawing.Size(6, 25)
        '
        'butUndoSimple
        '
        Me.butUndoSimple.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butUndoSimple.Image = CType(resources.GetObject("butUndoSimple.Image"), System.Drawing.Image)
        Me.butUndoSimple.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butUndoSimple.Name = "butUndoSimple"
        Me.butUndoSimple.Size = New System.Drawing.Size(23, 22)
        Me.butUndoSimple.Tag = "undo"
        Me.butUndoSimple.ToolTipText = "Undo"
        '
        'butRedoSimple
        '
        Me.butRedoSimple.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butRedoSimple.Image = CType(resources.GetObject("butRedoSimple.Image"), System.Drawing.Image)
        Me.butRedoSimple.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butRedoSimple.Name = "butRedoSimple"
        Me.butRedoSimple.Size = New System.Drawing.Size(23, 22)
        Me.butRedoSimple.Tag = "redo"
        Me.butRedoSimple.ToolTipText = "Redo"
        '
        'butUndo
        '
        Me.butUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butUndo.Image = CType(resources.GetObject("butUndo.Image"), System.Drawing.Image)
        Me.butUndo.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butUndo.Name = "butUndo"
        Me.butUndo.Size = New System.Drawing.Size(32, 22)
        Me.butUndo.Tag = ""
        Me.butUndo.Text = "Undo"
        '
        'butRedo
        '
        Me.butRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butRedo.Image = CType(resources.GetObject("butRedo.Image"), System.Drawing.Image)
        Me.butRedo.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butRedo.Name = "butRedo"
        Me.butRedo.Size = New System.Drawing.Size(32, 22)
        Me.butRedo.Tag = ""
        Me.butRedo.Text = "Redo"
        '
        'ToolStripSeparator3
        '
        Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
        Me.ToolStripSeparator3.Size = New System.Drawing.Size(6, 25)
        '
        'butAddPage
        '
        Me.butAddPage.Image = CType(resources.GetObject("butAddPage.Image"), System.Drawing.Image)
        Me.butAddPage.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butAddPage.Name = "butAddPage"
        Me.butAddPage.Size = New System.Drawing.Size(78, 22)
        Me.butAddPage.Tag = "addpage"
        Me.butAddPage.Text = "Add Page"
        Me.butAddPage.ToolTipText = "Add Page"
        '
        'butAddRoom
        '
        Me.butAddRoom.Image = CType(resources.GetObject("butAddRoom.Image"), System.Drawing.Image)
        Me.butAddRoom.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None
        Me.butAddRoom.ImageTransparentColor = System.Drawing.Color.Silver
        Me.butAddRoom.Name = "butAddRoom"
        Me.butAddRoom.Size = New System.Drawing.Size(59, 22)
        Me.butAddRoom.Tag = "addroom"
        Me.butAddRoom.Text = "Room"
        Me.butAddRoom.ToolTipText = "Add Room"
        '
        'butAddObject
        '
        Me.butAddObject.Image = CType(resources.GetObject("butAddObject.Image"), System.Drawing.Image)
        Me.butAddObject.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None
        Me.butAddObject.ImageTransparentColor = System.Drawing.Color.Silver
        Me.butAddObject.Name = "butAddObject"
        Me.butAddObject.Size = New System.Drawing.Size(62, 22)
        Me.butAddObject.Tag = "addobject"
        Me.butAddObject.Text = "Object"
        Me.butAddObject.ToolTipText = "Add Object"
        '
        'butDelete
        '
        Me.butDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butDelete.Image = CType(resources.GetObject("butDelete.Image"), System.Drawing.Image)
        Me.butDelete.ImageTransparentColor = System.Drawing.Color.Silver
        Me.butDelete.Name = "butDelete"
        Me.butDelete.Size = New System.Drawing.Size(23, 22)
        Me.butDelete.Tag = "delete"
        Me.butDelete.Text = "ToolStripButton6"
        Me.butDelete.ToolTipText = "Delete"
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        Me.ToolStripSeparator2.Size = New System.Drawing.Size(6, 25)
        '
        'butBack
        '
        Me.butBack.Image = CType(resources.GetObject("butBack.Image"), System.Drawing.Image)
        Me.butBack.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butBack.Name = "butBack"
        Me.butBack.Size = New System.Drawing.Size(64, 22)
        Me.butBack.Text = "Back"
        '
        'butForward
        '
        Me.butForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butForward.Image = CType(resources.GetObject("butForward.Image"), System.Drawing.Image)
        Me.butForward.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butForward.Name = "butForward"
        Me.butForward.Size = New System.Drawing.Size(32, 22)
        Me.butForward.Text = "Forward"
        '
        'ToolStripSeparator8
        '
        Me.ToolStripSeparator8.Name = "ToolStripSeparator8"
        Me.ToolStripSeparator8.Size = New System.Drawing.Size(6, 25)
        '
        'butPlay
        '
        Me.butPlay.Image = CType(resources.GetObject("butPlay.Image"), System.Drawing.Image)
        Me.butPlay.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butPlay.Name = "butPlay"
        Me.butPlay.Size = New System.Drawing.Size(49, 22)
        Me.butPlay.Tag = "play"
        Me.butPlay.Text = "Play"
        '
        'butCode
        '
        Me.butCode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butCode.Image = CType(resources.GetObject("butCode.Image"), System.Drawing.Image)
        Me.butCode.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butCode.Name = "butCode"
        Me.butCode.Size = New System.Drawing.Size(23, 22)
        Me.butCode.Tag = "code"
        Me.butCode.ToolTipText = "Code View"
        '
        'ToolStripSeparator5
        '
        Me.ToolStripSeparator5.Name = "ToolStripSeparator5"
        Me.ToolStripSeparator5.Size = New System.Drawing.Size(6, 25)
        '
        'butHelp
        '
        Me.butHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butHelp.Image = CType(resources.GetObject("butHelp.Image"), System.Drawing.Image)
        Me.butHelp.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butHelp.Name = "butHelp"
        Me.butHelp.Size = New System.Drawing.Size(23, 22)
        Me.butHelp.Tag = "help"
        Me.butHelp.Text = "He&lp"
        '
        'ToolStripSeparator6
        '
        Me.ToolStripSeparator6.Name = "ToolStripSeparator6"
        Me.ToolStripSeparator6.Size = New System.Drawing.Size(6, 25)
        Me.ToolStripSeparator6.Visible = False
        '
        'butLogError
        '
        Me.butLogError.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.butLogError.Image = CType(resources.GetObject("butLogError.Image"), System.Drawing.Image)
        Me.butLogError.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butLogError.Name = "butLogError"
        Me.butLogError.Size = New System.Drawing.Size(140, 22)
        Me.butLogError.Tag = "logbug"
        Me.butLogError.Text = "Log a Bug or Suggestion"
        Me.butLogError.Visible = False
        '
        'tmrUndoTimer
        '
        Me.tmrUndoTimer.Interval = 20
        '
        'MainToolbar
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlToolStrip)
        Me.Name = "MainToolbar"
        Me.Size = New System.Drawing.Size(776, 49)
        Me.ctlToolStrip.ResumeLayout(False)
        Me.ctlToolStrip.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ctlToolStrip As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents butAddRoom As System.Windows.Forms.ToolStripButton
    Friend WithEvents butAddObject As System.Windows.Forms.ToolStripButton
    Friend WithEvents butDelete As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents butNew As System.Windows.Forms.ToolStripButton
    Friend WithEvents butOpen As System.Windows.Forms.ToolStripButton
    Friend WithEvents butSave As System.Windows.Forms.ToolStripButton
    Friend WithEvents butCut As System.Windows.Forms.ToolStripButton
    Friend WithEvents butCopy As System.Windows.Forms.ToolStripButton
    Friend WithEvents butPaste As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator8 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents butHelp As System.Windows.Forms.ToolStripButton
    Friend WithEvents butBack As System.Windows.Forms.ToolStripSplitButton
    Friend WithEvents ToolStripSeparator4 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents butForward As System.Windows.Forms.ToolStripSplitButton
    Friend WithEvents butUndo As System.Windows.Forms.ToolStripSplitButton
    Friend WithEvents butRedo As System.Windows.Forms.ToolStripSplitButton
    Friend WithEvents ToolStripSeparator3 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents tmrUndoTimer As System.Windows.Forms.Timer
    Friend WithEvents butPlay As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator5 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents butCode As System.Windows.Forms.ToolStripButton
    Friend WithEvents butUndoSimple As System.Windows.Forms.ToolStripButton
    Friend WithEvents butRedoSimple As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator6 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents butLogError As System.Windows.Forms.ToolStripButton
    Friend WithEvents butAddPage As System.Windows.Forms.ToolStripButton

End Class
