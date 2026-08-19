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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UnitSymbol , Category , IsBaseUnit, ConversionFactor
            // DisplayOrder, CreatedBy, CreatedAt , UpdatedBy ,
            // UpdatedAt, IsDeleted
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
        }

    }
}
