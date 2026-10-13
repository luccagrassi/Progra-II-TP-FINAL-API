using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Interfaces;
using DTOs;
using Models;
using Enums;
using Context;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuentasController : ControllerBase
{
    private readonly iCuentaService _cuentaService;
    private readonly BankLinkContext _context;

    public CuentasController(iCuentaService cuentaService, BankLinkContext context)
    {
        _cuentaService = cuentaService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Cuenta>>> GetAll()
    {
        return Ok(await _cuentaService.GetAll());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Cuenta>> GetById(int id)
    {
        var cuenta = await _cuentaService.ObtenerPorId(id);
        if (cuenta == null) return NotFound($"Cuenta {id} no encontrada.");
        return Ok(cuenta);
    }

    [HttpPost]
    public async Task<ActionResult<Cuenta>> Create([FromBody] CuentaCreateDTO dto)
    {
        var nuevaCuenta = await _cuentaService.Crear(dto);
        return CreatedAtAction(nameof(GetById), new { id = nuevaCuenta.Id }, nuevaCuenta);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Cuenta>> Update(int id, [FromBody] CuentaUpdateDTO dto)
    {
        var cuenta = await _cuentaService.Actualizar(id, dto);
        if (cuenta == null) return NotFound();
        return Ok(cuenta);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _cuentaService.Eliminar(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }

//Depósito
    [HttpPost("{id}/deposito")]
    public async Task<IActionResult> Deposito(int id, [FromBody] OperacionDTO dto)
    {
        if (dto.Monto <= 0) return BadRequest("El monto debe ser mayor a cero.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuenta = await _context.Cuentas.FindAsync(id);
            if (cuenta == null || !cuenta.Estado) return BadRequest("Cuenta inválida o inactiva.");

            cuenta.Saldo += dto.Monto;

            var movimiento = new Movimiento
            {
                CuentaId = id,
                Tipo = TipoMovimiento.DEPOSITO,
                Monto = dto.Monto,
                Fecha = DateTime.UtcNow,
                Descripcion = dto.Descripcion
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { cuenta.Saldo, Mensaje = "Depósito exitoso." });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Error procesando el depósito.");
        }
    }

//Retiro
    [HttpPost("{id}/retiro")]
    public async Task<IActionResult> Retiro(int id, [FromBody] OperacionDTO dto)
    {
        if (dto.Monto <= 0) return BadRequest("El monto debe ser mayor a cero.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuenta = await _context.Cuentas.FindAsync(id);
            if (cuenta == null || !cuenta.Estado) return BadRequest("Cuenta inválida o inactiva.");
            if (cuenta.Saldo < dto.Monto) return BadRequest("Saldo insuficiente.");

            cuenta.Saldo -= dto.Monto;

            var movimiento = new Movimiento
            {
                CuentaId = id,
                Tipo = TipoMovimiento.RETIRO,
                Monto = dto.Monto,
                Fecha = DateTime.UtcNow,
                Descripcion = dto.Descripcion
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { cuenta.Saldo, Mensaje = "Retiro exitoso." });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Error procesando el retiro.");
        }
    }
}