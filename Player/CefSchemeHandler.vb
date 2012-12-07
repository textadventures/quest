Imports CefSharp
Imports System.IO
Imports System.Text

Public Class CefSchemeHandlerFactory
    Implements ISchemeHandlerFactory

    Private _images As New Dictionary(Of String, String)

    Public Function Create() As ISchemeHandler Implements ISchemeHandlerFactory.Create
        Return New CefSchemeHandler(Me)
    End Function

    Public Function AddImage(filename As String) As String
        Dim id As String = Guid.NewGuid().ToString()
        _images.Add(id, filename)
        Return id
    End Function

    Public Function GetImageId(filename As String) As String
        If Not _images.ContainsKey(filename) Then Return Nothing
        Return _images(filename)
    End Function
End Class

Public Class CefSchemeHandler
    Implements ISchemeHandler

    Private _parent As CefSchemeHandlerFactory

    Public Sub New(parent As CefSchemeHandlerFactory)
        _parent = parent
    End Sub

    Public Function ProcessRequest(request As IRequest, ByRef mimeType As String, ByRef stream As IO.Stream) As Boolean Implements ISchemeHandler.ProcessRequest
        Dim uri = New Uri(request.Url)
        Dim id = uri.AbsolutePath.Substring(1)
        Dim filename = _parent.GetImageId(id)
        If filename IsNot Nothing Then
            stream = New System.IO.FileStream(filename, FileMode.Open, FileAccess.Read)
            Select Case Path.GetExtension(filename).ToLowerInvariant()
                Case ".jpg", ".jpeg"
                    mimeType = "image/jpeg"
                Case ".gif"
                    mimeType = "image/gif"
                Case ".bmp"
                    mimeType = "image/bmp"
                Case ".png"
                    mimeType = "image/png"
                Case ".js"
                    mimeType = "text/javascript"
                Case Else
                    Throw New Exception("Unknown MIME type")
            End Select
            Return True
        End If

        Return False
    End Function
End Class

Public Class CefResourceSchemeHandlerFactory
    Implements ISchemeHandlerFactory

    Public Function Create() As ISchemeHandler Implements ISchemeHandlerFactory.Create
        Return New CefResourceSchemeHandler(Me)
    End Function

    Public Property HTML As String
End Class

Public Class CefResourceSchemeHandler
    Implements ISchemeHandler

    Private _parent As CefResourceSchemeHandlerFactory

    Public Sub New(parent As CefResourceSchemeHandlerFactory)
        _parent = parent
    End Sub

    Public Function ProcessRequest(request As IRequest, ByRef mimeType As String, ByRef stream As Stream) As Boolean Implements ISchemeHandler.ProcessRequest
        Dim uri = New Uri(request.Url)

        If uri.AbsolutePath = "/ui" Then
            mimeType = "text/html"
            Dim bytes = Encoding.UTF8.GetBytes(_parent.HTML)
            stream = New MemoryStream(bytes)
            Return True
        End If

        Dim filepath = Path.Combine(My.Application.Info.DirectoryPath(), uri.AbsolutePath.Substring(1))

        If File.Exists(filepath) Then
            System.Diagnostics.Debug.WriteLine("Served {0} from {1}", request.Url, filepath)

            stream = New System.IO.FileStream(filepath, FileMode.Open, FileAccess.Read)

            Select Case Path.GetExtension(filepath)
                Case ".js"
                    mimeType = "text/javascript"
                Case ".css"
                    mimeType = "text/css"
                Case ".png"
                    mimeType = "image/png"
                Case Else
                    Throw New Exception("Unknown MIME type")
            End Select

            Return True
        Else
            System.Diagnostics.Debug.WriteLine("Not found " + filepath)
            Return False
        End If

    End Function
End Class