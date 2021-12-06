using System;

namespace StellarShowcase.Domain.Dto
{
    public class CreateBuyOrderDto
    {
        public Guid MarketId { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
    }

    public class CreateSellOrderDto : CreateBuyOrderDto { }
}
