namespace Interfaces;

using Models;
using DTOs;

public interface ITransferenciaService
{
    public Task<bool> Enviar(TransferenciaPushDTO dto);
    public Task<bool> Recibir(TransferenciaPullDTO dto);
}
