using Microsoft.EntityFrameworkCore;
using PizzaTownDHA.Data;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Services
{
    public class ProductService : IProductService
    {
        private readonly PizzaTownContext db;
        public ProductService(PizzaTownContext _db)
        {
            db = _db;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await db.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.ProductIngredients)
                .ThenInclude(pi => pi.Ingredient)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await db.Products
                .Include(p => p.ProductIngredients)
                .ThenInclude(pi => pi.Ingredient)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> RegisterProductAsync(Product product, List<Guid> selectedIngredientIds, List<decimal> quantities)
        {
            if (product.Id == Guid.Empty)
                product.Id = Guid.NewGuid();

            product.CreatedAt = DateTime.Now;
            product.IsDeleted = false;
            product.CreatedBy = "System";
            product.UpdatedBy = null;

            db.Products.Add(product);
            await db.SaveChangesAsync();

            for (int i = 0; i < selectedIngredientIds.Count; i++)
            {
                var productIngredient = new ProductIngredient
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    IngredientId = selectedIngredientIds[i],
                    QuantityRequired = quantities[i]
                };
                db.ProductIngredients.Add(productIngredient);
            }

            await db.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product, List<Guid> selectedIngredientIds, List<decimal> quantities)
        {
            var existingProduct = await db.Products
                .FirstOrDefaultAsync(p => p.Id == product.Id && !p.IsDeleted);

            if (existingProduct == null)
                throw new InvalidOperationException("Product not found.");

            // 1. Update the Product name
            existingProduct.Name = product.Name;
            existingProduct.UpdatedAt = DateTime.Now;
            existingProduct.UpdatedBy = "System";

            // 2. Remove old recipe ingredients
            var oldIngredients = db.ProductIngredients.Where(pi => pi.ProductId == product.Id);
            db.ProductIngredients.RemoveRange(oldIngredients);
            await db.SaveChangesAsync();

            // 3. Add the new recipe ingredients
            for (int i = 0; i < selectedIngredientIds.Count; i++)
            {
                var productIngredient = new ProductIngredient
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    IngredientId = selectedIngredientIds[i],
                    QuantityRequired = quantities[i]
                };
                db.ProductIngredients.Add(productIngredient);
            }

            await db.SaveChangesAsync();
            return existingProduct;
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var product = await db.Products
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null)
                return false;

            // Soft delete
            product.IsDeleted = true;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ProductNameExistsAsync(string name)
        {
            return await db.Products.AnyAsync(p => p.Name == name);
        }

        public async Task<bool> ProductNameExistsAsync(string name, Guid currentId)
        {
            return await db.Products.AnyAsync(p => p.Name == name && p.Id != currentId);
        }
    }
}