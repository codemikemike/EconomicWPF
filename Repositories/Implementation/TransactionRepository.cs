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
    /// Repository implementation for Transaction using Entity Framework
    /// </summary>
    public class TransactionRepository : ITransactionRepository
    {
        private readonly EconomicDbContext _context;

        public TransactionRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction> GetByIdAsync(int id)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id);
        }

        public async Task<int> AddAsync(Transaction transaction)
        {
            transaction.CreatedDate = DateTime.Now;
            transaction.ModifiedDate = DateTime.Now;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction.TransactionId;
        }

        public async Task<bool> UpdateAsync(Transaction transaction)
        {
            transaction.ModifiedDate = DateTime.Now;

            _context.Transactions.Update(transaction);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return false;

            _context.Transactions.Remove(transaction);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Transactions.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Transactions.AnyAsync(t => t.TransactionId == id);
        }

        public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Transactions
                .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByYearAsync(int year)
        {
            return await _context.Transactions
                .Where(t => t.TransactionDate.Year == year)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetPostedTransactionsAsync()
        {
            return await _context.Transactions
                .Where(t => t.IsPosted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetUnpostedTransactionsAsync()
        {
            return await _context.Transactions
                .Where(t => !t.IsPosted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction> GetByVoucherNumberAsync(string voucherNumber)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.VoucherNumber == voucherNumber);
        }
    }
}