using Microsoft.AspNetCore.Mvc;
using Interfaces;
using DTOs;
using Models;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly iClienteService _clienteService;

    public ClientesController(iClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Cliente>>> GetAll()
    {
        var clientes = await _clienteService.GetAll();
        return Ok(clientes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Cliente>> GetById(int id)
    {
        var cliente = await _clienteService.ObtenerPorId(id);
        if (cliente == null)
        {
            return NotFound($"No se encontró el cliente con ID {id}.");
        }
        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<Cliente>> Create([FromBody] ClienteDTO dto)
    {
        var nuevoCliente = await _clienteService.Crear(dto);
        return CreatedAtAction(nameof(GetById), new { id = nuevoCliente.Id }, nuevoCliente);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Cliente>> Update(int id, [FromBody] ClienteDTO dto)
    {
        var clienteActualizado = await _clienteService.Actualizar(id, dto);
        if (clienteActualizado == null)
        {
            return NotFound($"No se encontró el cliente con ID {id} para actualizar.");
        }
        return Ok(clienteActualizado);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _clienteService.Eliminar(id);
        if (!eliminado)
        {
            return NotFound($"No se encontró el cliente con ID {id} para eliminar.");
        }
        return NoContent();
    }
}
