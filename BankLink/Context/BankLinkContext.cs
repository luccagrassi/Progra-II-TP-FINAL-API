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
}
