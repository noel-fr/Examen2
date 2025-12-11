
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SegundoExamen.DTOs;
using SegundoExamen.Services;

namespace SegundoExamen.Controllers
{
    [ApiController]
    [Route("api/libros")]                   
    [Authorize]                              
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // POST: api/libros (Solo Bibliotecario y Admin)
        [HttpPost]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> CreateBook([FromBody] BookDto bookDto)
        {
            try
            {
                // Validación adicional de copias 
                if (bookDto.AvailableCopies > bookDto.TotalCopies)
                {
                    return BadRequest(new { error = "Las copias disponibles no pueden exceder las copias totales." });
                }

                var book = await _bookService.CreateBook(bookDto);
                return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/libros
        // Accesible para todos los usuarios autenticados
        [HttpGet]
        public async Task<IActionResult> GetAllBooks(
            [FromQuery] string? category,
            [FromQuery] string? author,
            [FromQuery] bool onlyAvailable = false)
        {
            try
            {
                var books = await _bookService.GetAllBooks(category, author, onlyAvailable);
                return Ok(books);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/libros/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(string id)
        {
            try
            {
                var book = await _bookService.GetBookById(id);
                if (book == null)
                    return NotFound(new { error = "Libro no encontrado" });

                return Ok(book);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: api/libros/{id} (Solo Bibliotecario y Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> UpdateBook(string id, [FromBody] BookDto bookDto)
        {
            try
            {
                var updatedBook = await _bookService.UpdateBook(id, bookDto);

                if (updatedBook == null)
                    return NotFound(new { error = "Libro no encontrado" });

                return Ok(updatedBook);
            }
            catch (Exception ex)
            {
                // Maneja el error de "No se puede reducir el total de copias"
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/libros/{id} (Solo Bibliotecario y Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> DeleteBook(string id)
        {
            try
            {
                var deleted = await _bookService.DeleteBook(id);

                if (!deleted)
                    return NotFound(new { error = "Libro no encontrado" });

                // 204 No Content para eliminación exitosa
                return NoContent();
            }
            catch (Exception ex)
            {
                // Maneja el error de "tiene préstamos activos o reservas pendientes"
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
