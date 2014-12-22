Public Class Config
    Public Shared ReadOnly Property ReadGameFileFromAzureBlob As Boolean
        Get
            Return System.Configuration.ConfigurationManager.AppSettings("FileManagerType") = "WebPlayer.AzureFileManager, WebPlayer"
        End Get
    End Property
End Class
