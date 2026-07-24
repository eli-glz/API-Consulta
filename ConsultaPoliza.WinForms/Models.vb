Public Class BranchResponse
    Public Property Codigo As Integer
    Public Property Descripcion As String

    Public ReadOnly Property DisplayText As String
        Get
            Return Codigo.ToString() & "; " & Descripcion
        End Get
    End Property
End Class

Public Class PolicyResponse
    Public Property NumeroPoliza As String
    Public Property Estado As String
    Public Property Asegurado As String
    Public Property Producto As String
    Public Property VigenciaDesde As DateTime?
    Public Property VigenciaHasta As DateTime?
    Public Property Ramo As String
    Public Property NumeroCliente As String
    Public Property CoberturaPrincipal As Integer?
    Public Property NumeroCertificado As Integer?
    Public Property RamoCodigo As Integer?
    Public Property ProductoCodigo As Integer?
    Public Property FechaEfecto As DateTime?
End Class
