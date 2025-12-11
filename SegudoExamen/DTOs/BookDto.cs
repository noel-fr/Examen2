

using System.ComponentModel.DataAnnotations;
using System;

namespace SegundoExamen.DTOs
{
    public class BookDto
    {
        [Required(ErrorMessage = "El título es requerido")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El autor es requerido")]
        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El ISBN es requerido")]
        [MaxLength(20)]
        public string ISBN { get; set; } = string.Empty; // Debe ser único
        
        [Required(ErrorMessage = "La categoría es requerida")]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La editorial es requerida")]
        [MaxLength(100)]
        public string Publisher { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El año de publicación es requerido")]
        [Range(1000, 2025, ErrorMessage = "Año de publicación inválido")]
        public int PublicationYear { get; set; }
        
        [Required(ErrorMessage = "El total de copias es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El total de copias debe ser al menos 1")]
        public int TotalCopies { get; set; } // copiasTotal
        
        [Required(ErrorMessage = "Las copias disponibles son requeridas")]
        [Range(0, int.MaxValue, ErrorMessage = "Las copias disponibles no pueden ser negativas")]
        public int AvailableCopies { get; set; } // copiasDisponibles
        
        [Required(ErrorMessage = "La ubicación es requerida")]
        [MaxLength(50)]
        public string Location { get; set; } = string.Empty;
        
        [MaxLength(500, ErrorMessage = "La descripción no debe exceder los 500 caracteres")]
        public string Description { get; set; } = string.Empty;
        
        // Validación en el controlador: AvailableCopies <= TotalCopies
    }
}