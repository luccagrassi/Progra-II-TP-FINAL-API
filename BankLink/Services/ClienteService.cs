using Context;
using Interfaces;
using Models;
using DTOs;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class ClienteService(BankLinkContext context) : iClienteService
{
    public async Task<List<Cliente>> GetAll()
    {
        return await context.Clientes.ToListAsync();
    }
    public async Task<Cliente> ObtenerPorId(int id)
    {
        return await context.Clientes.FindAsync(id);
    }
    public async Task<Cliente> Crear(ClienteDTO nuevoCliente)
    {
        Cliente cliente = new Cliente
        {
            Nombre = nuevoCliente.Nombre,
            Apellido = nuevoCliente.Apellido,
            Dni = nuevoCliente.Dni,
            Direccion = nuevoCliente.Direccion,
            Email = nuevoCliente.Email,
            Telefono = nuevoCliente.Telefono
        };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        return cliente;
    }
    public async Task<Cliente?> Actualizar(int id, ClienteDTO clienteActualizado)
    {
        Cliente ? cliente = await ObtenerPorId(id);
        if (cliente == null) return null;
        cliente.Nombre = clienteActualizado.Nombre;
        cliente.Apellido = clienteActualizado.Apellido;
        cliente.Dni = clienteActualizado.Dni;
        cliente.Direccion = clienteActualizado.Direccion;
        cliente.Email = clienteActualizado.Email;
        cliente.Telefono = clienteActualizado.Telefono;

        await context.SaveChangesAsync();
        return cliente;
    }
    public async Task<bool> Eliminar(int id)
    {
        Cliente ? cliente = await ObtenerPorId(id);
        if (cliente == null) return false;

        context.Clientes.Remove(cliente);
        await context.SaveChangesAsync();
        return true;
    }
}