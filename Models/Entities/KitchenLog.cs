using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaTownDHA.Models.Entities
{
    public class KitchenLog
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int QuantityMade { get; set; }
        public DateTime DateLogged { get; set; }

        // Navigation property
        public virtual Product Product { get; set; } = null!;
    }
}