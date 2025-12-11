using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs
{
    public class UpdateLibroDto
    {
        public string? Titulo { get; set; }

        public string? Autor { get; set; }

        public string? Categoria { get; set; }

        public string? Editorial { get; set; }

        public int? AnoPublicacion { get; set; }

        public int? CopiasTotal { get; set; } // Agregado para validación de Escenario 2

        public int? CopiasDisponibles { get; set; }

        public string? Ubicacion { get; set; }

        [RegularExpression("^(disponible|agotado|en mantenimiento)$", ErrorMessage = "Estado inválido")]
        public string? Estado { get; set; } //

        public string? Descripcion { get; set; }
    }
}