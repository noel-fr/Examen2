

using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs 
{
    public class LoanDto
    {
        [Required(ErrorMessage = "El ID del libro es requerido")]
        public string BookId { get; set; } = string.Empty; 
    }
}