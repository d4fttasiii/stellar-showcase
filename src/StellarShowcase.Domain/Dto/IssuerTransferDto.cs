using System;

namespace StellarShowcase.Domain.Dto
{
    public class IssuerTransferDto
    {
        public Guid UserAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Memo { get; set; }
    }
}
