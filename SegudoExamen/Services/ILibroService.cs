using SegundoExamen.DTOs;
using SegundoExamen.Models;

namespace SegundoExamen.Services.Interfaces
{
    public interface ILibroService
    {
        Task<Libro> CreateLibro(CreateLibroDto createLibroDto);
        Task<Libro?> GetLibroById(string libroId);
        Task<List<Libro>> GetAllLibros();
        Task<List<Libro>> GetLibrosByCategoria(string categoria);
        Task<List<Libro>> GetLibrosByAutor(string autor);
        Task<Libro?> UpdateLibro(string libroId, UpdateLibroDto updateLibroDto);
        Task<bool> DeleteLibro(string libroId);
        Task<List<Libro>> SearchLibros(string searchTerm);
    }
}