using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Interfaces
{
    public interface IUnitService
    {
        Task<List<Unit>> GetAllAsync();
        Task<Unit?> GetByIdAsync(Guid id);
        Task<Unit> RegisterUnitAsync(Unit unit);
        Task<Unit> UpdateUnitAsync(Unit unit);
        Task<Unit?> GetUnitBySymbolAsync(string unitSymbol);
        Task<bool> DeleteUnitAsync(Guid id);

        Task<bool> UnitSymbolExistsAsync(string unitSymbol);
        Task<bool> UnitSymbolExistsAsync(string unitSymbol, Guid currentId);

        Task<bool> UnitNameExistsAsync(string unitName);
        Task<bool> UnitNameExistsAsync(string unitName, Guid currentId);

        Task<bool> DisplayOrderExistsAsync(int displayOrder);
        Task<bool> DisplayOrderExistsAsync(int displayOrder, Guid currentId);
    }
}
