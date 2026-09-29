using Models;
using DTOs;

namespace Interfaces;

public interface iCuentaService
{
    public Task<List<Cuenta>> GetAll();
    public Task<Cuenta> ObtenerPorId(int id);
    public Task<Cuenta> Crear(CuentaCreateDTO nuevaCuenta);
    public Task<Cuenta?> Actualizar(int id, CuentaUpdateDTO cuentaActualizada);
    public Task<bool> Eliminar(int id);
}