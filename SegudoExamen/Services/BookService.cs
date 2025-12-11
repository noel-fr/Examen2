

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using SegundoExamen.DTOs;
using SegundoExamen.Models;

namespace SegundoExamen.Services
{
    public class BookService : IBookService
    {
        private readonly FirebaseServices _firebaseService;

        public BookService(FirebaseServices firebaseService)
        {
            _firebaseService = firebaseService;
        }

        private CollectionReference BooksCollection =>
            _firebaseService.GetCollection("libros");

        // 1. Crear libro
        public async Task<Book> CreateBook(BookDto bookDto)
        {
            if (bookDto.AvailableCopies > bookDto.TotalCopies)
                throw new Exception("Las copias disponibles no pueden superar las copias totales.");

            // ISBN único
            var existing = await GetBookByISBN(bookDto.ISBN);
            if (existing != null)
                throw new Exception("Ya existe un libro con ese ISBN.");

            var id = Guid.NewGuid().ToString();

            var book = new Book
            {
                Id = id,
                Title = bookDto.Title,
                Author = bookDto.Author,
                ISBN = bookDto.ISBN,
                Category = bookDto.Category,
                Publisher = bookDto.Publisher,
                PublicationYear = bookDto.PublicationYear,
                TotalCopies = bookDto.TotalCopies,
                AvailableCopies = bookDto.AvailableCopies,
                Location = bookDto.Location,
                Description = bookDto.Description,
                Status = "disponible",
                EntryDate = DateTime.UtcNow
            };

            await BooksCollection.Document(id).SetAsync(book);
            return book;
        }

        // 2. Obtener todos los libros
        public async Task<List<Book>> GetAllBooks(string? category, string? author, bool onlyAvailable)
        {
            try
            {
                Query query = BooksCollection;

                if (!string.IsNullOrEmpty(category))
                    query = query.WhereEqualTo("Category", category);

                if (!string.IsNullOrEmpty(author))
                    query = query.WhereEqualTo("Author", author);

                if (onlyAvailable)
                    query = query.WhereGreaterThan("AvailableCopies", 0);

                var snapshot = await query.GetSnapshotAsync();
                return snapshot.Documents.Select(d => d.ConvertTo<Book>()).ToList();
            }
            catch
            {
                return new List<Book>();
            }
        }

        // 3. Obtener por Id
        public async Task<Book?> GetBookById(string bookId)
        {
            if (string.IsNullOrWhiteSpace(bookId))
                return null;

            var doc = await BooksCollection.Document(bookId).GetSnapshotAsync();
            return doc.Exists ? doc.ConvertTo<Book>() : null;
        }

        // 4. Actualizar
        public async Task<Book?> UpdateBook(string bookId, BookDto bookDto)
        {
            var existing = await GetBookById(bookId);
            if (existing == null)
                return null;

            if (bookDto.AvailableCopies > bookDto.TotalCopies)
                throw new Exception("Las copias disponibles no pueden superar las copias totales.");

            existing.Title = bookDto.Title;
            existing.Author = bookDto.Author;
            existing.ISBN = bookDto.ISBN;
            existing.Category = bookDto.Category;
            existing.Publisher = bookDto.Publisher;
            existing.PublicationYear = bookDto.PublicationYear;
            existing.TotalCopies = bookDto.TotalCopies;
            existing.AvailableCopies = bookDto.AvailableCopies;
            existing.Location = bookDto.Location;
            existing.Description = bookDto.Description;

            await BooksCollection.Document(bookId).SetAsync(existing, SetOptions.Overwrite);
            return existing;
        }

        // 5. Eliminar libro
        public async Task<bool> DeleteBook(string bookId)
        {
            var existing = await GetBookById(bookId);
            if (existing == null)
                return false;

            // Verificar préstamos activos y reservas pendientes sin usar ILoanService
            var hasRelations = await HasActiveLoansOrReservations(bookId);
            if (hasRelations)
                throw new Exception("No se puede eliminar el libro porque tiene préstamos activos o reservas pendientes.");

            await BooksCollection.Document(bookId).DeleteAsync();
            return true;
        }

        // 6. Decrementar copias
        public async Task DecrementAvailableCopies(string bookId)
        {
            var docRef = BooksCollection.Document(bookId);
            await docRef.UpdateAsync("AvailableCopies", FieldValue.Increment(-1));
        }

        // 7. Incrementar copias
        public async Task IncrementAvailableCopies(string bookId)
        {
            var docRef = BooksCollection.Document(bookId);
            await docRef.UpdateAsync("AvailableCopies", FieldValue.Increment(1));
        }

        // 8. Ver si tiene préstamos activos o reservas pendientes
        public async Task<bool> HasActiveLoansOrReservations(string bookId)
        {
            // Préstamos activos del libro
            var loansSnapshot = await _firebaseService
                .GetCollection("prestamos")
                .WhereEqualTo("BookId", bookId)
                .WhereIn("Status", new[] { "activo", "vencido" }) // ajusta según tu modelo
                .GetSnapshotAsync();

            // Reservas pendientes del libro
            var reservationsSnapshot = await _firebaseService
                .GetCollection("reservas")
                .WhereEqualTo("BookId", bookId)
                .WhereEqualTo("Status", "pendiente")
                .GetSnapshotAsync();

            return loansSnapshot.Count > 0 || reservationsSnapshot.Count > 0;
        }

        // 9. Buscar por ISBN
        public async Task<Book?> GetBookByISBN(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return null;

            var query = BooksCollection
                .WhereEqualTo("ISBN", isbn)
                .Limit(1);

            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents.Any()
                ? snapshot.Documents[0].ConvertTo<Book>()
                : null;
        }
    }
}

