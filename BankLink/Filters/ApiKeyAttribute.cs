using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BibliotecaApi.Filters;

// Filtro de autorización por API KEY (para comunicación entre servicios / integración).
// Exige el header "X-API-Key" y lo compara con la clave guardada en configuración
// ("ApiExterna:ApiKey"). Si falta o no coincide, corta la petición con 401.
// Se aplica como atributo [ApiKey] sobre una acción o un controlador.
//
// Ojo: esto es independiente del JWT. El JWT (con [Authorize]) autentica USUARIOS;
// la API key autentica a otra APLICACIÓN/servicio que nos consume.
public class ApiKeyAttribute : Attribute, IActionFilter
{
    private const string NombreHeader = "X-API-Key";

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // La configuración se resuelve desde el contenedor de dependencias.
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var keyEsperada = config["ApiExterna:ApiKey"];

        // ¿Vino el header X-API-Key y coincide con la clave configurada?
        var tieneHeader = context.HttpContext.Request.Headers.TryGetValue(NombreHeader, out var keyRecibida);

        if (!tieneHeader || string.IsNullOrEmpty(keyEsperada) || keyRecibida.ToString() != keyEsperada)
        {
            context.Result = new UnauthorizedObjectResult(
                new { mensaje = "API key faltante o inválida." });   // 401
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
