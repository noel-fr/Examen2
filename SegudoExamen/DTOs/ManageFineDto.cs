using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs
{
    public class ManageFineDto
    {
        [Required(ErrorMessage = "El monto a gestionar es requerido")]
        // Monto puede ser positivo (agregar) o negativo (condonar)
        public decimal Monto { get; set; } 
    }
}