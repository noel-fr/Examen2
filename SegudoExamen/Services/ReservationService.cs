
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using SegundoExamen.DTOs;
using SegundoExamen.Models;

namespace SegundoExamen.Services
{
    public class ReservationService : IReservationService
    {
        private readonly FirebaseServices _firebaseService;

        public ReservationService(FirebaseServices firebaseService)
        {
            _firebaseService = firebaseService;
        }

        private CollectionReference ReservationsCollection =>
            _firebaseService.GetCollection("reservas");

        // ============================================================
        // 1. Crear reserva
        // ============================================================
        public async Task<Reservation> CreateReservation(string userId, ReservationDto reservationDto)
        {
            if (reservationDto == null)
                throw new ArgumentNullException(nameof(reservationDto));

            if (string.IsNullOrWhiteSpace(reservationDto.BookId))
                throw new Exception("El ID del libro es requerido para la reserva.");

            var db = _firebaseService.GetFirestoreDb();

            // Usamos una transacción para calcular la prioridad y guardar la reserva
            return await db.RunTransactionAsync(async transaction =>
            {
                // Traemos TODAS las reservas del libro (un solo filtro en Firestore)
                var snapshot = await ReservationsCollection
                    .WhereEqualTo("BookId", reservationDto.BookId)
                    .GetSnapshotAsync();

                // Calculamos la prioridad en memoria
                int nextPriority = snapshot.Count + 1;

                var reservationId = Guid.NewGuid().ToString();
                var docRef = ReservationsCollection.Document(reservationId);

                var reservation = new Reservation
                {
                    Id = reservationId,
                    UserId = userId,
                    BookId = reservationDto.BookId,
                    ReservationDate = DateTime.UtcNow,
                    Status = "pendiente",        // pendiente, notificada, completada, cancelada
                    NotificationDate = null,
                    ExpirationDate = null,
                    Priority = nextPriority
                };

                transaction.Set(docRef, reservation);
                return reservation;
            });
        }

        // ============================================================
        // 2. Obtener reservas de un usuario
        // ============================================================
        public async Task<List<Reservation>> GetUserReservations(string userId)
        {
            var snapshot = await ReservationsCollection
                .WhereEqualTo("UserId", userId)
                .GetSnapshotAsync();

            // Ordenamos en memoria por fecha desc.
            return snapshot.Documents
                .Select(d => d.ConvertTo<Reservation>())
                .OrderByDescending(r => r.ReservationDate)
                .ToList();
        }

        // ============================================================
        // 3. Cancelar reserva (solo el dueño puede cancelarla)
        // ============================================================
        public async Task<bool> CancelReservation(string reservationId, string userId)
        {
            var docRef = ReservationsCollection.Document(reservationId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return false;

            var reservation = snapshot.ConvertTo<Reservation>();

            if (reservation.UserId != userId)
                return false;

            if (reservation.Status == "completada")
                return false;

            reservation.Status = "cancelada";
            await docRef.SetAsync(reservation, SetOptions.Overwrite);

            return true;
        }

        // ============================================================
        // 4. ¿Hay reservas pendientes para este libro?
        // ============================================================
        public async Task<bool> CheckPendingReservations(string bookId)
        {
            var snapshot = await ReservationsCollection
                .WhereEqualTo("BookId", bookId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(d => d.ConvertTo<Reservation>())
                .Any(r => r.Status == "pendiente");
        }

        // ============================================================
        // 5. Notificar siguiente reserva (llamado desde LoanService)
        // ============================================================
        public async Task NotifyNextReservationInTransaction(Transaction transaction, string bookId)
        {
            // OJO: no usamos 'transaction' realmente, pero respetamos la firma
            var snapshot = await ReservationsCollection
                .WhereEqualTo("BookId", bookId)
                .GetSnapshotAsync();

            var next = snapshot.Documents
                .Select(d => new { Snapshot = d, Reservation = d.ConvertTo<Reservation>() })
                .Where(x => x.Reservation.Status == "pendiente")
                .OrderBy(x => x.Reservation.Priority)
                .FirstOrDefault();

            if (next == null)
                return;

            var reservation = next.Reservation;
            reservation.Status = "notificada";
            reservation.NotificationDate = DateTime.UtcNow;
            reservation.ExpirationDate = reservation.NotificationDate.Value.AddHours(48);

            await next.Snapshot.Reference.SetAsync(reservation, SetOptions.Overwrite);
        }
    }
}
