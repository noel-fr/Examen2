using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Contraseña { get; set; } = string.Empty;
    }
}