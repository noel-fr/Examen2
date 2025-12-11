using System.Security.Claims;
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
        public async Task<IActionResult> CreateLibro([FromBody] CreateLibroDto createLibroDto)
        {
            try
            {
                var libro = await _libroService.CreateLibro(createLibroDto);
                return CreatedAtAction(nameof(GetLibroById), new { id = libro.Id }, libro);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLibros()
        {
            try
            {
                var libros = await _libroService.GetAllLibros();
                return Ok(libros);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLibroById(string id)
        {
            try
            {
                var libro = await _libroService.GetLibroById(id);
                if (libro == null)
                    return NotFound(new { error = "Libro no encontrado" });

                return Ok(libro);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("search/{searchTerm}")]
        public async Task<IActionResult> SearchLibros(string searchTerm)
        {
            try
            {
                var libros = await _libroService.SearchLibros(searchTerm);
                return Ok(libros);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("categoria/{categoria}")]
        public async Task<IActionResult> GetLibrosByCategoria(string categoria)
        {
            try
            {
                var libros = await _libroService.GetLibrosByCategoria(categoria);
                return Ok(libros);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("autor/{autor}")]
        public async Task<IActionResult> GetLibrosByAutor(string autor)
        {
            try
            {
                var libros = await _libroService.GetLibrosByAutor(autor);
                return Ok(libros);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> UpdateLibro(string id, [FromBody] UpdateLibroDto updateLibroDto)
        {
            try
            {
                var libro = await _libroService.UpdateLibro(id, updateLibroDto);
                if (libro == null)
                    return NotFound(new { error = "Libro no encontrado" });

                return Ok(libro);
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
                if (!result)
                    return NotFound(new { error = "Libro no encontrado" });

                return Ok(new { message = "Libro eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
