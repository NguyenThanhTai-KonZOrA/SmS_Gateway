using Microsoft.EntityFrameworkCore;
using SmsGateway.Common.BaseEntity;
using SmsGateway.Implement.EntityModels;

namespace SmsGateway.Implement.ApplicationDbContext
{
    public class SmsGatewayDbContext : DbContext
    {
        public SmsGatewayDbContext(DbContextOptions<SmsGatewayDbContext> options) : base(options) { }

        public DbSet<SmsSendLog> SmsSendLogs => Set<SmsSendLog>();
        public DbSet<SmsDailyStatistic> SmsDailyStatistics => Set<SmsDailyStatistic>();
        public DbSet<OtpCode> OtpCodes => Set<OtpCode>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SmsSendLog>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.PhoneNumber).HasMaxLength(32).IsRequired();
                b.Property(x => x.Provider).HasMaxLength(32).IsRequired();
                b.Property(x => x.Message).HasMaxLength(1000);
                b.Property(x => x.ErrorCode).HasMaxLength(128);
                b.Property(x => x.ErrorMessage).HasMaxLength(1000);
                b.HasIndex(x => new { x.PhoneNumber, x.CreatedAtUtc });
            });

            modelBuilder.Entity<SmsDailyStatistic>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.PhoneNumber).HasMaxLength(32).IsRequired();
                b.Property(x => x.Day).HasColumnType("date");
                b.Property(x => x.RowVersion).IsRowVersion();
                b.HasIndex(x => new { x.PhoneNumber, x.Day }).IsUnique();
            });

            modelBuilder.Entity<OtpCode>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.PhoneNumber).HasMaxLength(32).IsRequired();
                b.Property(x => x.Code).HasMaxLength(12).IsRequired();
                b.Property(x => x.CreatedAt).HasPrecision(0);
                b.Property(x => x.ExpiresAtUtc).HasPrecision(0);
                b.Property(x => x.UsedAtUtc).HasPrecision(0);
                // Ensure only one unused OTP exists per phone to avoid collisions and simplify verification.
                b.HasIndex(x => new { x.PhoneNumber, x.IsUsed });
            });
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.IsActive = true;
                    entry.Entity.IsDelete = false;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChanges();
        }
    }
}
