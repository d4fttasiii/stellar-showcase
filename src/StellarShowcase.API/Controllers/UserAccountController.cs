using Microsoft.AspNetCore.Mvc;
using StellarShowcase.Domain.Dto;
using StellarShowcase.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StellarShowcase.API.Controllers
{
    [Route("api/user-account")]
    public class UserAccountController : BaseController
    {
        private readonly IUserAccountRepository _userAccountRepository;

        public UserAccountController(IUserAccountRepository repository)
        {
            _userAccountRepository = repository;
        }

        /// <summary>
        /// Gets all the user accounts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserAccountDto>>> GetAll()
        {
            return await HandleRequest(async () => await _userAccountRepository.GetUserAccounts());
        }

        /// <summary>
        /// Get a user account by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("{id}")]
        public async Task<ActionResult<UserAccountDto>> Get(Guid id)
        {
            return await HandleRequest(async () => await _userAccountRepository.GetUserAccount(id));
        }

        /// <summary>
        /// Creates new user account with a stellar account
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateUserAccountDto data)
        {
            return await HandleRequest(async () => await _userAccountRepository.AddUserAccount(data));
        }

        [HttpPost, Route("{id}/asset/{assetId}/create-trustline/{issuerId}")]
        public async Task<ActionResult<Guid>> CreateTrustline(
            [FromRoute] Guid id,
            [FromRoute] Guid assetId,
            [FromRoute] Guid issuerId,
            [FromBody] CredentialsDto credentials)
        {
            return await HandleRequest(async () => await _userAccountRepository.CreateTrustline(id, credentials.Passphrase, issuerId, assetId));
        }

        [HttpGet, Route("{id}/orders")]
        public async Task<ActionResult<List<ActiveOrderDto>>> GetActiveOrders([FromRoute] Guid id)
        {
            return await HandleRequest(async () => await _userAccountRepository.GetActiveOrders(id));
        }

        [HttpPost, Route("{id}/orders/buy")]
        public async Task<ActionResult> CreateBuyOrder([FromRoute] Guid id, [FromBody] CreateBuyOrderDto data)
        {
            return await HandleRequest(async () => await _userAccountRepository.CreateBuyOrder(id, data.Passphrase, data.MarketId, data.Volume, data.Price));
        }

        [HttpPost, Route("{id}/orders/sell")]
        public async Task<ActionResult> CreateSellOrder([FromRoute] Guid id, [FromBody] CreateSellOrderDto data)
        {
            return await HandleRequest(async () => await _userAccountRepository.CreateSellOrder(id, data.Passphrase, data.MarketId, data.Volume, data.Price));
        }

        [HttpDelete, Route("{id}/orders/{orderId}/cancel")]
        public async Task<ActionResult<bool>> CancelOrder([FromRoute] Guid id, [FromRoute] long orderId, [FromBody] CredentialsDto credentials)
        {
            return await HandleRequest(async () => await _userAccountRepository.CancelOrder(id, credentials.Passphrase, orderId));
        } 
    }
}
