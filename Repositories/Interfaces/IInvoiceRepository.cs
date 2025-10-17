using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EconomicWPF.Models;
using EconomicWPF.Enums;

namespace EconomicWPF.Repositories.Interfaces
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<Invoice> GetWithLinesAsync(int invoiceId);
        Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status);
        Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
        Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<string> GenerateNextInvoiceNumberAsync();
    }
}