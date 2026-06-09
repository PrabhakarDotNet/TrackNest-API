using Microsoft.EntityFrameworkCore;
using TrackNest.Domain.Entities;

namespace TrackNest.Infrastructure.Persistence
{
    public class TrackNestDbContext : DbContext
    {
        public TrackNestDbContext(DbContextOptions<TrackNestDbContext> options)
            : base(options)
        {
        }

        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Expense>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);
        }
    }
}