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
            var response = await db.Units.ToListAsync();
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
    }
}
