using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Interfaces
{
    public interface IProductService
    {
        // --- READ ---
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(Guid id);

        // --- CREATE ---
        Task<Product> RegisterProductAsync(Product product, List<Guid> selectedIngredientIds, List<decimal> quantities);

        // --- UPDATE ---
        Task<Product> UpdateProductAsync(Product product, List<Guid> selectedIngredientIds, List<decimal> quantities);

        // --- DELETE ---
        Task<bool> DeleteProductAsync(Guid id);

        // --- VALIDATION ---
        Task<bool> ProductNameExistsAsync(string name);
        Task<bool> ProductNameExistsAsync(string name, Guid currentId);
    }
}