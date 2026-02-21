using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs
{
    public class CreatePrestamoDto
    {
        [Required(ErrorMessage = "El ID del libro es requerido")]
        public string LibroId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los días de préstamo son requeridos")]
        [Range(1, 30, ErrorMessage = "El préstamo puede ser entre 1 y 30 días")]
        public int DiasPrestamoDefault { get; set; } = 14;
    }
}