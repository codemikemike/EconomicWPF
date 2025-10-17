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
    /// Repository implementation for Product using Entity Framework
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly EconomicDbContext _context;

        public ProductRepository(EconomicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<int> AddAsync(Product product)
        {
            product.CreatedDate = DateTime.Now;
            product.ModifiedDate = DateTime.Now;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product.ProductId;
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            product.ModifiedDate = DateTime.Now;

            _context.Products.Update(product);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            _context.Products.Remove(product);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Products.CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.ProductId == id);
        }

        public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
        {
            return await _context.Products
                .Where(p => p.Name.Contains(name))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product> GetByProductNumberAsync(string productNumber)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.ProductNumber == productNumber);
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
        {
            return await _context.Products
                .Where(p => p.StockQuantity <= p.ReorderLevel && p.IsActive)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantityChange)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return false;

            product.StockQuantity += quantityChange;
            product.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}