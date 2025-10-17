using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EconomicWPF.Database;
using EconomicWPF.Models;
using EconomicWPF.Repositories.Interfaces;

namespace EconomicWPF.Repositories.Implementation
{
    /// <summary>
    /// Repository implementation for BankTransaction using Entity Framework
    /// </summary>
    public class BankTransactionRepository : IBankTransactionRepository
    {
        private readonly EconomicDbContext _context;

        public BankTransactionRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<BankTransaction>> GetAllAsync()
        {
            return await _context.BankTransactions
                .OrderByDescending(b => b.TransactionDate)
                .ToListAsync();
        }

        public async Task<BankTransaction> GetByIdAsync(int id)
        {
            return await _context.BankTransactions
                .FirstOrDefaultAsync(b => b.BankTransactionId == id);
        }

        public async Task<int> AddAsync(BankTransaction bankTransaction)
        {
            bankTransaction.CreatedDate = DateTime.Now;

            _context.BankTransactions.Add(bankTransaction);
            await _context.SaveChangesAsync();

            return bankTransaction.BankTransactionId;
        }

        public async Task<bool> UpdateAsync(BankTransaction bankTransaction)
        {
            _context.BankTransactions.Update(bankTransaction);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var bankTransaction = await _context.BankTransactions.FindAsync(id);
            if (bankTransaction == null)
                return false;

            _context.BankTransactions.Remove(bankTransaction);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.BankTransactions.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.BankTransactions.AnyAsync(b => b.BankTransactionId == id);
        }

        public async Task<IEnumerable<BankTransaction>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.BankTransactions
                .Where(b => b.TransactionDate >= fromDate && b.TransactionDate <= toDate)
                .OrderByDescending(b => b.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BankTransaction>> GetReconciledTransactionsAsync()
        {
            return await _context.BankTransactions
                .Where(b => b.IsReconciled)
                .OrderByDescending(b => b.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BankTransaction>> GetUnreconciledTransactionsAsync()
        {
            return await _context.BankTransactions
                .Where(b => !b.IsReconciled)
                .OrderByDescending(b => b.TransactionDate)
                .ToListAsync();
        }

        public async Task<BankTransaction> GetByReferenceAsync(string reference)
        {
            return await _context.BankTransactions
                .FirstOrDefaultAsync(b => b.Reference == reference);
        }
    }
}