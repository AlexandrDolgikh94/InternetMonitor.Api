using InternetMonitor.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternetMonitor.Api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Provider> Providers => Set<Provider>();
        public DbSet<ProviderPeriod> ProviderPeriods => Set<ProviderPeriod>();
        public DbSet<InternetCheck> InternetChecks => Set<InternetCheck>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Provider>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.Comment)
                    .HasMaxLength(1000);

                entity.HasIndex(p => p.Name)
                    .IsUnique();
            });

            builder.Entity<ProviderPeriod>(entity =>
            {
                entity.HasKey(pp => pp.Id);

                entity.Property(pp => pp.TariffName)
                    .HasMaxLength(200);

                entity.Property(pp => pp.Comment)
                    .HasMaxLength(1000);

                entity.HasOne(pp => pp.Provider)
                    .WithMany(p => p.Periods)
                    .HasForeignKey(pp => pp.ProviderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(pp => new { pp.ProviderId, pp.StartUtc })
                    .IsUnique();
            });

            builder.Entity<InternetCheck>(entity =>
            {
                entity.HasKey(ic => ic.Id);

                entity.Property(ic => ic.TargetUrl)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(ic => ic.ErrorMessage)
                    .HasMaxLength(4000);

                entity.HasOne(ic => ic.Provider)
                    .WithMany(p => p.Checks)
                    .HasForeignKey(ic => ic.ProviderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(ic => ic.CheckedAtUtc);
                entity.HasIndex(ic => new { ic.ProviderId, ic.CheckedAtUtc });
                entity.HasIndex(ic => new {ic.ProviderId, ic.IsSuccess, ic.CheckedAtUtc });
            });

            base.OnModelCreating(builder);
        }
    }
}
