namespace SegundoExamen.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;
    }
}