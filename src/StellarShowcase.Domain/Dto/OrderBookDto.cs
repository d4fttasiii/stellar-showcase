using System.Collections.Generic;

namespace StellarShowcase.Domain.Dto
{
    public class OrderBookDto
    {
        public IEnumerable<Buy> Buys { get; set; }
        public IEnumerable<Sell> Sells { get; set; }
    }

    public class Buy
    {
        public decimal Volume { get; set; }
        public decimal Price { get; set; }
    }

    public class Sell : Buy { }
}
