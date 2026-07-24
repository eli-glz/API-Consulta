Imports System.Data
Imports System.Threading
Imports System.Threading.Tasks
Imports ConsultaPoliza.Api.Models
Imports ConsultaPoliza.Api.Options
Imports Oracle.ManagedDataAccess.Client

Namespace ConsultaPoliza.Api.Services
    Public Interface IBranchRepository
        Function GetAllAsync(cancellationToken As CancellationToken) As Task(Of IReadOnlyList(Of BranchResponse))
    End Interface

    Public Class OracleBranchRepository
        Implements IBranchRepository

        Private Const BranchQuery As String =
            "SELECT NBRANCH, TRIM(SDESCRIPT) AS SDESCRIPT " &
            "  FROM TABLE10 " &
            " WHERE NBRANCH IS NOT NULL " &
            "   AND SDESCRIPT IS NOT NULL " &
            " ORDER BY NBRANCH"

        Private ReadOnly _options As OraclePolicyOptions

        Public Sub New(options As OraclePolicyOptions)
            _options = options
        End Sub

        Public Function GetAllAsync(cancellationToken As CancellationToken) As Task(Of IReadOnlyList(Of BranchResponse)) Implements IBranchRepository.GetAllAsync
            Return Task.Run(Function() GetAll(cancellationToken), cancellationToken)
        End Function

        Private Function GetAll(cancellationToken As CancellationToken) As IReadOnlyList(Of BranchResponse)
            If String.IsNullOrWhiteSpace(_options.ConnectionString) Then
                Throw New InvalidOperationException("OraclePolicy:ConnectionString is not configured.")
            End If

            cancellationToken.ThrowIfCancellationRequested()

            Using connection As New OracleConnection(_options.ConnectionString)
                connection.Open()
                OracleReadOnlySession.Begin(connection, cancellationToken)

                Try
                    Dim branches As New List(Of BranchResponse)()

                    Using command = connection.CreateCommand()
                        command.CommandText = BranchQuery
                        command.CommandType = CommandType.Text

                        Using reader = command.ExecuteReader()
                            While reader.Read()
                                cancellationToken.ThrowIfCancellationRequested()
                                branches.Add(New BranchResponse(
                                    Convert.ToInt32(reader("NBRANCH")),
                                    If(Convert.ToString(reader("SDESCRIPT")), "").Trim()))
                            End While
                        End Using
                    End Using

                    Return branches
                Finally
                    OracleReadOnlySession.Rollback(connection)
                End Try
            End Using
        End Function
    End Class
End Namespace
