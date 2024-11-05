Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports Microsoft.VisualBasic

''' <summary>
''' Winsock designer class provides designer time support for the Winsock component.
''' </summary>
''' <history>
''' 06-09-2006 Added
''' </history>
Public Class WinsockDesigner
    Inherits System.ComponentModel.Design.ComponentDesigner

    Private lists As DesignerActionListCollection

    Public Sub New()
    End Sub

    Public Overrides Sub Initialize(ByVal component As System.ComponentModel.IComponent)
        MyBase.Initialize(component)
    End Sub

    Public Overrides ReadOnly Property Verbs() As System.ComponentModel.Design.DesignerVerbCollection
        Get
            Return New DesignerVerbCollection()
        End Get
    End Property

    Public Overrides ReadOnly Property ActionLists() As DesignerActionListCollection
        Get
            If lists Is Nothing Then
                lists = New DesignerActionListCollection()
                lists.Add(New WinsockActionList(Me.Component))
            End If
            Return lists
        End Get
    End Property

End Class

''' <summary>
''' Provides the action list for the Winsock component during design time.
''' </summary>
''' <history>
''' 06-09-2006 Added
''' </history>
Public Class WinsockActionList
    Inherits DesignerActionList

    Private _wsk As Winsock
    Private designerActionUISvc As DesignerActionUIService = Nothing
    Public Sub New(ByVal component As IComponent)
        MyBase.New(component)
        Me._wsk = component

        Me.designerActionUISvc = CType(GetService(GetType(DesignerActionUIService)), DesignerActionUIService)
    End Sub

    Public Property LegacySupport() As Boolean
        Get
            Return _wsk.LegacySupport
        End Get
        Set(ByVal value As Boolean)

            '_wsk.LegacySupport = value
            GetPropertyByName("LegacySupport").SetValue(_wsk, value)

            Me.designerActionUISvc.Refresh(Me.Component)
        End Set
    End Property

    Public Overrides Function GetSortedActionItems() As DesignerActionItemCollection
        Dim items As New DesignerActionItemCollection()

        'Boolean property for legacy support.
        items.Add(New DesignerActionPropertyItem("LegacySupport", "Legacy Support", "Legacy", "Enables legacy (VB6) send/receive support."))

        Return items
    End Function

    Private Function GetPropertyByName(ByVal propName As String) As PropertyDescriptor
        Dim prop As PropertyDescriptor
        prop = TypeDescriptor.GetProperties(_wsk)(propName)
        If prop Is Nothing Then
            Throw New ArgumentException("Invalid property.", propName)
        Else
            Return prop
        End If
    End Function
End Class
