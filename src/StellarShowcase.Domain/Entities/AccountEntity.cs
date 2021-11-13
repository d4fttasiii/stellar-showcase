using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StellarShowcase.Domain.Entities
{
    [Table("Account")]
    public class AccountEntity : EntityBase
    {
        public AccountEntity()
        {
            Orders = new List<OrderEntity>();
        }

        public string FullName { get; set; }
        public string FullAddress { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mnemonic { get; set; }

        public virtual ICollection<OrderEntity> Orders { get; set; }
    }
}
