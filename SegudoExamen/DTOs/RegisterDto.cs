


using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs 
{
    public class RegisterDto
    {
        // Campos de Autenticación y Validación
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email es inválido")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El password es requerido")]
        [MinLength(8, ErrorMessage = "El password debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;
        
        // Campos de Datos Personales (Colección 'usuarios')
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // nombre 

        [Required(ErrorMessage = "El apellido es requerido")]
        [MaxLength(50)]
        public string Lastname { get; set; } = string.Empty; // apellido 
        
        [Required(ErrorMessage = "El número de identidad es requerido")]
        [MaxLength(30)]
        public string IdentityNumber { get; set; } = string.Empty; // numeroIdentidad 
        
        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        public string Phone { get; set; } = string.Empty; // telefono 
        
        [Required(ErrorMessage = "La edad es requerida")]
        [Range(18, 100, ErrorMessage = "Debe ser mayor de 18 años para registrarse")]
        public int Age { get; set; } // edad 
    }
}