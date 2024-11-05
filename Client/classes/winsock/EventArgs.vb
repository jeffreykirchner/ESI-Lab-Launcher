''' <summary>
''' Provides data for the Winsock.StateChanged event.
''' </summary>
Public Class WinsockStateChangingEventArgs
    Inherits System.EventArgs

    Private m_OldState As WinsockStates
    Private m_NewState As WinsockStates

    ''' <summary>
    ''' Initializes a new instance of the WinsockStateChangingEventArgs class.
    ''' </summary>
    ''' <param name="oldState">The old state of the Winsock control.</param>
    ''' <param name="newState">The state the Winsock control is changing to.</param>
    Public Sub New(ByVal oldState As WinsockStates, ByVal newState As WinsockStates)
        m_OldState = oldState
        m_NewState = newState
    End Sub

    ''' <summary>
    ''' Gets a value indicating the previous state of the Winsock control.
    ''' </summary>
    Public ReadOnly Property Old_State() As WinsockStates
        Get
            Return m_OldState
        End Get
    End Property

    ''' <summary>
    ''' Gets a value indicating the new state of the Winsock control.
    ''' </summary>
    Public ReadOnly Property New_State() As WinsockStates
        Get
            Return m_NewState
        End Get
    End Property
End Class

''' <summary>
''' Provides data for the Winsock.ErrorReceived event.
''' </summary>
Public Class WinsockErrorEventArgs
    Inherits System.EventArgs

    Private m_errorMsg As String
    Private m_function As String
    Private m_errorCode As System.Net.Sockets.SocketError
    Private m_Details As String

    ''' <summary>
    ''' Initializes a new instance of the WinsockErrorEventArgs class.
    ''' </summary>
    ''' <param name="error_message">A String containing the error message.</param>
    ''' <param name="function_name">A String containing the name of the function that produced the error.</param>
    ''' <param name="error_code">A value containing the socket's ErrorCode.</param>
    Public Sub New(ByVal error_message As String, Optional ByVal function_name As String = Nothing, Optional ByVal error_code As System.Net.Sockets.SocketError = Net.Sockets.SocketError.SocketError, Optional ByVal extra_details As String = "")
        m_errorMsg = error_message
        m_function = function_name
        m_errorCode = error_code
        m_Details = extra_details
    End Sub

    ''' <summary>
    ''' Gets a value containing the error message.
    ''' </summary>
    Public ReadOnly Property Message() As String
        Get
            Return m_errorMsg
        End Get
    End Property
    ''' <summary>
    ''' Gets a value containing the name of the function that produced the error.
    ''' </summary>
    Public ReadOnly Property [Function]() As String
        Get
            Return m_function
        End Get
    End Property
    ''' <summary>
    ''' Gets a value indicating the error code returned by the socket.
    ''' </summary>
    ''' <remarks>If it wasn't returned by the socket, it defaults to success.</remarks>
    Public ReadOnly Property ErrorCode() As System.Net.Sockets.SocketError
        Get
            Return m_errorCode
        End Get
    End Property
    ''' <summary>
    ''' Gets a value containing more details than the typical error message.
    ''' </summary>
    Public ReadOnly Property Details() As String
        Get
            Return m_Details
        End Get
    End Property
End Class

''' <summary>
''' Provides data for the Winsock.ConnectionRequest event.
''' </summary>
Public Class WinsockClientReceivedEventArgs
    Inherits System.EventArgs

    Private _client As System.Net.Sockets.Socket
    Private _cancel As Boolean = False

    ''' <summary>
    ''' Initializes a new instance of the WinsockClientReceivedEventArgs class.
    ''' </summary>
    ''' <param name="new_client">A Socket object containing the new client that needs to be accepted.</param>
    Public Sub New(ByVal new_client As System.Net.Sockets.Socket)
        _client = new_client
    End Sub

    ''' <summary>
    ''' Gets a value containing the client information.
    ''' </summary>
    ''' <remarks>Used in accepting the client.</remarks>
    Public ReadOnly Property Client() As System.Net.Sockets.Socket
        Get
            Return _client
        End Get
    End Property

    ''' <summary>
    ''' Gets a value containing the incoming clients IP address.
    ''' </summary>
    Public ReadOnly Property ClientIP() As String
        Get
            Dim rEP As System.Net.IPEndPoint = _client.RemoteEndPoint
            Return rEP.Address.ToString()
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets a value indicating whether the incoming client request should be cancelled.
    ''' </summary>
    Public Property Cancel() As Boolean
        Get
            Return _cancel
        End Get
        Set(ByVal value As Boolean)
            _cancel = value
        End Set
    End Property
End Class

''' <summary>
''' Provides data for the Winsock.SendComplete event.
''' </summary>
''' <history>
''' 06-12-2006 Changed name from WinsockSendCompleteEventArgs to
'''             WinsockSendEventArgs (SendProgress uses same type).
''' </history>
Public Class WinsockSendEventArgs
    Inherits System.EventArgs

    Private _bTotal As Integer
    Private _bSent As Integer

    ''' <summary>
    ''' Initializes a new instance of the WinsockSendEventArgs class.
    ''' </summary>
    ''' <param name="bytes_sent">The total number of bytes sent.</param>
    ''' <param name="bytes_total">The total number of bytes that were supposed to be sent.</param>
    Public Sub New(ByVal bytes_sent As Integer, ByVal bytes_total As Integer)
        _bTotal = bytes_total
        _bSent = bytes_sent
    End Sub

    ''' <summary>
    ''' Gets a value indicating the number of bytes sent.
    ''' </summary>
    Public ReadOnly Property BytesSent() As Integer
        Get
            Return _bSent
        End Get
    End Property
    ''' <summary>
    ''' Gets a value indicating the total number of bytes that should have been sent.
    ''' </summary>
    Public ReadOnly Property BytesTotal() As Integer
        Get
            Return _bTotal
        End Get
    End Property
    ''' <summary>
    ''' Gets a value indicating the percentage of bytes that where sent.
    ''' </summary>
    Public ReadOnly Property SentPercent() As Double
        Get
            Return (_bSent / _bTotal) * 100
        End Get
    End Property
End Class

''' <summary>
''' Provides data for the Winsock.DataArrival event.
''' </summary>
Public Class WinsockDataArrivalEventArgs
    Inherits System.EventArgs

    Private _bTotal As Integer
    Private _IP As String
    Private _Port As Integer

    ''' <summary>
    ''' Initializes a new instance of the WinsockDataArrivalEventArgs class.
    ''' </summary>
    ''' <param name="bytes_total">The number of bytes that were received.</param>
    Public Sub New(ByVal bytes_total As Integer, ByVal source_ip As String, ByVal source_port As Integer)
        _bTotal = bytes_total
        _IP = source_ip
        _Port = source_port
    End Sub

    ''' <summary>
    ''' Gets a value indicating the number of bytes received.
    ''' </summary>
    Public ReadOnly Property TotalBytes() As Integer
        Get
            Return _bTotal
        End Get
    End Property

    Public ReadOnly Property SourceIP() As String
        Get
            Return _IP
        End Get
    End Property

    Public ReadOnly Property SourcePort() As Integer
        Get
            Return _Port
        End Get
    End Property
End Class