using Microsoft.EntityFrameworkCore;

namespace PillCat.Models.DbContexts;

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
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd().IsRequired();

            entity.OwnsOne(e => e.PeriodOfTreatment, period =>
            {
                period.Property(p => p.Amount);

                period.Property(p => p.TimeMeasure);
            });


            entity.OwnsOne(e => e.FrequencyOfPill, frequency =>
            {
                frequency.Property(f => f.IntervalPeriod);

                frequency.Property(f => f.TimeMeasure);
            });

            entity.OwnsOne(e => e.AmountPerUse, amountPerUse =>
            {
                amountPerUse.Property(f => f.Amount);

                amountPerUse.Property(f => f.PillType);
            });
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("PillCat");
    }
}