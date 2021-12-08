using System;

namespace StellarShowcase.Domain.Dto
{
    public class ActiveOrderDto
    {
        public long Id { get; set; }
        public Guid? MarketId { get; set; }
        public AssetDto Buying { get; set; }
        public AssetDto Selling { get; set; }
        public decimal Volume { get; set; }
        public decimal Price { get; set; }
    }
}
