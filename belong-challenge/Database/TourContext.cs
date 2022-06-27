using belong_challenge.Models;
using Microsoft.EntityFrameworkCore;

namespace belong_challenge.Database
{
    public class TourContext : DbContext
    {
        public TourContext(DbContextOptions<TourContext> options) : base(options)
        {
        }

        public DbSet<Tour> Tours { get; set; } = null!;
    }
}
