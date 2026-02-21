using Google.Cloud.Firestore;

namespace SegudoExamen.Models;

[FirestoreData]
public class Usuario
{
    [FirestoreDocumentId] public string Id { get; set; } = Guid.NewGuid().ToString();
    [FirestoreProperty] public string Nombre { get; set; }
    [FirestoreProperty] public string Apellido { get; set; }
    [FirestoreProperty] public string Correo { get; set; }
    [FirestoreProperty] public string Contrasena { get; set; }
    [FirestoreProperty] public int Edad { get; set; }
    [FirestoreProperty] public string NumeroIdentidad { get; set; }
    [FirestoreProperty] public string Telefono { get; set; }
    [FirestoreProperty] public string Rol { get; set; } = "usuario";
    [FirestoreProperty] public bool Activo { get; set; } = true;
    [FirestoreProperty] public Timestamp FechaRegistro { get; set; }
    [FirestoreProperty] public double Multas { get; set; } = 0;
}