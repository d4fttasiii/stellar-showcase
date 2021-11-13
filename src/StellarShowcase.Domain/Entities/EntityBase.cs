using System;
using System.ComponentModel.DataAnnotations;

namespace StellarShowcase.Domain.Entities
{
    public class EntityBase
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
    }
}