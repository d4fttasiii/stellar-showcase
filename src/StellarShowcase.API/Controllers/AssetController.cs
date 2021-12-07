using Microsoft.AspNetCore.Mvc;
using StellarShowcase.Domain.Dto;
using StellarShowcase.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StellarShowcase.API.Controllers
{
    [Route("api/asset")]
    public class AssetController : BaseController
    {
        private readonly IAssetRepository _assetRepository;

        public AssetController(IAssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<AssetDto>>> GetAssets()
        {
            return await HandleRequest(async () => await _assetRepository.GetAll());
        }
    }
}
