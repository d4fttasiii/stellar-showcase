using System.Collections.Generic;

namespace StellarShowcase.Domain.Dto
{
    public class OrderBookDto
    {
        public IEnumerable<BidDto> Bids { get; set; }
        public IEnumerable<AskDto> Asks { get; set; }
    }

    public class BidDto
    {
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
    }

    public class AskDto : BidDto { }
}
