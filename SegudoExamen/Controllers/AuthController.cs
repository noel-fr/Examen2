using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SegudoExamen.Models;
using SegudoExamen.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SegudoExamen.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly DataService _dataService;
    private readonly IConfiguration _config;

    public AuthController(DataService dataService, IConfiguration config)
    {
        _dataService = dataService;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Validaciones básicas
            if (string.IsNullOrEmpty(request.Correo) || string.IsNullOrEmpty(request.Contrasena))
                return BadRequest(new { mensaje = "Correo y contraseña requeridos" });

            // Verificar correo único
            var existe = await _dataService.GetUsuarioByEmail(request.Correo);
            if (existe != null)
                return BadRequest(new { mensaje = "El correo ya está registrado" });

            // Crear usuario
            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Correo = request.Correo,
                Contrasena = BCrypt.Net.BCrypt.HashPassword(request.Contrasena),
                Edad = request.Edad,
                NumeroIdentidad = request.NumeroIdentidad,
                Telefono = request.Telefono,
                FechaRegistro = Google.Cloud.Firestore.Timestamp.FromDateTime(DateTime.UtcNow)
            };

            await _dataService.AddUsuario(usuario);

            return Ok(new
            {
                mensaje = "Usuario registrado exitosamente",
                userId = usuario.Id
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var usuario = await _dataService.GetUsuarioByEmail(request.Correo);

            if (usuario == null)
                return Unauthorized(new { mensaje = "Credenciales inválidas" });

            if (!usuario.Activo)
                return Unauthorized(new { mensaje = "Cuenta inactiva" });

            if (!BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.Contrasena))
                return Unauthorized(new { mensaje = "Credenciales inválidas" });

            // Generar JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"] ?? "CLAVE_SECRETA_MUY_LARGA_PARA_JWT_32_CHARS!");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("userId", usuario.Id),
                    new Claim("correo", usuario.Correo),
                    new Claim("rol", usuario.Rol),
                    new Claim("nombre", usuario.Nombre)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                usuario = new
                {
                    id = usuario.Id,
                    nombre = usuario.Nombre,
                    apellido = usuario.Apellido,
                    correo = usuario.Correo,
                    rol = usuario.Rol
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }
}

public class RegisterRequest
{
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Correo { get; set; }
    public string Contrasena { get; set; }
    public int Edad { get; set; }
    public string NumeroIdentidad { get; set; }
    public string Telefono { get; set; }
}

public class LoginRequest
{
    public string Correo { get; set; }
    public string Contrasena { get; set; }
}