using Microsoft.EntityFrameworkCore;
using StellarShowcase.Domain.Entities;
using System.Threading.Tasks;

namespace StellarShowcase.DataAccess.Database
{
    internal class AppDbContext : DbContext, IAppDbContext
    {
        public DbSet<AccountEntity> Account { get; set; }
        public DbSet<IssuerEntity> Issuer { get; set; }

        public AppDbContext(DbContextOptions options) : base(options) { }

        public async Task SaveChangesAsync() => await SaveChangesAsync();
    }
}
