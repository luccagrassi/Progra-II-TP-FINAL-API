using Models;
using DTOs;

namespace Interfaces;

public interface iCuenta
{
    public Task<List<Cuenta>> GetAll();
    public Task<Cuenta> ObtenerPorId(int id);
    public Task<Cuenta> Crear(CuentaCreateDTO nuevaCuenta);
    public Task<bool> Actualizar(int id, CuentaUpdateDTO cuentaActualizada);
    public Task<bool> Eliminar(int id);
}