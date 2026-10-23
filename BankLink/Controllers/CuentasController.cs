using Microsoft.AspNetCore.Mvc;
using Interfaces;
using DTOs;
using Models;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuentasController : ControllerBase
{
    private readonly iCuentaService _cuentaService;
    private readonly iMovimientoService _movimientoService;

    public CuentasController(iCuentaService cuentaService, iMovimientoService movimientoService)
    {
        _cuentaService = cuentaService;
        _movimientoService = movimientoService;
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
        try
        {
            var nuevaCuenta = await _cuentaService.Crear(dto);
            return CreatedAtAction(nameof(GetById), new { id = nuevaCuenta.Id }, nuevaCuenta);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
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

    //deposito nuevo
    [HttpPost("{id}/deposito")]
    public async Task<IActionResult> Deposito(int id, [FromBody] MovimientoDTO dto)
    {
        try
        {
            var nuevoSaldo = await _movimientoService.Depositar(id, dto);
            return Ok(new { Saldo = nuevoSaldo, Mensaje = "Depósito exitoso." });
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
        catch
        {
            return StatusCode(500, "Error procesando el depósito.");
        }
    }

    //retiro nuevo 
    [HttpPost("{id}/retiro")]
    public async Task<IActionResult> Retiro(int id, [FromBody] MovimientoDTO dto)
    {
        try
        {
            var nuevoSaldo = await _movimientoService.Retirar(id, dto);
            return Ok(new { Saldo = nuevoSaldo, Mensaje = "Retiro exitoso." });
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
        catch
        {
            return StatusCode(500, "Error procesando el retiro.");
        }
    }
}