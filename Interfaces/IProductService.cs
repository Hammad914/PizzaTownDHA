using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(Guid id);
        Task<Product> RegisterProductAsync(Product product, List<Guid> selectedIngredientIds, List<decimal> quantities);
        Task<Product> UpdateProductAsync(Product product, List<Guid> selectedIngredientIds, List<decimal> quantities);
        Task<bool> DeleteProductAsync(Guid id);
        Task<bool> ProductNameExistsAsync(string name);
        Task<bool> ProductNameExistsAsync(string name, Guid currentId);
    }
}