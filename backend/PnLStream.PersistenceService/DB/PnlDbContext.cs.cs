using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PnLStream.Common.Entities;
using System.Collections;
using System.Text.Json;

namespace PnLStream.Persistence.Db;

public class PnlDbContext : DbContext
{
    public PnlDbContext(DbContextOptions<PnlDbContext> options)
        : base(options)
    {
    }

    public DbSet<PnlRecord> PnlRecords => Set<PnlRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PnlRecord>(static entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SourceSystem)
                  .HasMaxLength(100);

            entity.Property(x => x.ValidationReasons)
                  .HasConversion(
                        v => string.Join('|', v),
                        v => string.IsNullOrWhiteSpace(v)
                                ? new List<string>()
                                : v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.Property(x => x.DataSource)
                   .HasConversion<string>()
                    .HasMaxLength(20);

            entity.HasIndex(x => new
            {
                x.SourceSystem,
                x.PortfolioNumber
            }).IsUnique();

        });

        base.OnModelCreating(modelBuilder);
    }
}