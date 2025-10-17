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
    /// Repository implementation for Invoice using Entity Framework
    /// </summary>
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly EconomicDbContext _context;

        public InvoiceRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<Invoice> GetByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
        }

        public async Task<Invoice> GetWithLinesAsync(int invoiceId)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceLines)
                    .ThenInclude(il => il.Product)
                .Include(i => i.InvoiceLines)
                    .ThenInclude(il => il.Account)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        }

        public async Task<int> AddAsync(Invoice invoice)
        {
            invoice.CreatedDate = DateTime.Now;
            invoice.ModifiedDate = DateTime.Now;

            // Set timestamps for invoice lines
            if (invoice.InvoiceLines != null)
            {
                foreach (var line in invoice.InvoiceLines)
                {
                    line.CreatedDate = DateTime.Now;
                }
            }

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice.InvoiceId;
        }

        public async Task<bool> UpdateAsync(Invoice invoice)
        {
            invoice.ModifiedDate = DateTime.Now;

            _context.Invoices.Update(invoice);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return false;

            _context.Invoices.Remove(invoice);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Invoices.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Invoices.AnyAsync(i => i.InvoiceId == id);
        }

        public async Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.Status == status)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        {
            var today = DateTime.Now;

            return await _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.DueDate < today && !i.IsPaid)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<string> GenerateNextInvoiceNumberAsync()
        {
            var lastInvoice = await _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith("F"))
                .OrderByDescending(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            if (lastInvoice == null)
            {
                return "F1001";
            }

            var numberPart = lastInvoice.InvoiceNumber.Substring(1);

            if (int.TryParse(numberPart, out int number))
            {
                return $"F{number + 1}";
            }

            return "F1001";
        }
    }
}