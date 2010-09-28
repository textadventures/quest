Friend Class GameListItems
    Inherits ObjectModel.KeyedCollection(Of String, GameListItemData)

    Protected Overrides Function GetKeyForItem(ByVal item As GameListItemData) As String
        Return item.Filename
    End Function
End Class