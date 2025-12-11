

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SegundoExamen.Services;
using System;
using System.Threading.Tasks;

namespace SegundoExamen.Controllers
{
    [ApiController]
    [Route("api/reportes")]
    [Authorize] 
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: api/reportes/usuarios-morosos (Escenario 5)
        [HttpGet("usuarios-morosos")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetMorosoUsers()
        {
            try
            {
                var morosos = await _reportService.GetMorosoUsers();
                return Ok(morosos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        // GET: api/reportes/libros-populares (Escenario 5)
        [HttpGet("libros-populares")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetPopularBooks([FromQuery] int days = 30)
        {
            try
            {
                var books = await _reportService.GetPopularBooks(days);
                return Ok(books);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/reportes/mi-historial (Escenario 5)
        [HttpGet("mi-historial")]
        public async Task<IActionResult> GetMyLoanHistory()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Token inválido" });
                    
                var history = await _reportService.GetUserLoanHistory(userId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}