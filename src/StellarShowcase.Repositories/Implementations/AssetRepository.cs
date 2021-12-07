using Microsoft.EntityFrameworkCore;
using StellarShowcase.DataAccess.Database;
using StellarShowcase.Domain.Dto;
using StellarShowcase.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StellarShowcase.Repositories.Implementations
{
    internal class AssetRepository : IAssetRepository
    {
        private readonly IAppDbContext _dbContext;

        public AssetRepository(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AssetDto>> GetAll()
        {
            var assets = await _dbContext.Asset
                .Select(a => new AssetDto
                {
                    Id = a.Id,
                    IssuerAccountId = a.IssuerAccountId,
                    IssuerId = a.IssuerId,
                    TotalSupply = a.TotalSupply,
                    UnitName = a.UnitName,
                })
                .ToListAsync();

            return assets;
        }
    }
}
