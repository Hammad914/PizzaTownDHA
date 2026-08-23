using Microsoft.EntityFrameworkCore;
using PizzaTownDHA.Data;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Services
{
    public class StockInService : IStockInService
    {
        private readonly PizzaTownContext db;
        private readonly IUnitConvertorService unitConvertorService;
        private readonly IUnitService unitService;
        public StockInService(PizzaTownContext _db, IUnitConvertorService _unitConvertorService, IUnitService _unitService)
        {
            db = _db;
            unitConvertorService = _unitConvertorService;
            unitService = _unitService;
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

        public async Task<StockIn> AddStockInAsync(StockIn stockIn, Guid selectedUnitId)
        {
            if (selectedUnitId == Guid.Empty)
                throw new InvalidOperationException("Invalid UnitId selected");

            if (stockIn.Id == Guid.Empty)
                stockIn.Id = Guid.NewGuid();

            var quantity = stockIn.QuantityReceived;
            var unit = await unitService.GetByIdAsync(selectedUnitId);

            if (unit == null || quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero and a valid unit must be selected.");


            var convertedQuantity = unitConvertorService.GetConvertedQuantity(unit.ConversionFactor, quantity);

            stockIn.QuantityReceived = convertedQuantity;
            stockIn.CreatedAt = DateTime.Now;
            stockIn.IsDeleted = false;
            stockIn.CreatedBy = "System";

            db.StockIns.Add(stockIn);
            await db.SaveChangesAsync();
            return stockIn;
        }

        public async Task<StockIn> UpdateStockInAsync(StockIn stockIn, Guid selectUnitId)
        {
            var existing = await db.StockIns.FirstOrDefaultAsync(s => s.Id == stockIn.Id);
            if (existing == null)
                throw new InvalidOperationException("Stock In record not found.");

            var quantity = stockIn.QuantityReceived;

            if (quantity <= 0 || selectUnitId == Guid.Empty)
                throw new InvalidOperationException("Quantity must be greater than 0 and a valid unit must be selected.");

            var unit = await unitService.GetByIdAsync(selectUnitId);

            if (unit == null)
                throw new InvalidOperationException("Selected unit not found.");

            // Calculate the NEW converted quantity (e.g., 400g -> 0.4kg)
            var newConvertedQuantity = unitConvertorService.GetConvertedQuantity(unit.ConversionFactor, quantity);

            // Calculate the DELTA (Old - New)
            var delta = existing.QuantityReceived - newConvertedQuantity;

            // SAFETY CHECK: If we are reducing stock (delta > 0), ensure we don't go negative
            if (delta > 0)
            {
                var currentTotalStock = await db.StockIns
                    .Where(s => s.IngredientId == existing.IngredientId && !s.IsDeleted)
                    .SumAsync(s => s.QuantityReceived);

                if (currentTotalStock < delta)
                {
                    throw new InvalidOperationException("Insufficient stock to reduce. Ingredient may already be used.");
                }
            }

            // Update the record
            existing.IngredientId = stockIn.IngredientId;
            existing.QuantityReceived = newConvertedQuantity;
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
        public async Task<decimal> GetTotalStockByIngredientIdAsync(Guid ingredientId)
        {
            return await db.StockIns
                .Where(s => s.IngredientId == ingredientId && !s.IsDeleted)
                .SumAsync(s => s.QuantityReceived);
        }
    }
}