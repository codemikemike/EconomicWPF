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
    /// Repository implementation for VATCode using Entity Framework
    /// </summary>
    public class VATCodeRepository : IVATCodeRepository
    {
        private readonly EconomicDbContext _context;

        public VATCodeRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<VATCode>> GetAllAsync()
        {
            return await _context.VATCodes
                .OrderBy(v => v.Code)
                .ToListAsync();
        }

        public async Task<VATCode> GetByIdAsync(int id)
        {
            return await _context.VATCodes
                .FirstOrDefaultAsync(v => v.VATCodeId == id);
        }

        public async Task<int> AddAsync(VATCode vatCode)
        {
            vatCode.CreatedDate = DateTime.Now;

            _context.VATCodes.Add(vatCode);
            await _context.SaveChangesAsync();

            return vatCode.VATCodeId;
        }

        public async Task<bool> UpdateAsync(VATCode vatCode)
        {
            _context.VATCodes.Update(vatCode);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var vatCode = await _context.VATCodes.FindAsync(id);
            if (vatCode == null)
                return false;

            _context.VATCodes.Remove(vatCode);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.VATCodes.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.VATCodes.AnyAsync(v => v.VATCodeId == id);
        }

        public async Task<IEnumerable<VATCode>> GetActiveVATCodesAsync()
        {
            return await _context.VATCodes
                .Where(v => v.IsActive)
                .OrderBy(v => v.Code)
                .ToListAsync();
        }

        public async Task<VATCode> GetByCodeAsync(string code)
        {
            return await _context.VATCodes
                .FirstOrDefaultAsync(v => v.Code == code);
        }
    }
}