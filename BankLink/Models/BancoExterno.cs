namespace Models;

public class Banco
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Codigo { get; set; }
    public string UrlApi { get; set; }

    public Banco(int id, string nombre, string codigo, string urlApi)
    {
        Id = id;
        Nombre = nombre;
        Codigo = codigo;
        UrlApi = urlApi;
    }

    public Banco() {}
}