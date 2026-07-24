using Oracle.ManagedDataAccess.Client;
using System.Globalization;
using System.Data;

namespace ConsultaPoliza.Api.Services;

public interface IReaGeneralPackage
{
    Task<string> GetBranchDescriptionAsync(OracleConnection connection, int branchCode, CancellationToken cancellationToken);

    Task<string> GetProductDescriptionAsync(OracleConnection connection, int branchCode, int productCode, CancellationToken cancellationToken);

    Task<string> GetClientNameAsync(OracleConnection connection, string clientCode, CancellationToken cancellationToken);

    Task<int?> GetPrimaryCoverageAsync(
        OracleConnection connection,
        string certificateType,
        int branchCode,
        int productCode,
        long policyNumber,
        DateTime? effectiveDate,
        string? moduleType,
        CancellationToken cancellationToken);
}

public sealed class ReaGeneralPackage : IReaGeneralPackage
{
    public async Task<string> GetBranchDescriptionAsync(OracleConnection connection, int branchCode, CancellationToken cancellationToken)
    {
        return await ExecuteStringFunctionAsync(
            connection,
            "REAGENERALPKG.REASBRANCH",
            200,
            cancellationToken,
            command => command.Parameters.Add("NBRANCH", OracleDbType.Decimal, branchCode, ParameterDirection.Input));
    }

    public async Task<string> GetProductDescriptionAsync(OracleConnection connection, int branchCode, int productCode, CancellationToken cancellationToken)
    {
        return await ExecuteStringFunctionAsync(
            connection,
            "REAGENERALPKG.REASPRODUCT",
            200,
            cancellationToken,
            command =>
            {
                command.Parameters.Add("NBRANCH", OracleDbType.Decimal, branchCode, ParameterDirection.Input);
                command.Parameters.Add("NPRODUCT", OracleDbType.Decimal, productCode, ParameterDirection.Input);
            });
    }

    public async Task<string> GetClientNameAsync(OracleConnection connection, string clientCode, CancellationToken cancellationToken)
    {
        return await ExecuteStringFunctionAsync(
            connection,
            "REAGENERALPKG.REANAMECLI",
            200,
            cancellationToken,
            command => command.Parameters.Add("SCLIENT", OracleDbType.Varchar2, clientCode, ParameterDirection.Input));
    }

    public async Task<int?> GetPrimaryCoverageAsync(
        OracleConnection connection,
        string certificateType,
        int branchCode,
        int productCode,
        long policyNumber,
        DateTime? effectiveDate,
        string? moduleType,
        CancellationToken cancellationToken)
    {
        if (!effectiveDate.HasValue)
        {
            return null;
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "REAGENERALPKG.REACOVER_PPAL";
        command.CommandType = CommandType.StoredProcedure;

        var returnValue = command.Parameters.Add("return_value", OracleDbType.Decimal);
        returnValue.Direction = ParameterDirection.ReturnValue;

        command.Parameters.Add("SCERTYPE", OracleDbType.Varchar2, certificateType, ParameterDirection.Input);
        command.Parameters.Add("NBRANCH", OracleDbType.Decimal, branchCode, ParameterDirection.Input);
        command.Parameters.Add("NPRODUCT", OracleDbType.Decimal, productCode, ParameterDirection.Input);
        command.Parameters.Add("NPOLICY", OracleDbType.Decimal, policyNumber, ParameterDirection.Input);
        command.Parameters.Add("DEFFECDATE", OracleDbType.Date, effectiveDate.Value, ParameterDirection.Input);
        command.Parameters.Add("STYP_MODULE", OracleDbType.Varchar2, string.IsNullOrWhiteSpace(moduleType) ? DBNull.Value : moduleType, ParameterDirection.Input);

        await command.ExecuteNonQueryAsync(cancellationToken);

        if (returnValue.Value is null || returnValue.Value == DBNull.Value)
        {
            return null;
        }

        return Convert.ToInt32(returnValue.Value.ToString(), CultureInfo.InvariantCulture);
    }

    private static async Task<string> ExecuteStringFunctionAsync(
        OracleConnection connection,
        string functionName,
        int returnSize,
        CancellationToken cancellationToken,
        Action<OracleCommand> addInputParameters)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = functionName;
        command.CommandType = CommandType.StoredProcedure;

        var returnValue = command.Parameters.Add("return_value", OracleDbType.Varchar2, returnSize);
        returnValue.Direction = ParameterDirection.ReturnValue;

        addInputParameters(command);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return Convert.ToString(returnValue.Value)?.Trim() ?? "";
    }
}
