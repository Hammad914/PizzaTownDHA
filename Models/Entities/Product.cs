using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaTownDHA.Models.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;

        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<ProductIngredient> ProductIngredients { get; set; } = new List<ProductIngredient>();
    }
}