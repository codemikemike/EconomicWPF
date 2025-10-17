using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EconomicWPF.Models;
using EconomicWPF.Enums;

namespace EconomicWPF.Repositories.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetByInvoiceIdAsync(int invoiceId);
        Task<IEnumerable<Payment>> GetByPaymentMethodAsync(PaymentMethod paymentMethod);
        Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<decimal> GetTotalPaymentsByInvoiceIdAsync(int invoiceId);
    }
}