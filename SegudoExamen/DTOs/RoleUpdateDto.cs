
using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs
{
    public class RoleUpdateDto
    {
        [Required(ErrorMessage = "El nuevo rol es requerido")]
        public string NewRole { get; set; } = string.Empty;
    }
}