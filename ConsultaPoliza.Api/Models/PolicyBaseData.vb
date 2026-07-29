Namespace ConsultaPoliza.Api.Models
    Public Class PolicyBaseData
        Public Sub New(
            numeroPoliza As Long,
            numeroCertificado As Integer,
            tipoCertificado As String,
            ramoCodigo As Integer,
            productoCodigo As Integer,
            clienteCodigo As String,
            fechaInicio As DateTime?,
            fechaFin As DateTime?,
            tipoModulo As String,
            estadoCodigo As String,
            estadoDescripcion As String,
            estadoDetalleCodigo As Integer?,
            estadoDetalleDescripcion As String,
            motivoAnulacionCodigo As Integer?,
            motivoAnulacionDescripcion As String,
            fechaAnulacion As DateTime?,
            motivoSuspensionCodigo As Integer?,
            motivoSuspensionDescripcion As String,
            frecuenciaPagoCodigo as Integer?,
            frecuenciaPagoDescripcion as String)

            Me.NumeroPoliza = numeroPoliza
            Me.NumeroCertificado = numeroCertificado
            Me.TipoCertificado = tipoCertificado
            Me.RamoCodigo = ramoCodigo
            Me.ProductoCodigo = productoCodigo
            Me.ClienteCodigo = clienteCodigo
            Me.FechaInicio = fechaInicio
            Me.FechaFin = fechaFin
            Me.TipoModulo = tipoModulo
            Me.EstadoCodigo = estadoCodigo
            Me.EstadoDescripcion = estadoDescripcion
            Me.EstadoDetalleCodigo = estadoDetalleCodigo
            Me.EstadoDetalleDescripcion = estadoDetalleDescripcion
            Me.MotivoAnulacionCodigo = motivoAnulacionCodigo
            Me.MotivoAnulacionDescripcion = motivoAnulacionDescripcion
            Me.FechaAnulacion = fechaAnulacion
            Me.MotivoSuspensionCodigo = motivoSuspensionCodigo
            Me.MotivoSuspensionDescripcion = motivoSuspensionDescripcion
            Me.FrecuenciaPagoCodigo = frecuenciaPagoCodigo
            Me.FrecuenciaPagoDescripcion = frecuenciaPagoDescripcion
        End Sub

        Public ReadOnly Property NumeroPoliza As Long
        Public ReadOnly Property NumeroCertificado As Integer
        Public ReadOnly Property TipoCertificado As String
        Public ReadOnly Property RamoCodigo As Integer
        Public ReadOnly Property ProductoCodigo As Integer
        Public ReadOnly Property ClienteCodigo As String
        Public ReadOnly Property FechaInicio As DateTime?
        Public ReadOnly Property FechaFin As DateTime?
        Public ReadOnly Property TipoModulo As String
        Public ReadOnly Property EstadoCodigo As String
        Public ReadOnly Property EstadoDescripcion As String
        Public ReadOnly Property EstadoDetalleCodigo As Integer?
        Public ReadOnly Property EstadoDetalleDescripcion As String
        Public ReadOnly Property MotivoAnulacionCodigo As Integer?
        Public ReadOnly Property MotivoAnulacionDescripcion As String
        Public ReadOnly Property FechaAnulacion As DateTime?
        Public ReadOnly Property MotivoSuspensionCodigo As Integer?
        Public ReadOnly Property MotivoSuspensionDescripcion As String
        Public ReadOnly Property FrecuenciaPagoCodigo As Integer?
        Public ReadOnly Property FrecuenciaPagoDescripcion As String
    End Class
End Namespace
