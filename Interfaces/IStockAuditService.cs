using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Interfaces
{
    public interface IStockAuditService
    {
        Task<List<StockAudit>> GetAllAsync();
        Task<StockAudit?> GetByIngredientAndDateAsync(Guid ingredientId, DateTime date);
        Task<List<StockAudit>> GetByDateAsync(DateTime date);
        Task<StockAudit> RegisterStockAuditAsync(StockAudit stockAudit);
        Task<StockAudit> UpdateStockAuditAsync(StockAudit stockAudit);
        Task<bool> DeleteStockAuditAsync(Guid id);
    }
}