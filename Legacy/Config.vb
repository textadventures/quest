Option Strict On
Option Explicit On
Option Infer On

Public Class Config
    Public Shared ReadOnly Property ReadGameFileFromAzureBlob As Boolean
        Get
            Return System.Configuration.ConfigurationManager.AppSettings("FileManagerType") = "WebPlayer.AzureFileManager, WebPlayer"
        End Get
    End Property
End Class
