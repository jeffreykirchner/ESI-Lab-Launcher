Public Class WinsockCollection
    Inherits CollectionBase
    Private col As New Collection

    Public Sub Add(ByVal value As Winsock)
        Add(value, "")
    End Sub
    Public Sub Add(ByVal value As Winsock, ByVal Key As String)
        Try
            AddHandler value.Connected, AddressOf Wsk_Connected
            AddHandler value.ConnectionRequest, AddressOf Wsk_ConnectionRequest
            AddHandler value.DataArrival, AddressOf Wsk_DataArrival
            AddHandler value.Disconnected, AddressOf Wsk_Disconnected
            AddHandler value.Disposed, AddressOf Wsk_Disposed
            AddHandler value.ErrorReceived, AddressOf Wsk_ErrorReceived
            AddHandler value.SendComplete, AddressOf Wsk_SendComplete
            AddHandler value.SendProgress, AddressOf Wsk_SendProgress
            AddHandler value.StateChanged, AddressOf Wsk_StateChanged
            List.Add(value)
            col.Add(Key)
        Catch ex As Exception
            MsgBox("Add")
        End Try
    End Sub
    Public Shadows Sub Clear()
        RemoveAllHandlers()
        Me.CloseAll()
        MyBase.Clear()
    End Sub
    Private Sub RemoveAllHandlers()
        If List.Count > 0 Then
            For index As Integer = 0 To List.Count - 1
                RemoveHandler Item(index).Connected, AddressOf Wsk_Connected
                RemoveHandler Item(index).ConnectionRequest, AddressOf Wsk_ConnectionRequest
                RemoveHandler Item(index).DataArrival, AddressOf Wsk_DataArrival
                RemoveHandler Item(index).Disconnected, AddressOf Wsk_Disconnected
                RemoveHandler Item(index).Disposed, AddressOf Wsk_Disposed
                RemoveHandler Item(index).ErrorReceived, AddressOf Wsk_ErrorReceived
                RemoveHandler Item(index).SendComplete, AddressOf Wsk_SendComplete
                RemoveHandler Item(index).SendProgress, AddressOf Wsk_SendProgress
                RemoveHandler Item(index).StateChanged, AddressOf Wsk_StateChanged
            Next
        End If
    End Sub
    Public Sub CloseAll()
        If List.Count > 0 Then
            For i As Integer = List.Count - 1 To 0 Step -1
                If Item(i).State <> WinsockStates.Closed And _
                   Item(i).State <> WinsockStates.Closing Then
                    Item(i).Close()
                End If
            Next
        End If
    End Sub
    Public Sub Send(ByVal header As String, ByVal key As String, ByVal value As String)
        Try
            Dim outstr As String
            outstr = header & "|" & value & "|" & "#"

            Item(key).Send(outstr)
        Catch ex As Exception
            appEventLog_Write("error send message:", ex)
        End Try
    End Sub
    Public Sub Send(ByVal header As String, ByVal index As Integer, ByVal value As String)
        Dim outstr As String
        outstr = header & "|" & value & "|" & "#"

        Item(index).Send(outstr)
    End Sub
    Public Sub SendAll(ByVal value As String)
        For Each wsk As Winsock In List
            If wsk.State = WinsockStates.Connected Then
                wsk.Send(value)
            End If
        Next
    End Sub
    Public Sub SendAllBut(ByVal key As String, ByVal value As String)
        If col.Count < 1 Then Exit Sub
        For i As Integer = 1 To col.Count
            Dim cKey As String = col.Item(i)
            If cKey <> key Then
                If Item(cKey).State = WinsockStates.Connected Then
                    Item(cKey).Send(value)
                End If
            End If
        Next
    End Sub
    Public Sub SendAllBut(ByVal index As Integer, ByVal value As String)
        If Not List.Count > 1 Then Exit Sub
        For i As Integer = 0 To Count - 1
            If i <> index Then
                If Item(i).State = WinsockStates.Connected Then
                    Item(i).Send(value)
                End If
            End If
        Next
    End Sub
    Public Sub SendAllBut(ByVal source As Winsock, ByVal value As String)
        SendAllBut(IndexOf(source), value)
    End Sub
    Public Function IndexOf(ByVal value As Winsock) As Integer
        Return List.IndexOf(value)
    End Function
    Public Function GetKey(ByVal value As Winsock) As String
        Return col.Item(IndexOf(value) + 1)
    End Function
    Public Sub Remove(ByVal value As Winsock)
        Try
            'Exit Sub
            If List.Contains(value) Then
                If value.State <> WinsockStates.Closed And _
                   value.State <> WinsockStates.Closing Then
                    value.Close()
                End If
                RemoveHandler value.Connected, AddressOf Wsk_Connected
                RemoveHandler value.ConnectionRequest, AddressOf Wsk_ConnectionRequest
                RemoveHandler value.DataArrival, AddressOf Wsk_DataArrival
                RemoveHandler value.Disconnected, AddressOf Wsk_Disconnected
                RemoveHandler value.Disposed, AddressOf Wsk_Disposed
                RemoveHandler value.ErrorReceived, AddressOf Wsk_ErrorReceived
                RemoveHandler value.SendComplete, AddressOf Wsk_SendComplete
                RemoveHandler value.SendProgress, AddressOf Wsk_SendProgress
                RemoveHandler value.StateChanged, AddressOf Wsk_StateChanged
                col.Remove(IndexOf(value) + 1)
                List.Remove(value)
            Else
                MsgBox("Value doesn't exist in collection.")
            End If
        Catch ex As Exception
            MsgBox("Winsock/Remove")
        End Try
    End Sub
    Public Sub RemoveIndex(ByVal index As Integer)
        Try

            If index > Count - 1 Or index < 0 Then
                Throw New IndexOutOfRangeException
            Else
                If Item(index).State <> WinsockStates.Closed And _
                   Item(index).State <> WinsockStates.Closing Then
                    Item(index).Close()
                End If
                RemoveHandler Item(index).Connected, AddressOf Wsk_Connected
                RemoveHandler Item(index).ConnectionRequest, AddressOf Wsk_ConnectionRequest
                RemoveHandler Item(index).DataArrival, AddressOf Wsk_DataArrival
                RemoveHandler Item(index).Disconnected, AddressOf Wsk_Disconnected
                RemoveHandler Item(index).Disposed, AddressOf Wsk_Disposed
                RemoveHandler Item(index).ErrorReceived, AddressOf Wsk_ErrorReceived
                RemoveHandler Item(index).SendComplete, AddressOf Wsk_SendComplete
                RemoveHandler Item(index).SendProgress, AddressOf Wsk_SendProgress
                RemoveHandler Item(index).StateChanged, AddressOf Wsk_StateChanged
                List.RemoveAt(index)
                col.Remove(index + 1)
            End If
        Catch ex As Exception
            MsgBox("Winsock/RemoveIndex")
        End Try
    End Sub
    Default Public ReadOnly Property Item(ByVal index As Integer) As Winsock
        Get
            Try
                If index > Count - 1 Or index < 0 Then
                    Throw New IndexOutOfRangeException
                End If
                Return CType(List.Item(index), Winsock)
            Catch ex As Exception
                MsgBox("Collection/Item")
                Return New Winsock
            End Try
        End Get
    End Property
    Default Public ReadOnly Property Item(ByVal Key As String) As Winsock
        Get
            Try
                Dim idx As Integer = -1
                For i As Integer = 1 To col.Count
                    If col.Item(i) = Key Then
                        idx = i
                    End If
                Next
                If idx = -1 Then Return New Winsock
                Return CType(List.Item(idx - 1), Winsock)
            Catch ex As Exception
                MsgBox("Key/Item")
                Return New Winsock
            End Try
        End Get
    End Property

#Region " Events "
    Public Event Connected(ByVal sender As Object, ByVal e As System.EventArgs)
    Public Event ConnectionRequest(ByVal sender As Object, ByVal e As WinsockClientReceivedEventArgs)
    Public Event DataArrival(ByVal sender As Object, ByVal e As WinsockDataArrivalEventArgs)
    Public Event Disconnected(ByVal sender As Object, ByVal e As System.EventArgs)
    Public Event Winsock_Disposed(ByVal sender As Object, ByVal e As System.EventArgs)
    Public Event ErrorReceived(ByVal sender As Object, ByVal e As WinsockErrorEventArgs)
    Public Event SendComplete(ByVal sender As Object, ByVal e As WinsockSendEventArgs)
    Public Event SendProgress(ByVal sender As Object, ByVal e As WinsockSendEventArgs)
    Public Event StateChanged(ByVal sender As Object, ByVal e As WinsockStateChangingEventArgs)
#End Region

#Region " Winsock Event Calls "

    Private Sub Wsk_Connected(ByVal sender As Object, ByVal e As System.EventArgs)
        RaiseEvent Connected(sender, e)
    End Sub

    Private Sub Wsk_ConnectionRequest(ByVal sender As Object, ByVal e As WinsockClientReceivedEventArgs)
        RaiseEvent ConnectionRequest(sender, e)
    End Sub

    Private Sub Wsk_DataArrival(ByVal sender As Object, ByVal e As WinsockDataArrivalEventArgs)
        RaiseEvent DataArrival(sender, e)
    End Sub

    Private Sub Wsk_Disconnected(ByVal sender As Object, ByVal e As System.EventArgs)
        RaiseEvent Disconnected(sender, e)
    End Sub

    Private Sub Wsk_Disposed(ByVal sender As Object, ByVal e As System.EventArgs)
        RaiseEvent Winsock_Disposed(sender, e)
    End Sub

    Private Sub Wsk_ErrorReceived(ByVal sender As System.Object, ByVal e As WinsockErrorEventArgs)
        RaiseEvent ErrorReceived(sender, e)
    End Sub

    Private Sub Wsk_SendComplete(ByVal sender As Object, ByVal e As WinsockSendEventArgs)
        RaiseEvent SendComplete(sender, e)
    End Sub

    Private Sub Wsk_SendProgress(ByVal sender As Object, ByVal e As WinsockSendEventArgs)
        RaiseEvent SendProgress(sender, e)
    End Sub

    Private Sub Wsk_StateChanged(ByVal sender As Object, ByVal e As WinsockStateChangingEventArgs)
        RaiseEvent StateChanged(sender, e)
    End Sub

#End Region
End Class