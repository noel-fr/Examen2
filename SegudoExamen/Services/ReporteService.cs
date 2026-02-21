using Google.Cloud.Firestore;
using SegundoExamen.Models;
using SegundoExamen.Services.Interfaces;

namespace SegundoExamen.Services.Implementaciones
{
    public class ReporteService : IReporteService
    {
        private readonly IFirebaseService _firebaseService;
        private readonly IPrestamoService _prestamoService;
        private readonly IReservaService _reservaService;

        private readonly CollectionReference _usuariosCollection;
        private readonly CollectionReference _prestamosCollection;
        private readonly CollectionReference _librosCollection;
        private readonly CollectionReference _reservasCollection;

        public ReporteService(
            IFirebaseService firebaseService,
            IPrestamoService prestamoService,
            IReservaService reservaService)
        {
            _firebaseService = firebaseService;
            _prestamoService = prestamoService;
            _reservaService = reservaService;

            _usuariosCollection = _firebaseService.GetCollection("usuarios");
            _prestamosCollection = _firebaseService.GetCollection("prestamos");
            _librosCollection = _firebaseService.GetCollection("libros");
            _reservasCollection = _firebaseService.GetCollection("reservas");
        }

        // Escenario 5: usuarios con multas pendientes
        public async Task<List<Usuario>> GetUsersWithOverdueFines()
        {
            var snapshot = await _usuariosCollection
                .WhereGreaterThan("Multas", 0)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<Usuario>())
                .ToList();
        }

        // Escenario 5: libros populares en últimos 30 días
        public async Task<List<LibroPopular>> GetPopularBooks()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            // Préstamos de los últimos 30 días
            var snapshot = await _prestamosCollection
                .WhereGreaterThanOrEqualTo("FechaPrestamo", thirtyDaysAgo)
                .GetSnapshotAsync();

            var prestamos = snapshot.Documents
                .Select(doc => doc.ConvertTo<Prestamo>())
                .ToList();

            var popularBooks = prestamos
                .GroupBy(p => new { p.LibroId, p.LibroTitulo })
                .Select(g => new LibroPopular
                {
                    LibroId = g.Key.LibroId,
                    Titulo = g.Key.LibroTitulo,
                    Prestamos30Dias = g.Count()
                })
                .OrderByDescending(b => b.Prestamos30Dias)
                .Take(10)
                .ToList();

            // Completar información con Autor y Reservas Pendientes
            foreach (var book in popularBooks)
            {
                var libroSnapshot = await _librosCollection
                    .Document(book.LibroId)
                    .GetSnapshotAsync();

                if (libroSnapshot.Exists)
                {
                    var libroData = libroSnapshot.ConvertTo<Libro>();
                    book.Autor = libroData.Autor;
                }

                var reservas = await _reservaService.GetReservasPendientesByLibroId(book.LibroId);
                book.ReservasPendientes = reservas.Count;
            }

            return popularBooks;
        }

        // Escenario 5: historial de préstamos de un usuario
        public async Task<List<Prestamo>> GetUserLoanHistory(string userId)
        {
            return await _prestamoService.GetPrestamosByUsuarioId(userId);
        }

        // Escenario 6: dashboard de estadísticas
        public async Task<DashboardStats> GetDashboardStats()
        {
            var todosUsuariosSnapshot = await _usuariosCollection.GetSnapshotAsync();
            var todosPrestamosSnapshot = await _prestamosCollection.GetSnapshotAsync();
            var todasReservasSnapshot = await _reservasCollection.GetSnapshotAsync();
            var todosLibrosSnapshot = await _librosCollection.GetSnapshotAsync();

            var todosUsuarios = todosUsuariosSnapshot.Documents
                .Select(d => d.ConvertTo<Usuario>())
                .ToList();

            var todosPrestamos = todosPrestamosSnapshot.Documents
                .Select(d => d.ConvertTo<Prestamo>())
                .ToList();

            var todasReservas = todasReservasSnapshot.Documents
                .Select(d => d.ConvertTo<Reserva>())
                .ToList();

            var prestamosActivosOVencidos = todosPrestamos
                .Where(p => p.Estado == "activo" || p.Estado == "vencido")
                .ToList();

            var stats = new DashboardStats
            {
                TotalUsuariosActivos = todosUsuarios.Count(u => u.Activo),
                TotalLibrosCatalogo = todosLibrosSnapshot.Count,
                TotalPrestamosActivos = prestamosActivosOVencidos.Count(p => p.Estado == "activo"),
                TotalPrestamosVencidos = prestamosActivosOVencidos.Count(p => p.FechaDevolucionEsperada < DateTime.UtcNow),
                TotalMultasPendientes = todosUsuarios.Sum(u => u.Multas),
                TotalReservasPendientes = todasReservas.Count(r => r.Estado == "pendiente")
            };

            return stats;
        }
    }
}
