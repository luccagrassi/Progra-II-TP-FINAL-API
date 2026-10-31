using Microsoft.AspNetCore.Mvc;
using Interfaces;
using Models;
using DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/[controller]")]
public class BancosController : ControllerBase
{
    private readonly IBancoService _bancoService;

    public BancosController(IBancoService bancoService)
    {
        _bancoService = bancoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Banco>>> GetAll()
    {
        var bancos = await _bancoService.GetAll();
        return Ok(bancos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Banco>> GetById(int id)
    {
        var banco = await _bancoService.ObtenerPorId(id);
        if (banco == null)
        {
            return NotFound($"No se encontró el banco con ID {id}.");
        }
        return Ok(banco);
    }

    [HttpPost]
    public async Task<ActionResult<Banco>> Create([FromBody] BancoDTO dto)
    {
        try
        {
            var nuevoBanco = await _bancoService.Crear(dto);
            return CreatedAtAction(nameof(GetById), new { id = nuevoBanco.Id }, nuevoBanco);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Banco>> Update(int id, [FromBody] BancoDTO dto)
    {
        try
        {
            var bancoActualizado = await _bancoService.Actualizar(id, dto);
            if (bancoActualizado == null)
            {
                return NotFound($"No se encontró el banco con ID {id} para actualizar.");
            }
            return Ok(bancoActualizado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _bancoService.Eliminar(id);
        if (!eliminado)
        {
            return NotFound($"No se encontró el banco con ID {id} para eliminar.");
        }
        return NoContent();
    }
}