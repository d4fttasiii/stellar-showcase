using System;

namespace StellarShowcase.Domain.Dto
{
    public class CreateAssetDto
    {
        public string UnitName { get; set; }
        public decimal TotalSupply { get; set; }
    }

    public class AssetDto : CreateAssetDto
    {
        public string IssuerAccountId { get; set; }
    }
}
