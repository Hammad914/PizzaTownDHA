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
            var response = await db.Units.Where(u => !u.IsDeleted).OrderBy(u => u.DisplayOrder).ToListAsync();
            return response;
        }
        public async Task<Unit?> GetByIdAsync(Guid id)
        {
            var response = await db.Units.FirstOrDefaultAsync(e => e.Id == id);
            return response; 
        }
        public async Task<Unit> RegisterUnitAsync(Unit unit)
        {
            if(unit.Id == Guid.Empty)
            unit.Id = Guid.NewGuid();

            unit.CreatedAt = DateTime.UtcNow;
            unit.IsDeleted = false;
            unit.CreatedBy = "System";
            unit.UpdatedBy = null;

            var exits = await db.Units.AnyAsync(u => u.UnitSymbol == unit.UnitSymbol && !u.IsDeleted);

            if (exits)
                throw new InvalidOperationException($"Unit Symbol '${unit.UnitSymbol}' already exists");

            if (unit.IsBaseUnit)
            {
                var existingBase = await db.Units.FirstOrDefaultAsync(u => u.Category == unit.Category && u.IsBaseUnit && !u.IsDeleted);

                if (existingBase != null)
                    throw new InvalidOperationException($"Base Unit Already Exists for category '${unit.Category}': '${existingBase.UnitSymbol}'");
            }

            db.Units.Add(unit);
            await db.SaveChangesAsync();
            return unit;
        }

     public async Task<Unit> UpdateUnitAsync(Unit unit)
        {
          // Checks the Unit exists or not. If does then update possible otherwise show error unit doesn't exisits
            var exisitingUnit = await db.Units.FirstOrDefaultAsync(u => u.Id == unit.Id && !u.IsDeleted);

            if (exisitingUnit == null)
                throw new InvalidOperationException($"Unit With Id '{unit.Id}' Not Found");

            // Checking the duplicate symbol. This is to check that when updating we do not add the unitsymbol that already some other unit has.
            var duplicateSymbol = await db.Units.AnyAsync(u => u.UnitSymbol == unit.UnitSymbol && !u.IsDeleted && u.Id != unit.Id);

            if (duplicateSymbol)
                throw new InvalidOperationException($"UnitSymbol {unit.UnitSymbol} already exisits");

            exisitingUnit.UnitSymbol = unit.UnitSymbol;
            exisitingUnit.UnitName = unit.UnitName;
            exisitingUnit.Category = unit.Category;
            exisitingUnit.ConversionFactor = unit.ConversionFactor;
            exisitingUnit.IsBaseUnit = unit.IsBaseUnit;
            exisitingUnit.DisplayOrder = unit.DisplayOrder;

            exisitingUnit.UpdatedAt = DateTime.UtcNow;
            exisitingUnit.UpdatedBy = unit.UpdatedBy ?? "System";

            db.Units.Update(exisitingUnit);
            await db.SaveChangesAsync();
            return exisitingUnit;
        }

        public async Task<bool> UnitSymbolExistsAsync(String unitSymbol)
        {
            return await db.Units.AnyAsync(u => u.UnitSymbol == unitSymbol);
        }

        public async Task<bool> UnitSymbolExistsAsync(String unitSymbol, Guid currentId)
        {
            return await db.Units.AnyAsync(u => u.UnitSymbol == unitSymbol && u.Id != currentId);
        }

        public async Task<bool> DeleteUnitAsync(Guid id)
        {
            var unit = await db.Units.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (unit == null)
                return false;
            
            // soft delete
            unit.IsDeleted = true;

            await db.SaveChangesAsync();
            return true;
        }
    }
}
