using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
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
            if (cuentaOrigen == null || !cuentaOrigen.Estado) return BadRequest("Cuenta de origen inválida o inactiva.");
            if (cuentaOrigen.Saldo < dto.Monto) return BadRequest("Saldo insuficiente.");

            // 1. Descontar saldo y registrar movimiento de salida
            cuentaOrigen.Saldo -= dto.Monto;

            var movimiento = new Movimiento
            {
                CuentaId = cuentaOrigen.Id,
                Tipo = TipoMovimiento.TRANSFERENCIA_ENVIADA,
                Monto = dto.Monto,
                Fecha = DateTime.UtcNow,
                Descripcion = $"A Banco {dto.CodigoBancoDestino} (Cuenta {dto.NumeroCuentaDestino}): {dto.Descripcion}"
            };
            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            // 2. Construir el payload para notificar al banco externo (Inbound del banco destino)
            var pullDto = new TransferenciaPullDTO
            {
                NumeroCuentaDestino = dto.NumeroCuentaDestino,
                CodigoBancoOrigen = "BANKLINK",
                Monto = dto.Monto,
                Descripcion = dto.Descripcion
            };

            // Si UrlApi ya incluye la ruta de recepción la usamos directo; si es la URL base, agregamos /api/transferencias/recibir
            var baseUrl = bancoDestino.UrlApi.TrimEnd('/');
            var endpointDestino = baseUrl.EndsWith("/recibir", StringComparison.OrdinalIgnoreCase)
                ? baseUrl
                : $"{baseUrl}/api/transferencias/recibir";

            // 3. Invocar la API del banco externo mediante HTTP POST
            var response = await _httpClient.PostAsJsonAsync(endpointDestino, pullDto);

            if (!response.IsSuccessStatusCode)
            {
                var detalleError = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"El banco destino rechazó la transferencia ({(int)response.StatusCode}): {detalleError}");
            }

            await transaction.CommitAsync();
            return Ok(new
            {
                Mensaje = "Transferencia enviada con éxito.",
                SaldoRestante = cuentaOrigen.Saldo,
                MovimientoId = movimiento.Id
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new
            {
                Error = "Error procesando la transferencia. Transacción revertida.",
                Detalle = ex.Message
            });
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

            return Ok(new
            {
                Mensaje = "Transferencia recibida e imputada con éxito.",
                CuentaDestinoNumero = cuentaDestino.Numero,
                NuevoSaldo = cuentaDestino.Saldo,
                MovimientoId = movimiento.Id
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new
            {
                Error = "Error al imputar la transferencia recibida.",
                Detalle = ex.Message
            });
        }
    }
}