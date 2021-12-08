using Microsoft.EntityFrameworkCore;
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
    internal class UserAccountRepository : IUserAccountRepository
    {
        private readonly IAppDbContext _dbContext;
        private readonly IStellarClient _stellarClient;

        public UserAccountRepository(IAppDbContext dbContext, IStellarClient stellarClient)
        {
            _dbContext = dbContext;
            _stellarClient = stellarClient;
        }

        public async Task<Guid> AddUserAccount(CreateUserAccountDto userDto)
        {
            var userAccount = EntityFactory.Create<UserAccountEntity>(ua =>
            {
                ua.Mnemonic = _stellarClient.GenerateMnemonic();
                ua.FullName = userDto.FullName;
                ua.Email = userDto.Email;
                ua.FullAddress = userDto.FullAddress;
                ua.Phone = userDto.Phone;
            });

            //
            // Send 10.000 testnet XLM to addresses
            //
            var account = _stellarClient.DeriveKeyPair(userAccount.Mnemonic, 0);
            await _stellarClient.FundAccount(account.AccountId);

            await _dbContext.UserAccount.AddAsync(userAccount);
            await _dbContext.Save();

            return userAccount.Id;
        }

        public async Task<IEnumerable<UserAccountDto>> GetUserAccounts()
        {
            return await _dbContext.UserAccount
                .Select(u => new UserAccountDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Phone = u.Phone,
                    FullName = u.FullName,
                    FullAddress = u.FullAddress,
                })
                .ToListAsync();
        }

        public async Task<UserAccountDto> GetUserAccount(Guid id)
        {
            var userAccount = await _dbContext.UserAccount
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(i => i.Id == id);

            var accountKeyPair = _stellarClient.DeriveKeyPair(userAccount.Mnemonic, 0);
            var account = await _stellarClient.GetAccount(accountKeyPair.AccountId);

            return new UserAccountDto
            {
                Id = userAccount.Id,
                Email = userAccount.Email,
                FullAddress = userAccount.FullAddress,
                Phone = userAccount.Phone,
                FullName = userAccount.FullName,
                Account = account,
            };
        }

        public async Task CreateTrustline(Guid id, Guid issuerId, Guid assetId, decimal? limit = null)
        {
            var account = await _dbContext.UserAccount.FirstOrDefaultAsync(a => a.Id == id);
            var issuer = await _dbContext.Issuer
                .Include(i => i.Assets)
                .FirstOrDefaultAsync(i => i.Id == issuerId);
            var asset = issuer.Assets.FirstOrDefault(a => a.Id == assetId);

            var issuerKeyPair = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 0);
            var accountKeyPair = _stellarClient.DeriveKeyPair(account.Mnemonic, 0);

            var rawTx = await _stellarClient.BuildRawCreateTrustlineTransaction(new AssetDto
            {
                IssuerAccountId = asset.IssuerAccountId,
                UnitName = asset.UnitName,
                TotalSupply = asset.TotalSupply,
            }, limit ?? asset.TotalSupply, accountKeyPair.AccountId);

            _ = await _stellarClient.SignSubmitRawTransaction(accountKeyPair.PrivateKey, rawTx);
        }

        public async Task TransferAsset(Guid id, Guid issuerId, Guid assetId, string recipientAccountId, decimal amount, string memo = "")
        {
            var account = await _dbContext.UserAccount.FirstOrDefaultAsync(a => a.Id == id);
            var issuer = await _dbContext.Issuer
               .Include(i => i.Assets)
               .FirstOrDefaultAsync(i => i.Id == issuerId);
            var asset = issuer.Assets.FirstOrDefault(a => a.Id == assetId);

            var accountKeyPair = _stellarClient.DeriveKeyPair(account.Mnemonic, 0);

            var rawTx = await _stellarClient.BuildRawAssetTransaction(new AssetDto
            {
                IssuerAccountId = asset.IssuerAccountId,
                UnitName = asset.UnitName,
                TotalSupply = asset.TotalSupply,
            }, accountKeyPair.AccountId, recipientAccountId, amount, memo);

            _ = await _stellarClient.SignSubmitRawTransaction(accountKeyPair.PrivateKey, rawTx);
        }

        public async Task<Guid> CreateSellOrder(Guid userAccountId, Guid marketId, decimal volume, decimal price)
        {
            var account = await _dbContext.UserAccount.FirstOrDefaultAsync(a => a.Id == userAccountId);
            var market = await _dbContext.Market.FirstOrDefaultAsync(m => m.Id == marketId);
            var sellAsset = await _dbContext.Asset.FirstOrDefaultAsync(a => a.Id == market.BaseAssetId);
            var buyAsset = await _dbContext.Asset.FirstOrDefaultAsync(a => a.Id == market.QuoteAssetId);

            var accountKeyPair = _stellarClient.DeriveKeyPair(account.Mnemonic, 0);
            var orderTx = await _stellarClient.CreateSellOrderRawTransaction(accountKeyPair.AccountId,
                new AssetDto { IssuerAccountId = sellAsset.IssuerAccountId, UnitName = sellAsset.UnitName },
                new AssetDto { IssuerAccountId = buyAsset.IssuerAccountId, UnitName = buyAsset.UnitName },
                volume, price);

            var txId = await _stellarClient.SignSubmitRawTransaction(accountKeyPair.PrivateKey, orderTx);

            var order = EntityFactory.Create<OrderEntity>(o =>
            {
                o.UserAccountId = userAccountId;
                o.MarketId = marketId;
                o.Price = price;
                o.Volume = volume;
                o.TxId = txId;
            });

            await _dbContext.Order.AddAsync(order);
            await _dbContext.Save();

            return order.Id;
        }

        public async Task<Guid> CreateBuyOrder(Guid userAccountId, Guid marketId, decimal volume, decimal price)
        {
            var account = await _dbContext.UserAccount.FirstOrDefaultAsync(a => a.Id == userAccountId);
            var market = await _dbContext.Market.FirstOrDefaultAsync(m => m.Id == marketId);
            var buyAsset = await _dbContext.Asset.FirstOrDefaultAsync(a => a.Id == market.BaseAssetId);
            var sellAsset = await _dbContext.Asset.FirstOrDefaultAsync(a => a.Id == market.QuoteAssetId);

            var accountKeyPair = _stellarClient.DeriveKeyPair(account.Mnemonic, 0);
            var orderTx = await _stellarClient.CreateBuyOrderRawTransaction(accountKeyPair.AccountId,
                new AssetDto { IssuerAccountId = sellAsset.IssuerAccountId, UnitName = sellAsset.UnitName },
                new AssetDto { IssuerAccountId = buyAsset.IssuerAccountId, UnitName = buyAsset.UnitName },
                volume, price);

            var txId = await _stellarClient.SignSubmitRawTransaction(accountKeyPair.PrivateKey, orderTx);

            var order = EntityFactory.Create<OrderEntity>(o =>
            {
                o.UserAccountId = userAccountId;
                o.MarketId = marketId;
                o.Price = price;
                o.Volume = volume;
                o.TxId = txId;
            });

            await _dbContext.Order.AddAsync(order);
            await _dbContext.Save();

            return order.Id;
        }

        public async Task<List<ActiveOrderDto>> GetActiveOrders(Guid userAccountId)
        {
            var account = await _dbContext.UserAccount.FirstOrDefaultAsync(a => a.Id == userAccountId);
            var accountKeyPair = _stellarClient.DeriveKeyPair(account.Mnemonic, 0);

            return await GetOrders(accountKeyPair.AccountId);
        }

        public async Task<bool> CancelOrder(Guid userAccountId, long orderId)
        {
            var account = await _dbContext.UserAccount.FirstOrDefaultAsync(a => a.Id == userAccountId);
            var accountKeyPair = _stellarClient.DeriveKeyPair(account.Mnemonic, 0);
            var orders = await GetOrders(accountKeyPair.AccountId);

            var orderToCancel = orders.FirstOrDefault(o => o.Id == orderId) ?? 
                throw new ArgumentException($"Unable to find order: ${orderId}");

            var cancelOrderTx = await IsBuy(orderToCancel) ? 
                await _stellarClient.CreateCancelBuyOrderRawTransaction(accountKeyPair.AccountId, orderId, orderToCancel.Selling, orderToCancel.Buying) :
                await _stellarClient.CreateCancelSellOrderRawTransaction(accountKeyPair.AccountId, orderId, orderToCancel.Selling, orderToCancel.Buying);

            var txId = await _stellarClient.SignSubmitRawTransaction(accountKeyPair.PrivateKey, cancelOrderTx);

            return true;
        }

        private async Task<bool> IsBuy(ActiveOrderDto order)
        {
            var market = await (from m in _dbContext.Market
                                join b in _dbContext.Asset on m.BaseAssetId equals b.Id
                                join q in _dbContext.Asset on m.QuoteAssetId equals q.Id
                                where m.Id == order.MarketId
                                select new MarketDto
                                {
                                    Base = new AssetDto
                                    {
                                        IssuerAccountId = b.IssuerAccountId,
                                        UnitName = b.UnitName,
                                    },
                                    Quote = new AssetDto
                                    {
                                        IssuerAccountId = q.IssuerAccountId,
                                        UnitName = q.UnitName,
                                    },
                                }).FirstOrDefaultAsync();

            return market.Base.IssuerAccountId == order.Buying.IssuerAccountId && 
                   market.Base.UnitName == order.Buying.UnitName &&
                   market.Quote.IssuerAccountId == order.Selling.IssuerAccountId &&
                   market.Quote.UnitName == order.Selling.UnitName;
        }

        private async Task<List<ActiveOrderDto>> GetOrders(string accountId)
        {
            var orders = await _stellarClient.GetAccountOrders(accountId);
            var markets = await (from m in _dbContext.Market
                                 join b in _dbContext.Asset on m.BaseAssetId equals b.Id
                                 join q in _dbContext.Asset on m.QuoteAssetId equals q.Id
                                 select new MarketDto
                                 {
                                     Id = m.Id,
                                     Base = new AssetDto
                                     {
                                         IssuerAccountId = b.IssuerAccountId,
                                         UnitName = b.UnitName,
                                     },
                                     Quote = new AssetDto
                                     {
                                         IssuerAccountId = q.IssuerAccountId,
                                         UnitName = q.UnitName,
                                     },
                                 }).ToListAsync();

            foreach (var order in orders)
            {
                order.MarketId = markets.FirstOrDefault(m =>
                   (m.Base.IssuerAccountId == order.Buying.IssuerAccountId && m.Base.UnitName == order.Buying.UnitName &&
                   m.Quote.IssuerAccountId == order.Selling.IssuerAccountId && m.Quote.UnitName == order.Selling.UnitName) ||
                   (m.Base.IssuerAccountId == order.Selling.IssuerAccountId && m.Base.UnitName == order.Selling.UnitName &&
                   m.Quote.IssuerAccountId == order.Buying.IssuerAccountId && m.Quote.UnitName == order.Buying.UnitName))?.Id;
            }

            return orders;
        }
    }
}

