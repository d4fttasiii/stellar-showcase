using Microsoft.Extensions.Options;
using stellar_dotnet_sdk;
using StellarShowcase.Domain;
using StellarShowcase.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StellarShowcase.DataAccess.Blockchain
{
    internal class StellarClient : IStellarClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Settings _settings;

        public StellarClient(IHttpClientFactory httpClientFactory, IOptions<Settings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;

            Network.UseTestNetwork();
        }

        public string GenerateMnemonic()
        {
            var mnemonic = new NBitcoin.Mnemonic(NBitcoin.Wordlist.English);

            return mnemonic.ToString();
        }

        public KeyPairDto DeriveKeyPair(string mnemonic, int accountIndex)
        {
            var m = new NBitcoin.Mnemonic(mnemonic, NBitcoin.Wordlist.English);
            var kp = KeyPair.FromBIP39Seed(m.DeriveSeed(), Convert.ToUInt32(accountIndex));

            return new KeyPairDto
            {
                PrivateKey = kp.SecretSeed,
                AccountId = kp.AccountId,
            };
        }

        public async Task FundAccount(string accountId)
        {
            var url = $"https://friendbot.stellar.org/?addr={accountId}";
            using var httpClient = _httpClientFactory.CreateClient();

            var result = await httpClient.GetAsync(url);

            if (!result.IsSuccessStatusCode)
                throw new Exception("Something went wrong, unable to fund account!");
        }

        public async Task<AccountDto> GetAccount(string accountId)
        {
            using var server = GetServer();

            var account = await server.Accounts.Account(accountId);
            var stellarBalance = account.Balances.FirstOrDefault(b => b.AssetType == "native");
            var nonNativeBalances = account.Balances
                .Where(b => b.AssetType.StartsWith("credit_alphanum"))
                .Select(b => new Balance
                {
                    AssetCode = b.AssetCode,
                    AssetIssuer = b.AssetIssuer,
                    AssetType = b.AssetType,
                    Amount = string.IsNullOrEmpty(b.BalanceString) ? 0m : decimal.Parse(b.BalanceString, NumberStyles.Any, CultureInfo.InvariantCulture),
                })
                .ToList();

            return new AccountDto
            {
                AccountId = account.AccountId,
                SequenceNumber = account.SequenceNumber,
                IsAuthRequired = account.Flags.AuthRequired,
                IsAuthRevocable = account.Flags.AuthRevocable,
                IsAuthImmutable = account.Flags.AuthImmutable,
                IsClawbackEnabled = account.Flags.AuthClawback,
                NativeBalance = string.IsNullOrEmpty(stellarBalance?.BalanceString) ? 0m : decimal.Parse(stellarBalance.BalanceString, NumberStyles.Any, CultureInfo.InvariantCulture),
                NonNativeBalances = nonNativeBalances,
            };
        }

        public async Task<string> BuildConfigureIssuerWalletRawTransaction(string issuerAccountId, UpsertAssetDto config)
        {
            using var server = GetServer();
            var issuerAccount = await server.Accounts.Account(issuerAccountId);

            if (config.IsAuthRequired == issuerAccount.Flags.AuthRequired &&
                config.IsAuthRevocable == issuerAccount.Flags.AuthRevocable &&
                config.IsAuthImmutable == issuerAccount.Flags.AuthImmutable &&
                config.IsClawbackEnabled == issuerAccount.Flags.AuthClawback)
                return string.Empty;

            var flags =
                (config.IsAuthRequired ? (uint)stellar_dotnet_sdk.xdr.AccountFlags.AccountFlagsEnum.AUTH_REQUIRED_FLAG : 0) +
                (config.IsAuthRevocable ? (uint)stellar_dotnet_sdk.xdr.AccountFlags.AccountFlagsEnum.AUTH_REVOCABLE_FLAG : 0) +
                (config.IsAuthImmutable ? (uint)stellar_dotnet_sdk.xdr.AccountFlags.AccountFlagsEnum.AUTH_IMMUTABLE_FLAG : 0) +
                (config.IsClawbackEnabled ? (uint)stellar_dotnet_sdk.xdr.AccountFlags.AccountFlagsEnum.AUTH_CLAWBACK_ENABLED_FLAG : 0);

            var setOp = new SetOptionsOperation.Builder()
                .SetSetFlags(flags)
                .Build();

            var txBuilder = new TransactionBuilder(issuerAccount);
            var tx = txBuilder
                .AddOperation(setOp)
                .AddTimeBounds(GetTimeBounds())
                .Build();

            return tx.ToUnsignedEnvelopeXdrBase64();
        }

        public async Task<string> BuildRawCreateAssetTransaction(AssetDto asset, string distributorAccountId)
        {
            using var server = GetServer();
            var issuingAccount = await server.Accounts.Account(asset.IssuerAccountId);
            var stellarAsset = Asset.CreateNonNativeAsset(asset.UnitName, issuingAccount.AccountId);
            var createTokenTxBuilder = new TransactionBuilder(issuingAccount);
            var adjustedAssetAmount = asset.TotalSupply.ToString("N7", CultureInfo.InvariantCulture);

            // Second, the issuing account actually sends a payment using the assetf
            var issueAssetOp = new PaymentOperation.Builder(KeyPair.FromAccountId(distributorAccountId), stellarAsset, adjustedAssetAmount)
                .Build();

            var tx = createTokenTxBuilder
                .AddOperation(issueAssetOp)
                .AddTimeBounds(GetTimeBounds())
                .Build();

            return tx.ToUnsignedEnvelopeXdrBase64();
        }

        public async Task<string> BuildRawCreateTrustlineTransaction(AssetDto asset, decimal limit, string trustorAccountId)
        {
            using var server = GetServer();
            var trustorAccount = await server.Accounts.Account(trustorAccountId);
            var stellarAsset = ChangeTrustAsset.CreateNonNativeAsset(asset.UnitName, asset.IssuerAccountId);
            var txBuilder = new TransactionBuilder(trustorAccount);
            var adjustedLimit = limit.ToString("N7", CultureInfo.InvariantCulture);

            // Create trustline from optIn account to issuer account
            var changeTrustOp = new ChangeTrustOperation.Builder(stellarAsset, adjustedLimit)
                .SetSourceAccount(trustorAccount.KeyPair)
                .Build();

            return txBuilder
               .AddOperation(changeTrustOp)
               .AddTimeBounds(GetTimeBounds())
               .Build()
               .ToUnsignedEnvelopeXdrBase64();
        }

        public async Task<string> BuildRawAuthorizeTrustlineTransaction(AssetDto asset, string trustorAccountId)
        {
            using var server = GetServer();
            var trustorAccount = KeyPair.FromAccountId(trustorAccountId);
            var issuingAccount = await server.Accounts.Account(asset.IssuerAccountId);
            var stellarAsset = Asset.CreateNonNativeAsset(asset.UnitName, issuingAccount.AccountId);

            var txBuilder = new TransactionBuilder(issuingAccount);

            // authorize trustline from issuer account to opt in account
            var allowTrustOp = new SetTrustlineFlagsOperation.Builder(stellarAsset, trustorAccount,
                (uint)stellar_dotnet_sdk.xdr.TrustLineFlags.TrustLineFlagsEnum.AUTHORIZED_FLAG,
                (uint)stellar_dotnet_sdk.xdr.TrustLineFlags.TrustLineFlagsEnum.AUTHORIZED_TO_MAINTAIN_LIABILITIES_FLAG)
                .SetSourceAccount(issuingAccount.KeyPair)
                .Build();

            return txBuilder
               .AddOperation(allowTrustOp)
               .AddTimeBounds(GetTimeBounds())
               .Build()
               .ToUnsignedEnvelopeXdrBase64();
        }

        public async Task<string> BuildRawRevokeTrustlineTransaction(AssetDto asset, string trustorAccountId)
        {
            using var server = GetServer();
            var trustorAccount = KeyPair.FromAccountId(trustorAccountId);
            var issuer = await server.Accounts.Account(asset.IssuerAccountId);

            var stellarAsset = Asset.CreateNonNativeAsset(asset.UnitName, issuer.AccountId);
            var txBuilder = new TransactionBuilder(issuer);

            // authorize trustline from issuer account to freeze asset for an account
            var freezeAssetOp = new SetTrustlineFlagsOperation.Builder(stellarAsset, trustorAccount,
                    (uint)stellar_dotnet_sdk.xdr.TrustLineFlags.TrustLineFlagsEnum.AUTHORIZED_TO_MAINTAIN_LIABILITIES_FLAG,
                    (uint)stellar_dotnet_sdk.xdr.TrustLineFlags.TrustLineFlagsEnum.AUTHORIZED_FLAG)
                .SetSourceAccount(issuer.KeyPair)
                .Build();

            return txBuilder
               .AddOperation(freezeAssetOp)
               .AddTimeBounds(GetTimeBounds())
               .Build()
               .ToUnsignedEnvelopeXdrBase64();
        }

        public async Task<string> BuildRawAssetTransaction(AssetDto asset, string senderAccountId, string recipientAccountId, decimal amount, string memo = "")
        {
            using var server = GetServer();
            var senderAccount = await server.Accounts.Account(senderAccountId);
            var customAsset = Asset.CreateNonNativeAsset(asset.UnitName, asset.IssuerAccountId);
            var sendTokenTxBuilder = new TransactionBuilder(senderAccount);
            var adjustAssetAmount = amount.ToString("N7", CultureInfo.InvariantCulture);

            sendTokenTxBuilder.AddOperation(
                new PaymentOperation.Builder(KeyPair.FromAccountId(recipientAccountId), customAsset, adjustAssetAmount).Build());

            if (!string.IsNullOrEmpty(memo))
            {
                sendTokenTxBuilder.AddMemo(Memo.Text(memo));
            }

            return sendTokenTxBuilder
                .AddTimeBounds(GetTimeBounds())
                .Build()
                .ToUnsignedEnvelopeXdrBase64();
        }

        public async Task<string> BuildClawbackRawTransaction(AssetDto asset, decimal amount, string fromAccountId)
        {
            using var server = GetServer();
            var issuer = await server.Accounts.Account(asset.IssuerAccountId);
            var stellarAsset = Asset.CreateNonNativeAsset(asset.UnitName, issuer.AccountId);
            var adjustAssetAmount = amount.ToString("N7", CultureInfo.InvariantCulture);

            var clawbackOp = new ClawbackOperation.Builder(stellarAsset, adjustAssetAmount, KeyPair.FromAccountId(fromAccountId))
                .SetSourceAccount(issuer.KeyPair)
                .Build();

            return new TransactionBuilder(issuer)
               .AddOperation(clawbackOp)
               .AddTimeBounds(GetTimeBounds())
               .Build()
               .ToUnsignedEnvelopeXdrBase64();
        }

        public async Task<string> SignSubmitRawTransaction(string privateKey, string txRaw)
        {
            var tx = Transaction.FromEnvelopeXdr(txRaw);
            var pk = KeyPair.FromSecretSeed(privateKey);
            tx.Sign(pk);

            using var server = GetServer();
            var result = await server.SubmitTransaction(tx, new SubmitTransactionOptions
            {
                EnsureSuccess = true,
            });

            return result.Hash;
        }

        public async Task<string> BuildBuyOrderRawTransaction(string accountId, AssetDto sellingAsset, AssetDto buyingAsset, decimal amount, decimal price)
        {
            using var server = GetServer();

            var account = await server.Accounts.Account(accountId);

            var adjustAssetAmount = amount.ToString("N7", CultureInfo.InvariantCulture);
            var adjustedPrice = price.ToString("N7", CultureInfo.InvariantCulture);

            var sell = Asset.CreateNonNativeAsset(sellingAsset.UnitName, sellingAsset.IssuerAccountId);
            var buy = Asset.CreateNonNativeAsset(buyingAsset.UnitName, buyingAsset.IssuerAccountId);

            var buyOfferOp = new ManageBuyOfferOperation.Builder(sell, buy, adjustAssetAmount, adjustedPrice)
                .SetSourceAccount(account.KeyPair)
                .Build();

            return new TransactionBuilder(account)
               .AddOperation(buyOfferOp)
               .AddTimeBounds(GetTimeBounds())
               .SetFee(1_000_000)
               .Build()
               .ToUnsignedEnvelopeXdrBase64();
        }

        public async Task<string> BuildCancelBuyOrderRawTransaction(string accountId, long orderId, AssetDto sellingAsset, AssetDto buyingAsset)
        {
            using var server = GetServer();

            var account = await server.Accounts.Account(accountId);
            var sell = Asset.CreateNonNativeAsset(sellingAsset.UnitName, sellingAsset.IssuerAccountId);
            var buy = Asset.CreateNonNativeAsset(buyingAsset.UnitName, buyingAsset.IssuerAccountId);

            var sellOfferOp = new ManageBuyOfferOperation.Builder(sell, buy, "0", "1")
                .SetSourceAccount(account.KeyPair)
                .SetOfferId(orderId)
                .Build();

            return new TransactionBuilder(account)
               .AddOperation(sellOfferOp)
               .AddTimeBounds(GetTimeBounds())
               .SetFee(1_000_000)
               .Build()
               .ToUnsignedEnvelopeXdrBase64();
        }

        public async Task<string> BuildSellOrderRawTransaction(string accountId, AssetDto sellingAsset, AssetDto buyingAsset, decimal amount, decimal price)
        {
            using var server = GetServer();

            var account = await server.Accounts.Account(accountId);

            var adjustAssetAmount = amount.ToString("N7", CultureInfo.InvariantCulture);
            var adjustedPrice = price.ToString("N7", CultureInfo.InvariantCulture);

            var sell = Asset.CreateNonNativeAsset(sellingAsset.UnitName, sellingAsset.IssuerAccountId);
            var buy = Asset.CreateNonNativeAsset(buyingAsset.UnitName, buyingAsset.IssuerAccountId);

            var sellOfferOp = new ManageSellOfferOperation.Builder(sell, buy, adjustAssetAmount, adjustedPrice)
                .SetSourceAccount(account.KeyPair)
                .Build();

            return new TransactionBuilder(account)
               .AddOperation(sellOfferOp)
               .AddTimeBounds(GetTimeBounds())
               .SetFee(1_000_000)
               .Build()
               .ToUnsignedEnvelopeXdrBase64();
        }

        public async Task<string> BuildCancelSellOrderRawTransaction(string accountId, long orderId, AssetDto sellingAsset, AssetDto buyingAsset)
        {
            using var server = GetServer();

            var account = await server.Accounts.Account(accountId);
            var sell = Asset.CreateNonNativeAsset(sellingAsset.UnitName, sellingAsset.IssuerAccountId);
            var buy = Asset.CreateNonNativeAsset(buyingAsset.UnitName, buyingAsset.IssuerAccountId);

            var sellOfferOp = new ManageSellOfferOperation.Builder(sell, buy, "0", "1")
                .SetSourceAccount(account.KeyPair)
                .SetOfferId(orderId)
                .Build();

            return new TransactionBuilder(account)
               .AddOperation(sellOfferOp)
               .AddTimeBounds(GetTimeBounds())
               .SetFee(1_000_000)
               .Build()
               .ToUnsignedEnvelopeXdrBase64();
        }

        public async Task<OrderBookDto> GetOrderBook(AssetDto selling, AssetDto buying)
        {
            using var server = GetServer();

            var orderbook = await server.OrderBook
                .BuyingAsset(new AssetTypeCreditAlphaNum4(buying.UnitName, buying.IssuerAccountId))
                .SellingAsset(new AssetTypeCreditAlphaNum4(selling.UnitName, selling.IssuerAccountId))
                .Execute();

            return new OrderBookDto
            {
                Sells = orderbook.Asks.Select(a => new Sell
                {
                    Volume = decimal.Parse(a.Amount, NumberStyles.Float, CultureInfo.InvariantCulture),
                    Price = decimal.Parse(a.Price, NumberStyles.Float, CultureInfo.InvariantCulture),
                })
                .OrderBy(s => s.Price),
                Buys = orderbook.Bids.Select(b => new Buy
                {
                    Volume = decimal.Parse(b.Amount, NumberStyles.Float, CultureInfo.InvariantCulture) / decimal.Parse(b.Price, NumberStyles.Float, CultureInfo.InvariantCulture),
                    Price = decimal.Parse(b.Price, NumberStyles.Float, CultureInfo.InvariantCulture),
                })
                .OrderBy(b => b.Price),
            };
        }

        public async Task<List<ActiveOrderDto>> GetAccountOrders(string accountId)
        {
            using var server = GetServer();

            var offers = await server.Offers.ForAccount(accountId).Execute();

            var result = offers.Records.Select(r => new ActiveOrderDto
            {
                Id = long.Parse(r.Id),
                Price = decimal.Parse(r.Price, NumberStyles.Float, CultureInfo.InvariantCulture),
                Volume = decimal.Parse(r.Amount, NumberStyles.Float, CultureInfo.InvariantCulture),
                Buying = ToDto(r.Buying),
                Selling = ToDto(r.Selling),
            });

            return result.ToList();
        }

        public async Task<LiquidityPoolDto> GetLiquidityPool(AssetDto assetA, AssetDto assetB)
        {
            using var server = GetServer();

            var lpp = LiquidityPoolParameters.Create(
                stellar_dotnet_sdk.xdr.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                new AssetTypeCreditAlphaNum4(assetA.UnitName, assetA.IssuerAccountId),
                new AssetTypeCreditAlphaNum4(assetB.UnitName, assetB.IssuerAccountId),
                LiquidityPoolParameters.Fee);

            try
            {
                var lp = await server.LiquidityPools.LiquidityPool(lpp.GetID());

                return new LiquidityPoolDto
                {
                    Id = lp.ID.ToString(),
                    Reserves = lp.Reserves.Select(r => new LiquidityPoolReserveDto
                    {
                        Amount = decimal.Parse(r.Amount, NumberStyles.Float, CultureInfo.InvariantCulture),
                        UnitName = r.Asset.CanonicalName(),
                    }).ToList(),
                    TotalTrustlines = lp.TotalTrustlines,
                    TotalShares = lp.TotalShares,
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> BuildCreateLiquidityPoolRawTransaction(string accountId, AssetDto assetA, AssetDto assetB)
        {
            using var server = GetServer();

            var account = await server.Accounts.Account(accountId);
            var lpp = LiquidityPoolParameters.Create(
                stellar_dotnet_sdk.xdr.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                new AssetTypeCreditAlphaNum4(assetA.UnitName, assetA.IssuerAccountId),
                new AssetTypeCreditAlphaNum4(assetB.UnitName, assetB.IssuerAccountId),
                LiquidityPoolParameters.Fee);

            var changeTrustOp = new ChangeTrustOperation.Builder(ChangeTrustAsset.Create(lpp))
               .SetSourceAccount(account.KeyPair)
               .Build();

            return new TransactionBuilder(account)
              .AddOperation(changeTrustOp)
              .AddTimeBounds(GetTimeBounds())
              .SetFee(1_000_000)
              .Build()
              .ToUnsignedEnvelopeXdrBase64();
        }

        private Server GetServer()
        {
            var httpClient = _httpClientFactory.CreateClient();

            return new Server(_settings.Stellar.NodeUrl, httpClient);
        }

        private static TimeBounds GetTimeBounds(int minutes = 15)
        {
            var dateTimeOffsetMax = new DateTimeOffset(DateTime.UtcNow.AddMinutes(minutes));
            var timeBounds = new TimeBounds(null, dateTimeOffsetMax);
            return timeBounds;
        }

        private static AssetDto ToDto(Asset asset)
        {
            return asset switch
            {
                AssetTypeCreditAlphaNum4 nnt4 => new AssetDto
                {
                    IssuerAccountId = nnt4.Issuer,
                    UnitName = nnt4.Code
                },
                AssetTypeCreditAlphaNum12 nnt12 => new AssetDto
                {
                    IssuerAccountId = nnt12.Issuer,
                    UnitName = nnt12.Code
                },
                AssetTypeNative => new AssetDto { UnitName = "XLM" },
                _ => throw new NotImplementedException(),
            };
        }
    }
}
