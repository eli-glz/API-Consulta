namespace ConsultaPoliza.Api.Models;

public sealed record PolicyResponse(
    string NumeroPoliza,
    string Estado,
    string Asegurado,
    string Producto,
    DateTime? VigenciaDesde,
    DateTime? VigenciaHasta,
    string? Ramo = null,
    string? NumeroCliente = null,
    int? CoberturaPrincipal = null,
    int? NumeroCertificado = null,
    int? RamoCodigo = null,
    int? ProductoCodigo = null,
    DateTime? FechaEfecto = null);
