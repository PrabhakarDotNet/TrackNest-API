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
    }
}
