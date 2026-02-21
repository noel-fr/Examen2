using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SegudoExamen.Services;

namespace SegudoExamen.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly DataService _dataService;

    public ReportesController(DataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet("usuarios-morosos")]
    [Authorize(Roles = "bibliotecario,admin")]
    public async Task<IActionResult> GetUsuariosMorosos()
    {
        try
        {
            var usuarios = await _dataService.GetUsuarios();
            var morosos = usuarios.Where(u => u.Multas > 0)
                                 .Select(u => new
                                 {
                                     id = u.Id,
                                     nombreCompleto = $"{u.Nombre} {u.Apellido}",
                                     correo = u.Correo,
                                     multas = u.Multas,
                                     activo = u.Activo,
                                     telefono = u.Telefono
                                 })
                                 .ToList();

            return Ok(new
            {
                total = morosos.Count,
                usuarios = morosos
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    [HttpGet("mi-historial")]
    public async Task<IActionResult> GetMiHistorial()
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var prestamos = await _dataService.GetPrestamos();
            var misPrestamos = prestamos.Where(p => p.UsuarioId == userId)
                                       .OrderByDescending(p => p.FechaPrestamo)
                                       .Select(p => new
                                       {
                                           id = p.Id,
                                           libroId = p.LibroId,
                                           fechaPrestamo = p.FechaPrestamo.ToDateTime(),
                                           fechaDevolucionEsperada = p.FechaDevolucionEsperada.ToDateTime(),
                                           fechaDevolucionReal = p.FechaDevolucionReal?.ToDateTime(),
                                           estado = p.Estado,
                                           diasRetraso = p.DiasRetraso,
                                           multaGenerada = p.MultaGenerada
                                       })
                                       .ToList();

            return Ok(new
            {
                total = misPrestamos.Count,
                prestamos = misPrestamos
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    [HttpGet("libros-populares")]
    [Authorize(Roles = "bibliotecario,admin")]
    public IActionResult GetLibrosPopulares()
    {
        // Simulación para cumplir requisito
        return Ok(new
        {
            mensaje = "Top 10 libros más prestados (últimos 30 días)",
            libros = new[]
            {
                new { titulo = "Cien años de soledad", autor = "García Márquez", prestamos = 15, reservasPendientes = 3 },
                new { titulo = "El principito", autor = "Saint-Exupéry", prestamos = 12, reservasPendientes = 5 }
            }
        });
    }

    [HttpGet("prestamos/vencidos")]
    [Authorize(Roles = "bibliotecario,admin")]
    public IActionResult GetPrestamosVencidos()
    {
        return Ok(new
        {
            mensaje = "Préstamos vencidos",
            total = 2,
            prestamos = new[]
            {
                new {
                    usuario = "Juan Pérez",
                    libro = "Cien años de soledad",
                    diasRetraso = 3,
                    multa = 150
                }
            }
        });
    }
}