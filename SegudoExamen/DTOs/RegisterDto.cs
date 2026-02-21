using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MinLength(3, ErrorMessage = "El nombre debe tener mínimo 3 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [MinLength(3, ErrorMessage = "El apellido debe tener mínimo 3 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres")]
        public string Contraseña { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de identidad es requerido")]
        public string NumeroIdentidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "La edad es requerida")]
        [Range(10, 120, ErrorMessage = "La edad debe estar entre 10 y 120 años")]
        public int Edad { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "El teléfono no es válido")]
        public string Telefono { get; set; } = string.Empty;
    }
}