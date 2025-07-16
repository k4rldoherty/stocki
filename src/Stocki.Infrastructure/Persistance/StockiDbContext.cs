using Microsoft.EntityFrameworkCore;
using Stocki.Domain.Models;

namespace Stocki.Infrastructure.Persistance;

public class StockiDbContext : DbContext
{
    public DbSet<StockPriceSubscription> StockPriceSubscriptions { get; set; }

    public StockiDbContext(DbContextOptions<StockiDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StockPriceSubscription>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.ToTable("StockPriceSubscriptions"); // Specify the table name

            entity.HasIndex(s => new { s.DiscordId, s.Ticker }).IsUnique();

            entity.Property(s => s.Ticker).IsRequired().HasMaxLength(5);

            entity
                .Property(s => s.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        });
    }
}
