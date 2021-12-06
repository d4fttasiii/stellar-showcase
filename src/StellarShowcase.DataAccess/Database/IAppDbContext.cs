using Microsoft.EntityFrameworkCore;
using StellarShowcase.Domain.Entities;
using System.Threading.Tasks;

namespace StellarShowcase.DataAccess.Database
{
    internal interface IAppDbContext
    {
        DbSet<UserAccountEntity> UserAccount { get; }
        DbSet<IssuerEntity> Issuer { get; }
        DbSet<AssetEntity> Asset { get; }
        DbSet<OrderEntity> Order { get; }
        DbSet<MarketEntity> Market { get; }

        Task Save();
    }
}
