using System.Collections.Generic;
using System.Threading.Tasks;
using EconomicWPF.Models;

namespace EconomicWPF.Repositories.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<IEnumerable<Customer>> SearchByNameAsync(string name);
        Task<Customer> GetByCVRAsync(string cvr);
        Task<Customer> GetByCustomerNumberAsync(string customerNumber);
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task<IEnumerable<Customer>> GetCustomersWithOverdueInvoicesAsync();
    }
}