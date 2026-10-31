using Models;
using DTOs;

namespace Interfaces;

public interface IMovimientoService
{
    public Task<IEnumerable<Movimiento>?> ObtenerPorCuenta(int cuentaId);
    public Task<decimal> Depositar(int cuentaId, MovimientoDTO dto);
    public Task<decimal> Retirar(int cuentaId, MovimientoDTO dto);
}