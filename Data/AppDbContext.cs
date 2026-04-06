using Microsoft.EntityFrameworkCore;
using Pomolog.Api.Models;

namespace Pomolog.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints if needed
            modelBuilder.Entity<TaskItem>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- DATA SEEDING ---
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1, // Syarat seeding: ID harus diisi secara eksplisit (manual)
                    Name = "Si Tester",
                    Email = "tester@pomolog.com",
                    PasswordHash = "rahasia",
                    // Catatan Edge Case: Gunakan tanggal statis agar EF Core tidak mengira data ini berubah setiap kita buat migrasi baru.
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}