using StellarShowcase.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StellarShowcase.Repositories.Interfaces
{
    public interface IIssuerRepository
    {
        Task<Guid> AddIssuer();

        Task<IEnumerable<IssuerDto>> GetIssuers();

        Task<IssuerDto> GetIssuer(Guid id);

        Task<Guid> IssueAsset(Guid id, UpsertAssetDto asset);

        Task AuthorizeTrustline(Guid id, Guid assetId, Guid userAccountId);

        Task RevokeTrustline(Guid id, Guid assetId, Guid userAccountId);

        Task<string> TransferAsset(Guid id, Guid assetId, IssuerTransferDto tx);
    }
}
