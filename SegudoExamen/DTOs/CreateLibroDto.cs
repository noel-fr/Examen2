using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs
{
    public class CreateLibroDto
    {
        [Required(ErrorMessage = "El título es requerido")]
        [MinLength(3, ErrorMessage = "El título debe tener mínimo 3 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El autor es requerido")]
        [MinLength(3, ErrorMessage = "El autor debe tener mínimo 3 caracteres")]
        public string Autor { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ISBN es requerido")]
        [RegularExpression(@"^\d{10}(\d{3})?$", ErrorMessage = "ISBN debe tener 10 o 13 dígitos")]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es requerida")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "La editorial es requerida")]
        public string Editorial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El año de publicación es requerido")]
        [Range(1000, 2099, ErrorMessage = "El año debe ser válido")]
        public int AnoPublicacion { get; set; }

        // Copias Total debe ser igual a Copias Disponibles en la creación
        [Required(ErrorMessage = "El total de copias es requerido")]
        [Range(1, 1000, ErrorMessage = "Debe haber al menos 1 copia total")]
        public int CopiasTotal { get; set; } 
        
        [Required(ErrorMessage = "Las copias disponibles son requeridas")]
        [Range(1, 1000, ErrorMessage = "Debe haber al menos 1 copia")]
        public int CopiasDisponibles { get; set; } 

        [Required(ErrorMessage = "La ubicación es requerida")]
        public string Ubicacion { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string Descripcion { get; set; } = string.Empty;
    }
}