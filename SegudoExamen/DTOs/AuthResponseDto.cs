

namespace SegundoExamen.DTOs // ¡Namespace: SegundoExamen.DTOs!
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty; // Nombre + Apellido
        public string Role { get; set; } = string.Empty; // Rol: usuario, bibliotecario, admin 
    }
}