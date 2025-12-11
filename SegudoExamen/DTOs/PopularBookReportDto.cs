

namespace SegundoExamen.DTOs 
{
    public class PopularBookReportDto
    {
        public string BookId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int TotalLoansInPeriod { get; set; } 
        public int PendingReservationsCount { get; set; } 
    }
}