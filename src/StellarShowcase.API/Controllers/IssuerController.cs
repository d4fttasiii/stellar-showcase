using Microsoft.AspNetCore.Mvc;
using StellarShowcase.Domain.Dto;
using StellarShowcase.Domain.Entities;
using StellarShowcase.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StellarShowcase.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IssuerController : ControllerBase
    {
        private readonly IIssuerRepository _issuerRepository;

        public IssuerController(IIssuerRepository issuerRepository)
        {
            _issuerRepository = issuerRepository;
        }

        /// <summary>
        /// Gets all the issuers
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IssuerEntity>>> GetAll()
        {
            var issuer = await _issuerRepository.GetIssuers();

            return Ok(issuer);
        }

        /// <summary>
        /// Get an issuer and id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("{id}")]
        public async Task<ActionResult<IssuerEntity>> Get(Guid id)
        {
            var issuer = await _issuerRepository.GetIssuer(id);

            return Ok(issuer);
        }

        /// <summary>
        /// Creates a new issuer, also creates 2 new accounts on the blockchain to issue assets from 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create()
        {
            var id = await _issuerRepository.AddIssuer();

            return Ok(id);
        }

        /// <summary>
        /// Creates a new asset on the blockchain with the given issuer 
        /// </summary>
        /// <param name="id">Issuer id</param>
        /// <param name="asset">Asset details</param>
        /// <returns></returns>
        [HttpPost, Route("{id}/asset")]
        public async Task<ActionResult<Guid>> IssueAsset(Guid id, [FromBody] AssetDto asset)
        {
            var assetId = await _issuerRepository.IssueAsset(id, asset);

            return Ok(assetId);
        }

        /// <summary>
        /// Authorizes trustline for an asset for the given user.
        /// The user will be allowed to hold the given asset in his/her wallet.
        /// </summary>
        /// <param name="id">Issuer id</param>
        /// <param name="assetId">Asset Id</param>
        /// <param name="userAccountId">User's Id</param>
        /// <returns></returns>
        [HttpPut, Route("{id}/asset/{assetId}/auth/{userAccountId}")]
        public async Task<ActionResult<Guid>> AuthorizeTrustline(
            [FromRoute] Guid id,
            [FromRoute] Guid assetId,
            [FromRoute] Guid userAccountId)
        {
            await _issuerRepository.AuthorizeTrustline(id, assetId, userAccountId);

            return Ok();
        }

        /// <summary>
        /// Revokes trustline for an asset for the given user.
        /// The user will not be allowed to hold the asset any more in his/her wallet, previously transferred
        /// assets will be frozen.
        /// </summary>
        /// <param name="id">Issuer id</param>
        /// <param name="assetId">Asset Id</param>
        /// <param name="userAccountId">User's Id</param>
        /// <returns></returns>
        [HttpPut, Route("{id}/asset/{assetId}/revoke/{userAccountId}")]
        public async Task<ActionResult<Guid>> RevokeTrustline(
            [FromRoute] Guid id,
            [FromRoute] Guid assetId,
            [FromRoute] Guid userAccountId)
        {
            await _issuerRepository.RevokeTrustline(id, assetId, userAccountId);

            return Ok();
        }

        /// <summary>
        /// Transferring asset from issuer to user account
        /// </summary>
        /// <param name="id">Issuer id</param>
        /// <param name="assetId">Asset Id</param>
        /// <param name="tx">Transaction details</param>
        /// <returns></returns>
        [HttpPut, Route("{id}/asset/{assetId}/transfer")]
        public async Task<ActionResult<string>> TransferAsset(
            [FromRoute] Guid id,
            [FromRoute] Guid assetId,
            [FromBody] IssuerTransferDto tx)
        {
            var txId = await _issuerRepository.TransferAsset(id, assetId, tx);

            return Ok(txId);
        }
    }
}
