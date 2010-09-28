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
        Me.ctlContainer = New System.Windows.Forms.SplitContainer
        Me.ctlGameList = New GameBrowser.GameList
        Me.BetaInfo1 = New GameBrowser.BetaInfo
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
        Me.ctlContainer.Panel1.Controls.Add(Me.ctlGameList)
        '
        'ctlContainer.Panel2
        '
        Me.ctlContainer.Panel2.Controls.Add(Me.BetaInfo1)
        Me.ctlContainer.Size = New System.Drawing.Size(600, 400)
        Me.ctlContainer.SplitterDistance = 300
        Me.ctlContainer.TabIndex = 0
        '
        'ctlGameList
        '
        Me.ctlGameList.BackColor = System.Drawing.Color.White
        Me.ctlGameList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlGameList.Location = New System.Drawing.Point(0, 0)
        Me.ctlGameList.Name = "ctlGameList"
        Me.ctlGameList.Size = New System.Drawing.Size(300, 400)
        Me.ctlGameList.TabIndex = 0
        '
        'BetaInfo1
        '
        Me.BetaInfo1.BackColor = System.Drawing.Color.White
        Me.BetaInfo1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.BetaInfo1.Location = New System.Drawing.Point(0, 0)
        Me.BetaInfo1.Name = "BetaInfo1"
        Me.BetaInfo1.Size = New System.Drawing.Size(296, 400)
        Me.BetaInfo1.TabIndex = 0
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
        Me.ctlContainer.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlContainer As System.Windows.Forms.SplitContainer
    Friend WithEvents ctlGameList As GameBrowser.GameList
    Friend WithEvents BetaInfo1 As GameBrowser.BetaInfo

End Class
