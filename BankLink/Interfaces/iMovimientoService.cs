using Models;
using DTOs;

namespace Interfaces;

public interface iMovimientoService
{
    public Task<List<Movimiento>> GetAll();
    public Task<Movimiento> ObtenerPorId(int id);
    public Task<Movimiento> Crear(MovimientoDTO nuevoMovimiento);
    public Task<Movimiento?> Actualizar(int id, MovimientoDTO movimientoActualizado);
    public Task<bool> Eliminar(int id);
}