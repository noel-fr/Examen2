using Google.Cloud.Firestore;

namespace SegundoExamen.Models
{
    [FirestoreData]
    public class Prestamo
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string UsuarioId { get; set; } = string.Empty; // ID del usuario
        
        // Propiedades de ayuda para reportes
        [FirestoreProperty]
        public string UsuarioNombre { get; set; } = string.Empty; 
        
        [FirestoreProperty]
        public string LibroId { get; set; } = string.Empty; // ID del libro
        
        [FirestoreProperty]
        public string LibroTitulo { get; set; } = string.Empty; 

        [FirestoreProperty]
        public DateTime FechaPrestamo { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime FechaDevolucionEsperada { get; set; } // 14 días después

        [FirestoreProperty]
        public DateTime? FechaDevolucionReal { get; set; }

        [FirestoreProperty]
        public int DiasRetraso { get; set; } = 0;

        [FirestoreProperty]
        public decimal MultaGenerada { get; set; } = 0; // 50 Lempiras por día

        [FirestoreProperty]
        public string Estado { get; set; } = "activo"; // "activo", "devuelto" o "vencido"

        [FirestoreProperty]
        public int Renovaciones { get; set; } = 0;
    }
}