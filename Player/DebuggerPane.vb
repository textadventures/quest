Imports AxeSoftware.Quest

Public Class DebuggerPane

    Private WithEvents m_game As IASLDebug
    Private m_filter As String
    Private m_objectsSorter As New ListViewColumnSorter
    Private m_attributesSorter As New ListViewColumnSorter

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        lstObjects.ListViewItemSorter = m_objectsSorter
        lstAttributes.ListViewItemSorter = m_attributesSorter
    End Sub

    Public Property Game() As IASLDebug
        Get
            Return m_game
        End Get
        Set(value As IASLDebug)
            m_game = value
            UpdateObjectList()
        End Set
    End Property

    Public Sub UpdateObjectList()
        lstObjects.Items.Clear()
        For Each obj As String In m_game.GetObjects(m_filter)
            lstObjects.Items.Add(obj)
        Next
    End Sub

    Private Sub lstObjects_ColumnClick(sender As Object, e As System.Windows.Forms.ColumnClickEventArgs) Handles lstObjects.ColumnClick
        SortList(lstObjects, m_objectsSorter, e.Column)
    End Sub

    Private Sub lstObjects_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles lstObjects.SelectedIndexChanged
        UpdateAttributes()
    End Sub

    Private Sub SortList(listView As ListView, columnSorter As ListViewColumnSorter, column As Integer)
        If (column = columnSorter.SortColumn) Then
            If (columnSorter.Order = SortOrder.Ascending) Then
                columnSorter.Order = SortOrder.Descending
            Else
                columnSorter.Order = SortOrder.Ascending
            End If
        Else
            columnSorter.SortColumn = column
            columnSorter.Order = SortOrder.Ascending
        End If

        listView.Sort()
    End Sub

    Private Sub UpdateAttributes()
        lstAttributes.Items.Clear()
        If lstObjects.SelectedItems.Count = 1 Then
            Dim data As DebugData
            data = m_game.GetDebugData(lstObjects.SelectedItems(0).Text)
            For Each attr As String In data.Data.Keys
                Dim lvItem As ListViewItem
                lvItem = lstAttributes.Items.Add(attr)
                lvItem.SubItems.Add(data.Data(attr).Value)
                If data.Data(attr).IsInherited Then
                    lvItem.ForeColor = Color.DarkGray
                End If
            Next
        End If
    End Sub

    Private Sub m_game_ObjectsUpdated() Handles m_game.ObjectsUpdated
        BeginInvoke(Sub() UpdateObjectList())
    End Sub

    Friend Property Filter() As String
        Get
            Return m_filter
        End Get
        Set(value As String)
            m_filter = value
        End Set
    End Property

    Private Sub lstAttributes_ColumnClick(sender As Object, e As System.Windows.Forms.ColumnClickEventArgs) Handles lstAttributes.ColumnClick
        SortList(lstAttributes, m_attributesSorter, e.Column)
    End Sub
End Class

' From http://msdn.microsoft.com/en-us/kb/kbarticle.aspx?id=319399
Public Class ListViewColumnSorter
    Implements System.Collections.IComparer

    Private ColumnToSort As Integer
    Private OrderOfSort As SortOrder
    Private ObjectCompare As CaseInsensitiveComparer

    Public Sub New()
        ' Initialize the column to '0'.
        ColumnToSort = 0

        ' Initialize the sort order to 'none'.
        OrderOfSort = SortOrder.None

        ' Initialize the CaseInsensitiveComparer object.
        ObjectCompare = New CaseInsensitiveComparer()
    End Sub

    Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
        Dim compareResult As Integer
        Dim listviewX As ListViewItem
        Dim listviewY As ListViewItem

        ' Cast the objects to be compared to ListViewItem objects.
        listviewX = CType(x, ListViewItem)
        listviewY = CType(y, ListViewItem)

        ' Compare the two items.
        compareResult = ObjectCompare.Compare(listviewX.SubItems(ColumnToSort).Text, listviewY.SubItems(ColumnToSort).Text)

        ' Calculate the correct return value based on the object 
        ' comparison.
        If (OrderOfSort = SortOrder.Ascending) Then
            ' Ascending sort is selected, return typical result of 
            ' compare operation.
            Return compareResult
        ElseIf (OrderOfSort = SortOrder.Descending) Then
            ' Descending sort is selected, return negative result of 
            ' compare operation.
            Return (-compareResult)
        Else
            ' Return '0' to indicate that they are equal.
            Return 0
        End If
    End Function

    Public Property SortColumn() As Integer
        Set(ByVal Value As Integer)
            ColumnToSort = Value
        End Set

        Get
            Return ColumnToSort
        End Get
    End Property

    Public Property Order() As SortOrder
        Set(ByVal Value As SortOrder)
            OrderOfSort = Value
        End Set

        Get
            Return OrderOfSort
        End Get
    End Property
End Class
