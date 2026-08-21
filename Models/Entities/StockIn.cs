namespace PizzaTownDHA.Models.Entities
{
    public class StockIn
    {
        public Guid Id { get; set; }
        public Guid IngredientId { get; set; }
        public decimal QuantityReceived { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string? Notes { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Ingredient? Ingredient { get; set; }
    }
}