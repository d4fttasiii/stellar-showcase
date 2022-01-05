using StellarShowcase.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StellarShowcase.Repositories.Interfaces
{
    public interface IExchangeRepository
    {
        Task<List<MarketDto>> GetMarkets();

        Task<MarketDto> GetMarket(Guid marketId);

        Task<Guid> CreateMarket(Guid baseAssetId, Guid quoteAssetId, string name);

        Task<List<LiquidityPoolDto>> GetLiquidityPools();

        Task<LiquidityPoolDto> GetLiquidityPool(Guid marketId);

        Task CreateLiquidityPool(Guid marketId, Guid issuerAccountId);
    }
}
