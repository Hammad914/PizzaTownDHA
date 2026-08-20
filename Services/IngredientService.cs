using Microsoft.EntityFrameworkCore;
using PizzaTownDHA.Data;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly PizzaTownContext db;
        public IngredientService(PizzaTownContext _db)
        {
            db = _db;
        }

        public async Task<List<Ingredient>> GetAllAsync()
        {
            return await db.Ingredients
                .Where(i => !i.IsDeleted)
                .Include(i => i.Unit)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<Ingredient?> GetByIdAsync(Guid id)
        {
            return await db.Ingredients
                .Include(i => i.Unit)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Ingredient> RegisterIngredientAsync(Ingredient ingredient)
        {
            if (ingredient.Id == Guid.Empty)
                ingredient.Id = Guid.NewGuid();

            ingredient.CreatedAt = DateTime.UtcNow;
            ingredient.IsDeleted = false;
            ingredient.CreatedBy = "System";
            ingredient.UpdatedBy = null;

            db.Ingredients.Add(ingredient);
            await db.SaveChangesAsync();
            return ingredient;
        }

        public async Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient)
        {
            var existing = await db.Ingredients
                .FirstOrDefaultAsync(i => i.Id == ingredient.Id && !i.IsDeleted);

            if (existing == null)
                throw new InvalidOperationException("Ingredient not found.");

            existing.Name = ingredient.Name;
            existing.UnitId = ingredient.UnitId;
            existing.PhysicalStock = ingredient.PhysicalStock;
            existing.MinimumStock = ingredient.MinimumStock;

            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = "System";

            db.Ingredients.Update(existing);
            await db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteIngredientAsync(Guid id)
        {
            var ingredient = await db.Ingredients
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (ingredient == null)
                return false;

            ingredient.IsDeleted = true;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IngredientNameExistsAsync(string name)
        {
            return await db.Ingredients.AnyAsync(i => i.Name == name);
        }

        public async Task<bool> IngredientNameExistsAsync(string name, Guid currentId)
        {
            return await db.Ingredients.AnyAsync(i => i.Name == name && i.Id != currentId);
        }
    }
}