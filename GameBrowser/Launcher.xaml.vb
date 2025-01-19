﻿Imports TextAdventures.Utility.Language.L

Public Class Launcher
    Public Event BrowseForGame()
    Public Event LaunchGame(filename As String)
    Public Event EditGame(filename As String)
    Public Event CreateNewGame()
    Public Event BrowseForGameEdit()
    Public Event Tutorial()

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ctlVersionInfo.Visibility = Windows.Visibility.Collapsed
        ctlTabs.SelectedIndex = CInt(TextAdventures.Utility.Registry.GetSetting("Quest", "Settings", "SelectedTab", 0))
        AddHandler ctlPlayBrowser.LaunchGame, AddressOf ctlPlayBrowser_LaunchGame
        AddHandler ctlPlayBrowser.GotUpdateData, AddressOf ctlPlayBrowser_GotUpdateData
        AddHandler ctlEditBrowser.EditGame, AddressOf ctlEditBrowser_EditGame
        AddHandler ctlEditBrowser.CreateNewGame, AddressOf ctlEditBrowser_CreateNewGame
        AddHandler ctlEditBrowser.OpenGame, AddressOf ctlEditBrowser_OpenGame
        AddHandler ctlEditBrowser.Tutorial, AddressOf ctlEditBrowser_Tutorial
    End Sub

    Public Sub AddToRecent(filename As String, name As String)
        ctlPlayBrowser.AddToRecent(filename, name)
    End Sub

    Public Sub AddToEditorRecent(filename As String, name As String)
        ctlEditBrowser.AddToRecent(filename, name)
    End Sub

    Private Sub ctlPlayBrowser_LaunchGame(filename As String)
        RaiseEvent LaunchGame(filename)
    End Sub

    Private Sub ctlEditBrowser_EditGame(filename As String)
        RaiseEvent EditGame(filename)
    End Sub

    Private Sub ctlEditBrowser_CreateNewGame()
        RaiseEvent CreateNewGame()
    End Sub

    Private Sub ctlEditBrowser_OpenGame()
        RaiseEvent BrowseForGameEdit()
    End Sub

    Private Sub ctlEditBrowser_Tutorial()
        RaiseEvent Tutorial()
    End Sub

    Private Sub ctlTabs_SelectionChanged(sender As System.Object, e As System.Windows.Controls.SelectionChangedEventArgs) Handles ctlTabs.SelectionChanged
        TextAdventures.Utility.Registry.SaveSetting("Quest", "Settings", "SelectedTab", ctlTabs.SelectedIndex)
    End Sub

    Public Sub RefreshLists()
        ctlPlayBrowser.Populate()
        ctlEditBrowser.Populate()
    End Sub

    Public Sub MainWindowShown()
        ctlPlayBrowser.MainWindowShown()
    End Sub

    Private m_questVersion As Version

    Public Property QuestVersion As Version
        Get
            Return m_questVersion
        End Get
        Set(value As Version)
            m_questVersion = value
            WebClientFactory.QuestVersion = value.ToString
        End Set
    End Property

    Public Property MaxASLVersion As Integer
        Get
            Return WebClientFactory.MaxASLVersion
        End Get
        Set(value As Integer)
            WebClientFactory.MaxASLVersion = value
        End Set
    End Property

    Private Sub ctlPlayBrowser_GotUpdateData(data As UpdatesData)
        ctlVersionInfo.UpdateInfo = data
        If IsNewVersion(data) Then
            Dispatcher.BeginInvoke(Sub()
                                       ctlVersionInfo.UpdateInfo = data
                                       ctlVersionInfo.Visibility = Windows.Visibility.Visible
                                   End Sub)
        End If
    End Sub

    Private Function IsNewVersion(updateData As UpdatesData) As Boolean
        If updateData Is Nothing Then Return False
        Dim latestMajor As Integer
        Dim latestMinor As Integer
        Dim latestBuild As Integer
        Dim latestRevision As Integer

        Integer.TryParse(updateData.Major, latestMajor)
        Integer.TryParse(updateData.Minor, latestMinor)
        Integer.TryParse(updateData.Build, latestBuild)
        Integer.TryParse(updateData.Revision, latestRevision)

        Dim latestVersion As New Version(latestMajor, latestMinor, latestBuild, latestRevision)
        Return latestVersion > QuestVersion
    End Function

    Public Property DownloadFolder As String
        Get
            Return ctlPlayBrowser.DownloadFolder
        End Get
        Set(value As String)
            ctlPlayBrowser.DownloadFolder = value
        End Set
    End Property

    Public Function CloseLauncher() As Boolean
        If ctlPlayBrowser.DownloadingCount = 0 Then
            Return True
        Else
            Dim result = MsgBox(String.Format("{0} games are still downloading. Are you sure you wish to exit?", ctlPlayBrowser.DownloadingCount), MsgBoxStyle.Exclamation Or MsgBoxStyle.YesNo)
            If result = MsgBoxResult.Yes Then
                ctlPlayBrowser.CancelDownloads()
                Return True
            End If
        End If
    End Function

    Private Sub lblPlay_Initialized(sender As Object, e As EventArgs) Handles lblPlay.Initialized
        lblPlay.Text = T("LauncherPlay")
    End Sub

    Private Sub lblCreate_Initialized(sender As Object, e As EventArgs) Handles lblCreate.Initialized
        lblCreate.Text = T("LauncherCreate")
    End Sub
End Class