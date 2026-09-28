namespace Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string DNI { get; set; }
    public string Direccion { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }

    public Cliente(int id, string nombre, string apellido, string dni, string direccion, string email, string telefono)
    {
        Id = id;
        Nombre = nombre;
        Apellido = apellido;
        DNI = dni;
        Direccion = direccion;
        Email = email;
        Telefono = telefono;
    }
}
