using StellarShowcase.Domain.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StellarShowcase.Repositories.Interfaces
{
    public interface IAssetRepository
    {
        Task<List<AssetDto>> GetAll();
    }
}
