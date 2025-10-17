using System.Collections.Generic;
using System.Threading.Tasks;
using EconomicWPF.Models;

namespace EconomicWPF.Repositories.Interfaces
{
    public interface IDimensionRepository : IRepository<Dimension>
    {
        Task<IEnumerable<Dimension>> GetDimensionHierarchyAsync();
        Task<IEnumerable<Dimension>> GetChildDimensionsAsync(int parentDimensionId);
        Task<IEnumerable<Dimension>> GetByNameAsync(string dimensionName);
    }
}