namespace PizzaTownDHA.Models.Entities
{
    public class StockAudit
    {
        public Guid Id { get; set; }
        public Guid IngredientId { get; set; }
        public decimal OpeningStock { get; set; }
        public decimal ActualClosingStock { get; set; }
        public DateTime AuditDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Ingredient? Ingredient { get; set; }
    }
}