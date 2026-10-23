using Context;
using Interfaces;
using Models;
using DTOs;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class CuentaService(BankLinkContext context) : iCuentaService
{
    public async Task<List<Cuenta>> GetAll()
    {
        return await context.Cuentas.ToListAsync();
    }
    public async Task<Cuenta> ObtenerPorId(int id)
    {
        return await context.Cuentas.FindAsync(id);
    }
    public async Task<Cuenta> Crear(CuentaCreateDTO nuevaCuenta)
    {
        //validacion de cliente con su numero de cuenta existente
        bool clienteExiste = await context.Clientes.AnyAsync(c => c.Id == nuevaCuenta.ClienteId);
        if (!clienteExiste)
        {
            throw new KeyNotFoundException($"No existe un cliente con ID {nuevaCuenta.ClienteId}.");
        }

        // validacion de numero de cuenta unico
        bool numeroExiste = await context.Cuentas.AnyAsync(c => c.Numero == nuevaCuenta.Numero);
        if (numeroExiste)
        {
            throw new InvalidOperationException($"El número de cuenta {nuevaCuenta.Numero} ya está registrado.");
        }

        Cuenta cuenta = new Cuenta
        {
            Numero = nuevaCuenta.Numero,
            Saldo = 0,
            TipoCuenta = nuevaCuenta.TipoCuenta,
            ClienteId = nuevaCuenta.ClienteId,
            Estado = nuevaCuenta.Estado
        };
        context.Cuentas.Add(cuenta);
        await context.SaveChangesAsync();
        return cuenta;
    }
    public async Task<Cuenta?> Actualizar(int id, CuentaUpdateDTO cuentaActualizada)
    {
        Cuenta ? cuenta = await ObtenerPorId(id);
        if (cuenta == null) return null;
        cuenta.TipoCuenta = cuentaActualizada.TipoCuenta;
        cuenta.Estado = cuentaActualizada.Estado;

        await context.SaveChangesAsync();
        return cuenta;
    }
    public async Task<bool> Eliminar(int id)
    {
        Cuenta ? cuenta = await ObtenerPorId(id);
        if (cuenta == null) return false;

        context.Cuentas.Remove(cuenta);
        await context.SaveChangesAsync();
        return true;
    }
}
