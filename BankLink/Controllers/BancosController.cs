using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Context;
using Models;
using DTOs;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class BancosController : ControllerBase
{
    private readonly BankLinkContext _context;

    public BancosController(BankLinkContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Banco>>> GetAll()
    {
        return Ok(await _context.BancosExternos.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<Banco>> Create([FromBody] BancoDTO dto)
    {
        var banco = new Banco { Nombre = dto.Nombre, Codigo = dto.Codigo, UrlApi = dto.UrlApi };
        _context.BancosExternos.Add(banco);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = banco.Id }, banco);
    }
}