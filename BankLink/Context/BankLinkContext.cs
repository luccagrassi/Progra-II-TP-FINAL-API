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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //vereificicaicon de  que el numero de cuenta sea unico
        modelBuilder.Entity<Cuenta>().HasIndex(c => c.Numero).IsUnique();

        //coneccion clave cliente
        modelBuilder.Entity<Cuenta>().HasOne(c => c.Cliente).WithMany().HasForeignKey(c => c.ClienteId).OnDelete(DeleteBehavior.Cascade);
    }
}
