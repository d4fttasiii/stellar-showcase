using Microsoft.AspNetCore.Mvc;
using StellarShowcase.Domain.Dto;
using StellarShowcase.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace StellarShowcase.API.Controllers
{
    [Route("api/exchange")]
    [ApiController]
    public class ExchangeController : ControllerBase
    {
        private readonly IExchangeRepository _exchangeRepository;

        public ExchangeController(IExchangeRepository exchangeRepository)
        {
            _exchangeRepository = exchangeRepository;
        }

        /// <summary>
        /// Gets orderbook for asset pairs
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("{baseAssetId}/{quoteAssetId}")]
        public async Task<ActionResult<OrderBookDto>> GetOrderbook([FromRoute] Guid baseAssetId, [FromRoute] Guid quoteAssetId)
        {
            var orderBook = await _exchangeRepository.GetOrderBook(baseAssetId, quoteAssetId);

            return Ok(orderBook);
        }
    }
}
