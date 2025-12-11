
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SegundoExamen.DTOs;
using SegundoExamen.Models;

namespace SegundoExamen.Services
{
    public class AuthService : IAuthService
    {
        private readonly FirebaseServices _firebaseService;
        private readonly IConfiguration _config;

        public AuthService(FirebaseServices firebaseService, IConfiguration config)
        {
            _firebaseService = firebaseService;
            _config = config;
        }

        private CollectionReference UsersCollection =>
            _firebaseService.GetCollection("usuarios");

        public async Task<AuthResponseDto> Register(RegisterDto registerDto)
        {
            // Verificar si ya existe
            var existing = await GetUserByEmail(registerDto.Email);
            if (existing != null)
                throw new Exception("Ya existe un usuario con ese correo.");

            var userId = Guid.NewGuid().ToString();

            var user = new User
            {
                Id = userId,
                Name = registerDto.Name,
                Lastname = registerDto.Lastname,
                Email = registerDto.Email,
                Age = registerDto.Age,
                IdentityNumber = registerDto.IdentityNumber,
                Phone = registerDto.Phone,
                Role = "usuario",
                IsActive = true,
                RegistrationDate = DateTime.UtcNow,
                Fines = 0
            };

            await UsersCollection.Document(userId).SetAsync(user);

            // ⚠️ Aquí deberías guardar contraseña hasheada en otra colección/campo.

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.Name} {user.Lastname}",
                Role = user.Role
            };
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            var user = await GetUserByEmail(loginDto.Email)
                       ?? throw new Exception("Credenciales inválidas.");

            if (!user.IsActive)
                throw new Exception("La cuenta está desactivada.");

            // ⚠️ Aquí deberías verificar la contraseña hasheada.

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.Name} {user.Lastname}",
                Role = user.Role
            };
        }

        public async Task<User?> GetUserById(string userId)
        {
            var doc = await UsersCollection.Document(userId).GetSnapshotAsync();
            return doc.Exists ? doc.ConvertTo<User>() : null;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            var snapshot = await UsersCollection
                .WhereEqualTo("Email", email)
                .Limit(1)
                .GetSnapshotAsync();

            return snapshot.Count == 0 ? null : snapshot.Documents[0].ConvertTo<User>();
        }

        public string GenerateJwtToken(User user)
        {
            var key = _config["Jwt:Key"] 
                      ?? throw new InvalidOperationException("Falta Jwt:Key en configuración.");
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.Name} {user.Lastname}"),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.TryParse(_config["Jwt:ExpireInMinutes"], out var m) ? m : 60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User> UpdateUserRole(string userId, string newRole)
        {
            var user = await GetUserById(userId) 
                       ?? throw new Exception("Usuario no encontrado.");

            user.Role = newRole;
            await UsersCollection.Document(userId).SetAsync(user, SetOptions.Overwrite);

            return user;
        }

        public async Task<User> UpdateUserFines(string userId, double amountChange)
        {
            var user = await GetUserById(userId) 
                       ?? throw new Exception("Usuario no encontrado.");

            user.Fines += amountChange;
            if (user.Fines < 0) user.Fines = 0;

            await UsersCollection.Document(userId).SetAsync(user, SetOptions.Overwrite);
            return user;
        }

        public async Task<User> ToggleUserStatus(string userId, bool isActive)
        {
            var user = await GetUserById(userId) 
                       ?? throw new Exception("Usuario no encontrado.");

            user.IsActive = isActive;
            await UsersCollection.Document(userId).SetAsync(user, SetOptions.Overwrite);
            return user;
        }
    }
}
