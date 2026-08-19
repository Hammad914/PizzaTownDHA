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

    }
}
