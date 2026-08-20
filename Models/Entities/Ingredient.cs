using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaTownDHA.Models.Entities
{
    public class Ingredient
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid UnitId { get; set; }
        public decimal PhysicalStock { get; set; }
        public decimal MinimumStock { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }
    }
}