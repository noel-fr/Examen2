

using Google.Cloud.Firestore;
using System;

namespace SegundoExamen.Models 
{
    [FirestoreData]
    public class Reservation
    {
        [FirestoreProperty] public string Id { get; set; } = string.Empty;
        
        [FirestoreProperty] public string UserId { get; set; } = string.Empty; // usuarioId
        [FirestoreProperty] public string BookId { get; set; } = string.Empty; // libroId
        
        [FirestoreProperty] public DateTime ReservationDate { get; set; } = DateTime.UtcNow; // fechaReserva
        
        // Estados: pendiente, notificada, completada, cancelada
        [FirestoreProperty] public string Status { get; set; } = "pendiente"; 
        
        [FirestoreProperty] public DateTime? NotificationDate { get; set; } = null; // fechaNotificacion (puede ser null)
        // La fecha de expiración es 48 horas después de la notificación
        [FirestoreProperty] public DateTime? ExpirationDate { get; set; } = null; 
        
        [FirestoreProperty] public int Priority { get; set; } // posición en la cola
    }
}