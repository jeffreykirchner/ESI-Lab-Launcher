'Programmed by Jeffrey Kirchner and Your Name Here
'kirchner@chapman.edu/jkirchner@gmail.com
'Programmed by Jeffrey Kirchner and Your Name Here
'Economic Science Institute, Chapman University 2008-2009 ©

Module modMain
    Public sfile As String

    'Public inumber As Integer                  'client ID number
    'Public numberOfPlayers As Integer          'number of total players in experiment
    'Public currentPeriod As Integer            'current period of experiment
    'Public myIPAddress As String               'IP address of client 
    Public myPortNumber As String              'port number of client      
    Public runAsAdmin As Boolean


    Public closing As Boolean = False

    Public connections(100) As clsConnection

    Public selectionCounter As Integer
    Public selectionOrder(100) As Integer

    Public launchIncrement As Integer = 1

#Region " Winsock Code "
    
#End Region

#Region " General Functions "
    Public Sub main()
        AppEventLog_Init()
        appEventLog_Write("Begin")

        Application.EnableVisualStyles()
        Application.Run(frmMain)

        appEventLog_Write("End")
        AppEventLog_Close()
    End Sub

    Public Sub setID(ByVal sinstr As String)
        Try
            'appEventLog_Write("set id :" & sinstr)

            'Dim msgtokens() As String

            'msgtokens = sinstr.Split(";")

            'inumber = msgtokens(0)
        Catch ex As Exception
            appEventLog_Write("error setID:", ex)
        End Try
    End Sub


    Public Sub sendIPAddress(ByVal sinstr As String)
        Try
            'appEventLog_Write("send ip :" & sinstr)

            With frmMain
                'Dim outstr As String

                Dim msgtokens() As String = sinstr.Split(";")

                Dim tempC As Integer = msgtokens(0)
                Dim tempName As String = msgtokens(1)
                Dim tempIP As String = msgtokens(2)

                For i As Integer = 1 To 50
                    If connections(i).myIPAddress.ToString.ToUpper = tempIP Or
                       connections(i).myIPAddress.ToString.ToUpper = tempName.ToString.ToUpper Then

                        connections(i).connectionNumber = tempC
                        .connectionCounter += 1

                        Dim tempCR As Integer = .connectionsRequired
                        Dim tempCC As Integer = .connectionCounter

                        If tempCR = tempCC Then
                            .launchSoftware()
                        End If

                        Exit For
                    End If
                Next

                'inumber = sinstr

                'outstr = SystemInformation.ComputerName
                '.wskClient.Send("03", outstr)
            End With
        Catch ex As Exception
            appEventLog_Write("error sendIPAddress:", ex)
        End Try
    End Sub

   
#End Region

    
End Module
