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
                .OrderByDescending(s => s.ReceivedDate)
                .ThenByDescending(s => s.CreatedAt) // 🚨 NEWEST FIRST
                .ToListAsync();
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
                .Where(s => s.ReceivedDate.Date == date.Date)
                .OrderByDescending(s => s.CreatedAt) // 🚨 NEWEST FIRST
                .ToListAsync();
        }
    }
}