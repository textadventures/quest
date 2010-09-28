<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class GameList
    Inherits System.Windows.Forms.UserControl

    'UserControl1 overrides dispose to clean up the component list.
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
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog
        Me.ctlTableLayout = New System.Windows.Forms.TableLayoutPanel
        Me.SuspendLayout()
        '
        'ctlTableLayout
        '
        Me.ctlTableLayout.AutoScroll = True
        Me.ctlTableLayout.BackColor = System.Drawing.Color.White
        Me.ctlTableLayout.ColumnCount = 1
        Me.ctlTableLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.ctlTableLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlTableLayout.Location = New System.Drawing.Point(0, 0)
        Me.ctlTableLayout.Name = "ctlTableLayout"
        Me.ctlTableLayout.RowCount = 1
        Me.ctlTableLayout.RowStyles.Add(New System.Windows.Forms.RowStyle)
        Me.ctlTableLayout.Size = New System.Drawing.Size(316, 260)
        Me.ctlTableLayout.TabIndex = 0
        '
        'GameList
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlTableLayout)
        Me.Name = "GameList"
        Me.Size = New System.Drawing.Size(316, 260)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents dlgOpenFile As System.Windows.Forms.OpenFileDialog
    Friend WithEvents ctlTableLayout As System.Windows.Forms.TableLayoutPanel

End Class
