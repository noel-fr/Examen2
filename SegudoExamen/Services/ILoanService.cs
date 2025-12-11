
using System.Collections.Generic;
using System.Threading.Tasks;
using SegundoExamen.DTOs;
using SegundoExamen.Models;

namespace SegundoExamen.Services
{
    public interface ILoanService
    {
        // 1. Crear préstamo (Escenario 3)
        Task<Loan> CreateLoan(string userId, LoanDto loanDto);
        
        // 2. Procesar devolución (Escenario 3)
        Task<Loan> ProcessReturn(string loanId);
        
        // 3. Auxiliar: contar préstamos activos por usuario (límite 3)
        Task<int> CountActiveLoansByUser(string userId);
        
        // 4. Auxiliar: validar si un libro tiene préstamos activos
        Task<bool> CheckActiveLoans(string bookId); 
        
        // 5. Lectura
        Task<Loan?> GetLoanById(string loanId);
        Task<List<Loan>> GetUserLoanHistory(string userId);
        Task<List<Loan>> GetOverdueLoans(); // préstamos vencidos
    }
}