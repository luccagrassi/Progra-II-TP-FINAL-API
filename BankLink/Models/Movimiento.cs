namespace Models;

using Enums;
using System.Text.Json.Serialization;

public class Movimiento
{
    public int Id { get; set; }
    public int CuentaId { get; set; }
    public Cuenta? Cuenta { get; set; }
    public TipoMovimiento Tipo { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; }

    public Movimiento(int id, int cuentaId, TipoMovimiento tipo, decimal monto, DateTime fecha, string descripcion)
    {
        Id = id;
        CuentaId = cuentaId;
        Tipo = tipo;
        Monto = monto;
        Fecha = fecha;
        Descripcion = descripcion;
    }

    public Movimiento() {}
}