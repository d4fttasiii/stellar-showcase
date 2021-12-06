using System;

namespace StellarShowcase.Domain.Entities
{
    public class MarketEntity : EntityBase
    {
        public Guid BaseAssetId { get; set; }
        public Guid QuoteAssetId { get; set; }
        public string Name { get; set; }
    }
}
