using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SegudoExamen.Models;
using SegudoExamen.Services;

namespace SegudoExamen.Controllers;

[ApiController]
[Route("api/libros")]
[Authorize]
public class LibrosController : ControllerBase
{
    private readonly DataService _dataService;

    public LibrosController(DataService dataService)
    {
        _dataService = dataService;
    }

    [HttpPost]
    [Authorize(Roles = "bibliotecario,admin")]
    public async Task<IActionResult> CrearLibro([FromBody] LibroRequest request)
    {
        try
        {
            // Validar ISBN único (simplificado)
            var libros = await _dataService.GetLibros();
            if (libros.Any(l => l.ISBN == request.ISBN))
                return BadRequest(new { mensaje = "ISBN ya existe" });

            if (request.CopiasDisponibles > request.CopiasTotal)
                return BadRequest(new { mensaje = "Copias disponibles no pueden ser > total" });

            var libro = new Libro
            {
                Titulo = request.Titulo,
                Autor = request.Autor,
                ISBN = request.ISBN,
                Categoria = request.Categoria,
                Editorial = request.Editorial,
                AnioPublicacion = request.AnioPublicacion,
                CopiasDisponibles = request.CopiasDisponibles,
                CopiasTotal = request.CopiasTotal,
                Ubicacion = request.Ubicacion,
                Descripcion = request.Descripcion,
                Estado = request.CopiasDisponibles > 0 ? "disponible" : "agotado",
                FechaIngreso = Google.Cloud.Firestore.Timestamp.FromDateTime(DateTime.UtcNow)
            };

            await _dataService.AddLibro(libro);

            return Ok(new
            {
                mensaje = "Libro creado",
                id = libro.Id,
                titulo = libro.Titulo
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetLibros(
        [FromQuery] string categoria = null,
        [FromQuery] string autor = null,
        [FromQuery] bool? disponible = null)
    {
        try
        {
            var libros = await _dataService.GetLibros();

            // Filtros
            if (!string.IsNullOrEmpty(categoria))
                libros = libros.Where(l => l.Categoria?.Contains(categoria, StringComparison.OrdinalIgnoreCase) == true).ToList();

            if (!string.IsNullOrEmpty(autor))
                libros = libros.Where(l => l.Autor?.Contains(autor, StringComparison.OrdinalIgnoreCase) == true).ToList();

            if (disponible.HasValue)
                libros = disponible.Value
                    ? libros.Where(l => l.CopiasDisponibles > 0).ToList()
                    : libros.Where(l => l.CopiasDisponibles == 0).ToList();

            return Ok(libros);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLibro(string id)
    {
        try
        {
            var libro = await _dataService.GetLibroById(id);
            return libro == null
                ? NotFound(new { mensaje = "Libro no encontrado" })
                : Ok(libro);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "bibliotecario,admin")]
    public async Task<IActionResult> ActualizarLibro(string id, [FromBody] ActualizarLibroRequest request)
    {
        try
        {
            var libro = await _dataService.GetLibroById(id);
            if (libro == null)
                return NotFound(new { mensaje = "Libro no encontrado" });

            // Validación de copias
            if (request.CopiasTotal.HasValue && request.CopiasTotal < libro.CopiasTotal)
            {
                int prestadas = libro.CopiasTotal - libro.CopiasDisponibles;
                if (request.CopiasTotal < prestadas)
                    return BadRequest(new { mensaje = "No se pueden eliminar copias prestadas" });
            }

            // Actualizar
            libro.Titulo = request.Titulo ?? libro.Titulo;
            libro.Autor = request.Autor ?? libro.Autor;
            libro.Categoria = request.Categoria ?? libro.Categoria;
            libro.Editorial = request.Editorial ?? libro.Editorial;
            libro.AnioPublicacion = request.AnioPublicacion ?? libro.AnioPublicacion;
            libro.CopiasTotal = request.CopiasTotal ?? libro.CopiasTotal;
            libro.CopiasDisponibles = request.CopiasDisponibles ?? libro.CopiasDisponibles;
            libro.Ubicacion = request.Ubicacion ?? libro.Ubicacion;
            libro.Descripcion = request.Descripcion ?? libro.Descripcion;
            libro.Estado = libro.CopiasDisponibles > 0 ? "disponible" : "agotado";

            await _dataService.UpdateLibro(libro);

            return Ok(new { mensaje = "Libro actualizado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "bibliotecario,admin")]
    public async Task<IActionResult> EliminarLibro(string id)
    {
        try
        {
            var libro = await _dataService.GetLibroById(id);
            if (libro == null)
                return NotFound(new { mensaje = "Libro no encontrado" });

            // Validaciones (simplificadas)
            if (libro.CopiasDisponibles < libro.CopiasTotal)
                return BadRequest(new { mensaje = "No se puede eliminar libro con préstamos activos" });

            await _dataService.DeleteLibro(id);

            return Ok(new { mensaje = "Libro eliminado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }
}

public class LibroRequest
{
    public string Titulo { get; set; }
    public string Autor { get; set; }
    public string ISBN { get; set; }
    public string Categoria { get; set; }
    public string Editorial { get; set; }
    public int AnioPublicacion { get; set; }
    public int CopiasDisponibles { get; set; }
    public int CopiasTotal { get; set; }
    public string Ubicacion { get; set; }
    public string Descripcion { get; set; }
}

public class ActualizarLibroRequest
{
    public string? Titulo { get; set; }
    public string? Autor { get; set; }
    public string? Categoria { get; set; }
    public string? Editorial { get; set; }
    public int? AnioPublicacion { get; set; }
    public int? CopiasDisponibles { get; set; }
    public int? CopiasTotal { get; set; }
    public string? Ubicacion { get; set; }
    public string? Descripcion { get; set; }
}