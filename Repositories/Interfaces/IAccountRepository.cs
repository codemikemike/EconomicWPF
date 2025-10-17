using System.Collections.Generic;
using System.Threading.Tasks;
using EconomicWPF.Models;
using EconomicWPF.Enums;

namespace EconomicWPF.Repositories.Interfaces
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<Account> GetByAccountNumberAsync(string accountNumber);
        Task<IEnumerable<Account>> GetByTypeAsync(AccountType accountType); // ✅ Ændret fra string til enum
        Task<IEnumerable<Account>> GetAccountHierarchyAsync();
        Task<bool> UpdateBalanceAsync(int accountId, decimal amount);
    }
}