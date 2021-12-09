using Microsoft.EntityFrameworkCore;
using StellarShowcase.Domain.Entities;
using System.Threading.Tasks;

namespace StellarShowcase.DataAccess.Database
{
    internal class AppDbContext : DbContext, IAppDbContext
    {
        public DbSet<UserAccountEntity> UserAccount { get; set; }
        public DbSet<IssuerEntity> Issuer { get; set; }
        public DbSet<AssetEntity> Asset { get; set; }
        public DbSet<MarketEntity> Market { get; set; }

        public AppDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        public async Task Save()
        {
            await base.SaveChangesAsync();
        }
    }
}
