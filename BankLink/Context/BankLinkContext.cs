using Microsoft.EntityFrameworkCore;
using Models;

namespace Context;
public class BankLinkContext : DbContext
{
    public BankLinkContext(DbContextOptions<BankLinkContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Cuenta> Cuentas { get; set; }
    public DbSet<Movimiento> Movimientos { get; set; }
    public DbSet<Banco> BancosExternos { get; set; }
}
