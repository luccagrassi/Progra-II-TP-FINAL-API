using Models;
using DTOs;

namespace Interfaces;

public interface iBancoService
{
    public Task<List<Banco>> GetAll();
    public Task<Banco> ObtenerPorId(int id);
    public Task<Banco> Crear(BancoDTO nuevoBanco);
    public Task<Banco?> Actualizar(int id, BancoDTO bancoActualizado);
    public Task<bool> Eliminar(int id);
}