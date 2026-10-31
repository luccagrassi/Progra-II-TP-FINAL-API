using Microsoft.AspNetCore.Mvc;
using DTOs;
using Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransferenciasController : ControllerBase
{
    private readonly ITransferenciaService _transferenciaService;

    public TransferenciasController(ITransferenciaService transferenciaService)
    {
        _transferenciaService = transferenciaService;
    }

    [HttpPost("enviar")]
    public async Task<IActionResult> Enviar([FromBody] TransferenciaPushDTO dto)
    {
        try
        {
            await _transferenciaService.Enviar(dto);
            return Ok(new { Mensaje = "Transferencia enviada con éxito." });
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Error procesando la transferencia. Transacción revertida.",
                Detalle = ex.Message
            });
        }
    }

    [HttpPost("recibir")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Recibir([FromBody] TransferenciaPullDTO dto)
    {
        try
        {
            var exito = await _transferenciaService.Recibir(dto);

            // El controlador arma la respuesta bonita para el cliente web
            return Ok(new
            {
                Mensaje = "Transferencia recibida e imputada con éxito.",
                CuentaDestinoNumero = dto.NumeroCuentaDestino,
                Monto = dto.Monto
            });
        }
        catch (ArgumentException ex) // Atrapa las validaciones de Monto o Cuenta
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex) // Atrapa fallos graves (base de datos)
        {
            return StatusCode(500, new
            {
                Error = "Error al imputar la transferencia recibida.",
                Detalle = ex.Message
            });
        }
    }
}