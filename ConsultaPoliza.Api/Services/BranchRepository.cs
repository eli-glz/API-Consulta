using ConsultaPoliza.Api.Models;
using ConsultaPoliza.Api.Options;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace ConsultaPoliza.Api.Services;

public interface IBranchRepository
{
    Task<IReadOnlyList<BranchResponse>> GetAllAsync(CancellationToken cancellationToken);
}

public sealed class OracleBranchRepository : IBranchRepository
{
    private const string BranchQuery = """
        SELECT NBRANCH, TRIM(SDESCRIPT) AS SDESCRIPT
          FROM TABLE10
         WHERE NBRANCH IS NOT NULL
           AND SDESCRIPT IS NOT NULL
         ORDER BY NBRANCH
        """;

    private readonly OraclePolicyOptions _options;

    public OracleBranchRepository(IOptions<OraclePolicyOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IReadOnlyList<BranchResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException("OraclePolicy:ConnectionString is not configured.");
        }

        await using var connection = new OracleConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await OracleReadOnlySession.BeginAsync(connection, cancellationToken);

        try
        {
            var branches = new List<BranchResponse>();
            await using var command = connection.CreateCommand();
            command.CommandText = BranchQuery;
            command.CommandType = CommandType.Text;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                branches.Add(new BranchResponse(
                    Codigo: Convert.ToInt32(reader["NBRANCH"]),
                    Descripcion: Convert.ToString(reader["SDESCRIPT"])?.Trim() ?? ""));
            }

            return branches;
        }
        finally
        {
            await OracleReadOnlySession.RollbackAsync(connection);
        }
    }
}
