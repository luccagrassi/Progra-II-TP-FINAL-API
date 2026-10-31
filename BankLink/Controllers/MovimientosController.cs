using Microsoft.AspNetCore.Mvc;
using Interfaces;
using Models;
using Microsoft.AspNetCore.Authorization;

namespace Controllers;

[ApiController]
[Authorize]
[Route("api/cuentas/{cuentaId}/[controller]")]
public class MovimientosController : ControllerBase
{
    private readonly IMovimientoService _movimientoService;

    public MovimientosController(IMovimientoService movimientoService)
    {
        _movimientoService = movimientoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Movimiento>>> GetByCuenta(int cuentaId)
    {
        var movimientos = await _movimientoService.ObtenerPorCuenta(cuentaId);
        if (movimientos == null)
        {
            return NotFound("La cuenta no existe");
        }

        return Ok(movimientos);
    }
}