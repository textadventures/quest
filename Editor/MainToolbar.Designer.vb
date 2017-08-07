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
        resources.ApplyResources(Me.ctlToolStrip, "ctlToolStrip")
        Me.ctlToolStrip.BackColor = System.Drawing.Color.Transparent
        Me.ctlToolStrip.ImageScalingSize = New System.Drawing.Size(32, 32)
        Me.ctlToolStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.butNew, Me.butOpen, Me.butSave, Me.ToolStripSeparator1, Me.butCut, Me.butCopy, Me.butPaste, Me.ToolStripSeparator4, Me.butUndoSimple, Me.butRedoSimple, Me.butUndo, Me.butRedo, Me.ToolStripSeparator3, Me.butAddPage, Me.butAddRoom, Me.butAddObject, Me.butDelete, Me.ToolStripSeparator2, Me.butBack, Me.butForward, Me.ToolStripSeparator8, Me.butPlay, Me.butCode, Me.ToolStripSeparator5, Me.butHelp, Me.ToolStripSeparator6, Me.butLogError})
        Me.ctlToolStrip.Name = "ctlToolStrip"
        '
        'butNew
        '
        resources.ApplyResources(Me.butNew, "butNew")
        Me.butNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butNew.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Datei_32
        Me.butNew.Name = "butNew"
        Me.butNew.Tag = "new"
        '
        'butOpen
        '
        resources.ApplyResources(Me.butOpen, "butOpen")
        Me.butOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butOpen.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Ordner_öffnen_32
        Me.butOpen.Name = "butOpen"
        Me.butOpen.Tag = "open"
        '
        'butSave
        '
        resources.ApplyResources(Me.butSave, "butSave")
        Me.butSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butSave.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Speichern_32
        Me.butSave.Name = "butSave"
        Me.butSave.Tag = "save"
        '
        'ToolStripSeparator1
        '
        resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        '
        'butCut
        '
        resources.ApplyResources(Me.butCut, "butCut")
        Me.butCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butCut.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Ausschneiden_32
        Me.butCut.Name = "butCut"
        Me.butCut.Tag = "cut"
        '
        'butCopy
        '
        resources.ApplyResources(Me.butCopy, "butCopy")
        Me.butCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butCopy.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Kopieren_32
        Me.butCopy.Name = "butCopy"
        Me.butCopy.Tag = "copy"
        '
        'butPaste
        '
        resources.ApplyResources(Me.butPaste, "butPaste")
        Me.butPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butPaste.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Paste_32
        Me.butPaste.Name = "butPaste"
        Me.butPaste.Tag = "paste"
        '
        'ToolStripSeparator4
        '
        resources.ApplyResources(Me.ToolStripSeparator4, "ToolStripSeparator4")
        Me.ToolStripSeparator4.Name = "ToolStripSeparator4"
        '
        'butUndoSimple
        '
        resources.ApplyResources(Me.butUndoSimple, "butUndoSimple")
        Me.butUndoSimple.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butUndoSimple.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Rückgängig_machen_32
        Me.butUndoSimple.Name = "butUndoSimple"
        Me.butUndoSimple.Tag = "undo"
        '
        'butRedoSimple
        '
        resources.ApplyResources(Me.butRedoSimple, "butRedoSimple")
        Me.butRedoSimple.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butRedoSimple.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Wiederholen_32
        Me.butRedoSimple.Name = "butRedoSimple"
        Me.butRedoSimple.Tag = "redo"
        '
        'butUndo
        '
        resources.ApplyResources(Me.butUndo, "butUndo")
        Me.butUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butUndo.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Rückgängig_machen_32
        Me.butUndo.Name = "butUndo"
        Me.butUndo.Tag = ""
        '
        'butRedo
        '
        resources.ApplyResources(Me.butRedo, "butRedo")
        Me.butRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butRedo.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Wiederholen_32
        Me.butRedo.Name = "butRedo"
        Me.butRedo.Tag = ""
        '
        'ToolStripSeparator3
        '
        resources.ApplyResources(Me.ToolStripSeparator3, "ToolStripSeparator3")
        Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
        '
        'butAddPage
        '
        resources.ApplyResources(Me.butAddPage, "butAddPage")
        Me.butAddPage.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Datei_hinzufügen_32
        Me.butAddPage.Name = "butAddPage"
        Me.butAddPage.Tag = "addpage"
        '
        'butAddRoom
        '
        resources.ApplyResources(Me.butAddRoom, "butAddRoom")
        Me.butAddRoom.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Zimmer_32
        Me.butAddRoom.Name = "butAddRoom"
        Me.butAddRoom.Tag = "addroom"
        '
        'butAddObject
        '
        resources.ApplyResources(Me.butAddObject, "butAddObject")
        Me.butAddObject.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Topfpflanze_32
        Me.butAddObject.Name = "butAddObject"
        Me.butAddObject.Tag = "addobject"
        '
        'butDelete
        '
        resources.ApplyResources(Me.butDelete, "butDelete")
        Me.butDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butDelete.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Löschen_32
        Me.butDelete.Name = "butDelete"
        Me.butDelete.Tag = "delete"
        '
        'ToolStripSeparator2
        '
        resources.ApplyResources(Me.ToolStripSeparator2, "ToolStripSeparator2")
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        '
        'butBack
        '
        resources.ApplyResources(Me.butBack, "butBack")
        Me.butBack.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Zurückspulen_32
        Me.butBack.Name = "butBack"
        '
        'butForward
        '
        resources.ApplyResources(Me.butForward, "butForward")
        Me.butForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butForward.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Schnellvorlauf_32
        Me.butForward.Name = "butForward"
        '
        'ToolStripSeparator8
        '
        resources.ApplyResources(Me.ToolStripSeparator8, "ToolStripSeparator8")
        Me.ToolStripSeparator8.Name = "ToolStripSeparator8"
        '
        'butPlay
        '
        resources.ApplyResources(Me.butPlay, "butPlay")
        Me.butPlay.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Spielen_32
        Me.butPlay.Name = "butPlay"
        Me.butPlay.Tag = "play"
        '
        'butCode
        '
        resources.ApplyResources(Me.butCode, "butCode")
        Me.butCode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butCode.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Quellcode_32
        Me.butCode.Name = "butCode"
        Me.butCode.Tag = "code"
        '
        'ToolStripSeparator5
        '
        resources.ApplyResources(Me.ToolStripSeparator5, "ToolStripSeparator5")
        Me.ToolStripSeparator5.Name = "ToolStripSeparator5"
        '
        'butHelp
        '
        resources.ApplyResources(Me.butHelp, "butHelp")
        Me.butHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.butHelp.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Hilfe_32
        Me.butHelp.Name = "butHelp"
        Me.butHelp.Tag = "help"
        '
        'ToolStripSeparator6
        '
        resources.ApplyResources(Me.ToolStripSeparator6, "ToolStripSeparator6")
        Me.ToolStripSeparator6.Name = "ToolStripSeparator6"
        '
        'butLogError
        '
        resources.ApplyResources(Me.butLogError, "butLogError")
        Me.butLogError.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.butLogError.Name = "butLogError"
        Me.butLogError.Tag = "logbug"
        '
        'tmrUndoTimer
        '
        Me.tmrUndoTimer.Interval = 20
        '
        'MainToolbar
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Transparent
        Me.Controls.Add(Me.ctlToolStrip)
        Me.Name = "MainToolbar"
        Me.ctlToolStrip.ResumeLayout(False)
        Me.ctlToolStrip.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ctlToolStrip As System.Windows.Forms.ToolStrip
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
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
End Class
