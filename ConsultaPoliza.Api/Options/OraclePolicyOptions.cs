namespace ConsultaPoliza.Api.Options;

public sealed class OraclePolicyOptions
{
    public const string SectionName = "OraclePolicy";

    public string ConnectionString { get; set; } = "";
}
