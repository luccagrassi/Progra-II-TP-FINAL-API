using Context;
using Microsoft.EntityFrameworkCore;
using Interfaces;
using Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configurar controladores y serialización de enums como texto
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<BankLinkContext>(options =>
options.UseSqlite(builder.Configuration.GetConnectionString("BankLinkDb")));

// HttpClient TIPADO para consumir otra BibliotecaApi (la de un compañero).
// La URL base se lee de appsettings ("ApiExterna:BaseUrl"); por defecto apunta
// a este mismo proyecto para poder probar el autoconsumo.
builder.Services.AddHttpClient<iBancoService, BancoService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiExterna:BaseUrl"]!);
    // API KEY de servicio: viaja en el header X-API-Key en CADA request que haga este cliente.
    // (La key sale de config; en producción, del .env. No hay login ni token que venza.)
    client.DefaultRequestHeaders.Add("X-API-Key", builder.Configuration["ApiExterna:ApiKey"]);
});

// Inyección de dependencias de servicios
builder.Services.AddScoped<iClienteService, ClienteService>();
builder.Services.AddScoped<iCuentaService, CuentaService>();
builder.Services.AddScoped<iMovimientoService, MovimientoService>();
builder.Services.AddScoped<iBancoService, BancoService>();
builder.Services.AddScoped<iTransferenciaService, TransferenciaService>();
  
// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

// Genera el documento OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // expone el JSON de OpenAPI
    app.UseSwaggerUI(); // sirve la página web interactiva
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
