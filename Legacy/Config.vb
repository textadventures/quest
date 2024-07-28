Option Strict On
Option Explicit On
Option Infer On

Public Class Config
    Public Shared ReadOnly Property ReadGameFileFromAzureBlob As Boolean
        Get
            ' TODO - QUESTCORE
            ' Return System.Configuration.ConfigurationManager.AppSettings("FileManagerType") = "WebPlayer.AzureFileManager, WebPlayer"
            Return False
        End Get
    End Property
End Class
