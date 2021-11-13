using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarShowcase.Domain.Entities
{
    public class OrderEntity : EntityBase
    {
        public Guid FromAssetId { get; set; }
        public Guid ToAssetId { get; set; }
        public decimal Amount { get; set; }
        public Guid CreatorAccountId { get; set; }
        public Guid? FulfillerAccountId { get; set; }
        public DateTime? Completed { get; set; }
    }
}
