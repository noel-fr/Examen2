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
    public class PrestamosController : ControllerBase
    {
        private readonly IPrestamoService _prestamoService;

        public PrestamosController(IPrestamoService prestamoService)
        {
            _prestamoService = prestamoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePrestamo([FromBody] CreatePrestamoDto createPrestamoDto)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized(new { error = "Token no válido" });

                var prestamo = await _prestamoService.CreatePrestamo(usuarioId, createPrestamoDto);
                return CreatedAtAction(nameof(GetPrestamoById), new { id = prestamo.Id }, prestamo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetAllPrestamos()
        {
            try
            {
                var prestamos = await _prestamoService.GetAllPrestamos();
                return Ok(prestamos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("mis-prestamos")]
        public async Task<IActionResult> GetMisPrestamos()
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized(new { error = "Token no válido" });

                var prestamos = await _prestamoService.GetPrestamosByUsuarioId(usuarioId);
                return Ok(prestamos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPrestamoById(string id)
        {
            try
            {
                var prestamo = await _prestamoService.GetPrestamoById(id);
                if (prestamo == null)
                    return NotFound(new { error = "Préstamo no encontrado" });

                return Ok(prestamo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("vencidos")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetVencidosPrestamos()
        {
            try
            {
                var prestamos = await _prestamoService.GetVencidosPrestamos();
                return Ok(prestamos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/devolver")]
        [Authorize(Roles = "usuario,bibliotecario,admin")]
        public async Task<IActionResult> DevolverPrestamo(string id)
        {
            try
            {
                var prestamo = await _prestamoService.DevolverPrestamo(id);
                if (prestamo == null)
                    return NotFound(new { error = "Préstamo no encontrado" });

                return Ok(prestamo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/renovar")]
        public async Task<IActionResult> RenovarPrestamo(string id)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized(new { error = "Token no válido" });

                var prestamo = await _prestamoService.RenovarPrestamo(id);
                if (prestamo == null)
                    return NotFound(new { error = "Préstamo no encontrado" });

                return Ok(prestamo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
