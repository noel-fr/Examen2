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
        public async Task<IActionResult> CreatePrestamo([FromBody] CreatePrestamoDto dto)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized(new { error = "Token inválido" });

                // ⬇️ AHORA SOLO PASAMOS 2 PARÁMETROS
                var prestamo = await _prestamoService.CreatePrestamo(usuarioId, dto);
                return Ok(prestamo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetAll()
        {
            var prestamos = await _prestamoService.GetAllPrestamos();
            return Ok(prestamos);
        }

        [HttpGet("mis-prestamos")]
        public async Task<IActionResult> MisPrestamos()
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized(new { error = "Token inválido" });

            var prestamos = await _prestamoService.GetPrestamosByUsuarioId(usuarioId);
            return Ok(prestamos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var prestamo = await _prestamoService.GetPrestamoById(id);
            if (prestamo == null) return NotFound(new { error = "Préstamo no encontrado" });
            return Ok(prestamo);
        }

        [HttpGet("vencidos")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetVencidos()
        {
            var vencidos = await _prestamoService.GetVencidosPrestamos();
            return Ok(vencidos);
        }

        [HttpPut("{id}/devolver")]
        [Authorize(Roles = "usuario,bibliotecario,admin")]
        public async Task<IActionResult> Devolver(string id)
        {
            try
            {
                var prestamo = await _prestamoService.DevolverPrestamo(id);
                if (prestamo == null) return NotFound(new { error = "Préstamo no encontrado" });
                return Ok(prestamo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/renovar")]
        public async Task<IActionResult> Renovar(string id)
        {
            try
            {
                var prestamo = await _prestamoService.RenovarPrestamo(id);
                return Ok(prestamo);
            }
            catch (NotImplementedException)
            {
                return BadRequest(new { error = "La renovación no está implementada." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
