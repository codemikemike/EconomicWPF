using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EconomicWPF.Models;

namespace EconomicWPF.Repositories.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId);
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Transaction>> GetByYearAsync(int year);
        Task<IEnumerable<Transaction>> GetPostedTransactionsAsync();
        Task<IEnumerable<Transaction>> GetUnpostedTransactionsAsync();
        Task<Transaction> GetByVoucherNumberAsync(string voucherNumber);
    }
}