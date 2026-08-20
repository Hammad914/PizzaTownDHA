using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaTownDHA.Models.Entities
{
    public class ProductIngredient
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }
        public Guid IngredientId { get; set; }

        public decimal QuantityRequired { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("IngredientId")]
        public virtual Ingredient Ingredient { get; set; } = null!;
    }
}