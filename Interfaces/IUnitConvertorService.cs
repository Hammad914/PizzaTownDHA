namespace PizzaTownDHA.Interfaces
{
    public interface IUnitConvertorService
    {
        public decimal GetConvertedQuantity(decimal fractionConversion, decimal userInputQuantity);
    }
}
