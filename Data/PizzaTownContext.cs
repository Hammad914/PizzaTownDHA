using Microsoft.EntityFrameworkCore;
using PizzaTownDHA.Models.Entities;

namespace PizzaTownDHA.Data
{
    public class PizzaTownContext : DbContext
    {
        public PizzaTownContext(DbContextOptions<PizzaTownContext> option):base(option)
        {
        }
        public virtual DbSet<Unit> Units { get; set; }
        public virtual DbSet<Ingredient> Ingredients { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductIngredient> ProductIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Unit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitSymbol);
                entity.Property(e => e.Category);
                entity.Property(e => e.IsBaseUnit);
                entity.Property(e => e.ConversionFactor);
                entity.Property(e => e.DisplayOrder);
                entity.Property(e => e.CreatedBy);
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedBy);
                entity.Property(e => e.UpdatedAt);
                entity.Property(e => e.IsDeleted);
            });

            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name);
                entity.Property(e => e.UnitId);
                entity.Property(e => e.PhysicalStock);
                entity.Property(e => e.MinimumStock);
                entity.Property(e => e.CreatedBy);
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedBy);
                entity.Property(e => e.UpdatedAt);
                entity.Property(e => e.IsDeleted);
            });
        }

    }
}
