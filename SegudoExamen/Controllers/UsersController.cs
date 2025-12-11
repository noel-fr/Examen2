

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SegundoExamen.DTOs;
using SegundoExamen.Services;

namespace SegundoExamen.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize] 
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILoanService _loanService;
        private readonly IReportService _reportService;

        public UsersController(
            IAuthService authService,
            ILoanService loanService,
            IReportService reportService)
        {
            _authService = authService;
            _loanService = loanService;
            _reportService = reportService;
        }

        // ============================================================
        // PUT: api/usuarios/{id}/cambiar-rol  (Admin)
        // ============================================================
        [HttpPut("{id}/cambiar-rol")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangeRole(string id, [FromBody] RoleUpdateDto roleDto)
        {
            try
            {
                var updatedUser = await _authService.UpdateUserRole(id, roleDto.NewRole);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ============================================================
        // PUT: api/usuarios/{id}/gestionar-multa (Admin/Bibliotecario)
        // ============================================================
        [HttpPut("{id}/gestionar-multa")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> ManageFines(string id, [FromBody] FineManagementDto fineDto)
        {
            try
            {
                var updatedUser = await _authService.UpdateUserFines(id, fineDto.AmountChange);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ============================================================
        // PUT: api/usuarios/{id}/toggle-estado (Admin)
        // ============================================================
        [HttpPut("{id}/toggle-estado")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ToggleAccountStatus(string id, [FromBody] StatusUpdateDto statusDto)
        {
            try
            {
                var updatedUser = await _authService.ToggleUserStatus(id, statusDto.IsActive);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ============================================================
        // GET: api/usuarios/mi-historial-prestamos
        // ============================================================
        [HttpGet("mi-historial-prestamos")]
        public async Task<IActionResult> GetMyLoanHistory()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { error = "Token inválido o expirado." });

            var loans = await _reportService.GetUserLoanHistory(userId);
            return Ok(loans);
        }

        // ============================================================
        // GET: api/usuarios/dashboard (Admin/Bibliotecario)
        // ============================================================
        [HttpGet("dashboard")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            var stats = await _reportService.GetGeneralStatistics();
            return Ok(stats);
        }
    }
}
