using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SegudoExamen.Models;
using SegudoExamen.Services;

namespace SegudoExamen.Controllers;

[ApiController]
[Route("api/prestamos")]
[Authorize]
public class PrestamosController : ControllerBase
{
    private readonly DataService _dataService;

    public PrestamosController(DataService dataService)
    {
        _dataService = dataService;
    }

    [HttpPost]
    public async Task<IActionResult> CrearPrestamo([FromBody] CrearPrestamoRequest request)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { mensaje = "Usuario no autenticado" });

            // 1. Verificar libro
            var libro = await _dataService.GetLibroById(request.LibroId);
            if (libro == null)
                return BadRequest(new { mensaje = "Libro no encontrado" });

            if (libro.CopiasDisponibles <= 0)
                return BadRequest(new { mensaje = "No hay copias disponibles" });

            // 2. Verificar usuario
            var usuario = await _dataService.GetUsuarioById(userId);
            if (usuario == null)
                return BadRequest(new { mensaje = "Usuario no encontrado" });

            if (usuario.Multas > 500)
                return BadRequest(new { mensaje = "Multas pendientes > 500 Lempiras" });

            // 3. Verificar préstamos activos (máx 3)
            var prestamosActivos = await _dataService.CountPrestamosActivosUsuario(userId);
            if (prestamosActivos >= 3)
                return BadRequest(new { mensaje = "Máximo 3 préstamos activos permitidos" });

            // 4. Crear préstamo
            var prestamo = new Prestamo
            {
                UsuarioId = userId,
                LibroId = request.LibroId,
                FechaPrestamo = Google.Cloud.Firestore.Timestamp.FromDateTime(DateTime.UtcNow),
                FechaDevolucionEsperada = Google.Cloud.Firestore.Timestamp.FromDateTime(DateTime.UtcNow.AddDays(14)),
                Estado = "activo",
                Renovaciones = 0
            };

            // 5. Actualizar libro
            libro.CopiasDisponibles--;
            if (libro.CopiasDisponibles == 0)
                libro.Estado = "agotado";

            await _dataService.AddPrestamo(prestamo);
            await _dataService.UpdateLibro(libro);

            return Ok(new
            {
                mensaje = "Préstamo creado exitosamente",
                prestamoId = prestamo.Id,
                fechaDevolucion = prestamo.FechaDevolucionEsperada.ToDateTime().ToString("yyyy-MM-dd")
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    [HttpPut("{id}/devolver")]
    public async Task<IActionResult> DevolverPrestamo(string id)
    {
        try
        {
            var prestamo = await _dataService.GetPrestamoById(id);
            if (prestamo == null)
                return NotFound(new { mensaje = "Préstamo no encontrado" });

            if (prestamo.Estado != "activo")
                return BadRequest(new { mensaje = "Préstamo ya devuelto o no activo" });

            // 1. Actualizar préstamo
            prestamo.FechaDevolucionReal = Google.Cloud.Firestore.Timestamp.FromDateTime(DateTime.UtcNow);
            prestamo.Estado = "devuelto";

            // 2. Calcular días de retraso y multa
            var fechaEsperada = prestamo.FechaDevolucionEsperada.ToDateTime();
            var fechaReal = prestamo.FechaDevolucionReal.Value.ToDateTime();

            if (fechaReal > fechaEsperada)
            {
                prestamo.DiasRetraso = (int)(fechaReal - fechaEsperada).TotalDays;
                prestamo.MultaGenerada = prestamo.DiasRetraso * 50; // 50 Lempiras por día

                // Sumar multa al usuario
                var usuario = await _dataService.GetUsuarioById(prestamo.UsuarioId);
                if (usuario != null)
                {
                    usuario.Multas += prestamo.MultaGenerada;
                    await _dataService.UpdateUsuario(usuario);
                }
            }

            // 3. Actualizar libro
            var libro = await _dataService.GetLibroById(prestamo.LibroId);
            if (libro != null)
            {
                libro.CopiasDisponibles++;
                libro.Estado = libro.CopiasDisponibles > 0 ? "disponible" : "agotado";
                await _dataService.UpdateLibro(libro);
            }

            await _dataService.UpdatePrestamo(prestamo);

            return Ok(new
            {
                mensaje = "Préstamo devuelto exitosamente",
                diasRetraso = prestamo.DiasRetraso,
                multaGenerada = prestamo.MultaGenerada,
                multaTotalUsuario = prestamo.MultaGenerada > 0 ? $"Multa agregada: {prestamo.MultaGenerada} Lempiras" : "Sin multa"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }
}

public class CrearPrestamoRequest
{
    public string LibroId { get; set; }
}