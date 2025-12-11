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
    public class ReservasController : ControllerBase
    {
        private readonly IReservaService _reservaService;

        public ReservasController(IReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReserva([FromBody] CreateReservaDto createReservaDto)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized(new { error = "Token no válido" });

                var reserva = await _reservaService.CreateReserva(usuarioId, createReservaDto);
                return CreatedAtAction(nameof(GetReservaById), new { id = reserva.Id }, reserva);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetAllReservas()
        {
            try
            {
                var reservas = await _reservaService.GetAllReservas();
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("mis-reservas")]
        public async Task<IActionResult> GetMisReservas()
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized(new { error = "Token no válido" });

                var reservas = await _reservaService.GetReservasByUsuarioId(usuarioId);
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservaById(string id)
        {
            try
            {
                var reserva = await _reservaService.GetReservaById(id);
                if (reserva == null)
                    return NotFound(new { error = "Reserva no encontrada" });

                return Ok(reserva);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("libro/{libroId}")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetReservasByLibro(string libroId)
        {
            try
            {
                var reservas = await _reservaService.GetReservasByLibroId(libroId);
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReserva(string id)
        {
            try
            {
                var result = await _reservaService.CancelReserva(id);
                if (!result)
                    return NotFound(new { error = "Reserva no encontrada" });

                return Ok(new { message = "Reserva cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
