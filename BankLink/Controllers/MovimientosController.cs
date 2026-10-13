using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Context;
using Models;

namespace Controllers;

[ApiController]
[Route("api/cuentas/{cuentaId}/[controller]")]
public class MovimientosController : ControllerBase
{
    private readonly BankLinkContext _context;

    public MovimientosController(BankLinkContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Movimiento>>> GetByCuenta(int cuentaId)
    {
        var cuentaExiste = await _context.Cuentas.AnyAsync(c => c.Id == cuentaId);
        if (!cuentaExiste) return NotFound("La cuenta no existe.");

        var movimientos = await _context.Movimientos
            .Where(m => m.CuentaId == cuentaId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

        return Ok(movimientos);
    }
}