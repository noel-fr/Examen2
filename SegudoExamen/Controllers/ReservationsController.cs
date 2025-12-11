
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SegundoExamen.DTOs;
using SegundoExamen.Models;
using SegundoExamen.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SegundoExamen.Controllers
{
    [ApiController]
    [Route("api/reservas")]
    [Authorize] 
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationDto reservationDto)
        {
            if (reservationDto == null)
                return BadRequest(new { error = "Los datos de la reserva son requeridos." });

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Usuario no autenticado." });

                var reservationRecord = await _reservationService.CreateReservation(userId, reservationDto);

                return CreatedAtAction(nameof(GetUserReservations), null, new
                {
                    message = "Reserva creada con éxito.",
                    reservationRecord.Id,
                    reservationRecord.Priority
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("mis-reservas")]
        public async Task<IActionResult> GetUserReservations()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Usuario no autenticado." });

                var reservations = await _reservationService.GetUserReservations(userId);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReservation(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "El ID de la reserva es requerido." });

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Usuario no autenticado." });

                var cancelled = await _reservationService.CancelReservation(id, userId);

                if (!cancelled)
                    return NotFound(new { error = "Reserva no encontrada." });

                return NoContent(); // 204
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
