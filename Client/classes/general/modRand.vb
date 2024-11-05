Module modRand
    'random number generator
    Public Function rand(ByVal upper As Integer, ByVal lower As Integer) As Integer
        Randomize()
        rand = Int((upper - lower + 1) * Rnd() + lower)
    End Function
End Module
