using ConsultaPoliza.Api.Models;
using Oracle.ManagedDataAccess.Client;

namespace ConsultaPoliza.Api.Services;

public interface IPolicyResponseBuilder
{
    Task<PolicyResponse> BuildAsync(
        OracleConnection connection,
        PolicyBaseData policy,
        DateTime? effectiveDate,
        CancellationToken cancellationToken);
}

public sealed class PolicyResponseBuilder : IPolicyResponseBuilder
{
    private readonly IReaGeneralPackage _reaGeneralPackage;

    public PolicyResponseBuilder(IReaGeneralPackage reaGeneralPackage)
    {
        _reaGeneralPackage = reaGeneralPackage;
    }

    public async Task<PolicyResponse> BuildAsync(
        OracleConnection connection,
        PolicyBaseData policy,
        DateTime? effectiveDate,
        CancellationToken cancellationToken)
    {
        var branch = await _reaGeneralPackage.GetBranchDescriptionAsync(connection, policy.RamoCodigo, cancellationToken);
        var product = await _reaGeneralPackage.GetProductDescriptionAsync(connection, policy.RamoCodigo, policy.ProductoCodigo, cancellationToken);
        var clientName = await _reaGeneralPackage.GetClientNameAsync(connection, policy.ClienteCodigo, cancellationToken);
        var coverageEffectiveDate = effectiveDate ?? policy.FechaInicio;
        var primaryCoverage = await _reaGeneralPackage.GetPrimaryCoverageAsync(
            connection,
            policy.TipoCertificado,
            policy.RamoCodigo,
            policy.ProductoCodigo,
            policy.NumeroPoliza,
            coverageEffectiveDate,
            policy.TipoModulo,
            cancellationToken);

        return new PolicyResponse(
            NumeroPoliza: policy.NumeroPoliza.ToString(),
            Estado: "",
            Asegurado: clientName,
            Producto: product,
            VigenciaDesde: policy.FechaInicio,
            VigenciaHasta: policy.FechaFin,
            Ramo: branch,
            NumeroCliente: policy.ClienteCodigo,
            CoberturaPrincipal: primaryCoverage,
            NumeroCertificado: policy.NumeroCertificado,
            RamoCodigo: policy.RamoCodigo,
            ProductoCodigo: policy.ProductoCodigo,
            FechaEfecto: effectiveDate);
    }
}
