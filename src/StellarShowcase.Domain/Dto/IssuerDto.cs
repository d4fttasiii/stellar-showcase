using System;
using System.Collections.Generic;

namespace StellarShowcase.Domain.Dto
{
    public class IssuerDto
    {
        public Guid Id { get; set; }
        public string IssuerAccountId { get; set; }
        public string DistributorAccountId { get; set; }
        public AccountDto Issuer { get; set; }
        public AccountDto Distributor { get; set; }
        public List<AssetDto> Assets { get; set; }
    }
}
