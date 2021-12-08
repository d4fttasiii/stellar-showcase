using StellarShowcase.Domain.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StellarShowcase.DataAccess.Blockchain
{
    internal interface IStellarClient
    {
        string GenerateMnemonic();

        KeyPairDto DeriveKeyPair(string mnemonic, int accountIndex);

        Task<AccountDto> GetAccount(string accountId);

        Task FundAccount(string accountId);

        Task<string> BuildConfigureIssuerWalletRawTransaction(string issuerAccountId, UpsertAssetDto config);

        Task<string> BuildRawCreateAssetTransaction(AssetDto asset, string distributorAccountId);

        Task<string> BuildRawCreateTrustlineTransaction(AssetDto asset, decimal limit, string trustorAccountId);

        Task<string> BuildRawAuthorizeTrustlineTransaction(AssetDto asset, string trustorAccountId);

        Task<string> BuildRawRevokeTrustlineTransaction(AssetDto asset, string trustorAccountId);

        Task<string> BuildRawAssetTransaction(AssetDto asset, string senderAccountId, string recipientAccountId, decimal amount, string memo = "");

        Task<string> BuildClawbackRawTransaction(AssetDto asset, decimal amount, string fromAccountId);

        Task<string> SignSubmitRawTransaction(string privateKey, string txRaw);

        Task<string> CreateBuyOrderRawTransaction(string accountId, AssetDto sellingAsset, AssetDto buyingAsset, decimal amount, decimal price);

        Task<string> CreateCancelSellOrderRawTransaction(string accountId, long orderId, AssetDto sellingAsset, AssetDto buyingAsset);

        Task<string> CreateSellOrderRawTransaction(string accountId, AssetDto sellingAsset, AssetDto buyingAsset, decimal amount, decimal price);

        Task<string> CreateCancelBuyOrderRawTransaction(string accountId, long orderId, AssetDto sellingAsset, AssetDto buyingAsset);

        Task<OrderBookDto> GetOrderBook(AssetDto baseAsset, AssetDto quoteAsset);

        Task<List<ActiveOrderDto>> GetAccountOrders(string accountId);
    }
}
