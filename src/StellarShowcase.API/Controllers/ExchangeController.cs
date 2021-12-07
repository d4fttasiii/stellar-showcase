using Microsoft.AspNetCore.Mvc;
using StellarShowcase.Domain.Dto;
using StellarShowcase.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StellarShowcase.API.Controllers
{
    [Route("api/exchange")]
    public class ExchangeController : BaseController
    {
        private readonly IExchangeRepository _exchangeRepository;

        public ExchangeController(IExchangeRepository exchangeRepository)
        {
            _exchangeRepository = exchangeRepository;
        }

        [HttpGet, Route("markets")]
        public async Task<ActionResult<List<MarketDto>>> GetMarkets()
        {
            return await HandleRequest(async () => await _exchangeRepository.GetMarkets());
        }

        [HttpPost, Route("markets")]
        public async Task<ActionResult<Guid>> CreateMarket([FromBody] CreateMarketDto data)
        {
            return await HandleRequest(async () => await _exchangeRepository.CreateMarket(data.BaseAssetId, data.QuoteAssetId, data.Name));
        }

        [HttpGet, Route("markets/{id}")]
        public async Task<ActionResult<MarketDto>> GetMarket([FromRoute] Guid id)
        {
            return await HandleRequest(async () => await _exchangeRepository.GetMarket(id));
        }
    }
}
