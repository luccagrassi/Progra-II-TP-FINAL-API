namespace Models;

public class Cuenta
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public Tipo TipoCuenta { get; set; }
    public int Saldo { get; set; }
    public int ClienteId { get; set; }
    public bool Estado { get; set; }

    public Cuenta(int id, int numero, Tipo tipoCuenta, int saldo, int clienteId, bool estado)
    {
        Id = id;
        Numero = numero;
        TipoCuenta = tipoCuenta;
        Saldo = saldo;
        ClienteId = clienteId;
        Estado = estado;
    }
}