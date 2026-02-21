using Google.Cloud.Firestore;

namespace SegudoExamen.Models;

[FirestoreData]
public class Libro
{
    [FirestoreDocumentId]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [FirestoreProperty]
    public string Titulo { get; set; }

    [FirestoreProperty]
    public string Autor { get; set; }

    [FirestoreProperty]
    public string ISBN { get; set; }

    [FirestoreProperty]
    public string Categoria { get; set; }

    [FirestoreProperty]
    public string Editorial { get; set; }

    [FirestoreProperty]
    public int AnioPublicacion { get; set; }
    [FirestoreProperty]
    public int CopiasDisponibles { get; set; }
    [FirestoreProperty]
    public int CopiasTotal { get; set; }
    [FirestoreProperty]
    public string Ubicacion { get; set; }
    [FirestoreProperty]
    public string Estado { get; set; } = "disponible"; // "disponible", "agotado", "en mantenimiento"
    [FirestoreProperty]
    public string Descripcion { get; set; }
    [FirestoreProperty]
    public Timestamp FechaIngreso { get; set; }
}