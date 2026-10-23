namespace Models;

using Enums;
using System.Text.Json.Serialization;

public class Cuenta
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public TipoCuenta TipoCuenta { get; set; }
    public decimal Saldo { get; set; }
    public int ClienteId { get; set; }
    [JsonIgnore]
    public Cliente? Cliente { get; set; }
    public bool Estado { get; set; }

    public Cuenta(int id, int numero, TipoCuenta tipoCuenta, decimal saldo, int clienteId, bool estado)
    {
        Id = id;
        Numero = numero;
        TipoCuenta = tipoCuenta;
        Saldo = saldo;
        ClienteId = clienteId;
        Estado = estado;
    }

    public Cuenta() {}
}
