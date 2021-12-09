using StellarShowcase.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StellarShowcase.Repositories.Interfaces
{
    public interface IUserAccountRepository
    {
        Task<Guid> AddUserAccount(CreateUserAccountDto userDto);

        Task<IEnumerable<UserAccountDto>> GetUserAccounts();

        Task<UserAccountDto> GetUserAccount(Guid id);

        Task CreateTrustline(Guid id, string passphrase, Guid issuerId, Guid assetId, decimal? limit = null);

        Task TransferAsset(Guid id, string passphrase, Guid issuerId, Guid assetId, string recipientAccountId, decimal amount, string memo = "");

        Task CreateBuyOrder(Guid userAccountId, string passphrase, Guid marketId, decimal volume, decimal price);

        Task CreateSellOrder(Guid userAccountId, string passphrase, Guid marketId, decimal volume, decimal price);

        Task<List<ActiveOrderDto>> GetActiveOrders(Guid userAccountId);

        Task<bool> CancelOrder(Guid userAccountId, string passphrase, long orderId);
    }
}
