
Module modINI
    Private Declare Function GetPrivateProfileString Lib "kernel32" Alias _
       "GetPrivateProfileStringA" (ByVal lpApplicationName As String, _
       ByVal lpKeyName As String, _
       ByVal lpDefault As String, _
       ByVal lpReturnedString As System.Text.StringBuilder, _
       ByVal nSize As Integer, _
       ByVal lpFileName As String) As Integer

    Private Declare Function WritePrivateProfileString Lib "kernel32" Alias _
       "WritePrivateProfileStringA" (ByVal lpApplicationName As String, _
       ByVal lpKeyName As String, _
       ByVal lpString As String, _
       ByVal lpFileName As String) As Integer

    'INI file functionality
    Public Function getINI(ByVal sFileName As String, ByVal family As String, ByVal key As String) As String
        Dim strItem As New System.Text.StringBuilder(1024)
        GetPrivateProfileString(family, key, "?", strItem, strItem.Capacity, sFileName)

        getINI = strItem.ToString
    End Function

    Public Sub writeINI(ByVal sFileName As String, ByVal family As String, ByVal key As String, ByVal value As String)

        WritePrivateProfileString(family, key, value, sFileName)
    End Sub


End Module
