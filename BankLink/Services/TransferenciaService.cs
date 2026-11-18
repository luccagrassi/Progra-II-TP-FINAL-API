using Context;
using DTOs;
using Enums;
using Interfaces;
using Models;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class TransferenciaService(BankLinkContext context, HttpClient httpClient) : ITransferenciaService
    {
        public async Task<bool> Enviar(TransferenciaPushDTO dto)
        {
            if (dto.Monto <= 0)
                throw new ArgumentException("Monto inválido."); // Lanza excepción en vez de BadRequest

            var bancoDestino = await context.BancosExternos.FirstOrDefaultAsync(b => b.Codigo == dto.CodigoBancoDestino);
            if (bancoDestino == null)
                throw new ArgumentException("Banco destino no registrado."); // En vez de NotFound

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var cuentaOrigen = await context.Cuentas.FindAsync(dto.CuentaOrigenId);
                if (cuentaOrigen == null || !cuentaOrigen.Estado)
                    throw new ArgumentException("Cuenta de origen inválida o inactiva.");

                if (cuentaOrigen.Saldo < dto.Monto)
                    throw new InvalidOperationException("Saldo insuficiente.");

                // 1. Descontar saldo y registrar movimiento de salida
                cuentaOrigen.Saldo -= dto.Monto;

                var movimiento = new Movimiento
                {
                    CuentaId = cuentaOrigen.Id,
                    Tipo = TipoMovimiento.TRANSFERENCIA_ENVIADA, // Asegúrate de que este Enum exista
                    Monto = dto.Monto,
                    Fecha = DateTime.UtcNow,
                    Descripcion = $"A Banco {dto.CodigoBancoDestino} (Cuenta {dto.NumeroCuentaDestino}): {dto.Descripcion}"
                };

                context.Movimientos.Add(movimiento);
                await context.SaveChangesAsync();

                // 2. Construir el payload para notificar al banco externo (Inbound del banco destino)
                var pullDto = new TransferenciaPullDTO
                {
                    NumeroCuentaDestino = dto.NumeroCuentaDestino,
                    CodigoBancoOrigen = "BANKLINK",
                    Monto = dto.Monto,
                    Descripcion = dto.Descripcion
                };

                // Si UrlApi ya incluye la ruta de recepción la usamos directo; si es la URL base, agregamos /api/transferencias/recibir
                var baseUrl = bancoDestino.UrlApi.TrimEnd('/');
                var endpointDestino = baseUrl.EndsWith("/recibir", StringComparison.OrdinalIgnoreCase)
                    ? baseUrl
                    : $"{baseUrl}/api/transferencias/recibir";

                // 3. Invocar la API del banco externo mediante HTTP POST
                var response = await httpClient.PostAsJsonAsync(endpointDestino, pullDto);

                if (!response.IsSuccessStatusCode)
                {
                    var detalleError = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"El banco destino rechazó la transferencia ({(int)response.StatusCode}): {detalleError}");
                }

                await transaction.CommitAsync();

                // Si todo salió bien, el servicio simplemente dice "true"
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Relanza la excepción hacia arriba para que el Controlador la atrape o la maneje el middleware global
            }
        }

        public async Task<bool> Recibir(TransferenciaPullDTO dto)
        {
            if (dto.Monto <= 0)
                throw new ArgumentException("Monto inválido."); // Lanza excepción en vez de BadRequest

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var cuentaDestino = await context.Cuentas.FirstOrDefaultAsync(c => c.Numero == dto.NumeroCuentaDestino);

                if (cuentaDestino == null || !cuentaDestino.Estado)
                    throw new ArgumentException("Cuenta destino no encontrada o inactiva."); // En vez de NotFound

                cuentaDestino.Saldo += dto.Monto;

                var movimiento = new Movimiento
                {
                    CuentaId = cuentaDestino.Id,
                    Tipo = TipoMovimiento.TRANSFERENCIA_RECIBIDA,
                    Monto = dto.Monto,
                    Fecha = DateTime.UtcNow,
                    Descripcion = $"De Banco {dto.CodigoBancoOrigen}: {dto.Descripcion}"
                };

                context.Movimientos.Add(movimiento);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Si todo salió bien, devolvemos true
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Relanza el error al Controlador
            }
        }
    }
}