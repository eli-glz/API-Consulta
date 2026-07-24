Imports System.Data
Imports System.Threading
Imports Oracle.ManagedDataAccess.Client

Namespace ConsultaPoliza.Api.Services
    Public Module OracleReadOnlySession
        Public Sub Begin(connection As OracleConnection, cancellationToken As CancellationToken)
            cancellationToken.ThrowIfCancellationRequested()

            Using command = connection.CreateCommand()
                command.CommandText = "SET TRANSACTION READ ONLY"
                command.CommandType = CommandType.Text
                command.ExecuteNonQuery()
            End Using
        End Sub

        Public Sub Rollback(connection As OracleConnection)
            If connection.State <> ConnectionState.Open Then
                Return
            End If

            Try
                Using command = connection.CreateCommand()
                    command.CommandText = "ROLLBACK"
                    command.CommandType = CommandType.Text
                    command.ExecuteNonQuery()
                End Using
            Catch
                ' Best effort cleanup; callers should preserve the original Oracle error.
            End Try
        End Sub
    End Module
End Namespace
