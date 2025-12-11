using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SegundoExamen.DTOs;
using SegundoExamen.Services.Interfaces;

namespace SegundoExamen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IPrestamoService _prestamoService;

        public UsuariosController(IAuthService authService, IPrestamoService prestamoService)
        {
            _authService = authService;
            _prestamoService = prestamoService;
        }

        [HttpPut("{id}/cambiar-rol")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CambiarRol(string id, [FromBody] string nuevoRol)
        {
            try
            {
                var adminId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                if (string.IsNullOrEmpty(adminId))
                    return Unauthorized(new { error = "Token inválido" });

                if (adminId == id)
                    return BadRequest(new { error = "No puedes cambiar tu propio rol." });

                var usuario = await _authService.GetUsuarioById(id);
                if (usuario == null)
                    return NotFound(new { error = "Usuario no encontrado" });

                if (nuevoRol != "usuario" && nuevoRol != "bibliotecario" && nuevoRol != "admin")
                    return BadRequest(new { error = "Rol inválido." });

                usuario.Rol = nuevoRol;
                await _authService.UpdateUsuarioAsync(usuario);

                return Ok(new { message = "Rol actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/gestionar-multa")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GestionarMulta(string id, [FromBody] ManageFineDto dto)
        {
            try
            {
                var usuario = await _authService.GetUsuarioById(id);
                if (usuario == null)
                    return NotFound(new { error = "Usuario no encontrado" });

                var nuevaMulta = usuario.Multas + dto.Monto;
                if (nuevaMulta < 0)
                    return BadRequest(new { error = "La multa no puede quedar negativa." });

                usuario.Multas = nuevaMulta;
                await _authService.UpdateUsuarioAsync(usuario);

                return Ok(new
                {
                    message = "Multa actualizada correctamente.",
                    multas = usuario.Multas
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/toggle-estado")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ToggleEstado(string id)
        {
            try
            {
                var usuario = await _authService.GetUsuarioById(id);
                if (usuario == null)
                    return NotFound(new { error = "Usuario no encontrado" });

                if (usuario.Activo)
                {
                    // ⬇️ Aquí usamos el método que ahora SÍ está en IPrestamoService
                    var prestamosActivos = await _prestamoService.GetPrestamosActivosByUsuarioId(id);
                    if (prestamosActivos.Any())
                    {
                        return BadRequest(new
                        {
                            error = "No se puede desactivar un usuario con préstamos activos."
                        });
                    }

                    usuario.Activo = false;
                }
                else
                {
                    usuario.Activo = true;
                }

                await _authService.UpdateUsuarioAsync(usuario);

                return Ok(new
                {
                    message = "Estado del usuario actualizado correctamente.",
                    activo = usuario.Activo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
