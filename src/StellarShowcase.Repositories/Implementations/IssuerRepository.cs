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
            var issuer = EntityFactory.Create<IssuerEntity>(i =>
            {
                i.Mnemonic = _stellarClient.GenerateMnemonic();
            });
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

        public async Task<IEnumerable<IssuerDto>> GetIssuers()
        {
            var result = new List<IssuerDto>();
            var issuers = await _dbContext.Issuer
                .ToListAsync();

            foreach (var issuer in issuers)
            {
                var issuerAccount = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 0);
                var distributorAccount = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 1);

                result.Add(new IssuerDto
                {
                    Id = issuer.Id,
                    DistributorAccountId = distributorAccount.AccountId,
                    IssuerAccountId = issuerAccount.AccountId,
                });
            }

            return result;
        }

        public async Task<IssuerDto> GetIssuer(Guid id)
        {
            var issuer = await _dbContext.Issuer
                .Include(i => i.Assets)
                .FirstOrDefaultAsync(i => i.Id == id);

            var issuerAccount = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 0);
            var distributorAccount = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 1);

            return new IssuerDto
            {
                Id = issuer.Id,
                DistributorAccountId = distributorAccount.AccountId,
                IssuerAccountId = issuerAccount.AccountId,
                Assets = issuer.Assets.Select(a => new AssetDto
                {
                    Id = a.Id,
                    IssuerAccountId = issuerAccount.AccountId,
                    TotalSupply = a.TotalSupply,
                    UnitName = a.UnitName,
                }).ToList(),
                Distributor = await _stellarClient.GetAccount(distributorAccount.AccountId),
                Issuer = await _stellarClient.GetAccount(issuerAccount.AccountId),
            };
        }

        public async Task<Guid> IssueAsset(Guid id, UpsertAssetDto data)
        {
            var issuer = await _dbContext.Issuer.FirstOrDefaultAsync(i => i.Id == id);

            // Make sure these are funded on the stellar testnet
            var issuerKeyPair = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 0);
            var distributorPair = _stellarClient.DeriveKeyPair(issuer.Mnemonic, 1);
            var asset = new AssetDto
            {
                IssuerAccountId = issuerKeyPair.AccountId,
                UnitName = data.UnitName,
                TotalSupply = data.TotalSupply,
            };

            // Configure issuer wallet
            var rawTx = await _stellarClient.BuildConfigureIssuerWalletRawTransaction(issuerKeyPair.AccountId, data);
            if (!string.IsNullOrWhiteSpace(rawTx))
                await _stellarClient.SignSubmitRawTransaction(issuerKeyPair.PrivateKey, rawTx);

            // Create trustline from distributor to issuer
            rawTx = await _stellarClient.BuildRawCreateTrustlineTransaction(asset, asset.TotalSupply, distributorPair.AccountId);
            _ = await _stellarClient.SignSubmitRawTransaction(distributorPair.PrivateKey, rawTx);

            // Authorize trusline
            rawTx = await _stellarClient.BuildRawAuthorizeTrustlineTransaction(asset, distributorPair.AccountId);
            _ = await _stellarClient.SignSubmitRawTransaction(issuerKeyPair.PrivateKey, rawTx);

            // Create asset
            rawTx = await _stellarClient.BuildRawCreateAssetTransaction(asset, distributorPair.AccountId);
            var txId = await _stellarClient.SignSubmitRawTransaction(issuerKeyPair.PrivateKey, rawTx);

            var assetEnt = EntityFactory.Create<AssetEntity>(a =>
            {
                a.IssuerId = issuer.Id;
                a.IssuerAccountId = issuerKeyPair.AccountId;
                a.TotalSupply = asset.TotalSupply;
                a.UnitName = asset.UnitName;
                a.CreatorTxId = txId;
            });

            await _dbContext.Asset.AddAsync(assetEnt);
            await _dbContext.Save();

            return assetEnt.Id;
        }

        public async Task AuthorizeTrustline(Guid id, Guid assetId, Guid userAccountId)
        {
            var issuer = await _dbContext.Issuer.FirstOrDefaultAsync(i => i.Id == id);
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
            var issuer = await _dbContext.Issuer.FirstOrDefaultAsync(i => i.Id == id);
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
            var issuer = await _dbContext.Issuer.FirstOrDefaultAsync(i => i.Id == id);
            var asset =  await _dbContext.Asset.FirstOrDefaultAsync(a => a.Id == assetId);
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
