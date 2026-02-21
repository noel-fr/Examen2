using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SegundoExamen.Services.Interfaces;

namespace SegundoExamen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reporteService;

        public ReportesController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        // GET: api/Reportes/usuarios-morosos
        [HttpGet("usuarios-morosos")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetUsuariosMorosos()
        {
            var usuarios = await _reporteService.GetUsersWithOverdueFines();
            return Ok(usuarios);
        }

        // GET: api/Reportes/libros-populares
        [HttpGet("libros-populares")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetLibrosPopulares()
        {
            var libros = await _reporteService.GetPopularBooks();
            return Ok(libros);
        }

        // GET: api/Reportes/mi-historial
        [HttpGet("mi-historial")]
        public async Task<IActionResult> GetMiHistorial()
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized(new { error = "Token inválido" });

            var historial = await _reporteService.GetUserLoanHistory(usuarioId);
            return Ok(historial);
        }

        // GET: api/Reportes/estadisticas
        [HttpGet("estadisticas")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetDashboard()
        {
            var stats = await _reporteService.GetDashboardStats();
            return Ok(stats);
        }
    }
}