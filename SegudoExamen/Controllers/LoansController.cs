using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SegundoExamen.DTOs;
using SegundoExamen.Services;

namespace SegundoExamen.Controllers
{
    [ApiController]
    [Route("api/prestamos")]
    [Authorize] 
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] LoanDto loanDto)
        {
            if (loanDto == null)
                return BadRequest(new { error = "Los datos del préstamo son requeridos." });

            try
            {
                // Usuario autenticado (del token JWT)
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "Usuario no autenticado." });

                var loan = await _loanService.CreateLoan(userId, loanDto);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Préstamo registrado exitosamente.",
                    id = loan.Id,
                    expectedReturnDate = loan.ExpectedReturnDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/devolver")]
        public async Task<IActionResult> ReturnLoan(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "El ID del préstamo es requerido." });

            try
            {
                var loan = await _loanService.ProcessReturn(id);

                return Ok(new
                {
                    message = "Libro devuelto correctamente.",
                    loanId = loan.Id,
                    fineGenerated = loan.FineGenerated,
                    daysLate = loan.DaysLate,
                    status = loan.Status,
                    realReturnDate = loan.RealReturnDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoanById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "El ID del préstamo es requerido." });

            var loan = await _loanService.GetLoanById(id);
            if (loan == null)
                return NotFound(new { error = "Préstamo no encontrado." });

            return Ok(loan);
        }
    }
}
