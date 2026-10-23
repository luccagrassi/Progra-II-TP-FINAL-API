using Microsoft.AspNetCore.Mvc;
using Interfaces;
using Models;

namespace Controllers;

[ApiController]
[Route("api/cuentas/{cuentaId}/[controller]")]
public class MovimientosController : ControllerBase
{
    private readonly iMovimientoService _movimientoService;

    public MovimientosController(iMovimientoService movimientoService)
    {
        _movimientoService = movimientoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Movimiento>>> GetByCuenta(int cuentaId)
    {
        var movimientos = await _movimientoService.ObtenerPorCuenta(cuentaId);
        if (movimientos == null)
        {
            return NotFound("La cuenta no existe");
        }

        return Ok(movimientos);
    }
}