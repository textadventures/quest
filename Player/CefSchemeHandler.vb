Imports CefSharp
Imports System.IO
Imports System.Text

Public Class CefSchemeHandlerFactory
    Implements ISchemeHandlerFactory

    Private _html As String

    Public Property HTML As String
        Get
            Return _html
        End Get
        Set(value As String)
            _html = value
        End Set
    End Property

    Public Function Create() As ISchemeHandler Implements ISchemeHandlerFactory.Create
        Return New CefSchemeHandler(Me)
    End Function
End Class

Public Class CefSchemeHandler
    Implements ISchemeHandler

    Private _parent As CefSchemeHandlerFactory

    Public Sub New(parent As CefSchemeHandlerFactory)
        _parent = parent
    End Sub

    Public Function ProcessRequest(request As IRequest, ByRef mimeType As String, ByRef stream As IO.Stream) As Boolean Implements ISchemeHandler.ProcessRequest
        mimeType = "text/html"
        Dim bytes = Encoding.UTF8.GetBytes(_parent.HTML)
        stream = New MemoryStream(bytes)
        Return True
    End Function

End Class

Public Class CefResourceSchemeHandlerFactory
    Implements ISchemeHandlerFactory

    Public Function Create() As ISchemeHandler Implements ISchemeHandlerFactory.Create
        Return New CefResourceSchemeHandler()
    End Function
End Class

Public Class CefResourceSchemeHandler
    Implements ISchemeHandler

    Public Function ProcessRequest(request As IRequest, ByRef mimeType As String, ByRef stream As Stream) As Boolean Implements ISchemeHandler.ProcessRequest
        Dim uri = New Uri(request.Url)
        Dim segments = uri.Segments
        Dim filepath = Path.Combine(My.Application.Info.DirectoryPath(), uri.AbsolutePath.Substring(1))

        If File.Exists(filepath) Then
            System.Diagnostics.Debug.WriteLine("Served " + filepath)

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