namespace ConsultaPoliza.Api.Models;

public sealed record PolicyBaseData(
    long NumeroPoliza,
    int NumeroCertificado,
    string TipoCertificado,
    int RamoCodigo,
    int ProductoCodigo,
    string ClienteCodigo,
    DateTime? FechaInicio,
    DateTime? FechaFin,
    string? TipoModulo);
