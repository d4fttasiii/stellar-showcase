using Microsoft.Extensions.Options;
using stellar_dotnet_sdk;
using StellarShowcase.Domain;
using StellarShowcase.Domain.Dto;
using System;
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
                NativeBalance = string.IsNullOrEmpty(stellarBalance?.BalanceString) ? 0m : decimal.Parse(stellarBalance.BalanceString, NumberStyles.Any, CultureInfo.InvariantCulture),
                NonNativeBalances = nonNativeBalances,
            };
        }

        public async Task<string> BuildConfigureIssuerWalletRawTransaction(string issuerAccountId)
        {
            using var server = GetServer();
            var issuerAccount = await server.Accounts.Account(issuerAccountId);

            if (issuerAccount.Flags.AuthRequired && issuerAccount.Flags.AuthRevocable)
                return string.Empty;

            var txBuilder = new TransactionBuilder(issuerAccount);
            var setOp = new SetOptionsOperation.Builder()
                .SetSetFlags(
                    (uint)stellar_dotnet_sdk.xdr.AccountFlags.AccountFlagsEnum.AUTH_REQUIRED_FLAG +
                    (uint)stellar_dotnet_sdk.xdr.AccountFlags.AccountFlagsEnum.AUTH_REVOCABLE_FLAG +
                    (uint)stellar_dotnet_sdk.xdr.AccountFlags.AccountFlagsEnum.AUTH_CLAWBACK_ENABLED_FLAG)
                .Build();

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
    }
}
