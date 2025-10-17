using System.Collections.Generic;
using System.Threading.Tasks;
using EconomicWPF.Models;

namespace EconomicWPF.Repositories.Interfaces
{
    public interface IVATCodeRepository : IRepository<VATCode>
    {
        Task<IEnumerable<VATCode>> GetActiveVATCodesAsync();
        Task<VATCode> GetByCodeAsync(string code);
    }
}