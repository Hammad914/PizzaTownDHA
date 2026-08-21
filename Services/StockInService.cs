using Microsoft.EntityFrameworkCore;
using PizzaTownDHA.Data;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Services
{
    public class StockInService : IStockInService
    {
        private readonly PizzaTownContext db;
        public StockInService(PizzaTownContext _db)
        {
            db = _db;
        }

        public async Task<List<StockIn>> GetAllAsync()
        {
            return await db.StockIns
                .Include(s => s.Ingredient)
                .ThenInclude(i => i.Unit)
                .Where(s => !s.IsDeleted) // ✅ Only show non-deleted
                .OrderByDescending(s => s.ReceivedDate)
                .ThenByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<StockIn?> GetByIdAsync(Guid id)
        {
            return await db.StockIns
                .Include(s => s.Ingredient)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<StockIn> AddStockInAsync(StockIn stockIn)
        {
            if (stockIn.Id == Guid.Empty)
                stockIn.Id = Guid.NewGuid();

            stockIn.CreatedAt = DateTime.UtcNow;
            stockIn.IsDeleted = false;
            stockIn.CreatedBy = "System";

            db.StockIns.Add(stockIn);
            await db.SaveChangesAsync();
            return stockIn;
        }

        public async Task<StockIn> UpdateStockInAsync(StockIn stockIn)
        {
            var existing = await db.StockIns.FirstOrDefaultAsync(s => s.Id == stockIn.Id);
            if (existing == null)
                throw new InvalidOperationException("Stock In record not found.");

            var oldQty = existing.QuantityReceived;
            var newQty = stockIn.QuantityReceived;
            var diff = newQty - oldQty;

            // If diff is negative, stock is being removed
            if (diff < 0)
            {
                var todayAudit = await db.StockAudits.FirstOrDefaultAsync(sa => sa.IngredientId == stockIn.IngredientId && sa.AuditDate.Date == DateTime.Now.Date);
                if (todayAudit != null)
                {
                    // We only remove from the 'Available' amount, not the Used amount
                    var available = todayAudit.OpeningStock + todayAudit.ActualClosingStock; // This is just an approximation
                                                                                             // Actually, we need to only allow removal if there is enough *unused* stock
                                                                                             // The controller now blocks if you try to go below 'used', so just update audit
                    todayAudit.ActualClosingStock += diff;
                    db.StockAudits.Update(todayAudit);
                }
            }
            else
            {
                // If diff is positive, add stock
                var todayAudit = await db.StockAudits.FirstOrDefaultAsync(sa => sa.IngredientId == stockIn.IngredientId && sa.AuditDate.Date == DateTime.Now.Date);
                if (todayAudit != null)
                {
                    todayAudit.ActualClosingStock += diff;
                    db.StockAudits.Update(todayAudit);
                }
            }

            existing.IngredientId = stockIn.IngredientId;
            existing.QuantityReceived = stockIn.QuantityReceived;
            existing.ReceivedDate = stockIn.ReceivedDate;
            existing.UpdatedAt = DateTime.UtcNow;

            db.StockIns.Update(existing);
            await db.SaveChangesAsync();
            return existing;
        }
        public async Task<bool> DeleteStockInAsync(Guid id)
        {
            var stockIn = await db.StockIns.FirstOrDefaultAsync(s => s.Id == id);
            if (stockIn == null) return false;

            // 🚨 STOCK ADJUSTMENT: Remove this stock from inventory
            var todayAudit = await db.StockAudits.FirstOrDefaultAsync(sa => sa.IngredientId == stockIn.IngredientId && sa.AuditDate.Date == DateTime.Now.Date);
            if (todayAudit != null)
            {
                todayAudit.ActualClosingStock -= stockIn.QuantityReceived;
                db.StockAudits.Update(todayAudit);
            }

            stockIn.IsDeleted = true;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<List<StockIn>> GetByDateAsync(DateTime date)
        {
            return await db.StockIns
                .Include(s => s.Ingredient)
                .ThenInclude(i => i.Unit)
                .Where(s => s.ReceivedDate.Date == date.Date && !s.IsDeleted) // ✅ Only show non-deleted
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}