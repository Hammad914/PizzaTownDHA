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

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.IsDeleted);
                entity.Property(e => e.CreatedBy);
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedBy);
                entity.Property(e => e.UpdatedAt);
            });

            modelBuilder.Entity<ProductIngredient>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("product_ingredients");
                entity.Property(e => e.QuantityRequired).HasColumnType("decimal(18,4)");

                entity.HasOne(pi => pi.Product)
                    .WithMany(p => p.ProductIngredients)
                    .HasForeignKey(pi => pi.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pi => pi.Ingredient)
                    .WithMany()
                    .HasForeignKey(pi => pi.IngredientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}
