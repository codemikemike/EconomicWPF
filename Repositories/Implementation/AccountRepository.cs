using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EconomicWPF.Database;
using EconomicWPF.Models;
using EconomicWPF.Enums;
using EconomicWPF.Repositories.Interfaces;

namespace EconomicWPF.Repositories.Implementation
{
    /// <summary>
    /// Repository implementation for Account (Kontoplan) using Entity Framework
    /// </summary>
    public class AccountRepository : IAccountRepository
    {
        private readonly EconomicDbContext _context;

        public AccountRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _context.Accounts
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
        }

        public async Task<Account> GetByIdAsync(int id)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == id);
        }

        public async Task<Account> GetByAccountNumberAsync(string accountNumber)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<IEnumerable<Account>> GetByTypeAsync(AccountType accountType)
        {
            return await _context.Accounts
                .Where(a => a.AccountType == accountType && a.IsActive)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> GetAccountHierarchyAsync()
        {
            // Returns all accounts - hierarchy can be built in ViewModel
            return await GetAllAsync();
        }

        public async Task<bool> UpdateBalanceAsync(int accountId, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                return false;

            account.Balance += amount;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> AddAsync(Account account)
        {
            account.CreatedDate = DateTime.Now;

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return account.AccountId;
        }

        public async Task<bool> UpdateAsync(Account account)
        {
            _context.Accounts.Update(account);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return false;

            _context.Accounts.Remove(account);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Accounts.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Accounts.AnyAsync(a => a.AccountId == id);
        }
    }
}