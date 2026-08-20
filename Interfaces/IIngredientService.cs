using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Interfaces
{
    public interface IIngredientService
    {
        Task<List<Ingredient>> GetAllAsync();
        Task<Ingredient?> GetByIdAsync(Guid id);
        Task<Ingredient> RegisterIngredientAsync(Ingredient ingredient);
        Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient);
        Task<bool> DeleteIngredientAsync(Guid id);
        Task<bool> IngredientNameExistsAsync(string name);
        Task<bool> IngredientNameExistsAsync(string name, Guid currentId);
    }
}