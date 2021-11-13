using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StellarShowcase.Domain.Entities
{
    [Table("Issuer")]
    public class IssuerEntity : EntityBase
    {
        public IssuerEntity()
        {
            Assets = new List<AssetEntity>();
        }

        public string Mnemonic { get; set; }

        [ForeignKey(nameof(AssetEntity.IssuerId))]
        public virtual ICollection<AssetEntity> Assets { get; set; }
    }
}
