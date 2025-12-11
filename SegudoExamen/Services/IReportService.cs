
using SegundoExamen.Models;
using SegundoExamen.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SegundoExamen.Services
{
    public interface IReportService
    {
        Task<List<MorosoReportDto>> GetMorosoUsers();
        Task<List<PopularBookReportDto>> GetPopularBooks(int days);
        Task<List<Loan>> GetUserLoanHistory(string userId);
        Task<List<OverdueLoanReportDto>> GetOverdueLoansDetails();
        Task<StatisticsDashboardDto> GetGeneralStatistics();
    }
}