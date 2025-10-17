using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EconomicWPF.Models;

namespace EconomicWPF.Repositories.Interfaces
{
    public interface IBankTransactionRepository : IRepository<BankTransaction>
    {
        Task<IEnumerable<BankTransaction>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<BankTransaction>> GetReconciledTransactionsAsync();
        Task<IEnumerable<BankTransaction>> GetUnreconciledTransactionsAsync();
        Task<BankTransaction> GetByReferenceAsync(string reference);
    }
}