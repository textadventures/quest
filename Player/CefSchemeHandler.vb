Imports CefSharp
Imports CefSharp.Callback
Imports CefSharp.DevTools.Network
Imports System.IO
Imports System.Text

Public Class CefSchemeHandlerFactory
    Implements ISchemeHandlerFactory

    Public Sub New(parent As PlayerHTML)
        Me.Parent = parent
    End Sub

    Public Property Parent As PlayerHTML

    Public Function Create(browser As IBrowser, frame As IFrame, schemeName As String, request As IRequest) As IResourceHandler Implements ISchemeHandlerFactory.Create
        Return New CefSchemeHandler(Me)
    End Function
End Class

Public Class CefSchemeHandler
    Implements IResourceHandler

    Private _parent As CefSchemeHandlerFactory

    Public Sub New(parent As CefSchemeHandlerFactory)
        _parent = parent
    End Sub

    Public Sub GetResponseHeaders(response As IResponse, ByRef responseLength As Long, ByRef redirectUrl As String) Implements IResourceHandler.GetResponseHeaders
        Throw New NotImplementedException()
    End Sub

    Public Sub Cancel() Implements IResourceHandler.Cancel
        Throw New NotImplementedException()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub

    Public Function Open(request As IRequest, ByRef handleRequest As Boolean, callback As ICallback) As Boolean Implements IResourceHandler.Open
        Throw New NotImplementedException()
    End Function

    <Obsolete>
    Public Function ProcessRequest(request As IRequest, callback As ICallback) As Boolean Implements IResourceHandler.ProcessRequest
        Dim uri = New Uri(request.Url)
        Dim filename = Uri.UnescapeDataString(uri.AbsolutePath.Substring(1))

        'Response.ResponseStream = _parent.Parent.CurrentGame.GetResource(filename)
        'If (Response.ResponseStream IsNot Nothing) Then
        '    Response.MimeType = PlayerHelper.GetContentType(filename)
        '    requestCompletedCallback()
        '    Return True
        'End If

        Return False
    End Function

    Public Function Skip(bytesToSkip As Long, ByRef bytesSkipped As Long, callback As IResourceSkipCallback) As Boolean Implements IResourceHandler.Skip
        Throw New NotImplementedException()
    End Function

    Public Function Read(dataOut As Stream, ByRef bytesRead As Integer, callback As IResourceReadCallback) As Boolean Implements IResourceHandler.Read
        Throw New NotImplementedException()
    End Function

    Public Function ReadResponse(dataOut As Stream, ByRef bytesRead As Integer, callback As ICallback) As Boolean Implements IResourceHandler.ReadResponse
        Throw New NotImplementedException()
    End Function
End Class

Public Class CefResourceSchemeHandlerFactory
    Implements ISchemeHandlerFactory

    Public Function Create(browser As IBrowser, frame As IFrame, schemeName As String, request As IRequest) As IResourceHandler Implements ISchemeHandlerFactory.Create
        Return New CefResourceSchemeHandler(Me)
    End Function

    Public Property HTML As String
End Class

Public Class CefResourceSchemeHandler
    Implements IResourceHandler

    Private _parent As CefResourceSchemeHandlerFactory

    Public Sub New(parent As CefResourceSchemeHandlerFactory)
        _parent = parent
    End Sub

    Public Sub GetResponseHeaders(response As IResponse, ByRef responseLength As Long, ByRef redirectUrl As String) Implements IResourceHandler.GetResponseHeaders
        Throw New NotImplementedException()
    End Sub

    Public Sub Cancel() Implements IResourceHandler.Cancel
        Throw New NotImplementedException()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub

    Public Function Open(request As IRequest, ByRef handleRequest As Boolean, callback As ICallback) As Boolean Implements IResourceHandler.Open
        Throw New NotImplementedException()
    End Function

    <Obsolete>
    Public Function ProcessRequest(request As IRequest, callback As ICallback) As Boolean Implements IResourceHandler.ProcessRequest
        Dim uri = New Uri(request.Url)

        'If uri.AbsolutePath = "/ui" Then
        '    Response.MimeType = "text/html"
        '    Dim bytes = Encoding.UTF8.GetBytes(_parent.HTML)
        '    Response.ResponseStream = New MemoryStream(bytes)
        '    requestCompletedCallback()
        '    Return True
        'End If

        'Dim filepath = Path.Combine(My.Application.Info.DirectoryPath(), uri.AbsolutePath.Substring(1))

        'If File.Exists(filepath) Then
        '    System.Diagnostics.Debug.WriteLine("Served {0} from {1}", request.Url, filepath)

        '    Response.ResponseStream = New System.IO.FileStream(filepath, FileMode.Open, FileAccess.Read)

        '    Select Case Path.GetExtension(filepath)
        '        Case ".js"
        '            Response.MimeType = "text/javascript"
        '        Case ".css"
        '            Response.MimeType = "text/css"
        '        Case ".png"
        '            Response.MimeType = "image/png"
        '        Case Else
        '            Throw New Exception("Unknown MIME type")
        '    End Select

        '    requestCompletedCallback()
        '    Return True
        'Else
        '    System.Diagnostics.Debug.WriteLine("Not found " + filepath)
        '    Return False
        'End If
    End Function

    Public Function Skip(bytesToSkip As Long, ByRef bytesSkipped As Long, callback As IResourceSkipCallback) As Boolean Implements IResourceHandler.Skip
        Throw New NotImplementedException()
    End Function

    Public Function Read(dataOut As Stream, ByRef bytesRead As Integer, callback As IResourceReadCallback) As Boolean Implements IResourceHandler.Read
        Throw New NotImplementedException()
    End Function

    Public Function ReadResponse(dataOut As Stream, ByRef bytesRead As Integer, callback As ICallback) As Boolean Implements IResourceHandler.ReadResponse
        Throw New NotImplementedException()
    End Function
End Class