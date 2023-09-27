using Microsoft.EntityFrameworkCore;

namespace PillCat.Models;

public class PillContext : DbContext
{
    public PillContext(DbContextOptions<PillContext> options)
    : base(options)
    {
    }

    public DbSet<Pill> Pills { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pill>(entity =>
        {
            entity.HasKey(entity => entity.Id);
            entity.Property(entity => entity.Id).ValueGeneratedOnAdd().IsRequired();
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("PillCat");
    }
}