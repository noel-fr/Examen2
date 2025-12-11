using SegundoExamen.Models;

namespace SegundoExamen.Services.Interfaces
{
    public interface IReporteService
    {
        // Escenario 5
        Task<List<Usuario>> GetUsersWithOverdueFines(); // ...
        Task<List<LibroPopular>> GetPopularBooks();     // ...
        Task<List<Prestamo>> GetUserLoanHistory(string userId); // ...

        // Escenario 6
        Task<DashboardStats> GetDashboardStats(); // ...
    }
}