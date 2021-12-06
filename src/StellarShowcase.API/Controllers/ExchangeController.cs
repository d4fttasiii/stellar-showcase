using Microsoft.AspNetCore.Mvc;
using StellarShowcase.Domain.Dto;
using StellarShowcase.Repositories.Interfaces;
using System;
using System.Collections.Generic;
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

        [HttpGet, Route("markets")]
        public async Task<ActionResult<List<MarketDto>>> GetMarkets()
        {
            var markets = await _exchangeRepository.GetMarkets();

            return Ok(markets);
        }

        [HttpPost, Route("markets")]
        public async Task<ActionResult<Guid>> CreateMarket([FromBody] CreateMarketDto data)
        {
            var marketId = await _exchangeRepository.CreateMarket(data.BaseAssetId, data.QuoteAssetId, data.Name);

            return Ok(marketId);
        }

        [HttpGet, Route("markets/{id}")]
        public async Task<ActionResult<List<MarketDto>>> GetMarket([FromRoute] Guid id)
        {
            var market = await _exchangeRepository.GetMarket(id);

            return Ok(market);
        }
    }
}
