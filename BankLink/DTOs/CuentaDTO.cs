using Models;

namespace DTOs;

public class CuentaCreateDTO
{
    public int Numero { get; set; }
    public Tipo TipoCuenta { get; set; }
    public int Saldo { get; set; }
    public int ClienteId { get; set; }
    public bool Estado { get; set; }
}

public class CuentaUpdateDTO
{
    public int Numero { get; set; }
    public Tipo TipoCuenta { get; set; }
    public int Saldo { get; set; }
    public bool Estado { get; set; }
}