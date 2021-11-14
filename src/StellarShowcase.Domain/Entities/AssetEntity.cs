using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace StellarShowcase.Domain.Entities
{
    [Table("Asset")]
    public class AssetEntity : EntityBase
    {
        public Guid IssuerId { get; set; }
        public string IssuerAccountId { get; set; }
        public string UnitName { get; set; }
        public decimal TotalSupply { get; set; }
        public string CreatorTxId { get; set; }
    }
}
