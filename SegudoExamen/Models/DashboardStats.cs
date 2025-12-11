namespace SegundoExamen.Models
{
    public class DashboardStats
    {
        public int TotalUsuariosActivos { get; set; }
        public int TotalLibrosCatalogo { get; set; }
        public int TotalPrestamosActivos { get; set; }
        public int TotalPrestamosVencidos { get; set; }
        public decimal TotalMultasPendientes { get; set; }
        public int TotalReservasPendientes { get; set; }
    }
}