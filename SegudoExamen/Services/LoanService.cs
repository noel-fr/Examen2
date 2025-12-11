using Google.Cloud.Firestore;
using SegundoExamen.DTOs;
using SegundoExamen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SegundoExamen.Services
{
    public class LoanService : ILoanService
    {
        private readonly FirebaseServices _firebaseService;
        private readonly IBookService _bookService;
        private readonly IAuthService _authService;

        public LoanService(
            FirebaseServices firebaseService,
            IBookService bookService,
            IAuthService authService)
        {
            _firebaseService = firebaseService;
            _bookService = bookService;
            _authService = authService;
        }

        // Colección de préstamos en Firestore
        private CollectionReference LoansCollection =>
            _firebaseService.GetCollection("prestamos");

        // ============================================
        // 1) CREAR PRÉSTAMO  (POST /api/prestamos)
        // ============================================
        public async Task<Loan> CreateLoan(string userId, LoanDto loanDto)
        {
            if (loanDto == null) throw new ArgumentNullException(nameof(loanDto));
            if (string.IsNullOrWhiteSpace(loanDto.BookId))
                throw new Exception("El ID del libro es requerido.");

            // 1. Validar usuario
            var user = await _authService.GetUserById(userId)
                       ?? throw new Exception("Usuario no encontrado.");

            if (!user.IsActive)
                throw new Exception("La cuenta del usuario está inactiva.");

            if (user.Fines > 500)
                throw new Exception("El usuario tiene multas pendientes mayores a 500 Lempiras.");

            // 2. Validar límite de préstamos activos (máx. 3)
            var activeLoans = await CountActiveLoansByUser(userId);
            if (activeLoans >= 3)
                throw new Exception("El usuario ya tiene 3 préstamos activos.");

            // 3. Validar libro y copias disponibles
            var book = await _bookService.GetBookById(loanDto.BookId)
                       ?? throw new Exception("Libro no encontrado.");

            if (book.AvailableCopies <= 0)
                throw new Exception("El libro no tiene copias disponibles.");

            // 4. Construir el préstamo
            var now = DateTime.UtcNow;
            var loan = new Loan
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                BookId = book.Id,
                LoanDate = now,
                ExpectedReturnDate = now.AddDays(14),   // 14 días después
                Status = "activo",
                RealReturnDate = null,
                DaysLate = 0,
                FineGenerated = 0,
                Renewals = 0
            };

            // 5. Guardar préstamo y decrementar copias del libro
            await LoansCollection.Document(loan.Id).SetAsync(loan);
            await _bookService.DecrementAvailableCopies(book.Id);

            return loan;
        }

        // =====================================================
        // 2) DEVOLVER PRÉSTAMO (PUT /api/prestamos/{id}/devolver)
        // =====================================================
        public async Task<Loan> ProcessReturn(string loanId)
        {
            if (string.IsNullOrWhiteSpace(loanId))
                throw new Exception("El ID del préstamo es requerido.");

            // 1. Obtener préstamo
            var loanSnap = await LoansCollection.Document(loanId).GetSnapshotAsync();
            if (!loanSnap.Exists)
                throw new Exception("Préstamo no encontrado.");

            var loan = loanSnap.ConvertTo<Loan>();

            if (loan.Status != "activo")
                throw new Exception("El préstamo ya fue devuelto o no está activo.");

            // 2. Calcular devolución y multa
            var now = DateTime.UtcNow;
            loan.RealReturnDate = now;

            if (now > loan.ExpectedReturnDate)
            {
                var daysLate = (int)Math.Ceiling((now - loan.ExpectedReturnDate).TotalDays);
                loan.DaysLate = daysLate;
                loan.FineGenerated = daysLate * 10.0; // 10 Lempiras por día de atraso

                if (loan.FineGenerated > 0)
                {
                    await _authService.UpdateUserFines(loan.UserId, loan.FineGenerated);
                }
            }

            loan.Status = "devuelto";

            // 3. Guardar cambios y devolver copia al inventario
            await LoansCollection.Document(loan.Id).SetAsync(loan);
            await _bookService.IncrementAvailableCopies(loan.BookId);

            return loan;
        }

        // =====================================================
        // 3) Contar préstamos activos de un usuario (máx. 3)
        // =====================================================
        public async Task<int> CountActiveLoansByUser(string userId)
        {
            var snapshot = await LoansCollection
                .WhereEqualTo("UserId", userId)
                .WhereEqualTo("Status", "activo")
                .GetSnapshotAsync();

            return snapshot.Count;
        }

        // =====================================================
        // 4) Saber si un libro tiene préstamos activos (para borrar)
        // =====================================================
        public async Task<bool> CheckActiveLoans(string bookId)
        {
            var snapshot = await LoansCollection
                .WhereEqualTo("BookId", bookId)
                .WhereEqualTo("Status", "activo")
                .GetSnapshotAsync();

            return snapshot.Count > 0;
        }

        // =====================================================
        // 5) Obtener préstamo por ID (GET /api/prestamos/{id})
        // =====================================================
        public async Task<Loan?> GetLoanById(string loanId)
        {
            var snap = await LoansCollection.Document(loanId).GetSnapshotAsync();
            if (!snap.Exists) return null;

            return snap.ConvertTo<Loan>();
        }

        // =====================================================
        // 6) Historial de préstamos de un usuario (Reportes)
        // =====================================================
        public async Task<List<Loan>> GetUserLoanHistory(string userId)
        {
            var snapshot = await LoansCollection
                .WhereEqualTo("UserId", userId)
                .OrderBy("LoanDate")
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(d => d.ConvertTo<Loan>())
                .ToList();
        }

        // =====================================================
        // 7) Préstamos vencidos (para reportes de morosos)
        // =====================================================
        public async Task<List<Loan>> GetOverdueLoans()
        {
            // Tomamos préstamos activos y filtramos los ya vencidos
            var snapshot = await LoansCollection
                .WhereEqualTo("Status", "activo")
                .GetSnapshotAsync();

            var now = DateTime.UtcNow;

            return snapshot.Documents
                .Select(d => d.ConvertTo<Loan>())
                .Where(l => l.ExpectedReturnDate < now)
                .ToList();
        }
    }
}
