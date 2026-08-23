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
                .Where(s => !s.IsDeleted)
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

            stockIn.CreatedAt = DateTime.Now;
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

            existing.IngredientId = stockIn.IngredientId;
            existing.QuantityReceived = stockIn.QuantityReceived;
            existing.ReceivedDate = stockIn.ReceivedDate;
            existing.UpdatedAt = DateTime.Now;

            db.StockIns.Update(existing);
            await db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteStockInAsync(Guid id)
        {
            var stockIn = await db.StockIns.FirstOrDefaultAsync(s => s.Id == id);
            if (stockIn == null) return false;

            stockIn.IsDeleted = true;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<List<StockIn>> GetByDateAsync(DateTime date)
        {
            return await db.StockIns
                .Include(s => s.Ingredient)
                .ThenInclude(i => i.Unit)
                .Where(s => s.ReceivedDate.Date == date.Date && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}