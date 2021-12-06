using StellarShowcase.Domain.Dto;
using System;
using System.Threading.Tasks;

namespace StellarShowcase.Repositories.Interfaces
{
    public interface IExchangeRepository
    {
        Task<OrderBookDto> GetOrderBook(Guid baseAssetId, Guid quoteAssetId);
        Task<string> CreateSellOrder(Guid userAccountId, Guid sellAssetId, Guid buyAssetId, decimal amount, decimal price);
        Task<string> CreateBuyOrder(Guid userAccountId, Guid buyAssetId, Guid sellAssetId, decimal amount, decimal price);
    }
}
