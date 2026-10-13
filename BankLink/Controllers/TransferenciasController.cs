using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Context;
using Models;
using Enums;
using DTOs;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransferenciasController : ControllerBase
{
    private readonly BankLinkContext _context;
    private readonly HttpClient _httpClient;

    public TransferenciasController(BankLinkContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    [HttpPost("enviar")]
    public async Task<IActionResult> Enviar([FromBody] TransferenciaPushDTO dto)
    {
        if (dto.Monto <= 0) return BadRequest("Monto inválido.");

        var bancoDestino = await _context.BancosExternos.FirstOrDefaultAsync(b => b.Codigo == dto.CodigoBancoDestino);
        if (bancoDestino == null) return NotFound("Banco destino no registrado.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaOrigen = await _context.Cuentas.FindAsync(dto.CuentaOrigenId);
            if (cuentaOrigen == null || !cuentaOrigen.Estado) return BadRequest("Cuenta de origen inválida.");
            if (cuentaOrigen.Saldo < dto.Monto) return BadRequest("Saldo insuficiente.");

        //Saldo y movimiento
            cuentaOrigen.Saldo -= dto.Monto;
            
            var movimiento = new Movimiento
            {
                CuentaId = cuentaOrigen.Id,
                Tipo = TipoMovimiento.TRANSFERENCIA_ENVIADA,
                Monto = dto.Monto,
                Fecha = DateTime.UtcNow,
                Descripcion = dto.Descripcion
            };
            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            
            bool simulacionHttpExitosa = true; 

            if (!simulacionHttpExitosa)
            {
                throw new Exception("Error en la comunicación con el banco externo.");
            }

            await transaction.CommitAsync();
            return Ok(new { Mensaje = "Transferencia enviada con éxito.", SaldoRestante = cuentaOrigen.Saldo });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Error procesando la transferencia. Transacción revertida.");
        }
    }

    [HttpPost("recibir")]
    public async Task<IActionResult> Recibir([FromBody] TransferenciaPullDTO dto)
    {
        if (dto.Monto <= 0) return BadRequest("Monto inválido.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cuentaDestino = await _context.Cuentas.FirstOrDefaultAsync(c => c.Numero == dto.NumeroCuentaDestino);
            if (cuentaDestino == null || !cuentaDestino.Estado) 
                return NotFound("Cuenta destino no encontrada o inactiva.");

            cuentaDestino.Saldo += dto.Monto;

            var movimiento = new Movimiento
            {
                CuentaId = cuentaDestino.Id,
                Tipo = TipoMovimiento.TRANSFERENCIA_RECIBIDA,
                Monto = dto.Monto,
                Fecha = DateTime.UtcNow,
                Descripcion = $"De Banco {dto.CodigoBancoOrigen}: {dto.Descripcion}"
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Mensaje = "Transferencia recibida e imputada con éxito." });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Error al imputar la transferencia recibida.");
        }
    }
}