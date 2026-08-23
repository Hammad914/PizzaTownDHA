using PizzaTownDHA.Interfaces;

namespace PizzaTownDHA.Utilities
{
    public class UnitConverterService : IUnitConvertorService
    {
        public decimal GetConvertedQuantity(decimal fractionConversion, decimal userInputQuantity)
        {
            var output = fractionConversion * userInputQuantity;
            return output;
        }

    }
}
