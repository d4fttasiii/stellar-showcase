using Microsoft.EntityFrameworkCore;
using StellarShowcase.DataAccess.Blockchain;
using StellarShowcase.DataAccess.Database;
using StellarShowcase.Domain.Dto;
using StellarShowcase.Repositories.Interfaces;
using System;
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

        public async Task<OrderBookDto> GetOrderBook(Guid baseAssetId, Guid quoteAssetId)
        {
            var baseAsset = await _dbContext.Asset.FirstOrDefaultAsync(a => a.Id == baseAssetId);
            var quoteAsset = await _dbContext.Asset.FirstOrDefaultAsync(a => a.Id == quoteAssetId);

            //var baseAsset = new { IssuerAccountId = "GC4MQOLFBOGBZ4GBNF7K7E56QUKNNX3BD4VREWMPDPHHUCABFNRS2A23", UnitName = "RIO" };
            //var quoteAsset = new { IssuerAccountId = "GC4MQOLFBOGBZ4GBNF7K7E56QUKNNX3BD4VREWMPDPHHUCABFNRS2A23", UnitName = "USD" };

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

        public async Task<string> CreateSellOrder(Guid userAccountId, Guid sellAssetId, Guid buyAssetId, decimal amount, decimal price)
        {
            var account = await _dbContext.UserAccount.FirstOrDefaultAsync(a => a.Id == userAccountId);
            var sellAsset = await _dbContext.Asset.FirstOrDefaultAsync(a => a.Id == sellAssetId);
            var buyAsset = await _dbContext.Asset.FirstOrDefaultAsync(a => a.Id == buyAssetId);

            var accountKeyPair = _stellarClient.DeriveKeyPair(account.Mnemonic, 0);
            var orderTx = await _stellarClient.CreateSellOrderRawTransaction(accountKeyPair.AccountId,
                new AssetDto { IssuerAccountId = sellAsset.IssuerAccountId, UnitName = sellAsset.UnitName },
                new AssetDto { IssuerAccountId = buyAsset.IssuerAccountId, UnitName = buyAsset.UnitName },
                amount, price);

            var txId = await _stellarClient.SignSubmitRawTransaction(accountKeyPair.PrivateKey, orderTx);

            return txId;
        }

        public async Task<string> CreateBuyOrder(Guid userAccountId, Guid buyAssetId, Guid sellAssetId, decimal amount, decimal price)
        {
            var account = await _dbContext.UserAccount.FirstOrDefaultAsync(a => a.Id == userAccountId);
            var buyAsset = await _dbContext.Asset.FirstOrDefaultAsync(a => a.Id == buyAssetId);
            var sellAsset = await _dbContext.Asset.FirstOrDefaultAsync(a => a.Id == sellAssetId);

            var accountKeyPair = _stellarClient.DeriveKeyPair(account.Mnemonic, 0);
            var orderTx = await _stellarClient.CreateBuyOrderRawTransaction(accountKeyPair.AccountId,
                new AssetDto { IssuerAccountId = sellAsset.IssuerAccountId, UnitName = sellAsset.UnitName },
                new AssetDto { IssuerAccountId = buyAsset.IssuerAccountId, UnitName = buyAsset.UnitName },
                amount, price);

            var txId = await _stellarClient.SignSubmitRawTransaction(accountKeyPair.PrivateKey, orderTx);

            return txId;
        }
    }
}
