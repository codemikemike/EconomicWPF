using System.Collections.Generic;
using System.Threading.Tasks;
using EconomicWPF.Models;

namespace EconomicWPF.Repositories.Interfaces
{
    public interface ISupplierRepository : IRepository<Supplier>
    {
        Task<IEnumerable<Supplier>> SearchByNameAsync(string name);
        Task<Supplier> GetByCVRAsync(string cvr);
        Task<Supplier> GetBySupplierNumberAsync(string supplierNumber);
        Task<IEnumerable<Supplier>> GetActiveSuppliersAsync();
    }
}