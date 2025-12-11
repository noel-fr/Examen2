
using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs 
{
    public class LoginDto
    {
        [Required(ErrorMessage ="El email es requerido")]
        [EmailAddress(ErrorMessage ="El email es inválido")]
        public string Email { get; set; } = string.Empty;
     
        [Required(ErrorMessage = "El password es requerido")]
        public string Password { get; set; } = string.Empty;
    }
}