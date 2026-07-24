Imports System.Diagnostics
Imports System.Net
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports ConsultaPoliza.Api.Infrastructure
Imports ConsultaPoliza.Api.Models
Imports ConsultaPoliza.Api.Services
Imports Oracle.ManagedDataAccess.Client

Namespace ConsultaPoliza.Api.Controllers
    <RoutePrefix("api/ramos")>
    Public Class BranchesController
        Inherits ApiController

        Private ReadOnly _branchRepository As IBranchRepository

        Public Sub New()
            Me.New(ServiceRegistry.BranchRepository)
        End Sub

        Friend Sub New(branchRepository As IBranchRepository)
            _branchRepository = branchRepository
        End Sub

        <HttpGet>
        <Route("")>
        Public Async Function GetAll() As Task(Of IHttpActionResult)
            Try
                Dim branches = Await _branchRepository.GetAllAsync(CancellationToken.None)
                Return Ok(branches)
            Catch ex As OracleException When IsOracleConnectionError(ex)
                Trace.TraceError("Oracle connection failed while loading branches: {0}", ex)
                Return Content(
                    HttpStatusCode.ServiceUnavailable,
                    New ErrorResponse With {
                        .Title = "Oracle no esta disponible.",
                        .Detail = "No se pudo conectar con Oracle para cargar los ramos. Verifique la red o VPN."
                    })
            Catch ex As Exception
                Trace.TraceError("Error loading branches: {0}", ex)
                Return Content(
                    HttpStatusCode.InternalServerError,
                    New ErrorResponse With {
                        .Title = "No se pudieron cargar los ramos.",
                        .Detail = "Ocurrio un error tecnico al consultar los ramos en Oracle."
                    })
            End Try
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
