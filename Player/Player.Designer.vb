<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Player
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.splitMain = New System.Windows.Forms.SplitContainer()
        Me.ctlPlayerHtml = New AxeSoftware.Quest.PlayerHTML()
        Me.pnlLocation = New System.Windows.Forms.Panel()
        Me.cmdPanes = New System.Windows.Forms.Button()
        Me.lblBanner = New System.Windows.Forms.Label()
        Me.pnlCommand = New System.Windows.Forms.Panel()
        Me.cmdGo = New System.Windows.Forms.Button()
        Me.txtCommand = New System.Windows.Forms.TextBox()
        Me.lblCompass = New System.Windows.Forms.Label()
        Me.ctlCompass = New AxeSoftware.Quest.Compass()
        Me.splitPane = New System.Windows.Forms.SplitContainer()
        Me.lstInventory = New AxeSoftware.Quest.ElementList()
        Me.lstPlacesObjects = New AxeSoftware.Quest.ElementList()
        Me.tmrTimer = New System.Windows.Forms.Timer(Me.components)
        Me.ctlSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.tmrInitialise = New System.Windows.Forms.Timer(Me.components)
        Me.ctlOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.tmrTick = New System.Windows.Forms.Timer(Me.components)
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.Panel2.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.pnlLocation.SuspendLayout()
        Me.pnlCommand.SuspendLayout()
        CType(Me.splitPane, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitPane.Panel1.SuspendLayout()
        Me.splitPane.Panel2.SuspendLayout()
        Me.splitPane.SuspendLayout()
        Me.SuspendLayout()
        '
        'splitMain
        '
        Me.splitMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.splitMain.Location = New System.Drawing.Point(0, 0)
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        Me.splitMain.Panel1.Controls.Add(Me.ctlPlayerHtml)
        Me.splitMain.Panel1.Controls.Add(Me.pnlLocation)
        Me.splitMain.Panel1.Controls.Add(Me.pnlCommand)
        '
        'splitMain.Panel2
        '
        Me.splitMain.Panel2.Controls.Add(Me.lblCompass)
        Me.splitMain.Panel2.Controls.Add(Me.ctlCompass)
        Me.splitMain.Panel2.Controls.Add(Me.splitPane)
        Me.splitMain.Size = New System.Drawing.Size(695, 482)
        Me.splitMain.SplitterDistance = 510
        Me.splitMain.TabIndex = 1
        '
        'ctlPlayerHtml
        '
        Me.ctlPlayerHtml.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlPlayerHtml.Location = New System.Drawing.Point(0, 20)
        Me.ctlPlayerHtml.Margin = New System.Windows.Forms.Padding(0)
        Me.ctlPlayerHtml.Name = "ctlPlayerHtml"
        Me.ctlPlayerHtml.Size = New System.Drawing.Size(510, 442)
        Me.ctlPlayerHtml.TabIndex = 7
        '
        'pnlLocation
        '
        Me.pnlLocation.Controls.Add(Me.cmdPanes)
        Me.pnlLocation.Controls.Add(Me.lblBanner)
        Me.pnlLocation.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlLocation.Location = New System.Drawing.Point(0, 0)
        Me.pnlLocation.Name = "pnlLocation"
        Me.pnlLocation.Size = New System.Drawing.Size(510, 20)
        Me.pnlLocation.TabIndex = 8
        '
        'cmdPanes
        '
        Me.cmdPanes.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdPanes.Location = New System.Drawing.Point(486, 0)
        Me.cmdPanes.Name = "cmdPanes"
        Me.cmdPanes.Size = New System.Drawing.Size(24, 20)
        Me.cmdPanes.TabIndex = 7
        Me.cmdPanes.Text = ">"
        Me.cmdPanes.UseVisualStyleBackColor = True
        '
        'lblBanner
        '
        Me.lblBanner.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblBanner.BackColor = System.Drawing.SystemColors.ActiveCaption
        Me.lblBanner.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBanner.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.lblBanner.Location = New System.Drawing.Point(0, 0)
        Me.lblBanner.Margin = New System.Windows.Forms.Padding(0)
        Me.lblBanner.Name = "lblBanner"
        Me.lblBanner.Padding = New System.Windows.Forms.Padding(4, 2, 0, 2)
        Me.lblBanner.Size = New System.Drawing.Size(485, 20)
        Me.lblBanner.TabIndex = 6
        '
        'pnlCommand
        '
        Me.pnlCommand.Controls.Add(Me.cmdGo)
        Me.pnlCommand.Controls.Add(Me.txtCommand)
        Me.pnlCommand.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlCommand.Location = New System.Drawing.Point(0, 462)
        Me.pnlCommand.Name = "pnlCommand"
        Me.pnlCommand.Size = New System.Drawing.Size(510, 20)
        Me.pnlCommand.TabIndex = 9
        '
        'cmdGo
        '
        Me.cmdGo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdGo.Location = New System.Drawing.Point(486, 0)
        Me.cmdGo.Margin = New System.Windows.Forms.Padding(0)
        Me.cmdGo.Name = "cmdGo"
        Me.cmdGo.Size = New System.Drawing.Size(24, 21)
        Me.cmdGo.TabIndex = 5
        Me.cmdGo.UseVisualStyleBackColor = True
        '
        'txtCommand
        '
        Me.txtCommand.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtCommand.Location = New System.Drawing.Point(0, 0)
        Me.txtCommand.Margin = New System.Windows.Forms.Padding(1)
        Me.txtCommand.Name = "txtCommand"
        Me.txtCommand.Size = New System.Drawing.Size(485, 20)
        Me.txtCommand.TabIndex = 4
        '
        'lblCompass
        '
        Me.lblCompass.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblCompass.BackColor = System.Drawing.SystemColors.InactiveCaption
        Me.lblCompass.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCompass.ForeColor = System.Drawing.SystemColors.InactiveCaptionText
        Me.lblCompass.Location = New System.Drawing.Point(0, 364)
        Me.lblCompass.Margin = New System.Windows.Forms.Padding(0, 0, 0, 4)
        Me.lblCompass.Name = "lblCompass"
        Me.lblCompass.Padding = New System.Windows.Forms.Padding(4, 2, 0, 2)
        Me.lblCompass.Size = New System.Drawing.Size(181, 20)
        Me.lblCompass.TabIndex = 11
        Me.lblCompass.Text = "Compass"
        '
        'ctlCompass
        '
        Me.ctlCompass.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ctlCompass.Location = New System.Drawing.Point(0, 388)
        Me.ctlCompass.Margin = New System.Windows.Forms.Padding(0)
        Me.ctlCompass.Name = "ctlCompass"
        Me.ctlCompass.Size = New System.Drawing.Size(181, 94)
        Me.ctlCompass.TabIndex = 10
        '
        'splitPane
        '
        Me.splitPane.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.splitPane.Location = New System.Drawing.Point(0, 0)
        Me.splitPane.Name = "splitPane"
        Me.splitPane.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'splitPane.Panel1
        '
        Me.splitPane.Panel1.Controls.Add(Me.lstInventory)
        '
        'splitPane.Panel2
        '
        Me.splitPane.Panel2.Controls.Add(Me.lstPlacesObjects)
        Me.splitPane.Size = New System.Drawing.Size(181, 360)
        Me.splitPane.SplitterDistance = 128
        Me.splitPane.TabIndex = 0
        '
        'lstInventory
        '
        Me.lstInventory.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstInventory.Location = New System.Drawing.Point(0, 0)
        Me.lstInventory.Name = "lstInventory"
        Me.lstInventory.Size = New System.Drawing.Size(181, 128)
        Me.lstInventory.TabIndex = 0
        Me.lstInventory.Title = "Inventory"
        '
        'lstPlacesObjects
        '
        Me.lstPlacesObjects.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstPlacesObjects.Location = New System.Drawing.Point(0, 0)
        Me.lstPlacesObjects.Name = "lstPlacesObjects"
        Me.lstPlacesObjects.Size = New System.Drawing.Size(181, 228)
        Me.lstPlacesObjects.TabIndex = 0
        Me.lstPlacesObjects.Title = "Places and Objects"
        '
        'tmrTimer
        '
        Me.tmrTimer.Interval = 50
        '
        'tmrInitialise
        '
        Me.tmrInitialise.Interval = 50
        '
        'tmrTick
        '
        Me.tmrTick.Interval = 1000
        '
        'Player
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.splitMain)
        Me.Name = "Player"
        Me.Size = New System.Drawing.Size(695, 482)
        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.Panel2.ResumeLayout(False)
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.pnlLocation.ResumeLayout(False)
        Me.pnlCommand.ResumeLayout(False)
        Me.pnlCommand.PerformLayout()
        Me.splitPane.Panel1.ResumeLayout(False)
        Me.splitPane.Panel2.ResumeLayout(False)
        CType(Me.splitPane, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitPane.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents splitMain As System.Windows.Forms.SplitContainer
    Friend WithEvents splitPane As System.Windows.Forms.SplitContainer
    Friend WithEvents lblCompass As System.Windows.Forms.Label
    Friend WithEvents ctlCompass As Quest.Compass
    Friend WithEvents lstInventory As Quest.ElementList
    Friend WithEvents lstPlacesObjects As Quest.ElementList
    Friend WithEvents tmrTimer As System.Windows.Forms.Timer
    Friend WithEvents ctlSaveFile As System.Windows.Forms.SaveFileDialog
    Friend WithEvents tmrInitialise As System.Windows.Forms.Timer
    Friend WithEvents ctlOpenFile As System.Windows.Forms.OpenFileDialog
    Friend WithEvents tmrTick As System.Windows.Forms.Timer
    Friend WithEvents ctlPlayerHtml As AxeSoftware.Quest.PlayerHTML
    Friend WithEvents pnlCommand As System.Windows.Forms.Panel
    Friend WithEvents cmdGo As System.Windows.Forms.Button
    Friend WithEvents txtCommand As System.Windows.Forms.TextBox
    Friend WithEvents pnlLocation As System.Windows.Forms.Panel
    Friend WithEvents cmdPanes As System.Windows.Forms.Button
    Friend WithEvents lblBanner As System.Windows.Forms.Label

End Class
