
using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs
{
    public class FineManagementDto
    {
        // Positivo para aumentar la multa, negativo para reducirla
        [Required(ErrorMessage = "El cambio de monto es requerido")]
        public double AmountChange { get; set; }
    }
}