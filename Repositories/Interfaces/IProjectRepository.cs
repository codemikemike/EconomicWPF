using System.Collections.Generic;
using System.Threading.Tasks;
using EconomicWPF.Models;
using EconomicWPF.Enums;

namespace EconomicWPF.Repositories.Interfaces
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<IEnumerable<Project>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status);
        Task<IEnumerable<Project>> GetActiveProjectsAsync();
        Task<Project> GetByProjectNumberAsync(string projectNumber);
    }
}