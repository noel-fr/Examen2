
namespace SegundoExamen.DTOs 
{
    public class StatisticsDashboardDto
    {
        public int TotalActiveUsers { get; set; }
        public int TotalBooksInCatalog { get; set; }
        public int TotalActiveLoans { get; set; }
        public int TotalOverdueLoans { get; set; }
        public double TotalPendingFines { get; set; } 
        public int TotalPendingReservations { get; set; }
    }
}