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
    /// Repository implementation for Supplier (Leverandør) using Entity Framework
    /// </summary>
    public class SupplierRepository : ISupplierRepository
    {
        private readonly EconomicDbContext _context;

        public SupplierRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Supplier> GetByIdAsync(int id)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierId == id);
        }

        public async Task<IEnumerable<Supplier>> SearchByNameAsync(string name)
        {
            return await _context.Suppliers
                .Where(s => s.Name.Contains(name))
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Supplier> GetByCVRAsync(string cvr)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.CVR == cvr);
        }

        public async Task<Supplier> GetBySupplierNumberAsync(string supplierNumber)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierNumber == supplierNumber);
        }

        public async Task<IEnumerable<Supplier>> GetActiveSuppliersAsync()
        {
            return await _context.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<int> AddAsync(Supplier supplier)
        {
            supplier.CreatedDate = DateTime.Now;
            supplier.ModifiedDate = DateTime.Now;

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return supplier.SupplierId;
        }

        public async Task<bool> UpdateAsync(Supplier supplier)
        {
            supplier.ModifiedDate = DateTime.Now;

            _context.Suppliers.Update(supplier);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return false;

            _context.Suppliers.Remove(supplier);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Suppliers.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Suppliers.AnyAsync(s => s.SupplierId == id);
        }
    }
}