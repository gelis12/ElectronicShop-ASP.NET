using ElecShop.Models;
using Microsoft.EntityFrameworkCore;

namespace ElecShop.Services
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
