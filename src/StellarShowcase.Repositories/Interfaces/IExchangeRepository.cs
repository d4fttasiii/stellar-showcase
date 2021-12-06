using StellarShowcase.Domain.Dto;
using System;
using System.Threading.Tasks;

namespace StellarShowcase.Repositories.Interfaces
{
    public interface IExchangeRepository
    {
        Task<OrderBookDto> GetOrderBook(Guid baseAssetId, Guid quoteAssetId);
    }
}
