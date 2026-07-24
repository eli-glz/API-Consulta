Imports System.Diagnostics
Imports System.Globalization
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports ConsultaPoliza.Api.Infrastructure
Imports ConsultaPoliza.Api.Models
Imports ConsultaPoliza.Api.Services
Imports Oracle.ManagedDataAccess.Client

Namespace ConsultaPoliza.Api.Controllers
    <RoutePrefix("api/polizas")>
    Public Class PoliciesController
        Inherits ApiController

        Private Shared ReadOnly PolicyNumberPattern As New Regex("^[0-9]+$", RegexOptions.Compiled)
        Private ReadOnly _policyRepository As IPolicyRepository

        Public Sub New()
            Me.New(ServiceRegistry.PolicyRepository)
        End Sub

        Friend Sub New(policyRepository As IPolicyRepository)
            _policyRepository = policyRepository
        End Sub

        <HttpGet>
        <Route("")>
        Public Async Function Search(
            ramo As Integer?,
            numeroPoliza As String,
            certificado As Integer?,
            fechaEfecto As DateTime?) As Task(Of IHttpActionResult)

            If Not ramo.HasValue OrElse ramo.Value <= 0 Then
                Return BadRequestMessage("Seleccione un ramo valido.")
            End If

            If Not IsValidPolicyNumber(numeroPoliza) Then
                Return BadRequestMessage("El numero de poliza es obligatorio y debe contener solamente numeros.")
            End If

            If Not certificado.HasValue OrElse certificado.Value < 0 Then
                Return BadRequestMessage("El certificado es obligatorio y no puede ser negativo.")
            End If

            If Not fechaEfecto.HasValue Then
                Return BadRequestMessage("La fecha de efecto es obligatoria.")
            End If

            Dim criteria = New PolicySearchCriteria(
                ramo.Value,
                Long.Parse(numeroPoliza.Trim(), CultureInfo.InvariantCulture),
                certificado.Value,
                fechaEfecto.Value.Date)

            Return Await FindPolicyAsync(
                Function() _policyRepository.SearchAsync(criteria, CancellationToken.None),
                numeroPoliza,
                ramo,
                certificado)
        End Function

        <HttpGet>
        <Route("{numeroPoliza}")>
        Public Async Function GetByNumber(numeroPoliza As String) As Task(Of IHttpActionResult)
            If Not IsValidPolicyNumber(numeroPoliza) Then
                Return BadRequestMessage("El numero de poliza es obligatorio y debe contener solamente numeros.")
            End If

            Return Await FindPolicyAsync(
                Function() _policyRepository.GetByNumberAsync(numeroPoliza.Trim(), CancellationToken.None),
                numeroPoliza,
                Nothing,
                Nothing)
        End Function

        Private Function BadRequestMessage(message As String) As IHttpActionResult
            Return Content(
                HttpStatusCode.BadRequest,
                New ErrorResponse With {
                    .Message = message
                })
        End Function

        Private Async Function FindPolicyAsync(
            findPolicy As Func(Of Task(Of PolicyResponse)),
            policyNumber As String,
            branchCode As Integer?,
            certificateNumber As Integer?) As Task(Of IHttpActionResult)

            Try
                Dim policy = Await findPolicy()
                If policy Is Nothing Then
                    Return Content(
                        HttpStatusCode.NotFound,
                        New ErrorResponse With {
                            .Message = "No se encontro una poliza con los criterios indicados."
                        })
                End If

                Return Ok(policy)
            Catch ex As OracleException When IsOracleConnectionError(ex)
                Trace.TraceError(
                    "Oracle connection failed while consulting policy {0}, branch {1}, certificate {2}: {3}",
                    policyNumber,
                    If(branchCode.HasValue, branchCode.Value.ToString(CultureInfo.InvariantCulture), ""),
                    If(certificateNumber.HasValue, certificateNumber.Value.ToString(CultureInfo.InvariantCulture), ""),
                    ex)

                Return Content(
                    HttpStatusCode.ServiceUnavailable,
                    New ErrorResponse With {
                        .Title = "Oracle no esta disponible.",
                        .Detail = "No se pudo conectar con Oracle. Verifique la red o VPN y que el servidor este disponible."
                    })
            Catch ex As Exception
                Trace.TraceError(
                    "Error consulting policy {0}, branch {1}, certificate {2}: {3}",
                    policyNumber,
                    If(branchCode.HasValue, branchCode.Value.ToString(CultureInfo.InvariantCulture), ""),
                    If(certificateNumber.HasValue, certificateNumber.Value.ToString(CultureInfo.InvariantCulture), ""),
                    ex)

                Return Content(
                    HttpStatusCode.InternalServerError,
                    New ErrorResponse With {
                        .Title = "No se pudo consultar la poliza.",
                        .Detail = "Ocurrio un error tecnico al consultar Oracle."
                    })
            End Try
        End Function

        Private Shared Function IsValidPolicyNumber(value As String) As Boolean
            If String.IsNullOrWhiteSpace(value) OrElse value.Length > 18 Then
                Return False
            End If

            Dim policyNumber As Long
            Return PolicyNumberPattern.IsMatch(value) AndAlso
                Long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, policyNumber) AndAlso
                policyNumber > 0
        End Function

        Private Shared Function IsOracleConnectionError(exception As OracleException) As Boolean
            If exception.Number = 50201 OrElse exception.Number = 50232 Then
                Return True
            End If

            For Each [error] As OracleError In exception.Errors
                If [error].Number = 50201 OrElse [error].Number = 50232 Then
                    Return True
                End If
            Next

            Return False
        End Function
    End Class
End Namespace
