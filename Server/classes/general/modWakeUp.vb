Module modWakeUp

    ' API call to prevent sleep (until the application exits)
    Private Declare Function SetThreadExecutionState Lib "kernel32" (ByVal esflags As EXECUTION_STATE) As EXECUTION_STATE

    ' Define the API execution states
    Private Enum EXECUTION_STATE
        ES_SYSTEM_REQUIRED = &H1    ' Stay in working state by resetting display idle timer
        ES_DISPLAY_REQUIRED = &H2   ' Force display on by resetting system idle timer
        ES_CONTINUOUS = &H80000000  ' Force this state until next ES_CONTINUOUS call and one of the other flags are cleared
    End Enum

    Public Sub wakeUp()
        SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED Or EXECUTION_STATE.ES_DISPLAY_REQUIRED)
        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS)
    End Sub
End Module
