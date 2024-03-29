﻿using Microsoft.EntityFrameworkCore;
using StellarShowcase.DataAccess.Blockchain;
using StellarShowcase.DataAccess.Database;
using StellarShowcase.Domain.Dto;
using StellarShowcase.Domain.Entities;
using StellarShowcase.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StellarShowcase.Repositories.Implementations
{
    internal class ExchangeRepository : IExchangeRepository
    {
        private readonly IAppDbContext _dbContext;
        private readonly IStellarClient _stellarClient;

        public ExchangeRepository(IAppDbContext dbContext, IStellarClient stellarClient)
        {
            _dbContext = dbContext;
            _stellarClient = stellarClient;
        }

        public async Task<List<MarketDto>> GetMarkets()
        {
            var markets = await (from m in _dbContext.Market
                                 join b in _dbContext.Asset on m.BaseAssetId equals b.Id
                                 join q in _dbContext.Asset on m.QuoteAssetId equals q.Id
                                 select new MarketDto
                                 {
                                     Id = m.Id,
                                     Name = m.Name,
                                     Base = new AssetDto
                                     {
                                         IssuerAccountId = b.IssuerAccountId,
                                         TotalSupply = b.TotalSupply,
                                         UnitName = b.UnitName,
                                     },
                                     Quote = new AssetDto
                                     {
                                         IssuerAccountId = q.IssuerAccountId,
                                         TotalSupply = q.TotalSupply,
                                         UnitName = q.UnitName,
                                     },
                                 }).ToListAsync();

            foreach (var m in markets)
            {
                var orderbook = await _stellarClient.GetOrderBook(m.Base, m.Quote);
                m.Price = orderbook.Sells.Any() ? orderbook.Sells.Min(s => s.Price) : 0m;
                m.LiquidityPool = await GetLiquidityPool(m.Id);
            }

            return markets;
        }

        public async Task<MarketDto> GetMarket(Guid marketId)
        {
            var market = await GetMarketDto(marketId);
            market.OrderBooks = await GetOrderBook(market.Base, market.Quote);
            market.LiquidityPool = await GetLiquidityPool(market.Id);

            return market;
        }

        public async Task<Guid> CreateMarket(Guid baseAssetId, Guid quoteAssetId, string name)
        {
            if (baseAssetId == quoteAssetId)
                throw new ArgumentNullException("Same assets selected!");

            if (await _dbContext.Market.AnyAsync(m => m.Name == name ||
                (m.BaseAssetId == baseAssetId && m.QuoteAssetId == quoteAssetId) ||
                (m.BaseAssetId == quoteAssetId && m.QuoteAssetId == baseAssetId)))
            {
                throw new ArgumentException($"Market {name} already exists");
            }

            var market = EntityFactory.Create<MarketEntity>(m =>
            {
                m.Name = name;
                m.BaseAssetId = baseAssetId;
                m.QuoteAssetId = quoteAssetId;
            });

            await _dbContext.Market.AddAsync(market);
            await _dbContext.Save();

            return market.Id;
        }

        public async Task<List<LiquidityPoolDto>> GetLiquidityPools()
        {
            var markets = await(from m in _dbContext.Market
                               join b in _dbContext.Asset on m.BaseAssetId equals b.Id
                               join q in _dbContext.Asset on m.QuoteAssetId equals q.Id
                               select new MarketDto
                               {
                                   Id = m.Id,
                                   Name = m.Name,
                                   Base = new AssetDto
                                   {
                                       IssuerAccountId = b.IssuerAccountId,
                                       TotalSupply = b.TotalSupply,
                                       UnitName = b.UnitName,
                                   },
                                   Quote = new AssetDto
                                   {
                                       IssuerAccountId = q.IssuerAccountId,
                                       TotalSupply = q.TotalSupply,
                                       UnitName = q.UnitName,
                                   },
                               }).ToListAsync();

            var pools = new List<LiquidityPoolDto>();

            foreach (var market in markets)
            {
                var liquidityPool = await _stellarClient.GetLiquidityPool(market.Base, market.Quote);
                pools.Add(liquidityPool);
            }

            return pools;
        }

        public async Task<LiquidityPoolDto> GetLiquidityPool(Guid marketId)
        {
            var market = await GetMarketDto(marketId);
            var liquidityPool = await _stellarClient.GetLiquidityPool(market.Base, market.Quote);

            return liquidityPool;
        }

        private async Task<OrderBookDto> GetOrderBook(AssetDto baseAsset, AssetDto quoteAsset)
        {
            var orderBook = await _stellarClient.GetOrderBook(new AssetDto
            {
                IssuerAccountId = baseAsset.IssuerAccountId,
                UnitName = baseAsset.UnitName,
            }, new AssetDto
            {
                IssuerAccountId = quoteAsset.IssuerAccountId,
                UnitName = quoteAsset.UnitName,
            });

            return orderBook;
        }

        private async Task<MarketDto> GetMarketDto(Guid marketId)
        {
            return await (from m in _dbContext.Market
                          join b in _dbContext.Asset on m.BaseAssetId equals b.Id
                          join q in _dbContext.Asset on m.QuoteAssetId equals q.Id
                          where m.Id == marketId
                          select new MarketDto
                          {
                              Id = m.Id,
                              Name = m.Name,
                              Base = new AssetDto
                              {
                                  IssuerAccountId = b.IssuerAccountId,
                                  TotalSupply = b.TotalSupply,
                                  UnitName = b.UnitName,
                              },
                              Quote = new AssetDto
                              {
                                  IssuerAccountId = q.IssuerAccountId,
                                  TotalSupply = q.TotalSupply,
                                  UnitName = q.UnitName,
                              },
                          }).FirstOrDefaultAsync();
        }
    }
}
