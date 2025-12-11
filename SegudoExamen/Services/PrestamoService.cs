using Google.Cloud.Firestore;
using SegundoExamen.DTOs;
using SegundoExamen.Models;
using SegundoExamen.Services.Interfaces;

namespace SegundoExamen.Services.Implementaciones
{
    public class PrestamoService : IPrestamoService
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILibroService _libroService;
        private readonly IAuthService _authService;
        private readonly IReservaService _reservaService;

        private readonly CollectionReference _prestamosCollection;
        private readonly CollectionReference _librosCollection;
        private readonly CollectionReference _usuariosCollection;

        public PrestamoService(
            IFirebaseService firebaseService,
            ILibroService libroService,
            IAuthService authService,
            IReservaService reservaService)
        {
            _firebaseService = firebaseService;
            _libroService = libroService;
            _authService = authService;
            _reservaService = reservaService;

            _prestamosCollection = _firebaseService.GetCollection("prestamos");
            _librosCollection = _firebaseService.GetCollection("libros");
            _usuariosCollection = _firebaseService.GetCollection("usuarios");
        }

        // Escenario 3: Crear préstamo con validaciones y transacción
        public async Task<Prestamo> CreatePrestamo(string usuarioId, CreatePrestamoDto createPrestamoDto)
        {
            // Usar transacción para asegurar consistencia
            return await _firebaseService
                .GetFirestoreDb()
                .RunTransactionAsync(async transaction =>
                {
                    var usuarioDocRef = _usuariosCollection.Document(usuarioId);
                    var libroDocRef = _librosCollection.Document(createPrestamoDto.LibroId);

                    var usuarioSnapshot = await transaction.GetSnapshotAsync(usuarioDocRef);
                    var libroSnapshot = await transaction.GetSnapshotAsync(libroDocRef);

                    if (!usuarioSnapshot.Exists)
                        throw new Exception("Usuario no encontrado.");

                    if (!libroSnapshot.Exists)
                        throw new Exception("Libro no encontrado.");

                    var usuario = usuarioSnapshot.ConvertTo<Usuario>();
                    var libro = libroSnapshot.ConvertTo<Libro>();

                    // 1. Validar copias disponibles
                    if (libro.CopiasDisponibles <= 0)
                    {
                        throw new Exception("El libro no tiene copias disponibles para préstamo.");
                    }

                    // 2. Validar multas pendientes > 500
                    if (usuario.Multas > 500)
                    {
                        throw new Exception(
                            $"Usuario tiene multas pendientes de {usuario.Multas} Lempiras. Máximo permitido: 500 L.");
                    }

                    // 3. Validar no más de 3 préstamos activos
                    var prestamosActivos = await GetPrestamosActivosByUsuarioId(usuarioId);
                    if (prestamosActivos.Count >= 3)
                    {
                        throw new Exception("El usuario ya tiene 3 préstamos activos. Límite alcanzado.");
                    }

                    // 4. Crear préstamo
                    var prestamoId = Guid.NewGuid().ToString();
                    var fechaPrestamo = DateTime.UtcNow;
                    var fechaDevolucionEsperada = fechaPrestamo.AddDays(14); // 14 días

                    var prestamo = new Prestamo
                    {
                        Id = prestamoId,
                        UsuarioId = usuarioId,
                        UsuarioNombre = $"{usuario.Nombre} {usuario.Apellido}",
                        LibroId = libro.Id,
                        LibroTitulo = libro.Titulo,
                        FechaPrestamo = fechaPrestamo,
                        FechaDevolucionEsperada = fechaDevolucionEsperada,
                        Estado = "activo",
                        Renovaciones = 0
                    };

                    // 5. Actualizar libro (decrementar copias disponibles)
                    transaction.Update(libroDocRef, new Dictionary<string, object>
                    {
                        { "CopiasDisponibles", FieldValue.Increment(-1) },
                        { "Estado", libro.CopiasDisponibles - 1 == 0 ? "agotado" : "disponible" }
                    });

                    // 6. Guardar préstamo
                    transaction.Create(_prestamosCollection.Document(prestamoId), prestamo);

                    return prestamo;
                });
        }

        public async Task<Prestamo?> GetPrestamoById(string prestamoId)
        {
            var snapshot = await _prestamosCollection.Document(prestamoId).GetSnapshotAsync();
            if (!snapshot.Exists) return null;

            return snapshot.ConvertTo<Prestamo>();
        }

        public async Task<List<Prestamo>> GetPrestamosByUsuarioId(string usuarioId)
        {
            var query = _prestamosCollection
                .WhereEqualTo("UsuarioId", usuarioId)
                .OrderByDescending("FechaPrestamo");

            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents
                .Select(d => d.ConvertTo<Prestamo>())
                .ToList();
        }

        public async Task<List<Prestamo>> GetAllPrestamos()
        {
            var snapshot = await _prestamosCollection
                .OrderByDescending("FechaPrestamo")
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(d => d.ConvertTo<Prestamo>())
                .ToList();
        }

        // Escenario 5: préstamos vencidos (activos que ya pasaron su fechaDevolucionEsperada)
        public async Task<List<Prestamo>> GetVencidosPrestamos()
        {
            var snapshot = await _prestamosCollection
                .WhereEqualTo("Estado", "activo")
                .GetSnapshotAsync();

            var now = DateTime.UtcNow;

            return snapshot.Documents
                .Select(d => d.ConvertTo<Prestamo>())
                .Where(p => p.FechaDevolucionEsperada < now)
                .ToList();
        }

        // Escenario 3: Devolver préstamo, calcular multa y actualizar usuario/libro
        public async Task<Prestamo?> DevolverPrestamo(string prestamoId)
        {
            // Usar transacción para asegurar consistencia
            return await _firebaseService
                .GetFirestoreDb()
                .RunTransactionAsync(async transaction =>
                {
                    var prestamoDocRef = _prestamosCollection.Document(prestamoId);
                    var prestamoSnapshot = await transaction.GetSnapshotAsync(prestamoDocRef);

                    if (!prestamoSnapshot.Exists) return null;

                    var prestamo = prestamoSnapshot.ConvertTo<Prestamo>();

                    if (prestamo.Estado != "activo" && prestamo.Estado != "vencido")
                    {
                        throw new Exception("El préstamo ya fue devuelto.");
                    }

                    var fechaActual = DateTime.UtcNow;

                    // 1. Calcular días de retraso y multa
                    int diasRetraso = 0;
                    decimal multaGenerada = 0m;

                    if (fechaActual > prestamo.FechaDevolucionEsperada)
                    {
                        TimeSpan retraso = fechaActual - prestamo.FechaDevolucionEsperada;
                        diasRetraso = (int)Math.Ceiling(retraso.TotalDays);
                        multaGenerada = diasRetraso * 50m; // 50 Lempiras por día
                    }

                    // 2. Actualizar préstamo
                    var prestamoUpdates = new Dictionary<string, object>
                    {
                        { "FechaDevolucionReal", fechaActual },
                        { "DiasRetraso", diasRetraso },
                        { "MultaGenerada", multaGenerada },
                        { "Estado", "devuelto" }
                    };

                    transaction.Update(prestamoDocRef, prestamoUpdates);

                    // 3. Sumar multa al usuario (IMPORTANTE: castear decimal -> double)
                    var usuarioDocRef = _usuariosCollection.Document(prestamo.UsuarioId);
                    transaction.Update(
                        usuarioDocRef,
                        "Multas",
                        FieldValue.Increment((double)multaGenerada)
                    );

                    // 4. Incrementar copias disponibles del libro
                    var libroDocRef = _librosCollection.Document(prestamo.LibroId);
                    transaction.Update(libroDocRef, new Dictionary<string, object>
                    {
                        { "CopiasDisponibles", FieldValue.Increment(1) },
                        { "Estado", "disponible" }
                    });

                    // 5. Notificar primera reserva (si hay reservas pendientes)
                    await _reservaService.NotificarPrimeraReserva(transaction, prestamo.LibroId);

                    // Actualizar objeto en memoria antes de devolverlo
                    prestamo.FechaDevolucionReal = fechaActual;
                    prestamo.DiasRetraso = diasRetraso;
                    prestamo.MultaGenerada = multaGenerada;
                    prestamo.Estado = "devuelto";

                    return prestamo;
                });
        }

        // Escenario 3: Renovar préstamo (máximo 2 renovaciones, 7 días c/u, sin reservas pendientes)
        public async Task<Prestamo?> RenovarPrestamo(string prestamoId)
        {
            return await _firebaseService
                .GetFirestoreDb()
                .RunTransactionAsync(async transaction =>
                {
                    var prestamoDocRef = _prestamosCollection.Document(prestamoId);
                    var prestamoSnapshot = await transaction.GetSnapshotAsync(prestamoDocRef);

                    if (!prestamoSnapshot.Exists) return null;

                    var prestamo = prestamoSnapshot.ConvertTo<Prestamo>();

                    if (prestamo.Estado != "activo")
                    {
                        throw new Exception("Solo se pueden renovar préstamos activos.");
                    }

                    if (prestamo.Renovaciones >= 2)
                    {
                        throw new Exception("El préstamo ya alcanzó el máximo de 2 renovaciones.");
                    }

                    // Validar que no existan reservas pendientes para ese libro
                    var reservasPendientes = await _reservaService.GetReservasPendientesByLibroId(prestamo.LibroId);
                    if (reservasPendientes.Any())
                    {
                        throw new Exception("No se puede renovar: existen reservas pendientes para este libro.");
                    }

                    var nuevaFechaDevolucion = prestamo.FechaDevolucionEsperada.AddDays(7);

                    var updates = new Dictionary<string, object>
                    {
                        { "FechaDevolucionEsperada", nuevaFechaDevolucion },
                        { "Renovaciones", prestamo.Renovaciones + 1 }
                    };

                    transaction.Update(prestamoDocRef, updates);

                    prestamo.FechaDevolucionEsperada = nuevaFechaDevolucion;
                    prestamo.Renovaciones += 1;

                    return prestamo;
                });
        }

        // Auxiliar para LibroService: préstamos activos por libro
        public async Task<List<Prestamo>> GetPrestamosActivosByLibroId(string libroId)
        {
            var snapshot = await _prestamosCollection
                .WhereEqualTo("LibroId", libroId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(d => d.ConvertTo<Prestamo>())
                .Where(p => p.Estado == "activo")
                .ToList();
        }

        // Auxiliar para UsuariosController: préstamos activos por usuario
        public async Task<List<Prestamo>> GetPrestamosActivosByUsuarioId(string usuarioId)
        {
            var snapshot = await _prestamosCollection
                .WhereEqualTo("UsuarioId", usuarioId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(d => d.ConvertTo<Prestamo>())
                .Where(p => p.Estado == "activo")
                .ToList();
        }
    }
}
