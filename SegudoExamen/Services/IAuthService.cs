using SegundoExamen.DTOs;
using SegundoExamen.Models;

namespace SegundoExamen.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterDto dto);
        Task<AuthResponseDto> Login(LoginDto dto);
        Task<Usuario?> GetUsuarioById(string id);
        Task<Usuario?> GetUsuarioByCorreo(string correo);

        // Para gestión administrativa
        Task UpdateUsuarioAsync(Usuario usuario);
        Task<List<Usuario>> GetAllUsuariosAsync();
    }
}