using ConsultaPoliza.Api.Models;
using ConsultaPoliza.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace ConsultaPoliza.Api.Controllers;

[ApiController]
[Route("api/ramos")]
public sealed class BranchesController : ControllerBase
{
    private readonly IBranchRepository _branchRepository;
    private readonly ILogger<BranchesController> _logger;

    public BranchesController(IBranchRepository branchRepository, ILogger<BranchesController> logger)
    {
        _branchRepository = branchRepository;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BranchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<BranchResponse>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _branchRepository.GetAllAsync(cancellationToken));
        }
        catch (OracleException ex) when (IsOracleConnectionError(ex))
        {
            _logger.LogError(ex, "Oracle connection failed while loading branches.");
            return Problem(
                title: "Oracle no esta disponible.",
                detail: "No se pudo conectar con Oracle para cargar los ramos. Verifique la red o VPN.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading branches.");
            return Problem(
                title: "No se pudieron cargar los ramos.",
                detail: "Ocurrio un error tecnico al consultar los ramos en Oracle.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static bool IsOracleConnectionError(OracleException exception)
    {
        return exception.Number is 50201 or 50232
            || exception.Errors.Cast<OracleError>().Any(error => error.Number is 50201 or 50232);
    }
}
