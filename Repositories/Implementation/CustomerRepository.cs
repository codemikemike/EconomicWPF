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
    /// Repository implementation for Customer using Entity Framework
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly EconomicDbContext _context;

        public CustomerRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == id);
        }

        public async Task<int> AddAsync(Customer customer)
        {
            customer.CreatedDate = DateTime.Now;
            customer.ModifiedDate = DateTime.Now;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return customer.CustomerId;
        }

        public async Task<bool> UpdateAsync(Customer customer)
        {
            customer.ModifiedDate = DateTime.Now;

            _context.Customers.Update(customer);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return false;

            _context.Customers.Remove(customer);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Customers.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Customers.AnyAsync(c => c.CustomerId == id);
        }

        public async Task<IEnumerable<Customer>> SearchByNameAsync(string name)
        {
            return await _context.Customers
                .Where(c => c.Name.Contains(name))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Customer> GetByCVRAsync(string cvr)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CVR == cvr);
        }

        public async Task<Customer> GetByCustomerNumberAsync(string customerNumber)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerNumber == customerNumber);
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetCustomersWithOverdueInvoicesAsync()
        {
            return await _context.Customers
                .Where(c => c.Invoices.Any(i => i.DueDate < DateTime.Now && !i.IsPaid))
                .Distinct()
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}