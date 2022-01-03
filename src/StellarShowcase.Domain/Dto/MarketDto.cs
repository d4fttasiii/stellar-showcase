using StellarShowcase.Domain.Entities;
using System;

namespace StellarShowcase.Domain.Dto
{
    public class MarketDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public AssetDto Base { get; set; }
        public AssetDto Quote { get; set; }
        public OrderBookDto OrderBooks { get; set; }
        public decimal Price { get; set; }
        public LiquidityPoolDto LiquidityPool { get; set; }
    }
}
