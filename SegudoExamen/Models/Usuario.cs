using Google.Cloud.Firestore;

namespace SegundoExamen.Models
{
    [FirestoreData]
    public class Usuario
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Apellido { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Correo { get; set; } = string.Empty;
        
        [FirestoreProperty] // <-- ¡CRÍTICO: Agregado para guardar el hash!
        public string? PasswordHash { get; set; } 

        [FirestoreProperty]
        public string NumeroIdentidad { get; set; } = string.Empty;

        [FirestoreProperty]
        public int Edad { get; set; }

        [FirestoreProperty]
        public string Telefono { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Rol { get; set; } = "usuario"; // "usuario", "bibliotecario", "admin"

        [FirestoreProperty]
        public bool Activo { get; set; } = true;

        [FirestoreProperty]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public decimal Multas { get; set; } = 0; // Multas en Lempiras
    }
}