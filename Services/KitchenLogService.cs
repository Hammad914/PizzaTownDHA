using Microsoft.EntityFrameworkCore;
using PizzaTownDHA.Data;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Services
{
    public class KitchenLogService : IKitchenLogService
    {
        private readonly PizzaTownContext db;
        public KitchenLogService(PizzaTownContext _db)
        {
            db = _db;
        }

        public async Task<List<KitchenLog>> GetAllAsync()
        {
            return await db.KitchenLogs
                .Include(kl => kl.Product)
                .OrderByDescending(kl => kl.DateLogged)
                .ToListAsync();
        }

        public async Task<KitchenLog?> GetByIdAsync(Guid id)
        {
            return await db.KitchenLogs
                .Include(kl => kl.Product)
                .FirstOrDefaultAsync(kl => kl.Id == id);
        }
        public async Task<List<KitchenLog>> LogMultiAsync(List<Guid> productIds, List<int> quantities)
        {
            var logs = new List<KitchenLog>();

            for (int i = 0; i < productIds.Count; i++)
            {
                if (quantities[i] > 0)
                {
                    logs.Add(new KitchenLog
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productIds[i],
                        QuantityMade = quantities[i],
                        DateLogged = DateTime.Now
                    });
                }
            }

            db.KitchenLogs.AddRange(logs);
            await db.SaveChangesAsync();
            return logs;
        }

        public async Task<KitchenLog> UpdateLogAsync(KitchenLog log)
        {
            db.KitchenLogs.Update(log);
            await db.SaveChangesAsync();
            return log;
        }

        public async Task<bool> DeleteLogAsync(Guid id)
        {
            var log = await db.KitchenLogs.FirstOrDefaultAsync(kl => kl.Id == id);
            if (log == null) return false;

            db.KitchenLogs.Remove(log);
            await db.SaveChangesAsync();
            return true;
        }
    }
}