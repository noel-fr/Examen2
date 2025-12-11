using Google.Cloud.Firestore;
using SegundoExamen.DTOs;
using SegundoExamen.Models;
using SegundoExamen.Services.Interfaces;

namespace SegundoExamen.Services.Implementaciones
{
    public class ReservaService : IReservaService
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILibroService _libroService;
        private readonly IAuthService _authService;
        private readonly CollectionReference _reservasCollection;

        public ReservaService(
            IFirebaseService firebaseService,
            ILibroService libroService,
            IAuthService authService)
        {
            _firebaseService = firebaseService;
            _libroService = libroService;
            _authService = authService;
            _reservasCollection = _firebaseService.GetCollection("reservas");
        }

        public async Task<Reserva> CreateReserva(string usuarioId, CreateReservaDto createReservaDto)
        {
            // Transacción para consistencia
            return await _firebaseService
                .GetFirestoreDb()
                .RunTransactionAsync(async transaction =>
                {
                    var libro = await _libroService.GetLibroById(createReservaDto.LibroId)
                                ?? throw new Exception("Libro no encontrado.");

                    // 1. Validar que no tenga copias disponibles
                    if (libro.CopiasDisponibles > 0)
                    {
                        throw new Exception("El libro tiene copias disponibles. No se puede reservar.");
                    }

                    // 2. Validar que el usuario no tenga ya una reserva pendiente o notificada
                    var reservasActivasUsuario = await GetReservasPendientesYNotificadasByUsuarioLibro(usuarioId, libro.Id);
                    if (reservasActivasUsuario.Count > 0)
                    {
                        throw new Exception("Ya tienes una reserva pendiente o notificada para este libro.");
                    }

                    // 3. Asignar prioridad
                    var reservasPendientes = await GetReservasPendientesByLibroId(libro.Id);
                    int nuevaPrioridad = reservasPendientes.Count + 1;

                    // 4. Crear reserva
                    var reservaId = Guid.NewGuid().ToString();
                    var usuario = await _authService.GetUsuarioById(usuarioId);

                    var reserva = new Reserva
                    {
                        Id = reservaId,
                        UsuarioId = usuarioId,
                        UsuarioNombre = usuario != null
                            ? $"{usuario.Nombre} {usuario.Apellido}"
                            : "Desconocido",
                        LibroId = libro.Id,
                        LibroTitulo = libro.Titulo,
                        FechaReserva = DateTime.UtcNow,
                        Estado = "pendiente",
                        Prioridad = nuevaPrioridad
                    };

                    // 5. Guardar reserva
                    transaction.Create(_reservasCollection.Document(reservaId), reserva);

                    return reserva;
                });
        }

        public async Task<bool> CancelReserva(string reservaId)
        {
            // Transacción para eliminar y recalcular prioridades
            return await _firebaseService
                .GetFirestoreDb()
                .RunTransactionAsync(async transaction =>
                {
                    var docRef = _reservasCollection.Document(reservaId);
                    var snapshot = await transaction.GetSnapshotAsync(docRef);

                    if (!snapshot.Exists) return false;

                    var reserva = snapshot.ConvertTo<Reserva>();

                    // Solo se cancelan pendientes o notificadas
                    if (reserva.Estado != "pendiente" && reserva.Estado != "notificada")
                    {
                        throw new Exception("Solo se pueden cancelar reservas pendientes o notificadas.");
                    }

                    // 1. Eliminar reserva
                    transaction.Delete(docRef);

                    // 2. Recalcular prioridades de las reservas restantes
                    var reservasRestantesSnapshot = await _reservasCollection
                        .WhereEqualTo("LibroId", reserva.LibroId)
                        .WhereEqualTo("Estado", "pendiente")
                        .OrderBy("Prioridad")
                        .GetSnapshotAsync();

                    int nuevaPrioridad = 1;
                    foreach (var doc in reservasRestantesSnapshot.Documents)
                    {
                        if (doc.Id != reservaId)
                        {
                            transaction.Update(doc.Reference, "Prioridad", nuevaPrioridad);
                            nuevaPrioridad++;
                        }
                    }

                    return true;
                });
        }

        public async Task<Reserva?> NotificarPrimeraReserva(Transaction transaction, string libroId)
        {
            // Usado por PrestamoService al devolver un libro
            var snapshot = await _reservasCollection
                .WhereEqualTo("LibroId", libroId)
                .WhereEqualTo("Estado", "pendiente")
                .OrderBy("Prioridad")
                .Limit(1)
                .GetSnapshotAsync();

            if (snapshot.Count == 0) return null;

            var reservaDoc = snapshot.Documents[0];
            var fechaNotificacion = DateTime.UtcNow;
            var fechaExpiracion = fechaNotificacion.AddHours(48);

            transaction.Update(reservaDoc.Reference, new Dictionary<string, object>
            {
                { "Estado", "notificada" },
                { "FechaNotificacion", fechaNotificacion },
                { "FechaExpiracion", fechaExpiracion }
            });

            return reservaDoc.ConvertTo<Reserva>();
        }

        // ============ Métodos requeridos por IReservaService ============

        public async Task<Reserva?> GetReservaById(string reservaId)
        {
            var snapshot = await _reservasCollection.Document(reservaId).GetSnapshotAsync();
            if (!snapshot.Exists) return null;
            return snapshot.ConvertTo<Reserva>();
        }

        public async Task<List<Reserva>> GetReservasByUsuarioId(string usuarioId)
        {
            var snapshot = await _reservasCollection
                .WhereEqualTo("UsuarioId", usuarioId)
                .OrderByDescending("FechaReserva")
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<Reserva>())
                .ToList();
        }

        public async Task<List<Reserva>> GetReservasByLibroId(string libroId)
        {
            var snapshot = await _reservasCollection
                .WhereEqualTo("LibroId", libroId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<Reserva>())
                .ToList();
        }

        public async Task<List<Reserva>> GetAllReservas()
        {
            var snapshot = await _reservasCollection.GetSnapshotAsync();
            return snapshot.Documents
                .Select(doc => doc.ConvertTo<Reserva>())
                .ToList();
        }

        public async Task<List<Reserva>> GetReservasPendientesByLibroId(string libroId)
        {
            var snapshot = await _reservasCollection
                .WhereEqualTo("LibroId", libroId)
                .WhereEqualTo("Estado", "pendiente")
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<Reserva>())
                .ToList();
        }

        // ============ Helper privado ============

        private async Task<List<Reserva>> GetReservasPendientesYNotificadasByUsuarioLibro(
            string usuarioId,
            string libroId)
        {
            var snapshot = await _reservasCollection
                .WhereEqualTo("UsuarioId", usuarioId)
                .WhereEqualTo("LibroId", libroId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<Reserva>())
                .Where(r => r.Estado == "pendiente" || r.Estado == "notificada")
                .ToList();
        }
    }
}
