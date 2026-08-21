using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Interfaces
{
    public interface IKitchenLogService
    {
        Task<List<KitchenLog>> GetAllAsync();
        Task<KitchenLog?> GetByIdAsync(Guid id);
        Task<List<KitchenLog>> LogMultiAsync(List<Guid> productIds, List<int> quantities);
        Task<KitchenLog> UpdateLogAsync(KitchenLog log);
        Task<bool> DeleteLogAsync(Guid id);
    }
}