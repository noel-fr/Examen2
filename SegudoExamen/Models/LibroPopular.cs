namespace SegundoExamen.Models
{
    public class LibroPopular 
    {
        public string LibroId { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public int Prestamos30Dias { get; set; }
        public int ReservasPendientes { get; set; }
    }
}