<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class PlayBrowser
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
        Me.ctlContainer = New System.Windows.Forms.SplitContainer()
        Me.ctlOnlineGameList = New GameBrowser.GameList()
        Me.ctlBrowseFilter = New GameBrowser.BrowseFilter()
        Me.lblBrowseTitle = New System.Windows.Forms.Label()
        Me.ctlGameList = New GameBrowser.GameList()
        Me.Label1 = New System.Windows.Forms.Label()
        CType(Me.ctlContainer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ctlContainer.Panel1.SuspendLayout()
        Me.ctlContainer.Panel2.SuspendLayout()
        Me.ctlContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlContainer
        '
        Me.ctlContainer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlContainer.IsSplitterFixed = True
        Me.ctlContainer.Location = New System.Drawing.Point(0, 0)
        Me.ctlContainer.Margin = New System.Windows.Forms.Padding(0)
        Me.ctlContainer.Name = "ctlContainer"
        '
        'ctlContainer.Panel1
        '
        Me.ctlContainer.Panel1.Controls.Add(Me.ctlOnlineGameList)
        Me.ctlContainer.Panel1.Controls.Add(Me.ctlBrowseFilter)
        Me.ctlContainer.Panel1.Controls.Add(Me.lblBrowseTitle)
        '
        'ctlContainer.Panel2
        '
        Me.ctlContainer.Panel2.Controls.Add(Me.ctlGameList)
        Me.ctlContainer.Panel2.Controls.Add(Me.Label1)
        Me.ctlContainer.Size = New System.Drawing.Size(600, 400)
        Me.ctlContainer.SplitterDistance = 300
        Me.ctlContainer.TabIndex = 0
        '
        'ctlOnlineGameList
        '
        Me.ctlOnlineGameList.BackColor = System.Drawing.Color.White
        Me.ctlOnlineGameList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlOnlineGameList.LaunchCaption = Nothing
        Me.ctlOnlineGameList.Location = New System.Drawing.Point(0, 45)
        Me.ctlOnlineGameList.Name = "ctlOnlineGameList"
        Me.ctlOnlineGameList.Size = New System.Drawing.Size(300, 355)
        Me.ctlOnlineGameList.TabIndex = 5
        '
        'ctlBrowseFilter
        '
        Me.ctlBrowseFilter.Dock = System.Windows.Forms.DockStyle.Top
        Me.ctlBrowseFilter.Location = New System.Drawing.Point(0, 23)
        Me.ctlBrowseFilter.Name = "ctlBrowseFilter"
        Me.ctlBrowseFilter.Size = New System.Drawing.Size(300, 22)
        Me.ctlBrowseFilter.TabIndex = 6
        '
        'lblBrowseTitle
        '
        Me.lblBrowseTitle.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblBrowseTitle.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblBrowseTitle.Location = New System.Drawing.Point(0, 0)
        Me.lblBrowseTitle.Name = "lblBrowseTitle"
        Me.lblBrowseTitle.Size = New System.Drawing.Size(300, 23)
        Me.lblBrowseTitle.TabIndex = 4
        Me.lblBrowseTitle.Text = "Title"
        '
        'ctlGameList
        '
        Me.ctlGameList.BackColor = System.Drawing.Color.White
        Me.ctlGameList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlGameList.LaunchCaption = Nothing
        Me.ctlGameList.Location = New System.Drawing.Point(0, 23)
        Me.ctlGameList.Name = "ctlGameList"
        Me.ctlGameList.Size = New System.Drawing.Size(296, 377)
        Me.ctlGameList.TabIndex = 2
        '
        'Label1
        '
        Me.Label1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(0, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(296, 23)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "Recent"
        '
        'PlayBrowser
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlContainer)
        Me.Name = "PlayBrowser"
        Me.Size = New System.Drawing.Size(600, 400)
        Me.ctlContainer.Panel1.ResumeLayout(False)
        Me.ctlContainer.Panel2.ResumeLayout(False)
        CType(Me.ctlContainer, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ctlContainer.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlContainer As System.Windows.Forms.SplitContainer
    Friend WithEvents ctlGameList As GameBrowser.GameList
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ctlOnlineGameList As GameBrowser.GameList
    Friend WithEvents lblBrowseTitle As System.Windows.Forms.Label
    Friend WithEvents ctlBrowseFilter As GameBrowser.BrowseFilter

End Class
