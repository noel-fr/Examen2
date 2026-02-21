using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SegudoExamen.Services;

namespace SegudoExamen.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly DataService _dataService;

    public AdminController(DataService dataService)
    {
        _dataService = dataService;
    }

    [HttpPut("usuarios/{id}/cambiar-rol")]
    public async Task<IActionResult> CambiarRol(string id, [FromBody] CambiarRolRequest request)
    {
        try
        {
            var usuario = await _dataService.GetUsuarioById(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            // Validar rol
            var rolesValidos = new[] { "usuario", "bibliotecario", "admin" };
            if (!rolesValidos.Contains(request.Rol))
                return BadRequest(new { mensaje = "Rol inválido. Valores: usuario, bibliotecario, admin" });

            // No permitir cambiar rol del propio admin actual
            var currentUserId = User.FindFirst("userId")?.Value;
            if (id == currentUserId)
                return BadRequest(new { mensaje = "No puede cambiar su propio rol" });

            usuario.Rol = request.Rol;
            await _dataService.UpdateUsuario(usuario);

            return Ok(new
            {
                mensaje = "Rol actualizado exitosamente",
                usuarioId = id,
                nuevoRol = request.Rol
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    [HttpGet("estadisticas")]
    public async Task<IActionResult> GetEstadisticas()
    {
        try
        {
            var usuarios = await _dataService.GetUsuarios();
            var libros = await _dataService.GetLibros();
            var prestamos = await _dataService.GetPrestamos();

            return Ok(new
            {
                usuariosActivos = usuarios.Count(u => u.Activo),
                totalUsuarios = usuarios.Count,
                totalLibros = libros.Count,
                prestamosActivos = prestamos.Count(p => p.Estado == "activo"),
                prestamosVencidos = prestamos.Count(p => p.Estado == "activo" &&
                    p.FechaDevolucionEsperada.ToDateTime() < DateTime.UtcNow),
                multasPendientes = usuarios.Sum(u => u.Multas),
                reservasPendientes = 3, // Simulado
                actualizado = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }
}

public class CambiarRolRequest { public string Rol { get; set; } }