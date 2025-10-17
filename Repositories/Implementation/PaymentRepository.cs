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
    /// Repository implementation for Payment using Entity Framework
    /// </summary>
    public class PaymentRepository : IPaymentRepository
    {
        private readonly EconomicDbContext _context;

        public PaymentRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment> GetByIdAsync(int id)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        public async Task<int> AddAsync(Payment payment)
        {
            payment.CreatedDate = DateTime.Now;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment.PaymentId;
        }

        public async Task<bool> UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return false;

            _context.Payments.Remove(payment);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Payments.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Payments.AnyAsync(p => p.PaymentId == id);
        }

        public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _context.Payments
                .Where(p => p.InvoiceId == invoiceId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByPaymentMethodAsync(PaymentMethod paymentMethod)
        {
            return await _context.Payments
                .Where(p => p.PaymentMethod == paymentMethod)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Payments
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaymentsByInvoiceIdAsync(int invoiceId)
        {
            var total = await _context.Payments
                .Where(p => p.InvoiceId == invoiceId)
                .SumAsync(p => p.Amount);

            return total;
        }
    }
}