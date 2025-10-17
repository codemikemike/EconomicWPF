using System.Collections.Generic;
using System.Threading.Tasks;
using EconomicWPF.Models;

namespace EconomicWPF.Repositories.Interfaces
{
    public interface IBudgetRepository : IRepository<Budget>
    {
        Task<IEnumerable<Budget>> GetByYearAsync(int year);
        Task<IEnumerable<Budget>> GetByYearAndMonthAsync(int year, int month);
        Task<IEnumerable<Budget>> GetByAccountIdAsync(int accountId);
        Task<Budget> GetByAccountAndPeriodAsync(int accountId, int year, int month);
    }
}