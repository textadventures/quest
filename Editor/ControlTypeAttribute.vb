<AttributeUsage(AttributeTargets.Class)> _
Public Class ControlTypeAttribute
    Inherits System.Attribute

    Private m_controlType As String

    Sub New(ByVal controlType As String)
        m_controlType = controlType
    End Sub

    Public ReadOnly Property ControlType() As String
        Get
            Return m_controlType
        End Get
    End Property
End Class
