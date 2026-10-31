using Context;
using Interfaces;
using Models;
using DTOs;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class BancoService(BankLinkContext context) : iBancoService
{
    public async Task<IEnumerable<Banco>> GetAll()
    {
        return await context.BancosExternos.ToListAsync();
    }

    public async Task<Banco> ObtenerPorId(int id)
    {
        return await context.BancosExternos.FindAsync(id);
    }

    public async Task<Banco> Crear(BancoDTO nuevoBanco)
    {
        bool codigoExiste = await context.BancosExternos.AnyAsync(b => b.Codigo == nuevoBanco.Codigo);
        if (codigoExiste)
        {
            throw new InvalidOperationException($"Ya existe un banco registrado con el código '{nuevoBanco.Codigo}'.");
        }

        Banco banco = new Banco
        {
            Nombre = nuevoBanco.Nombre,
            Codigo = nuevoBanco.Codigo,
            UrlApi = nuevoBanco.UrlApi
        };

        context.BancosExternos.Add(banco);
        await context.SaveChangesAsync();
        return banco;
    }

    public async Task<Banco?> Actualizar(int id, BancoDTO bancoActualizado)
    {
        Banco? banco = await ObtenerPorId(id);
        if (banco == null) return null;

        bool codigoDuplicado = await context.BancosExternos.AnyAsync(b => b.Codigo == bancoActualizado.Codigo && b.Id != id);
        if (codigoDuplicado)
        {
            throw new InvalidOperationException($"Ya existe otro banco registrado con el código '{bancoActualizado.Codigo}'.");
        }

        banco.Nombre = bancoActualizado.Nombre;
        banco.Codigo = bancoActualizado.Codigo;
        banco.UrlApi = bancoActualizado.UrlApi;

        await context.SaveChangesAsync();
        return banco;
    }

    public async Task<bool> Eliminar(int id)
    {
        Banco? banco = await ObtenerPorId(id);
        if (banco == null) return false;

        context.BancosExternos.Remove(banco);
        await context.SaveChangesAsync();
        return true;
    }
}