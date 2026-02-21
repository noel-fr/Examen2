using Google.Cloud.Firestore;

namespace SegudoExamen.Models;

[FirestoreData]
public class Prestamo
{
    [FirestoreDocumentId]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [FirestoreProperty]
    public string UsuarioId { get; set; }

    [FirestoreProperty]
    public string LibroId { get; set; }

    [FirestoreProperty]
    public Timestamp FechaPrestamo { get; set; }

    [FirestoreProperty]
    public Timestamp FechaDevolucionEsperada { get; set; }

    [FirestoreProperty]
    public Timestamp? FechaDevolucionReal { get; set; }

    [FirestoreProperty]
    public int DiasRetraso { get; set; } = 0;

    [FirestoreProperty]
    public double MultaGenerada { get; set; } = 0;

    [FirestoreProperty]
    public string Estado { get; set; } = "activo"; // "activo", "devuelto", "vencido"

    [FirestoreProperty]
    public int Renovaciones { get; set; } = 0;
}