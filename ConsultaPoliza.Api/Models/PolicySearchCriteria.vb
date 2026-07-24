Namespace ConsultaPoliza.Api.Models
    Public Class PolicySearchCriteria
        Public Sub New(
            ramoCodigo As Integer,
            numeroPoliza As Long,
            numeroCertificado As Integer,
            fechaEfecto As DateTime)

            Me.RamoCodigo = ramoCodigo
            Me.NumeroPoliza = numeroPoliza
            Me.NumeroCertificado = numeroCertificado
            Me.FechaEfecto = fechaEfecto
        End Sub

        Public ReadOnly Property RamoCodigo As Integer
        Public ReadOnly Property NumeroPoliza As Long
        Public ReadOnly Property NumeroCertificado As Integer
        Public ReadOnly Property FechaEfecto As DateTime
    End Class
End Namespace
