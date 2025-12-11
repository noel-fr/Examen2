
using SegundoExamen.DTOs;
using SegundoExamen.Models;

namespace SegundoExamen.Services 
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterDto registerdto);
        Task<AuthResponseDto> Login(LoginDto logindto);
        Task<User?> GetUserById(string userId);
        Task<User?> GetUserByEmail(string email);
        string GenerateJwtToken(User user);
        Task<User> UpdateUserRole(string userId, string newRole);
        Task<User> UpdateUserFines(string userId, double amountChange);
        Task<User> ToggleUserStatus(string userId, bool isActive);
    }
}