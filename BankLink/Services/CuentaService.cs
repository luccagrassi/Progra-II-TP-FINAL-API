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
        Cuenta cuenta = new Cuenta
        {
            Numero = nuevaCuenta.Numero,
            Saldo = 0,
            TipoCuenta = nuevaCuenta.TipoCuenta,
            ClienteId = nuevaCuenta.ClienteId,
            Estado = true
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