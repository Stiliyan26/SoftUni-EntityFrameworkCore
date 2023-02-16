using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace MiniORM
{
    internal class ChangeTracker<TEntity>
        where TEntity : class, new()
    {
        private readonly IList<TEntity> allEntities;

        private readonly IList<TEntity> added;

        private readonly IList<TEntity> removed;

        private ChangeTracker()
        {
            this.added = new List<TEntity>();
            this.removed = new List<TEntity>();
        }

        public ChangeTracker(IEnumerable<TEntity> entities)
            : this()
        {
            this.allEntities = CloneEntity(entities);
        }

        public IReadOnlyCollection<TEntity> AllEntities
            => (IReadOnlyCollection<TEntity>)this.allEntities;

        public IReadOnlyCollection<TEntity> Added
            => (IReadOnlyCollection<TEntity>)this.added;

        public IReadOnlyCollection<TEntity> Removed
            => (IReadOnlyCollection<TEntity>)this.removed;

        public void Add(TEntity entity)
            => this.added.Add(entity);

        public void Remove(TEntity entity)
            => this.removed.Add(entity);

        public IEnumerable<TEntity> GetModifiedEntities(DbSet<TEntity> dbSet)
        {
            IList<TEntity> modifiedEntities = new List<TEntity>();

            PropertyInfo[] primaryKeys = typeof(TEntity)
                .GetProperties()
                .Where(pi => pi.HasAttribute<KeyAttribute>())
                .ToArray();

            foreach (TEntity proxyEntity in this.AllEntities)
            {
                IEnumerable<object> proxyEntityPKValues = 
                    GetPrimaryKeyValues(primaryKeys, proxyEntity)
                        .ToArray();

                TEntity originalEntity = dbSet
                    .Entities
                    .Single(e => GetPrimaryKeyValues(primaryKeys, e)
                        .SequenceEqual(proxyEntityPKValues));

                bool isModified = IsModified(originalEntity, proxyEntity);

                if (isModified)
                {
                    modifiedEntities.Add(originalEntity);
                }
            }
            return modifiedEntities;
        }

        private static IList<TEntity> CloneEntity(IEnumerable<TEntity> entities)
        {
            IList<TEntity> clonedEntities = new List<TEntity>();

            PropertyInfo[] propertyToClone = typeof(TEntity)
                .GetProperties()
                .Where(pi => DbContext.AllowedSqlTypes.Contains(pi.PropertyType))
                .ToArray();

            foreach (TEntity originalEntity in entities)
            {
                TEntity clonedEntity = Activator.CreateInstance<TEntity>();

                foreach (PropertyInfo property in propertyToClone)
                {
                    object originalValue = property.GetValue(originalEntity);
                    property.SetValue(clonedEntity, originalValue);
                }

                clonedEntities.Add(clonedEntity);
            }

            return clonedEntities;
        }

        private static IEnumerable<object> GetPrimaryKeyValues(IEnumerable<PropertyInfo> primaryKeys, TEntity entity)
            => primaryKeys
                .Select(pk => pk.GetValue(entity));

        private static bool IsModified(TEntity orginal, TEntity proxy)
        {
            IEnumerable<PropertyInfo> monitoredProperties = typeof(TEntity)
                .GetProperties()
                .Where(pi => DbContext.AllowedSqlTypes.Contains(pi.PropertyType));

            ICollection<PropertyInfo> modifiedProperties = monitoredProperties
                .Where(pi => !Equals(pi.GetValue(orginal), pi.GetValue(proxy)))
                .ToArray();

            return modifiedProperties.Any();
        }
    } 
}