using Microsoft.EntityFrameworkCore;
using PizzaTownDHA.Data;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Services
{
    public class StockAuditService : IStockAuditService
    {
        private readonly PizzaTownContext db;
        public StockAuditService(PizzaTownContext _db)
        {
            db = _db;
        }

        public async Task<List<StockAudit>> GetAllAsync()
        {
            return await db.StockAudits
                .Include(s => s.Ingredient)
                .OrderByDescending(s => s.AuditDate)
                .ToListAsync();
        }

        public async Task<StockAudit?> GetByIngredientAndDateAsync(Guid ingredientId, DateTime date)
        {
            return await db.StockAudits
                .FirstOrDefaultAsync(s => s.IngredientId == ingredientId && s.AuditDate.Date == date.Date);
        }

        public async Task<List<StockAudit>> GetByDateAsync(DateTime date)
        {
            return await db.StockAudits
                .Include(s => s.Ingredient)
                .Where(s => s.AuditDate.Date == date.Date)
                .OrderBy(s => s.Ingredient.Name)
                .ToListAsync();
        }

        public async Task<StockAudit> RegisterStockAuditAsync(StockAudit stockAudit)
        {
            if (stockAudit.Id == Guid.Empty)
                stockAudit.Id = Guid.NewGuid();

            stockAudit.CreatedAt = DateTime.Now;
            stockAudit.IsDeleted = false;
            stockAudit.CreatedBy = "System";

            db.StockAudits.Add(stockAudit);
            await db.SaveChangesAsync();
            return stockAudit;
        }

        public async Task<StockAudit> UpdateStockAuditAsync(StockAudit stockAudit)
        {
            var existing = await db.StockAudits
                .FirstOrDefaultAsync(s => s.Id == stockAudit.Id);

            if (existing == null)
                throw new InvalidOperationException("Stock Audit record not found.");

            existing.OpeningStock = stockAudit.OpeningStock;
            existing.ActualClosingStock = stockAudit.ActualClosingStock;
            existing.UpdatedAt = DateTime.Now;

            db.StockAudits.Update(existing);
            await db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteStockAuditAsync(Guid id)
        {
            var audit = await db.StockAudits.FirstOrDefaultAsync(s => s.Id == id);
            if (audit == null)
                return false;

            audit.IsDeleted = true;
            await db.SaveChangesAsync();
            return true;
        }
    }
}