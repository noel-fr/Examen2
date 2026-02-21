using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs
{
    public class CreateReservaDto
    {
        [Required(ErrorMessage = "El ID del libro es requerido")]
        public string LibroId { get; set; } = string.Empty;
    }
}