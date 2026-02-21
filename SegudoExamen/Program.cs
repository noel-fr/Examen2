using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SegudoExamen.Services;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Servicio de datos
builder.Services.AddSingleton<DataService>();

// JWT CON DEBUG
var key = Encoding.ASCII.GetBytes("CLAVE_SECRETA_MUY_LARGA_PARA_JWT_32_CHARS!");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };

        // DEBUG: Mostrar errores de JWT en consola
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"🔴 JWT AUTH FAILED: {context.Exception.Message}");
                Console.WriteLine($"🔴 Token: {context.Request.Headers["Authorization"]}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"🟢 JWT VALIDATED");
                Console.WriteLine($"🟢 User: {context.Principal?.Identity?.Name}");
                Console.WriteLine($"🟢 Claims:");
                foreach (var claim in context.Principal?.Claims ?? Enumerable.Empty<Claim>())
                {
                    Console.WriteLine($"  - {claim.Type}: {claim.Value}");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"🟡 JWT CHALLENGE: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };

        // Para desarrollo, mostrar más detalles
        options.IncludeErrorDetails = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CONFIGURAR SWAGGER PARA JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Biblioteca API",
        Version = "v1",
        Description = "API para Sistema de Gestión Bibliotecaria"
    });

    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme.<br>Ejemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// MIDDLEWARE PARA DEBUG
app.Use(async (context, next) =>
{
    Console.WriteLine($"🌐 Request: {context.Request.Method} {context.Request.Path}");
    Console.WriteLine($"🌐 Auth Header: {context.Request.Headers["Authorization"]}");
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Biblioteca API v1");
        c.RoutePrefix = "swagger";  // Para acceder en /swagger
        c.EnablePersistAuthorization(); // Guardar token entre refrescos
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();  // IMPORTANTE: Esto va ANTES de Authorization
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/", () => "✅ Biblioteca API está funcionando");
app.MapGet("/health", () => new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0"
});

Console.WriteLine("🚀 Aplicación iniciada");
Console.WriteLine($"🔑 JWT Key: {Convert.ToBase64String(key).Substring(0, 10)}...");
Console.WriteLine($"📚 Swagger: https://localhost:7248/swagger");
Console.WriteLine($"🌐 API: http://localhost:5141");

app.Run();

