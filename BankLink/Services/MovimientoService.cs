using Context;
using Interfaces;
using Models;
using DTOs;
using Enums;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class MovimientoService(BankLinkContext context) : iMovimientoService
{
    // 1. Consultar todos los movimientos de una cuenta
    public async Task<List<Movimiento>?> ObtenerPorCuenta(int cuentaId)
    {
        bool cuentaExiste = await context.Cuentas.AnyAsync(c => c.Id == cuentaId);
        if (!cuentaExiste) return null;

        return await context.Movimientos
            .Where(m => m.CuentaId == cuentaId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }

    // 2. Ingresar dinero (Depósito) de forma atómica
    public async Task<decimal> Depositar(int cuentaId, MovimientoDTO dto)
    {
        if (dto.Monto <= 0)
            throw new ArgumentException("El monto debe ser mayor a cero.");

        // Iniciamos una transacción para asegurar que saldo y movimiento se guarden juntos
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var cuenta = await context.Cuentas.FindAsync(cuentaId);
            if (cuenta == null || !cuenta.Estado)
                throw new InvalidOperationException("Cuenta inválida o inactiva.");

            // Actualizamos el saldo
            cuenta.Saldo += dto.Monto;

            // Registramos el movimiento
            var movimiento = new Movimiento
            {
                CuentaId = cuentaId,
                Tipo = TipoMovimiento.DEPOSITO,
                Monto = dto.Monto,
                Fecha = DateTime.UtcNow,
                Descripcion = dto.Descripcion
            };

            context.Movimientos.Add(movimiento);
            await context.SaveChangesAsync();
            await transaction.CommitAsync(); // Confirmamos ambos cambios en la BD

            return cuenta.Saldo;
        }
        catch
        {
            await transaction.RollbackAsync(); // Si algo falla, deshace todo
            throw; // Re-lanza el error para que el Controller devuelva el código HTTP adecuado
        }
    }

    // 3. Retirar dinero (Retiro) verificando saldo suficiente de forma atómica
    public async Task<decimal> Retirar(int cuentaId, MovimientoDTO dto)
    {
        if (dto.Monto <= 0)
            throw new ArgumentException("El monto debe ser mayor a cero.");

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var cuenta = await context.Cuentas.FindAsync(cuentaId);
            if (cuenta == null || !cuenta.Estado)
                throw new InvalidOperationException("Cuenta inválida o inactiva.");

            if (cuenta.Saldo < dto.Monto)
                throw new InvalidOperationException("Saldo insuficiente.");

            // Descontamos el saldo
            cuenta.Saldo -= dto.Monto;

            // Registramos el movimiento
            var movimiento = new Movimiento
            {
                CuentaId = cuentaId,
                Tipo = TipoMovimiento.RETIRO,
                Monto = dto.Monto,
                Fecha = DateTime.UtcNow,
                Descripcion = dto.Descripcion
            };

            context.Movimientos.Add(movimiento);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return cuenta.Saldo;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}