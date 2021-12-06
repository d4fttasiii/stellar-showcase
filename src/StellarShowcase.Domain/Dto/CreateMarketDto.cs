using System;

namespace StellarShowcase.Domain.Dto
{
    public class CreateMarketDto
    {
        public string Name { get; set; }
        public Guid BaseAssetId { get; set; }
        public Guid QuoteAssetId { get; set; }
    }
}
