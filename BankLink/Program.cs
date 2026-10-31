using Context;
using Microsoft.EntityFrameworkCore;
using Interfaces;
using Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;

DotNetEnv.Env.Load();

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
builder.Services.AddSwaggerGen(options =>
{
    // 1) Declara el esquema: un Bearer Token (JWT) que viaja en el header
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegá SOLO el token JWT (sin escribir 'Bearer ' adelante)."
    });
    // 2) Aplica ese esquema como requisito para los endpoints protegidos
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwt["Audience"],
        ValidateLifetime = true, // rechaza tokens vencidos
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication(); // ¿quién sos? lee y valida el token
app.UseAuthorization(); // ¿qué podés? aplica [Authorize]

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // expone el JSON de OpenAPI
    app.UseSwaggerUI(); // sirve la página web interactiva
}

app.MapControllers();
app.Run();
