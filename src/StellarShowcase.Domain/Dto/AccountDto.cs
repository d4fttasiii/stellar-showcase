using System.Collections.Generic;

namespace StellarShowcase.Domain.Dto
{
    public class AccountDto
    {
        public string AccountId { get; set; }
        public long SequenceNumber { get; set; }
        public decimal NativeBalance { get; set; }
        public List<Balance> NonNativeBalances { get; set; }
    }

    public class Balance
    {
        public string AssetType { get; set; }
        public string AssetCode { get; set; }
        public string AssetIssuer { get; set; }
        public decimal Amount { get; set; }
    }
}
