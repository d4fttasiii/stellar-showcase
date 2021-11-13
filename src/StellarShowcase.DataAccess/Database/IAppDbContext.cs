using Microsoft.EntityFrameworkCore;
using StellarShowcase.Domain.Entities;
using System.Threading.Tasks;

namespace StellarShowcase.DataAccess.Database
{
    public interface IAppDbContext
    {
        DbSet<AccountEntity> Account { get; }
        DbSet<IssuerEntity> Issuer { get; }
        Task SaveChangesAsync();
    }
}
