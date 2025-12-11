

using System.ComponentModel.DataAnnotations;

namespace SegundoExamen.DTOs
{
    public class StatusUpdateDto
    {
        [Required]
        public bool IsActive { get; set; } 
    }
}