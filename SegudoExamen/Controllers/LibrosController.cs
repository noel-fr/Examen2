using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SegundoExamen.DTOs;
using SegundoExamen.Services.Interfaces;

namespace SegundoExamen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LibrosController : ControllerBase
    {
        private readonly ILibroService _libroService;

        public LibrosController(ILibroService libroService)
        {
            _libroService = libroService;
        }

        [HttpPost]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> CreateLibro([FromBody] CreateLibroDto dto)
        {
            try
            {
                var libro = await _libroService.CreateLibro(dto);
                return CreatedAtAction(nameof(GetLibroById), new { id = libro.Id }, libro);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLibros()
        {
            var libros = await _libroService.GetAllLibros();
            return Ok(libros);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLibroById(string id)
        {
            var libro = await _libroService.GetLibroById(id);
            if (libro == null) return NotFound(new { error = "Libro no encontrado" });
            return Ok(libro);
        }

        [HttpGet("categoria/{categoria}")]
        public async Task<IActionResult> GetByCategoria(string categoria)
        {
            var libros = await _libroService.GetLibrosByCategoria(categoria);
            return Ok(libros);
        }

        [HttpGet("autor/{autor}")]
        public async Task<IActionResult> GetByAutor(string autor)
        {
            var libros = await _libroService.GetLibrosByAutor(autor);
            return Ok(libros);
        }

        [HttpGet("search/{term}")]
        public async Task<IActionResult> Search(string term)
        {
            var libros = await _libroService.SearchLibros(term);
            return Ok(libros);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> UpdateLibro(string id, [FromBody] UpdateLibroDto dto)
        {
            try
            {
                var updated = await _libroService.UpdateLibro(id, dto);
                if (updated == null) return NotFound(new { error = "Libro no encontrado" });
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> DeleteLibro(string id)
        {
            try
            {
                var result = await _libroService.DeleteLibro(id);
                if (!result) return NotFound(new { error = "Libro no encontrado" });
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
