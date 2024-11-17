Imports CefSharp
Imports System.IO
Imports System.Text

Public Class QuestSchemeHandlerFactory
    Implements ISchemeHandlerFactory

    Public Sub New(parent As PlayerHTML)
        Me.Parent = parent
    End Sub

    Public Property Parent As PlayerHTML

    Public Function Create(browser As IBrowser, frame As IFrame, schemeName As String, request As IRequest) As IResourceHandler Implements ISchemeHandlerFactory.Create
        Return New QuestSchemeHandler(Parent)
    End Function
End Class

Public Class QuestSchemeHandler
    Inherits ResourceHandler

    Private _parent As PlayerHTML

    Public Sub New(parent As PlayerHTML)
        _parent = parent
    End Sub

    Public Overrides Function ProcessRequestAsync(request As IRequest, callback As ICallback) As CefReturnValue
        Dim uri = New Uri(request.Url)
        Dim filename = Uri.UnescapeDataString(uri.AbsolutePath.Substring(1))

        Stream = _parent.CurrentGame.GetResource(filename)
        If (Stream IsNot Nothing) Then
            MimeType = PlayerHelper.GetContentType(filename)
            callback.Continue()
            Return CefReturnValue.Continue
        End If

        callback.Dispose()
        Return CefReturnValue.Cancel
    End Function
End Class

Public Class ResourceSchemeHandlerFactory
    Implements ISchemeHandlerFactory

    Public Function Create(browser As IBrowser, frame As IFrame, schemeName As String, request As IRequest) As IResourceHandler Implements ISchemeHandlerFactory.Create
        Return New ResourceSchemeHandler(Me)
    End Function

    Public Property HTML As String
End Class

Public Class ResourceSchemeHandler
    Inherits ResourceHandler

    Private _parent As ResourceSchemeHandlerFactory

    Public Sub New(parent As ResourceSchemeHandlerFactory)
        _parent = parent
    End Sub

    Public Overrides Function ProcessRequestAsync(request As IRequest, callback As ICallback) As CefReturnValue
        Dim uri = New Uri(request.Url)

        If uri.AbsolutePath = "/ui" Then
            MimeType = "text/html"
            Dim bytes = Encoding.UTF8.GetBytes(_parent.HTML)
            Stream = New MemoryStream(bytes)
            callback.Continue()
            Return CefReturnValue.Continue
        End If

        Dim filepath = Path.Combine(My.Application.Info.DirectoryPath(), uri.AbsolutePath.Substring(1))

        If File.Exists(filepath) Then
            System.Diagnostics.Debug.WriteLine("Served {0} from {1}", request.Url, filepath)

            Stream = New System.IO.FileStream(filepath, FileMode.Open, FileAccess.Read)

            Select Case Path.GetExtension(filepath)
                Case ".js"
                    MimeType = "text/javascript"
                Case ".css"
                    MimeType = "text/css"
                Case ".png"
                    MimeType = "image/png"
                Case Else
                    Throw New Exception("Unknown MIME type")
            End Select

            callback.Continue()
            Return CefReturnValue.Continue
        Else
            System.Diagnostics.Debug.WriteLine("Not found " + filepath)
            callback.Dispose()
            Return CefReturnValue.Cancel
        End If
    End Function
End Class