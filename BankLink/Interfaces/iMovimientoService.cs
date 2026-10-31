using Models;
using DTOs;

namespace Interfaces;

public interface iMovimientoService
{
    public Task<IEnumerable<Movimiento>?> ObtenerPorCuenta(int cuentaId);
    public Task<decimal> Depositar(int cuentaId, MovimientoDTO dto);
    public Task<decimal> Retirar(int cuentaId, MovimientoDTO dto);
}