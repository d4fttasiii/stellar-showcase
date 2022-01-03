using System.Collections.Generic;

namespace StellarShowcase.Domain.Dto
{
    public class LiquidityPoolDto
    {
        public string Id { get; set; }
        public string TotalTrustlines { get; set; }
        public string TotalShares { get; set; }
        public List<LiquidityPoolReserveDto> Reserves { get; set; }
        public decimal? Price { get; set; }
    }

    public class LiquidityPoolReserveDto
    {
        public decimal Amount { get; set; }
        public string UnitName { get; set; }
        public string IssuerAccountId { get; set; }
    }
}
