<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class EditBrowser
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
        Me.lblRecent = New System.Windows.Forms.Label()
        Me.ctlElementHost = New System.Windows.Forms.Integration.ElementHost()
        Me.EditorWelcome1 = New GameBrowser.EditorWelcome()
        Me.ctlGameList = New GameBrowser.GameList()
        CType(Me.ctlContainer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ctlContainer.Panel1.SuspendLayout()
        Me.ctlContainer.Panel2.SuspendLayout()
        Me.ctlContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlContainer
        '
        Me.ctlContainer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlContainer.Location = New System.Drawing.Point(0, 0)
        Me.ctlContainer.Name = "ctlContainer"
        '
        'ctlContainer.Panel1
        '
        Me.ctlContainer.Panel1.Controls.Add(Me.ctlElementHost)
        '
        'ctlContainer.Panel2
        '
        Me.ctlContainer.Panel2.Controls.Add(Me.ctlGameList)
        Me.ctlContainer.Panel2.Controls.Add(Me.lblRecent)
        Me.ctlContainer.Size = New System.Drawing.Size(600, 350)
        Me.ctlContainer.SplitterDistance = 300
        Me.ctlContainer.TabIndex = 1
        '
        'lblRecent
        '
        Me.lblRecent.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblRecent.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRecent.Location = New System.Drawing.Point(0, 0)
        Me.lblRecent.Name = "lblRecent"
        Me.lblRecent.Size = New System.Drawing.Size(296, 23)
        Me.lblRecent.TabIndex = 4
        Me.lblRecent.Text = "Recent"
        '
        'ctlElementHost
        '
        Me.ctlElementHost.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlElementHost.Location = New System.Drawing.Point(0, 0)
        Me.ctlElementHost.Name = "ctlElementHost"
        Me.ctlElementHost.Size = New System.Drawing.Size(300, 350)
        Me.ctlElementHost.TabIndex = 0
        Me.ctlElementHost.Text = "ElementHost1"
        Me.ctlElementHost.Child = Me.EditorWelcome1
        '
        'ctlGameList
        '
        Me.ctlGameList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlGameList.LaunchCaption = Nothing
        Me.ctlGameList.Location = New System.Drawing.Point(0, 23)
        Me.ctlGameList.Name = "ctlGameList"
        Me.ctlGameList.Size = New System.Drawing.Size(296, 327)
        Me.ctlGameList.TabIndex = 3
        '
        'EditBrowser
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.ctlContainer)
        Me.Name = "EditBrowser"
        Me.Size = New System.Drawing.Size(600, 350)
        Me.ctlContainer.Panel1.ResumeLayout(False)
        Me.ctlContainer.Panel2.ResumeLayout(False)
        CType(Me.ctlContainer, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ctlContainer.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlContainer As System.Windows.Forms.SplitContainer
    Friend WithEvents ctlGameList As GameBrowser.GameList
    Friend WithEvents lblRecent As System.Windows.Forms.Label
    Friend WithEvents ctlElementHost As System.Windows.Forms.Integration.ElementHost
    Friend EditorWelcome1 As GameBrowser.EditorWelcome

End Class
