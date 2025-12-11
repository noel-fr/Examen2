

using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs 
{
    public class ReservationDto
    {
        [Required(ErrorMessage = "El ID del libro a reservar es requerido")]
        public string BookId { get; set; } = string.Empty; 
    }
}