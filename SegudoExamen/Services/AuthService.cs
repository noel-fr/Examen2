using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SegundoExamen.DTOs;
using SegundoExamen.Models;
using SegundoExamen.Services.Interfaces;

namespace SegundoExamen.Services.Implementaciones
{
    public class AuthService : IAuthService
    {
        private readonly IFirebaseService _firebaseService;
        private readonly IConfiguration _configuration;
        private readonly CollectionReference _usuariosCollection;

        public AuthService(IFirebaseService firebaseService, IConfiguration configuration)
        {
            _firebaseService = firebaseService;
            _configuration = configuration;
            _usuariosCollection = _firebaseService.GetCollection("usuarios");
        }

        public async Task<AuthResponseDto> Register(RegisterDto dto)
        {
            // Verificar si ya existe el correo
            var existingUser = await GetUsuarioByCorreo(dto.Correo);
            if (existingUser != null)
            {
                throw new Exception("Ya existe un usuario con este correo.");
            }

            var usuario = new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Correo = dto.Correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Contraseña),
                NumeroIdentidad = dto.NumeroIdentidad,
                Edad = dto.Edad,
                Telefono = dto.Telefono,
                Rol = "usuario",
                Activo = true,
                FechaRegistro = DateTime.UtcNow,
                Multas = 0m
            };

            await _usuariosCollection.Document(usuario.Id).SetAsync(usuario);

            var token = GenerateJwtToken(usuario);

            return new AuthResponseDto
            {
                Token = token,
                UsuarioId = usuario.Id,
                Correo = usuario.Correo,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                Rol = usuario.Rol
            };
        }

        public async Task<AuthResponseDto> Login(LoginDto dto)
        {
            var usuario = await GetUsuarioByCorreo(dto.Correo);
            if (usuario == null)
            {
                throw new Exception("Credenciales inválidas.");
            }

            if (!usuario.Activo)
            {
                throw new Exception("La cuenta se encuentra inactiva.");
            }

            var validPassword = BCrypt.Net.BCrypt.Verify(dto.Contraseña, usuario.PasswordHash);
            if (!validPassword)
            {
                throw new Exception("Credenciales inválidas.");
            }

            var token = GenerateJwtToken(usuario);

            return new AuthResponseDto
            {
                Token = token,
                UsuarioId = usuario.Id,
                Correo = usuario.Correo,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                Rol = usuario.Rol
            };
        }

        public async Task<Usuario?> GetUsuarioById(string id)
        {
            var snapshot = await _usuariosCollection.Document(id).GetSnapshotAsync();
            if (!snapshot.Exists) return null;
            return snapshot.ConvertTo<Usuario>();
        }

        public async Task<Usuario?> GetUsuarioByCorreo(string correo)
        {
            var query = _usuariosCollection.WhereEqualTo("Correo", correo);
            var snapshot = await query.GetSnapshotAsync();
            var doc = snapshot.Documents.FirstOrDefault();
            return doc?.ConvertTo<Usuario>();
        }

        public async Task UpdateUsuarioAsync(Usuario usuario)
        {
            await _usuariosCollection.Document(usuario.Id).SetAsync(usuario, SetOptions.Overwrite);
        }

        public async Task<List<Usuario>> GetAllUsuariosAsync()
        {
            var snapshot = await _usuariosCollection.GetSnapshotAsync();
            return snapshot.Documents.Select(d => d.ConvertTo<Usuario>()).ToList();
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var key = _configuration["Jwt:Key"]
                      ?? throw new InvalidOperationException("Jwt:Key no configurada");
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim(ClaimTypes.GivenName, $"{usuario.Nombre} {usuario.Apellido}")
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expireMinutes = int.TryParse(_configuration["Jwt:ExpireInMinutes"], out var mins)
                ? mins
                : 60;

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
