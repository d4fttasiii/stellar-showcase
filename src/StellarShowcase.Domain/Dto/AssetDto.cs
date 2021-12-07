using System;

namespace StellarShowcase.Domain.Dto
{
    public class UpsertAssetDto
    {
        public string UnitName { get; set; }
        public decimal TotalSupply { get; set; }
        public bool IsAuthRequired { get; set; }
        public bool IsAuthRevocable { get; set; }
        public bool IsAuthImmutable { get; set; }
        public bool IsClawbackEnabled { get; set; }
    }

    public class AssetDto
    {
        public Guid? Id { get; set; }
        public Guid? IssuerId { get; set; }
        public string UnitName { get; set; }
        public decimal TotalSupply { get; set; }
        public string IssuerAccountId { get; set; }
    }
}
