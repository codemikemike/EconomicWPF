using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EconomicWPF.Database;
using EconomicWPF.Models;
using EconomicWPF.Repositories.Interfaces;

namespace EconomicWPF.Repositories.Implementation
{
    /// <summary>
    /// Repository implementation for Dimension using Entity Framework
    /// </summary>
    public class DimensionRepository : IDimensionRepository
    {
        private readonly EconomicDbContext _context;

        public DimensionRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Dimension>> GetAllAsync()
        {
            return await _context.Dimensions
                .OrderBy(d => d.DimensionName)
                .ToListAsync();
        }

        public async Task<Dimension> GetByIdAsync(int id)
        {
            return await _context.Dimensions
                .FirstOrDefaultAsync(d => d.DimensionId == id);
        }

        public async Task<int> AddAsync(Dimension dimension)
        {
            dimension.CreatedDate = DateTime.Now;

            _context.Dimensions.Add(dimension);
            await _context.SaveChangesAsync();

            return dimension.DimensionId;
        }

        public async Task<bool> UpdateAsync(Dimension dimension)
        {
            _context.Dimensions.Update(dimension);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var dimension = await _context.Dimensions.FindAsync(id);
            if (dimension == null)
                return false;

            _context.Dimensions.Remove(dimension);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Dimensions.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Dimensions.AnyAsync(d => d.DimensionId == id);
        }

        public async Task<IEnumerable<Dimension>> GetDimensionHierarchyAsync()
        {
            // Returns all dimensions - hierarchy can be built in ViewModel
            return await GetAllAsync();
        }

        public async Task<IEnumerable<Dimension>> GetChildDimensionsAsync(int parentDimensionId)
        {
            return await _context.Dimensions
                .Where(d => d.ParentDimensionId == parentDimensionId)
                .OrderBy(d => d.DimensionName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dimension>> GetByNameAsync(string dimensionName)
        {
            return await _context.Dimensions
                .Where(d => d.DimensionName.Contains(dimensionName))
                .OrderBy(d => d.DimensionName)
                .ToListAsync();
        }
    }
}