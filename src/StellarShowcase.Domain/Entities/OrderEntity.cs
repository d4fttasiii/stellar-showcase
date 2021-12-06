using System;

namespace StellarShowcase.Domain.Entities
{
    public class OrderEntity : EntityBase
    {
        public OrderEntity() { }

        public Guid UserAccountId { get; set; }
        public Guid MarketId { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public string TxId { get; set; }
    }
}
