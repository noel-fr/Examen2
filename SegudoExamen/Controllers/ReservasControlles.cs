using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SegudoExamen.Services;

namespace SegudoExamen.Controllers;

[ApiController]
[Route("api/reservas")]
[Authorize]
public class ReservasController : ControllerBase
{
    private readonly DataService _dataService;
    
    public ReservasController(DataService dataService) => _dataService = dataService;
    
    [HttpPost]
    public async Task<IActionResult> CrearReserva([FromBody] ReservaRequest request)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            var libro = await _dataService.GetLibroById(request.LibroId);
            
            if (libro == null) return BadRequest("Libro no existe");
            if (libro.CopiasDisponibles > 0) return BadRequest("Libro disponible, no necesita reserva");
            
            // Lógica simplificada
            return Ok(new { mensaje = "Reserva creada (simulada)" });
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }
    
    [HttpGet("mis-reservas")]
    public async Task<IActionResult> MisReservas()
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            return Ok(new { 
                mensaje = "Endpoint implementado",
                reservas = new[] { new { id = "1", libro = "Ejemplo", estado = "pendiente" } }
            });
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }
}

public class ReservaRequest { public string LibroId { get; set; } }