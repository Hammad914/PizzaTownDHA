using Microsoft.EntityFrameworkCore;
using PizzaTownDHA.Data;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Services
{
    public class UnitService : IUnitService
    {
        private readonly PizzaTownContext db;
        public UnitService(PizzaTownContext _db)
        {
            db = _db;
        }

        public async Task<List<Unit>> GetAllAsync()
        {
            var response = await db.Units
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.DisplayOrder)
                .ToListAsync();
            return response;
        }

        public async Task<Unit?> GetByIdAsync(Guid id)
        {
            return await db.Units.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Unit> RegisterUnitAsync(Unit unit)
        {
            if (unit.Id == Guid.Empty)
                unit.Id = Guid.NewGuid();

            unit.CreatedAt = DateTime.Now;
            unit.IsDeleted = false;
            unit.CreatedBy = "System";
            unit.UpdatedBy = null;

            if (unit.IsBaseUnit)
            {
                var existingBase = await db.Units
                    .FirstOrDefaultAsync(u => u.Category == unit.Category && u.IsBaseUnit && !u.IsDeleted);

                if (existingBase != null)
                {
                    existingBase.IsDeleted = true;
                    db.Units.Update(existingBase);
                }
            }

            db.Units.Add(unit);
            await db.SaveChangesAsync();
            return unit;
        }

        public async Task<Unit> UpdateUnitAsync(Unit unit)
        {
            var existingUnit = await db.Units
                .FirstOrDefaultAsync(u => u.Id == unit.Id && !u.IsDeleted);

            if (existingUnit == null)
                throw new InvalidOperationException("Unit not found.");

            if (unit.IsBaseUnit)
            {
                var existingBase = await db.Units.FirstOrDefaultAsync(u =>
                    u.Category == unit.Category &&
                    u.IsBaseUnit &&
                    !u.IsDeleted &&
                    u.Id != unit.Id);

                if (existingBase != null)
                {
                    throw new InvalidOperationException(
                        $"Each category can only have ONE base unit. '{existingBase.UnitSymbol}' is already the base unit for the '{unit.Category}' category."
                    );
                }
            }

            var duplicateOrder = await db.Units.AnyAsync(u =>
                u.DisplayOrder == unit.DisplayOrder &&
                !u.IsDeleted &&
                u.Id != unit.Id);

            if (duplicateOrder)
            {
                throw new InvalidOperationException(
                    $"Display Order '{unit.DisplayOrder}' is already taken. Please choose a different number."
                );
            }

            existingUnit.UnitSymbol = unit.UnitSymbol;
            existingUnit.UnitName = unit.UnitName;
            existingUnit.Category = unit.Category;
            existingUnit.ConversionFactor = unit.ConversionFactor;
            existingUnit.IsBaseUnit = unit.IsBaseUnit;
            existingUnit.DisplayOrder = unit.DisplayOrder;

            existingUnit.UpdatedAt = DateTime.Now;
            existingUnit.UpdatedBy = unit.UpdatedBy ?? "System";

            db.Units.Update(existingUnit);
            await db.SaveChangesAsync();
            return existingUnit;
        }

        public async Task<bool> UnitSymbolExistsAsync(string unitSymbol)
        {
            return await db.Units.AnyAsync(u => u.UnitSymbol == unitSymbol);
        }

        public async Task<bool> UnitSymbolExistsAsync(string unitSymbol, Guid currentId)
        {
            return await db.Units.AnyAsync(u => u.UnitSymbol == unitSymbol && u.Id != currentId);
        }

        public async Task<bool> UnitNameExistsAsync(string unitName)
        {
            return await db.Units.AnyAsync(u => u.UnitName == unitName);
        }

        public async Task<bool> UnitNameExistsAsync(string unitName, Guid currentId)
        {
            return await db.Units.AnyAsync(u => u.UnitName == unitName && u.Id != currentId);
        }

        public async Task<bool> DisplayOrderExistsAsync(int displayOrder)
        {
            return await db.Units.AnyAsync(u => u.DisplayOrder == displayOrder);
        }

        public async Task<bool> DisplayOrderExistsAsync(int displayOrder, Guid currentId)
        {
            return await db.Units.AnyAsync(u => u.DisplayOrder == displayOrder && u.Id != currentId);
        }

        public async Task<bool> DeleteUnitAsync(Guid id)
        {
            var unit = await db.Units
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (unit == null)
                return false;

            unit.IsDeleted = true;
            await db.SaveChangesAsync();
            return true;
        }
    }
}