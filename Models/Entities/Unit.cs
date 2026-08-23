namespace PizzaTownDHA.Models.Entities
{
    public class Unit
    {
        // UnitSymbol , Category , IsBaseUnit, ConversionFactor
        // DisplayOrder, CreatedBy, CreatedAt , UpdatedBy ,
        // UpdatedAt, IsDeleted
        public Guid Id { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string UnitSymbol { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsBaseUnit { get; set; } = false;
        public decimal ConversionFactor { get; set; }
        public int DisplayOrder { get; set; }
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; } 
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
