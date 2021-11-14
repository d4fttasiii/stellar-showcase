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
    internal class IssuerRepository : IIssuerRepository
    {
        private readonly IAppDbContext _dbContext;
        private readonly IStellarClient _stellarClient;

        public IssuerRepository(IAppDbContext dbContext, IStellarClient stellarClient)
        {
            _dbContext = dbContext;
            _stellarClient = stellarClient;
        }

        public async Task<Guid> AddIssuer()
        {
            var issuer = new IssuerEntity
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Mnemonic = _stellarClient.GenerateMnemonic(),
            };
            //
            // Send 10.000 testnet XLM to both addresses
            //
            var issuerAccount = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 0);
            await _stellarClient.FundAccount(issuerAccount.AccountId);
            var distributorAccount = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 1);
            await _stellarClient.FundAccount(distributorAccount.AccountId);

            await _dbContext.Issuer.AddAsync(issuer);
            await _dbContext.Save();

            return issuer.Id;
        }

        public async Task<IEnumerable<IssuerEntity>> GetIssuers()
        {
            return await _dbContext.Issuer
                .Include(i => i.Assets)
                .ToListAsync();
        }

        public async Task<IssuerEntity> GetIssuer(Guid id)
        {
            return await _dbContext.Issuer
                .Include(i => i.Assets)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Guid> IssueAsset(Guid id, AssetDto asset)
        {
            var issuer = await GetIssuer(id);

            // Make sure these are funded on the stellar testnet
            var issuerKeyPair = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 0);
            var distributorPair = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 1);

            // Configure issuer wallet
            var rawTx = await _stellarClient.BuildConfigureIssuerWalletRawTransaction(issuerKeyPair.AccountId);
            _ = await _stellarClient.SignSubmitRawTransaction(issuerKeyPair.PrivateKey, rawTx);
            asset.IssuerAccountId = issuerKeyPair.AccountId;

            // Create trustline from distributor to issuer
            rawTx = await _stellarClient.BuildRawCreateTrustlineTransaction(asset, asset.TotalSupply, distributorPair.AccountId);
            _ = await _stellarClient.SignSubmitRawTransaction(distributorPair.PrivateKey, rawTx);

            // Authorize trusline
            rawTx = await _stellarClient.BuildRawAuthorizeTrustlineTransaction(asset, distributorPair.AccountId);
            _ = await _stellarClient.SignSubmitRawTransaction(issuerKeyPair.PrivateKey, rawTx);

            // Create asset
            rawTx = await _stellarClient.BuildRawCreateAssetTransaction(asset, distributorPair.AccountId);
            var txId = await _stellarClient.SignSubmitRawTransaction(issuerKeyPair.PrivateKey, rawTx);

            var assetEnt = new AssetEntity
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                IssuerId = issuer.Id,
                IssuerAccountId = issuerKeyPair.AccountId,
                TotalSupply = asset.TotalSupply,
                UnitName = asset.UnitName,
                CreatorTxId = txId,
            };

            await _dbContext.Asset.AddAsync(assetEnt);
            await _dbContext.Save();

            return assetEnt.Id;
        }

        public async Task AuthorizeTrustline(Guid id, Guid assetId, Guid userAccountId)
        {
            var issuer = await GetIssuer(id);
            var asset = issuer.Assets.FirstOrDefault(a => a.Id == assetId);
            var account = await _dbContext.UserAccount.FirstOrDefaultAsync(a => a.Id == userAccountId);

            var issuerKeyPair = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 0);
            var accountKeyPair = _stellarClient.DeriveKeyPair(account.Mnemonic, 0);

            var rawTx = await _stellarClient.BuildRawAuthorizeTrustlineTransaction(new AssetDto
            {
                IssuerAccountId = asset.IssuerAccountId,
                UnitName = asset.UnitName,
                TotalSupply = asset.TotalSupply,
            }, accountKeyPair.AccountId);

            _ = await _stellarClient.SignSubmitRawTransaction(issuerKeyPair.PrivateKey, rawTx);
        }

        public async Task RevokeTrustline(Guid id, Guid assetId, Guid userAccountId)
        {
            var issuer = await GetIssuer(id);
            var asset = issuer.Assets.FirstOrDefault(a => a.Id == assetId);
            var account = await _dbContext.UserAccount.FirstOrDefaultAsync(a => a.Id == userAccountId);

            var issuerKeyPair = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 0);
            var accountKeyPair = _stellarClient.DeriveKeyPair(account.Mnemonic, 0);

            var rawTx = await _stellarClient.BuildRawRevokeTrustlineTransaction(new AssetDto
            {
                IssuerAccountId = asset.IssuerAccountId,
                UnitName = asset.UnitName,
                TotalSupply = asset.TotalSupply,
            }, accountKeyPair.AccountId);

            _ = await _stellarClient.SignSubmitRawTransaction(issuerKeyPair.PrivateKey, rawTx);
        }

        public async Task<string> TransferAsset(Guid id, Guid assetId, IssuerTransferDto tx)
        {
            var issuer = await GetIssuer(id);
            var asset = issuer.Assets.FirstOrDefault(a => a.Id == assetId);
            var account = await _dbContext.UserAccount.FirstOrDefaultAsync(a => a.Id == tx.UserAccountId);

            var distributorKeyPair = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 1);
            var accountKeyPair = _stellarClient.DeriveKeyPair(account.Mnemonic, 0);

            var rawTx = await _stellarClient.BuildRawAssetTransaction(new AssetDto
            {
                IssuerAccountId = asset.IssuerAccountId,
                UnitName = asset.UnitName,
                TotalSupply = asset.TotalSupply,
            }, distributorKeyPair.AccountId, accountKeyPair.AccountId, tx.Amount, tx.Memo);

            return await _stellarClient.SignSubmitRawTransaction(distributorKeyPair.PrivateKey, rawTx);
        }
    }
}
