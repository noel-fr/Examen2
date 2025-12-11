
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using SegundoExamen.DTOs;
using SegundoExamen.Models;

namespace SegundoExamen.Services
{
    public class ReportService : IReportService
    {
        private readonly FirebaseServices _firebaseService;

        public ReportService(FirebaseServices firebaseService)
        {
            _firebaseService = firebaseService;
        }

        private CollectionReference Loans => _firebaseService.GetCollection("prestamos");
        private CollectionReference Users => _firebaseService.GetCollection("usuarios");
        private CollectionReference Books => _firebaseService.GetCollection("libros");

        public async Task<List<MorosoReportDto>> GetMorosoUsers()
        {
            var loansSnapshot = await Loans
                .WhereGreaterThan("FineGenerated", 0)
                .GetSnapshotAsync();

            var loans = loansSnapshot.Documents.Select(d => d.ConvertTo<Loan>()).ToList();
            var grouped = loans
                .GroupBy(l => l.UserId)
                .Select(g => new { UserId = g.Key, TotalFine = g.Sum(x => x.FineGenerated), OverdueCount = g.Count() })
                .ToList();

            var result = new List<MorosoReportDto>();

            foreach (var g in grouped)
            {
                var userDoc = await Users.Document(g.UserId).GetSnapshotAsync();
                if (!userDoc.Exists) continue;
                var user = userDoc.ConvertTo<User>();

                result.Add(new MorosoReportDto
                {
                    UserId = user.Id,
                    FullName = $"{user.Name} {user.Lastname}",
                    Email = user.Email,
                    PendingFines = g.TotalFine,
                    OverdueLoansCount = g.OverdueCount
                });
            }

            return result;
        }

        public async Task<List<PopularBookReportDto>> GetPopularBooks(int days)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var snapshot = await Loans
                .WhereGreaterThanOrEqualTo("LoanDate", fromDate)
                .GetSnapshotAsync();

            var loans = snapshot.Documents.Select(d => d.ConvertTo<Loan>()).ToList();

            var grouped = loans
                .GroupBy(l => l.BookId)
                .Select(g => new { BookId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(20)
                .ToList();

            var result = new List<PopularBookReportDto>();

            foreach (var g in grouped)
            {
                var bookDoc = await Books.Document(g.BookId).GetSnapshotAsync();
                if (!bookDoc.Exists) continue;
                var book = bookDoc.ConvertTo<Book>();

                // Reservas pendientes para ese libro
                var reservationsSnapshot = await _firebaseService.GetCollection("reservas")
                    .WhereEqualTo("BookId", g.BookId)
                    .WhereEqualTo("Status", "pendiente")
                    .GetSnapshotAsync();

                result.Add(new PopularBookReportDto
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    TotalLoansInPeriod = g.Count,
                    PendingReservationsCount = reservationsSnapshot.Count
                });
            }

            return result;
        }

        public async Task<List<Loan>> GetUserLoanHistory(string userId)
        {
            var snapshot = await Loans
                .WhereEqualTo("UserId", userId)
                .OrderByDescending("LoanDate")
                .GetSnapshotAsync();

            return snapshot.Documents.Select(d => d.ConvertTo<Loan>()).ToList();
        }

        public async Task<List<OverdueLoanReportDto>> GetOverdueLoansDetails()
        {
            var snapshot = await Loans
                .WhereEqualTo("Status", "vencido")
                .GetSnapshotAsync();

            var loans = snapshot.Documents.Select(d => d.ConvertTo<Loan>()).ToList();
            var result = new List<OverdueLoanReportDto>();

            foreach (var loan in loans)
            {
                var bookDoc = await Books.Document(loan.BookId).GetSnapshotAsync();
                var userDoc = await Users.Document(loan.UserId).GetSnapshotAsync();

                if (!bookDoc.Exists || !userDoc.Exists) continue;

                var book = bookDoc.ConvertTo<Book>();
                var user = userDoc.ConvertTo<User>();

                result.Add(new OverdueLoanReportDto
                {
                    LoanId = loan.Id,
                    ExpectedReturnDate = loan.ExpectedReturnDate,
                    DaysLate = loan.DaysLate,
                    BookTitle = book.Title,
                    UserId = user.Id,
                    UserFullName = $"{user.Name} {user.Lastname}",
                    UserEmail = user.Email
                });
            }

            return result;
        }

        public async Task<StatisticsDashboardDto> GetGeneralStatistics()
        {
            var usersSnapshot = await Users.GetSnapshotAsync();
            var booksSnapshot = await Books.GetSnapshotAsync();
            var loansSnapshot = await Loans.GetSnapshotAsync();
            var reservationsSnapshot = await _firebaseService.GetCollection("reservas").GetSnapshotAsync();

            var loans = loansSnapshot.Documents.Select(d => d.ConvertTo<Loan>()).ToList();

            var totalActiveLoans = loans.Count(l => l.Status == "activo");
            var totalOverdueLoans = loans.Count(l => l.Status == "vencido");
            var totalPendingFines = loans.Sum(l => l.FineGenerated);
            var pendingReservations = reservationsSnapshot.Documents
                .Where(d => d.GetValue<string>("Status") == "pendiente")
                .Count();

            return new StatisticsDashboardDto
            {
                TotalActiveUsers = usersSnapshot.Count, // podrías filtrar por IsActive
                TotalBooksInCatalog = booksSnapshot.Count,
                TotalActiveLoans = totalActiveLoans,
                TotalOverdueLoans = totalOverdueLoans,
                TotalPendingFines = totalPendingFines,
                TotalPendingReservations = pendingReservations
            };
        }
    }
}
