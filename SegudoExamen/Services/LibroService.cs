using Google.Cloud.Firestore;
using SegundoExamen.DTOs;
using SegundoExamen.Models;
using SegundoExamen.Services.Interfaces;

namespace SegundoExamen.Services.Implementaciones
{
    public class LibroService : ILibroService
    {
        private readonly IFirebaseService _firebaseService;
        private readonly IPrestamoService _prestamoService;
        private readonly IReservaService _reservaService;
        private readonly CollectionReference _librosCollection;

        public LibroService(
            IFirebaseService firebaseService,
            IPrestamoService prestamoService,
            IReservaService reservaService)
        {
            _firebaseService = firebaseService;
            _prestamoService = prestamoService;
            _reservaService = reservaService;
            _librosCollection = _firebaseService.GetCollection("libros");
        }

        public async Task<Libro> CreateLibro(CreateLibroDto createLibroDto)
        {
            // 1. Validación: ISBN único
            var existingBook = await _librosCollection
                .WhereEqualTo("ISBN", createLibroDto.ISBN)
                .Limit(1)
                .GetSnapshotAsync();

            if (existingBook.Count > 0)
            {
                throw new Exception("Ya existe un libro con este ISBN.");
            }

            // 2. Validación: Copias Disponibles <= Copias Total
            if (createLibroDto.CopiasDisponibles > createLibroDto.CopiasTotal)
            {
                throw new Exception("Las copias disponibles no pueden exceder las copias totales.");
            }

            // 3. Crear y guardar
            var libroId = Guid.NewGuid().ToString();
            var libro = new Libro
            {
                Id = libroId,
                Titulo = createLibroDto.Titulo,
                Autor = createLibroDto.Autor,
                ISBN = createLibroDto.ISBN,
                Categoria = createLibroDto.Categoria,
                Editorial = createLibroDto.Editorial,
                AnoPublicacion = createLibroDto.AnoPublicacion,
                CopiasDisponibles = createLibroDto.CopiasDisponibles,
                CopiasTotal = createLibroDto.CopiasTotal,
                Ubicacion = createLibroDto.Ubicacion,
                Descripcion = createLibroDto.Descripcion,
                Estado = createLibroDto.CopiasDisponibles > 0 ? "disponible" : "agotado",
                FechaIngreso = DateTime.UtcNow
            };

            await _librosCollection.Document(libroId).SetAsync(libro);
            return libro;
        }

        public async Task<Libro?> GetLibroById(string libroId)
        {
            var doc = await _librosCollection.Document(libroId).GetSnapshotAsync();
            if (!doc.Exists) return null;
            return doc.ConvertTo<Libro>();
        }

        public async Task<List<Libro>> GetAllLibros()
        {
            var snapshot = await _librosCollection.GetSnapshotAsync();
            return snapshot.Documents
                .Select(doc => doc.ConvertTo<Libro>())
                .ToList();
        }

        public async Task<List<Libro>> GetLibrosByCategoria(string categoria)
        {
            var snapshot = await _librosCollection
                .WhereEqualTo("Categoria", categoria)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<Libro>())
                .ToList();
        }

        public async Task<List<Libro>> GetLibrosByAutor(string autor)
        {
            var snapshot = await _librosCollection
                .WhereEqualTo("Autor", autor)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(doc => doc.ConvertTo<Libro>())
                .ToList();
        }

        public async Task<List<Libro>> SearchLibros(string searchTerm)
        {
            var allLibros = await GetAllLibros();

            return allLibros
                .Where(l =>
                    (l.Titulo != null && l.Titulo.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (l.Autor != null && l.Autor.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (l.Categoria != null && l.Categoria.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public async Task<Libro?> UpdateLibro(string libroId, UpdateLibroDto updateLibroDto)
        {
            var docRef = _librosCollection.Document(libroId);
            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists) return null;

            var existingLibro = snapshot.ConvertTo<Libro>();
            var updates = new Dictionary<string, object>();

            // Validación: no reducir CopiasTotal por debajo de los libros prestados
            var librosPrestados = existingLibro.CopiasTotal - existingLibro.CopiasDisponibles;

            if (updateLibroDto.CopiasTotal.HasValue &&
                updateLibroDto.CopiasTotal.Value < librosPrestados)
            {
                throw new Exception(
                    $"No se puede reducir CopiasTotal a {updateLibroDto.CopiasTotal.Value} porque hay {librosPrestados} copias prestadas.");
            }

            if (updateLibroDto.CopiasTotal.HasValue)
                updates["CopiasTotal"] = updateLibroDto.CopiasTotal.Value;

            // Actualizar Copias Disponibles y Estado
            if (updateLibroDto.CopiasDisponibles.HasValue)
            {
                updates["CopiasDisponibles"] = updateLibroDto.CopiasDisponibles.Value;

                if (updateLibroDto.CopiasDisponibles.Value == 0)
                {
                    updates["Estado"] = "agotado";
                }
                else if (updateLibroDto.CopiasDisponibles.Value > 0 &&
                         existingLibro.Estado == "agotado")
                {
                    updates["Estado"] = "disponible";
                }
            }

            if (!string.IsNullOrWhiteSpace(updateLibroDto.Titulo))
                updates["Titulo"] = updateLibroDto.Titulo;

            if (!string.IsNullOrWhiteSpace(updateLibroDto.Autor))
                updates["Autor"] = updateLibroDto.Autor;

            if (!string.IsNullOrWhiteSpace(updateLibroDto.Categoria))
                updates["Categoria"] = updateLibroDto.Categoria;

            if (!string.IsNullOrWhiteSpace(updateLibroDto.Editorial))
                updates["Editorial"] = updateLibroDto.Editorial;

            if (updateLibroDto.AnoPublicacion.HasValue)
                updates["AnoPublicacion"] = updateLibroDto.AnoPublicacion.Value;

            if (!string.IsNullOrWhiteSpace(updateLibroDto.Ubicacion))
                updates["Ubicacion"] = updateLibroDto.Ubicacion;

            if (!string.IsNullOrWhiteSpace(updateLibroDto.Estado))
                updates["Estado"] = updateLibroDto.Estado;

            if (!string.IsNullOrWhiteSpace(updateLibroDto.Descripcion))
                updates["Descripcion"] = updateLibroDto.Descripcion;

            if (updates.Count > 0)
            {
                await docRef.UpdateAsync(updates);
            }

            var updatedSnapshot = await docRef.GetSnapshotAsync();
            return updatedSnapshot.ConvertTo<Libro>();
        }

        public async Task<bool> DeleteLibro(string libroId)
        {
            var docRef = _librosCollection.Document(libroId);
            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists) return false;

            // Validación: no eliminar si tiene préstamos activos
            var prestamosActivos = await _prestamoService.GetPrestamosActivosByLibroId(libroId);
            if (prestamosActivos.Count > 0)
            {
                throw new Exception("No se puede eliminar el libro: tiene préstamos activos.");
            }

            // Validación: no eliminar si tiene reservas pendientes
            var reservasPendientes = await _reservaService.GetReservasPendientesByLibroId(libroId);
            if (reservasPendientes.Count > 0)
            {
                throw new Exception("No se puede eliminar el libro: tiene reservas pendientes.");
            }

            await docRef.DeleteAsync();
            return true;
        }
    }
}
