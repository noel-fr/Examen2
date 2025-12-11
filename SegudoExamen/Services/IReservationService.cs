

using Google.Cloud.Firestore;
using SegundoExamen.DTOs;
using SegundoExamen.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SegundoExamen.Services 
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservation(string userId, ReservationDto reservationDto);
        
        Task<List<Reservation>> GetUserReservations(string userId);
        
        Task<bool> CancelReservation(string reservationId, string userId);
        
        Task NotifyNextReservationInTransaction(Transaction transaction, string bookId);
        
        Task<bool> CheckPendingReservations(string bookId);
    }
}