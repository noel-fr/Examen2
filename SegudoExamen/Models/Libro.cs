using Google.Cloud.Firestore;

namespace SegundoExamen.Models
{
    [FirestoreData]
    public class Libro
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Titulo { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Autor { get; set; } = string.Empty;

        [FirestoreProperty]
        public string ISBN { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Categoria { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Editorial { get; set; } = string.Empty;

        [FirestoreProperty]
        public int AnoPublicacion { get; set; }

        [FirestoreProperty]
        public int CopiasDisponibles { get; set; }

        [FirestoreProperty]
        public int CopiasTotal { get; set; } // Requerido por el examen

        [FirestoreProperty]
        public string Ubicacion { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Estado { get; set; } = "disponible"; // "disponible", "agotado", "en mantenimiento"

        [FirestoreProperty]
        public string Descripcion { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;
    }
}