using StellarShowcase.DataAccess.Blockchain;
using StellarShowcase.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarShowcase.Repositories.Interfaces
{
    public interface IExchangeRepository
    {
        Task<OrderBookDto> GetOrderBook(Guid baseAssetId, Guid quoteAssetId);
    }
}
