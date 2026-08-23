using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Interfaces
{
    public interface IStockInService
    {
        Task<List<StockIn>> GetAllAsync();
        Task<StockIn?> GetByIdAsync(Guid id);
        Task<StockIn> AddStockInAsync(StockIn stockIn, Guid selectedUnitId);
        Task<StockIn> UpdateStockInAsync(StockIn stockIn, Guid selectedUnitId);
        Task<bool> DeleteStockInAsync(Guid id);
        Task<List<StockIn>> GetByDateAsync(DateTime date);
        Task<decimal> GetTotalStockByIngredientIdAsync(Guid ingredientId);

    }
}