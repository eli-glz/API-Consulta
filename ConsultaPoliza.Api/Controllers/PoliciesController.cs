using ConsultaPoliza.Api.Models;
using ConsultaPoliza.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ConsultaPoliza.Api.Controllers;

[ApiController]
[Route("api/polizas")]
public sealed partial class PoliciesController : ControllerBase
{
    private readonly IPolicyRepository _policyRepository;
    private readonly ILogger<PoliciesController> _logger;

    public PoliciesController(IPolicyRepository policyRepository, ILogger<PoliciesController> logger)
    {
        _policyRepository = policyRepository;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PolicyResponse>> Search(
        [FromQuery] int? ramo,
        [FromQuery] string? numeroPoliza,
        [FromQuery] int? certificado,
        [FromQuery] DateOnly? fechaEfecto,
        CancellationToken cancellationToken)
    {
        if (!ramo.HasValue || ramo.Value <= 0)
        {
            return BadRequest(new { message = "Seleccione un ramo valido." });
        }

        if (!IsValidPolicyNumber(numeroPoliza))
        {
            return BadRequest(new { message = "El numero de poliza es obligatorio y debe contener solamente numeros." });
        }

        if (!certificado.HasValue || certificado.Value < 0)
        {
            return BadRequest(new { message = "El certificado es obligatorio y no puede ser negativo." });
        }

        if (!fechaEfecto.HasValue)
        {
            return BadRequest(new { message = "La fecha de efecto es obligatoria." });
        }

        var criteria = new PolicySearchCriteria(
            RamoCodigo: ramo.Value,
            NumeroPoliza: long.Parse(numeroPoliza!.Trim(), CultureInfo.InvariantCulture),
            NumeroCertificado: certificado.Value,
            FechaEfecto: fechaEfecto.Value.ToDateTime(TimeOnly.MinValue));

        return await FindPolicyAsync(
            () => _policyRepository.SearchAsync(criteria, cancellationToken),
            numeroPoliza,
            ramo,
            certificado);
    }

    [HttpGet("{numeroPoliza}")]
    [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PolicyResponse>> GetByNumber(string numeroPoliza, CancellationToken cancellationToken)
    {
        if (!IsValidPolicyNumber(numeroPoliza))
        {
            return BadRequest(new { message = "El numero de poliza es obligatorio y debe contener solamente numeros." });
        }

        return await FindPolicyAsync(
            () => _policyRepository.GetByNumberAsync(numeroPoliza.Trim(), cancellationToken),
            numeroPoliza,
            branchCode: null,
            certificateNumber: null);
    }

    private async Task<ActionResult<PolicyResponse>> FindPolicyAsync(
        Func<Task<PolicyResponse?>> findPolicy,
        string policyNumber,
        int? branchCode,
        int? certificateNumber)
    {
        try
        {
            var policy = await findPolicy();
            if (policy is null)
            {
                return NotFound(new { message = "No se encontro una poliza con los criterios indicados." });
            }

            return Ok(policy);
        }
        catch (OracleException ex) when (IsOracleConnectionError(ex))
        {
            _logger.LogError(
                ex,
                "Oracle connection failed while consulting policy {PolicyNumber}, branch {BranchCode}, certificate {CertificateNumber}.",
                policyNumber,
                branchCode,
                certificateNumber);
            return Problem(
                title: "Oracle no esta disponible.",
                detail: "No se pudo conectar con Oracle. Verifique la red o VPN y que el servidor este disponible.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error consulting policy {PolicyNumber}, branch {BranchCode}, certificate {CertificateNumber}.",
                policyNumber,
                branchCode,
                certificateNumber);
            return Problem(
                title: "No se pudo consultar la poliza.",
                detail: "Ocurrio un error tecnico al consultar Oracle.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static bool IsValidPolicyNumber(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Length <= 18
            && PolicyNumberRegex().IsMatch(value)
            && long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var policyNumber)
            && policyNumber > 0;
    }

    [GeneratedRegex("^[0-9]+$")]
    private static partial Regex PolicyNumberRegex();

    private static bool IsOracleConnectionError(OracleException exception)
    {
        return exception.Number is 50201 or 50232
            || exception.Errors.Cast<OracleError>().Any(error => error.Number is 50201 or 50232);
    }
}

/*
Cuando llega GET /api/polizas/203561, ASP.NET entra en GetByNumber.
Primero valida que 203561 sea numérico, positivo y razonable.
Después llama a _policyRepository.GetByNumberAsync, que es donde realmente se abre Oracle y se consulta la póliza.
Si no hay datos, responde 404.
Si hay datos, responde 200.
Si Oracle no está disponible, responde 503.
Si ocurre otro error técnico, responde 500.
El controlador no contiene SQL ni lógica de Oracle directa.
Su responsabilidad está bien separada: validar HTTP, coordinar el repositorio y traducir resultados/errores a respuestas HTTP.
*/
