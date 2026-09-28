using Models;
using DTOs;

namespace Interfaces;

public interface iClienteService
{
    public Task<List<Cliente>> GetAll();
    public Task<Cliente> ObtenerPorId(int id);
    public Task<Cliente> Crear(ClienteDTO nuevoCliente);
    public Task<Cliente?> Actualizar(int id, ClienteDTO clienteActualizado);
    public Task<bool> Eliminar(int id);
}