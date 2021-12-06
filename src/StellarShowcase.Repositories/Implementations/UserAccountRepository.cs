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

        public async Task<Guid> AddUserAccount(UserAccountDto userDto)
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

        public async Task<IEnumerable<UserAccountEntity>> GetUserAccounts()
        {
            return await _dbContext.UserAccount
                .Include(u => u.Orders)
                .ToListAsync();
        }

        public async Task<UserAccountEntity> GetUserAccount(Guid id)
        {
            return await _dbContext.UserAccount.FirstOrDefaultAsync(i => i.Id == id);
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

        public async Task<string> CreateSellOrder(Guid userAccountId, Guid marketId, decimal volume, decimal price)
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

            return txId;
        }

        public async Task<string> CreateBuyOrder(Guid userAccountId, Guid marketId, decimal volume, decimal price)
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

            return txId;
        }
    }
}

