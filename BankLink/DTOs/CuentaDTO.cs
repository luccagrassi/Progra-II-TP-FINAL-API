using Enums;
using Models;

namespace DTOs;

public class CuentaCreateDTO
{
    public int Numero { get; set; }
    public TipoCuenta TipoCuenta { get; set; }
    public int ClienteId { get; set; }
    public bool Estado { get; set; }
}

public class CuentaUpdateDTO
{
    public TipoCuenta TipoCuenta { get; set; }
    public bool Estado { get; set; }
}