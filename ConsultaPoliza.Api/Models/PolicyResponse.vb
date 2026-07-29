Namespace ConsultaPoliza.Api.Models
    Public Class PolicyResponse
        Public Sub New()
        End Sub

        Public Sub New(
            numeroPoliza As String,
            estado As String,
            asegurado As String,
            producto As String,
            vigenciaDesde As DateTime?,
            vigenciaHasta As DateTime?,
            ramo As String,
            numeroCliente As String,
            coberturaPrincipal As Integer?,
            numeroCertificado As Integer?,
            ramoCodigo As Integer?,
            productoCodigo As Integer?,
            fechaEfecto As DateTime?,
            frecuenciaPago As String,
            estadoDetalle As PolicyStatusResponse,
            roles As List(Of PolicyRoleResponse))

            Me.NumeroPoliza = numeroPoliza
            Me.Estado = estado
            Me.Asegurado = asegurado
            Me.Producto = producto
            Me.VigenciaDesde = vigenciaDesde
            Me.VigenciaHasta = vigenciaHasta
            Me.Ramo = ramo
            Me.NumeroCliente = numeroCliente
            Me.CoberturaPrincipal = coberturaPrincipal
            Me.NumeroCertificado = numeroCertificado
            Me.RamoCodigo = ramoCodigo
            Me.ProductoCodigo = productoCodigo
            Me.FechaEfecto = fechaEfecto
            Me.FrecuenciaPago = frecuenciaPago
            Me.EstadoDetalle = estadoDetalle
            Me.Roles = If(roles, New List(Of PolicyRoleResponse)())
        End Sub

        Public Property NumeroPoliza As String = ""
        Public Property Estado As String = ""
        Public Property Asegurado As String = ""
        Public Property Producto As String = ""
        Public Property VigenciaDesde As DateTime?
        Public Property VigenciaHasta As DateTime?
        Public Property Ramo As String
        Public Property NumeroCliente As String
        Public Property CoberturaPrincipal As Integer?
        Public Property NumeroCertificado As Integer?
        Public Property RamoCodigo As Integer?
        Public Property ProductoCodigo As Integer?
        Public Property FechaEfecto As DateTime?
        Public Property FrecuenciaPago As String = ""
        Public Property EstadoDetalle As PolicyStatusResponse
        Public Property Roles As List(Of PolicyRoleResponse) = New List(Of PolicyRoleResponse)()
    End Class

    Public Class PolicyStatusResponse
        Public Sub New()
        End Sub

        Public Sub New(
            estado As String,
            motivoAnulacion As String,
            fechaEfectivaAnulacion As DateTime?,
            motivoSuspension As String)

            Me.Estado = estado
            Me.MotivoAnulacion = motivoAnulacion
            Me.FechaEfectivaAnulacion = fechaEfectivaAnulacion
            Me.MotivoSuspension = motivoSuspension
        End Sub

        Public Property Estado As String = ""
        Public Property MotivoAnulacion As String = ""
        Public Property FechaEfectivaAnulacion As DateTime?
        Public Property MotivoSuspension As String = ""
    End Class

    Public Class PolicyRoleResponse
        Public Sub New()
        End Sub

        Public Sub New(
            rol As String,
            clienteCodigo As String,
            cliente As String,
            fechaAnulacion As DateTime?,
            fechaEfecto As DateTime?,
            direcciones As List(Of PolicyRoleAddressResponse))

            Me.Rol = rol
            Me.ClienteCodigo = clienteCodigo
            Me.Cliente = cliente
            Me.FechaAnulacion = fechaAnulacion
            Me.FechaEfecto = fechaEfecto
            Me.Direcciones = If(direcciones, New List(Of PolicyRoleAddressResponse)())
        End Sub

        Public Property Rol As String = ""
        Public Property ClienteCodigo As String = ""
        Public Property Cliente As String = ""
        Public Property FechaAnulacion As DateTime?
        Public Property FechaEfecto As DateTime?
        Public Property Direcciones As List(Of PolicyRoleAddressResponse) = New List(Of PolicyRoleAddressResponse)()
    End Class

    Public Class PolicyRoleAddressResponse
        Public Sub New()
        End Sub

        Public Sub New(
            tipo As String,
            direccion As String,
            codigoPostal As String,
            localidad As String,
            provincia As String,
            pais As String,
            email As String)

            Me.Tipo = tipo
            Me.Direccion = direccion
            Me.CodigoPostal = codigoPostal
            Me.Localidad = localidad
            Me.Provincia = provincia
            Me.Pais = pais
            Me.Email = email
        End Sub

        Public Property Tipo As String = ""
        Public Property Direccion As String = ""
        Public Property CodigoPostal As String = ""
        Public Property Localidad As String = ""
        Public Property Provincia As String = ""
        Public Property Pais As String = ""
        Public Property Email As String = ""
    End Class
End Namespace
