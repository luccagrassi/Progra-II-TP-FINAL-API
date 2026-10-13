namespace DTOs;

public class TransferenciaPushDTO
{
    public int CuentaOrigenId { get; set; }
    public string CodigoBancoDestino { get; set; } = string.Empty;
    public int NumeroCuentaDestino { get; set; }
    public decimal Monto { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

public class TransferenciaPullDTO
{
    public int NumeroCuentaDestino { get; set; } // Cuenta destino dentro de BankLink
    public string CodigoBancoOrigen { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}