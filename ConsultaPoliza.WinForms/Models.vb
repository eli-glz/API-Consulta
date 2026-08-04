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
    Public Property FrecuenciaPago As String
    Public Property EstadoDetalle As PolicyStatusResponse
    Public Property Roles As List(Of PolicyRoleResponse)
    Public Property Recibos As List(Of PolicyReceiptResponse)
End Class

Public Class PolicyStatusResponse
    Public Property Estado As String
    Public Property MotivoAnulacion As String
    Public Property FechaEfectivaAnulacion As DateTime?
    Public Property MotivoSuspension As String
End Class

Public Class PolicyRoleResponse
    Public Property Rol As String
    Public Property ClienteCodigo As String
    Public Property Cliente As String
    Public Property FechaAnulacion As DateTime?
    Public Property FechaEfecto As DateTime?
    Public Property Direcciones As List(Of PolicyRoleAddressResponse)
End Class

Public Class PolicyRoleAddressResponse
    Public Property Tipo As String
    Public Property Direccion As String
    Public Property CodigoPostal As String
    Public Property Localidad As String
    Public Property Provincia As String
    Public Property Pais As String
    Public Property Email As String
End Class

Public Class PolicyReceiptResponse
    Public Property NumeroRecibo As Long
    Public Property FechaEmision As DateTime?
    Public Property InicioVigencia As DateTime?
    Public Property FinVigencia As DateTime?
    Public Property FechaUltimoPago As DateTime?
    Public Property FechaVencimiento As DateTime?
    Public Property Moneda As String
    Public Property Premio As Decimal?
    Public Property Saldo As Decimal?
    Public Property Estado As String
    Public Property FechaAnulacion As DateTime?
    Public Property CodigoAnulacion As String
    Public Property Situacion As String
    Public Property FechaSegundoVencimiento As DateTime?
End Class