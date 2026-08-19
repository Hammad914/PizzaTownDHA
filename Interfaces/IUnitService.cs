using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Interfaces
{
    public interface IUnitService
    {
        Task<List<Unit>> GetAllAsync();
        //Task<List<Unit?>> GetGetByIdAsync(Guid id);
        //Task<Unit> RegisterUnitAsync(Unit unit);
        //Task<Unit> UpdateUnitAsync(Unit unit);
        //Task<bool> DeleteUnitAsync(Guid id);
        //Task<bool> UnitSymbolExistsAsync(string unitSymbol);
        //Task<bool> CanDeleteAsync(Guid id);
    }
}
