using Microsoft.EntityFrameworkCore;

namespace ElecShop.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
