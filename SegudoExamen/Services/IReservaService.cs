using Google.Cloud.Firestore;
using SegundoExamen.DTOs;
using SegundoExamen.Models;

namespace SegundoExamen.Services.Interfaces
{
    public interface IReservaService
    {
        Task<Reserva> CreateReserva(string usuarioId, CreateReservaDto dto);

        Task<Reserva?> GetReservaById(string reservaId);
        Task<List<Reserva>> GetReservasByUsuarioId(string usuarioId);
        Task<List<Reserva>> GetReservasByLibroId(string libroId);
        Task<List<Reserva>> GetAllReservas();

        Task<bool> CancelReserva(string reservaId);

        Task<List<Reserva>> GetReservasPendientesByLibroId(string libroId);

        // Usado desde PrestamoService dentro de una transacción
        Task<Reserva?> NotificarPrimeraReserva(Transaction transaction, string libroId);
    }
}