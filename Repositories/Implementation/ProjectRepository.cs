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
    /// Repository implementation for Project using Entity Framework
    /// </summary>
    public class ProjectRepository : IProjectRepository
    {
        private readonly EconomicDbContext _context;

        public ProjectRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<Project> GetByIdAsync(int id)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == id);
        }

        public async Task<int> AddAsync(Project project)
        {
            project.CreatedDate = DateTime.Now;
            project.ModifiedDate = DateTime.Now;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return project.ProjectId;
        }

        public async Task<bool> UpdateAsync(Project project)
        {
            project.ModifiedDate = DateTime.Now;

            _context.Projects.Update(project);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return false;

            _context.Projects.Remove(project);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Projects.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Projects.AnyAsync(p => p.ProjectId == id);
        }

        public async Task<IEnumerable<Project>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Projects
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status)
        {
            return await _context.Projects
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
        {
            return await _context.Projects
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<Project> GetByProjectNumberAsync(string projectNumber)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectNumber == projectNumber);
        }
    }
}