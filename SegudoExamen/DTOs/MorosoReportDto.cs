

namespace SegundoExamen.DTOs
{
    public class MorosoReportDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public double PendingFines { get; set; }
        public int OverdueLoansCount { get; set; } 
    }
}
