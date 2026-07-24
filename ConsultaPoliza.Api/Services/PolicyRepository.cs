using ConsultaPoliza.Api.Models;
using ConsultaPoliza.Api.Options;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace ConsultaPoliza.Api.Services;

public interface IPolicyRepository
{
    Task<PolicyResponse?> GetByNumberAsync(string policyNumber, CancellationToken cancellationToken);

    Task<PolicyResponse?> SearchAsync(PolicySearchCriteria criteria, CancellationToken cancellationToken);
}

public sealed class OraclePolicyRepository : IPolicyRepository
{
    private const string PolicyByNumberQuery = """
        SELECT PO.NPOLICY, CE.NCERTIF, PO.SCERTYPE, PO.NBRANCH,
               CE.NPRODUCT, CE.SCLIENT, CE.DSTARTDATE, CE.DEXPIRDAT,
               PO.STYP_MODULE
          FROM POLICY PO
          INNER JOIN CERTIFICAT CE
                  ON CE.NBRANCH = PO.NBRANCH
                 AND CE.NPOLICY = PO.NPOLICY
                 AND CE.SCERTYPE = PO.SCERTYPE
                 AND CE.SCLIENT = PO.SCLIENT
         WHERE PO.NPOLICY = :policyNumber
         ORDER BY CASE WHEN CE.NCERTIF = 0 THEN 0 ELSE 1 END, CE.NCERTIF
         FETCH FIRST 1 ROWS ONLY
        """;

    private const string PolicySearchQuery = """
        SELECT PO.NPOLICY, CE.NCERTIF, PO.SCERTYPE, PO.NBRANCH,
               CE.NPRODUCT, CE.SCLIENT, CE.DSTARTDATE, CE.DEXPIRDAT,
               PO.STYP_MODULE
          FROM POLICY PO
          INNER JOIN CERTIFICAT CE
                  ON CE.NBRANCH = PO.NBRANCH
                 AND CE.NPOLICY = PO.NPOLICY
                 AND CE.SCERTYPE = PO.SCERTYPE
                 AND CE.SCLIENT = PO.SCLIENT
         WHERE PO.NBRANCH = :branchCode
           AND PO.NPOLICY = :policyNumber
           AND CE.NCERTIF = :certificateNumber
         ORDER BY CE.DSTARTDATE DESC
         FETCH FIRST 1 ROWS ONLY
        """;

    private readonly OraclePolicyOptions _options;
    private readonly IPolicyResponseBuilder _policyResponseBuilder;

    public OraclePolicyRepository(IOptions<OraclePolicyOptions> options, IPolicyResponseBuilder policyResponseBuilder)
    {
        _options = options.Value;
        _policyResponseBuilder = policyResponseBuilder;
    }

    public async Task<PolicyResponse?> GetByNumberAsync(string policyNumber, CancellationToken cancellationToken)
    {
        var parsedPolicyNumber = long.Parse(policyNumber, CultureInfo.InvariantCulture);

        return await QueryAsync(
            PolicyByNumberQuery,
            command =>
            {
                command.Parameters.Add(
                    "policyNumber",
                    OracleDbType.Decimal,
                    parsedPolicyNumber,
                    ParameterDirection.Input);
            },
            effectiveDate: null,
            cancellationToken);
    }

    public async Task<PolicyResponse?> SearchAsync(PolicySearchCriteria criteria, CancellationToken cancellationToken)
    {
        return await QueryAsync(
            PolicySearchQuery,
            command =>
            {
                command.Parameters.Add(
                    "branchCode",
                    OracleDbType.Decimal,
                    criteria.RamoCodigo,
                    ParameterDirection.Input);
                command.Parameters.Add(
                    "policyNumber",
                    OracleDbType.Decimal,
                    criteria.NumeroPoliza,
                    ParameterDirection.Input);
                command.Parameters.Add(
                    "certificateNumber",
                    OracleDbType.Decimal,
                    criteria.NumeroCertificado,
                    ParameterDirection.Input);
            },
            criteria.FechaEfecto,
            cancellationToken);
    }

    private async Task<PolicyResponse?> QueryAsync(
        string query,
        Action<OracleCommand> addParameters,
        DateTime? effectiveDate,
        CancellationToken cancellationToken)
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
            PolicyBaseData baseData;

            await using var command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.BindByName = true;
            addParameters(command);

            await using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken))
                {
                    return null;
                }

                baseData = new PolicyBaseData(
                    NumeroPoliza: GetLong(reader, "NPOLICY"),
                    NumeroCertificado: GetInt(reader, "NCERTIF"),
                    TipoCertificado: GetString(reader, "SCERTYPE"),
                    RamoCodigo: GetInt(reader, "NBRANCH"),
                    ProductoCodigo: GetInt(reader, "NPRODUCT"),
                    ClienteCodigo: GetString(reader, "SCLIENT"),
                    FechaInicio: GetDateTime(reader, "DSTARTDATE"),
                    FechaFin: GetDateTime(reader, "DEXPIRDAT"),
                    TipoModulo: GetStringOrNull(reader, "STYP_MODULE"));
            }

            return await _policyResponseBuilder.BuildAsync(connection, baseData, effectiveDate, cancellationToken);
        }
        finally
        {
            await OracleReadOnlySession.RollbackAsync(connection);
        }
    }

    private static string GetString(DbDataReader reader, params string[] names)
    {
        if (!TryGetOrdinal(reader, names, out var ordinal) || reader.IsDBNull(ordinal))
        {
            return "";
        }

        return Convert.ToString(reader.GetValue(ordinal)) ?? "";
    }

    private static DateTime? GetDateTime(DbDataReader reader, params string[] names)
    {
        if (!TryGetOrdinal(reader, names, out var ordinal) || reader.IsDBNull(ordinal))
        {
            return null;
        }

        return Convert.ToDateTime(reader.GetValue(ordinal));
    }

    private static string? GetStringOrNull(DbDataReader reader, params string[] names)
    {
        if (!TryGetOrdinal(reader, names, out var ordinal) || reader.IsDBNull(ordinal))
        {
            return null;
        }

        return Convert.ToString(reader.GetValue(ordinal));
    }

    private static int GetInt(DbDataReader reader, params string[] names)
    {
        if (!TryGetOrdinal(reader, names, out var ordinal) || reader.IsDBNull(ordinal))
        {
            throw new InvalidOperationException($"Oracle result cursor did not return required number field: {string.Join(", ", names)}.");
        }

        return Convert.ToInt32(reader.GetValue(ordinal));
    }

    private static long GetLong(DbDataReader reader, params string[] names)
    {
        if (!TryGetOrdinal(reader, names, out var ordinal) || reader.IsDBNull(ordinal))
        {
            throw new InvalidOperationException($"Oracle result cursor did not return required number field: {string.Join(", ", names)}.");
        }

        return Convert.ToInt64(reader.GetValue(ordinal));
    }

    private static bool TryGetOrdinal(DbDataReader reader, string[] names, out int ordinal)
    {
        for (var index = 0; index < reader.FieldCount; index++)
        {
            var columnName = reader.GetName(index);
            if (names.Any(name => string.Equals(name, columnName, StringComparison.OrdinalIgnoreCase)))
            {
                ordinal = index;
                return true;
            }
        }

        ordinal = -1;
        return false;
    }
}
