using Microsoft.AspNetCore.Mvc;
using StellarShowcase.Domain.Dto;
using StellarShowcase.Domain.Entities;
using StellarShowcase.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StellarShowcase.API.Controllers
{
    [Route("api/user-account")]
    [ApiController]
    public class UserAccountController : ControllerBase
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
        public async Task<ActionResult<IEnumerable<UserAccountEntity>>> GetAll()
        {
            var userAccounts = await _userAccountRepository.GetUserAccounts();

            return Ok(userAccounts);
        }

        /// <summary>
        /// Get a user account by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("{id}")]
        public async Task<ActionResult<IssuerEntity>> Get(Guid id)
        {
            var issuer = await _userAccountRepository.GetUserAccount(id);

            return Ok(issuer);
        }

        /// <summary>
        /// Creates new user account with a stellar account
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] UserAccountDto data)
        {
            var id = await _userAccountRepository.AddUserAccount(data);

            return Ok(id);
        }

        [HttpPost, Route("{id}/asset/{assetId}/create-trustline/{issuerId}")]
        public async Task<ActionResult<Guid>> CreateTrustline(
            [FromRoute] Guid id,
            [FromRoute] Guid assetId,
            [FromRoute] Guid issuerId)
        {
            await _userAccountRepository.CreateTrustline(id, issuerId, assetId);

            return Ok();
        }

        [HttpPost, Route("{id}/orders/buy")]
        public async Task<ActionResult<string>> CreateBuyOrder([FromRoute] Guid id, [FromBody] CreateBuyOrderDto data)
        {
            var txId = await _userAccountRepository.CreateBuyOrder(id, data.MarketId, data.Volume, data.Price);

            return Ok(txId);
        }

        [HttpPost, Route("{id}/orders/sell")]
        public async Task<ActionResult<string>> CreateSellOrder([FromRoute] Guid id, [FromBody] CreateSellOrderDto data)
        {
            var txId = await _userAccountRepository.CreateSellOrder(id, data.MarketId, data.Volume, data.Price);

            return Ok(txId);
        }
    }
}
