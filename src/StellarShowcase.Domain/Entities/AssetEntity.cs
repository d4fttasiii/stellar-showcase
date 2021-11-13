using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
