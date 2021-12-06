using System;

namespace StellarShowcase.Domain.Entities
{
    public static class EntityFactory
    {
        public static TEntity Create<TEntity>(Action<TEntity> setterFn = null) where TEntity : EntityBase, new()
        {
            var ent = new TEntity
            {
                Created = DateTime.UtcNow,
                Id = Guid.Empty,
            };

            setterFn?.Invoke(ent);

            return ent;
        }
    }
}