Public Class clsUser

    Private _loggedIn As Boolean = False
    Private _userName As String = ""
    Private _uid As String = ""

    Public Sub New()
        _uid = Guid.NewGuid.ToString()
    End Sub

    Public ReadOnly Property IsLoggedIn() As Boolean
        Get
            Return _loggedIn
        End Get
    End Property

    Public ReadOnly Property UserName() As String
        Get
            Return _userName
        End Get
    End Property

    Public ReadOnly Property UniqueID() As String
        Get
            Return _uid
        End Get
    End Property

    Public Function LogIn(ByVal user As String, ByVal password As String) As Boolean
        Dim userNames() As String = {"danny", "chuck", "bill"}
        Dim passWords() As String = {"pass1", "pass2", "pass3"}

        Dim idx As Integer = Array.IndexOf(userNames, user)
        If idx < 0 Then Return False

        If password <> passWords(idx) Then Return False
        _userName = user
        _loggedIn = True

        Dim x As New UserCollection

        Return True
    End Function


End Class

Public Class UserCollection
    Inherits CollectionBase

    Public Sub dTest()
        Dim x As New Hashtable
    End Sub

    Public Shadows Sub Add(ByVal value As clsUser)
        List.Add(value)
    End Sub

    Public ReadOnly Property ItemByUsername(ByVal username As String) As clsUser
        Get
            For Each user As clsUser In List
                If user.UserName = username Then Return user
            Next
            Return Nothing
        End Get
    End Property

    Default Public ReadOnly Property ItemByID(ByVal unique_id As String) As clsUser
        Get
            For Each user As clsUser In List
                If user.UniqueID = unique_id Then Return user
            Next
            Return Nothing
        End Get
    End Property

End Class
