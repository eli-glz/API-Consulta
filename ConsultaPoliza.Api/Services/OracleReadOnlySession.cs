using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace ConsultaPoliza.Api.Services;

public static class OracleReadOnlySession
{
    public static async Task BeginAsync(OracleConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SET TRANSACTION READ ONLY";
        command.CommandType = CommandType.Text;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public static async Task RollbackAsync(OracleConnection connection)
    {
        if (connection.State != ConnectionState.Open)
        {
            return;
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "ROLLBACK";
            command.CommandType = CommandType.Text;
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // Best effort cleanup; callers should preserve the original Oracle error.
        }
    }
}
