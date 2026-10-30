using Microsoft.EntityFrameworkCore;
using QTKD.Api.Models;

namespace QTKD.Api.Data;

public sealed class QtkdDbContext(DbContextOptions<QtkdDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceItem> PriceItems => Set<PriceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasData(
                new Customer { Id = 1, Name = "Công ty Minh Phát", Status = "Active" },
                new Customer { Id = 2, Name = "Công ty An Bình", Status = "Active" });
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ContractNo).IsUnique();
            entity.Property(x => x.ContractNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Contracts)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasData(new Contract
            {
                Id = 1, ContractNo = "HD-2026-018", CustomerId = 1,
                StartDate = new DateOnly(2026, 11, 1),
                EndDate = new DateOnly(2027, 10, 31),
                Status = "Draft"
            });
        });

        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasData(new PriceList
            {
                Id = 1, Name = "Bảng giá dịch vụ 2026",
                EffectiveDate = new DateOnly(2026, 11, 1)
            });
        });

        modelBuilder.Entity<PriceItem>(entity =>
        {
            entity.HasKey(x => new { x.PriceListId, x.ServiceCode });
            entity.Property(x => x.ServiceCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(x => x.PriceList)
                .WithMany(x => x.PriceItems)
                .HasForeignKey(x => x.PriceListId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasData(
                new PriceItem { PriceListId = 1, ServiceCode = "SVC-001", UnitPrice = 1500000m },
                new PriceItem { PriceListId = 1, ServiceCode = "SVC-002", UnitPrice = 2500000m });
        });
    }
}
