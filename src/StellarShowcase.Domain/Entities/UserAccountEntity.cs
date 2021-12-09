using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StellarShowcase.Domain.Entities
{
    [Table("UserAccount")]
    public class UserAccountEntity : EntityBase
    {
        public string FullName { get; set; }
        public string FullAddress { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mnemonic { get; set; }
        public string AccountId { get; set; }
    }
}
