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

// Inyección de dependencias de servicios
builder.Services.AddScoped<iClienteService, ClienteService>();
builder.Services.AddScoped<iCuentaService, CuentaService>();

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

// Registramos los controladores
builder.Services.AddControllers();

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
