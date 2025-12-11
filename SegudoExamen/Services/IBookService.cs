

using System.Collections.Generic;
using System.Threading.Tasks;
using SegundoExamen.DTOs;
using SegundoExamen.Models;

namespace SegundoExamen.Services
{
    public interface IBookService
    {
        Task<Book> CreateBook(BookDto bookDto);
        Task<List<Book>> GetAllBooks(string? category, string? author, bool onlyAvailable);
        Task<Book?> GetBookById(string bookId);
        Task<Book?> UpdateBook(string bookId, BookDto bookDto);
        Task<bool> DeleteBook(string bookId);

        Task DecrementAvailableCopies(string bookId);
        Task IncrementAvailableCopies(string bookId);
        Task<bool> HasActiveLoansOrReservations(string bookId);

        Task<Book?> GetBookByISBN(string isbn);
    }
}
