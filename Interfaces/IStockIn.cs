using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Interfaces
{
    public interface IStockInService
    {
        Task<List<StockIn>> GetAllAsync();
        Task<StockIn> AddStockInAsync(StockIn stockIn);
        Task<bool> DeleteStockInAsync(Guid id);
        Task<List<StockIn>> GetByDateAsync(DateTime date);
    }
}