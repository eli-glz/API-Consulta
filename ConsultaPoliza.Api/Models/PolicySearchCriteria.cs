namespace ConsultaPoliza.Api.Models;

public sealed record PolicySearchCriteria(
    int RamoCodigo,
    long NumeroPoliza,
    int NumeroCertificado,
    DateTime FechaEfecto);
