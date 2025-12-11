

using System;

namespace SegundoExamen.DTOs 
{
    public class OverdueLoanReportDto
    {
        // Información del Préstamo
        public string LoanId { get; set; } = string.Empty;
        public DateTime ExpectedReturnDate { get; set; }
        public int DaysLate { get; set; }
        
        // Información del Libro
        public string BookTitle { get; set; } = string.Empty;
        
        // Información del Usuario
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }
}