using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarShowcase.Domain.Dto
{
    public class LiquidityPoolDto
    {
        public string Id { get; set; }
        public string TotalTrustlines { get; set; }
        public string TotalShares { get; set; }
        public List<LiquidityPoolReserveDto> Reserves { get; set; }
    }

    public class LiquidityPoolReserveDto
    {
        public decimal Amount { get; set; }
        public string UnitName { get; set; }
    }
}
