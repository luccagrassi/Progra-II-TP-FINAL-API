using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Context;
using Models; // Ajusta según tu namespace
using DTOs;   // Ajusta según tu namespace

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BankLinkContext _context;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<Usuario> _hasher = new();

    // Inyección de dependencias con constructor clásico
    public AuthController(BankLinkContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegistroDto dto)
    {
        if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == dto.Usuario))
            return Conflict("Ese usuario ya existe.");

        var usuario = new Usuario 
        { 
            NombreUsuario = dto.Usuario, 
            Rol = "Usuario" 
        };
        
        // Se le pasa el usuario al hasher, no null
        usuario.PasswordHash = _hasher.HashPassword(usuario, dto.Password);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // NO devolvemos el hash
        return Ok(new { usuario.Id, usuario.NombreUsuario });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<object>> Login(LoginDto dto)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == dto.Usuario);
            
        if (usuario is null) 
            return Unauthorized("Credenciales inválidas.");

        var ok = _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, dto.Password);
        
        if (ok == PasswordVerificationResult.Failed) 
            return Unauthorized("Credenciales inválidas.");

        return Ok(new { token = GenerarToken(usuario) });
    }

    private string GenerarToken(Usuario usuario)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.Role, usuario.Rol) // el rol sale de la base
        };
        
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiraMinutos"]!)),
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}