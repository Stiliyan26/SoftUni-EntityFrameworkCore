using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace MiniORM
{
    public abstract class DbContext
    {
        private readonly DatabaseConnection connection;

        private readonly IDictionary<Type, PropertyInfo> dbSetProperties;

        internal static readonly Type[] AllowedSqlTypes =
        {
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(char),
            typeof(byte),
            typeof(sbyte),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(bool),
            typeof(DateTime),   
            typeof(TimeSpan),
            typeof(string),
        };

        protected DbContext(string connectionString)
        {
            this.connection = new DatabaseConnection(connectionString);
        }

        public void SaveChanges()
        {
            var dbSets = this.dbSetProperties
                .Select(kvp => kvp.Value.GetValue(this))
                .ToArray();

            foreach (IEnumerable<object> dbSet in dbSets)
            {
                ICollection<object> invalidEntities = dbSet
                    .Where(e => !IsObjectValid(e))
                    .ToArray();

                if (invalidEntities.Any())
                {
                    throw new InvalidOperationException(
                        String.Format(ExceptionMessages.InvalidEntitiesInContext,
                        invalidEntities.Count(), dbSet.GetType().Name)); 
                }
            }

            using ConnectionManager connectionManager = new ConnectionManager(this.connection);
            using SqlTransaction transaction = this.connection.StartTransaction();

            foreach (IEnumerable dbSet in dbSets)
            {
                Type dbSetType = dbSet.GetType().GetGenericArguments().First();
                MethodInfo persistGenericMethod = typeof(DbContext)
                    .GetMethod("Persist", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(dbSetType);

                try
                {
                    persistGenericMethod.Invoke(this, new object[] { dbSet });
                }
                catch (TargetInvocationException tie)
                {
                    throw tie.InnerException;
                }
                catch (InvalidOperationException)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (SqlException)
                {
                    transaction.Rollback();
                    throw;
                }

                transaction.Commit();
            }
        }

        private void Persist<TEntity>(DbSet<TEntity> dbSet)
               where TEntity : class, new()
        {
            string tableName = GetTableName(typeof(TEntity));

            string[] columns = this.connection
                .FetchColumnNames(tableName)
                .ToArray();

            if (dbSet.ChangeTracker.Added.Any())
            {
                this.connection.InsertEntities(dbSet.ChangeTracker.Added, tableName, columns);
            }

            ICollection<TEntity> modifiedEntities = dbSet
                .ChangeTracker
                .GetModifiedEntities(dbSet)
                .ToArray();

            if (modifiedEntities.Any())
            {
                this.connection.UpdateEntities(modifiedEntities, tableName, columns);
            }

            if (dbSet.ChangeTracker.Removed.Any())
            {
                this.connection.DeleteEntities(dbSet.ChangeTracker.Removed, tableName, columns);
            }
        }

        private void InitializeDbSets()
        {
            foreach (var dbSetKvp in this.dbSetProperties)
            {
                Type dbSetType = dbSetKvp.Key;
                PropertyInfo dbSetProperty = dbSetKvp.Value;

                MethodInfo populateDbSetMethodGeneric = typeof(DbContext)
                    .GetMethod("PopulateDbSet", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(dbSetType);

                populateDbSetMethodGeneric.Invoke(this, new object[] { dbSetProperty });
            }
        }

        private void PopulateDbSet<TEntitty>(PropertyInfo dbSetProperty)
            where TEntitty : class, new()
        {
            IEnumerable<TEntitty> entityValues = this.LoadTableEntities<TEntitty>();

            DbSet<TEntitty> dbSetInstance = new DbSet<TEntitty>(entityValues);
            ReflectionHelper.ReplaceBackingField(this, dbSetProperty.Name, dbSetInstance);
        }

        private void MapAllRelations()
        {
            foreach (var dbSetKvp in this.dbSetProperties)
            {
                Type dbSetType = dbSetKvp.Key;
                object dbSet = dbSetKvp.Value.GetValue(this);

                MethodInfo mapRelationMethodGeneric = typeof(DbContext)
                    .GetMethod("MapRelations", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(dbSetType);

                mapRelationMethodGeneric.Invoke(this, new object[] { dbSet }); 
            }
        }

        private void MapRelations<TEntity>(DbSet<TEntity> dbSet)
            where TEntity : class, new()
        {
            Type entityType = typeof(TEntity);

            this.MapNavigationProperties(dbSet);

            PropertyInfo[] collections = entityType
                .GetProperties()
                .Where(pi => pi.PropertyType.IsGenericType && 
                             pi.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                .ToArray();

            foreach(PropertyInfo collection in collections)
            {
                Type referredEntityType = collection.PropertyType.GetGenericArguments().First();

                MethodInfo mapCollectionMethodGeneric = typeof(DbContext)
                    .GetMethod("MapCollection", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(entityType, referredEntityType);

                mapCollectionMethodGeneric.Invoke(this, new object[] { dbSet, collection });
            }
        }

        private void MapCollection<TEntity, TCollection>(DbSet<TEntity> dbSet, PropertyInfo collectionProperty)
            where TEntity : class, new()
            where TCollection : class, new()
        {
            Type entityType = typeof(TEntity);
            Type collectionType = typeof(TCollection);

            PropertyInfo[] referrredEntityPrimaryKeys = collectionType
                .GetProperties()
                .Where(pi => pi.HasAttribute<KeyAttribute>())
                .ToArray();

            PropertyInfo primaryKey = referrredEntityPrimaryKeys.First();
            PropertyInfo foreignKey = entityType
                .GetProperties()
                .First(pi => pi.HasAttribute<KeyAttribute>());

            bool isManyToMany = referrredEntityPrimaryKeys.Length > 2;

            if (isManyToMany)
            {
                primaryKey = collectionType
                    .GetProperties()
                    .First(pi => collectionType
                    .GetProperty(
                        pi.GetCustomAttribute<ForeignKeyAttribute>().Name)
                        .PropertyType == entityType);
            }

            DbSet<TCollection> navigationDbSet = 
                (DbSet<TCollection>)this.dbSetProperties[collectionType].GetValue(this);

            foreach (TEntity entity in dbSet)
            {
                object primaryKeyValue = foreignKey.GetValue(entity);

                TCollection[] navigationEntities = navigationDbSet
                    .Where(ne => primaryKey.GetValue(ne).Equals(primaryKeyValue))
                    .ToArray();

                ReflectionHelper.ReplaceBackingField(entity, collectionProperty.Name, navigationEntities);
            }
        }


        private void MapNavigationProperties<TEntity>(DbSet<TEntity> dbSet)
            where TEntity : class, new()
        {
            Type entityType = typeof(TEntity);

            PropertyInfo[] foreignKeys = entityType
                .GetProperties()
                .Where(pi => pi.HasAttribute<ForeignKeyAttribute>())
                .ToArray();

            foreach (PropertyInfo fk in foreignKeys)
            {
                string navigationPropName = fk
                    .GetCustomAttribute<ForeignKeyAttribute>().Name;

                PropertyInfo navProp = entityType
                    .GetProperty(navigationPropName);

                object navDbSet = this.dbSetProperties[navProp.PropertyType].GetValue(this);

                PropertyInfo navPropPrimaryKey = navProp
                    .PropertyType
                    .GetProperties()
                    .First(pi => pi.HasAttribute<KeyAttribute>());

                foreach (TEntity entity in dbSet)
                {
                    object fkValue = fk.GetValue(entity);
                    object navPropValue = ((IEnumerable<object>)navDbSet)
                        .First(cnp => navPropPrimaryKey.GetValue(cnp).Equals(fkValue));

                    navProp.SetValue(entity, navPropValue);
                }
            }
        }
       
        private Dictionary<Type, PropertyInfo> DiscoverDbSets()
        {
            Dictionary<Type, PropertyInfo> dbSets = this
                .GetType()
                .GetProperties()
                .Where(pi => pi.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .ToDictionary(pi => pi.PropertyType.GenericTypeArguments.First(), pi => pi);

            return dbSets;
        }

        private static bool IsObjectValid(object obj)
        {
            ValidationContext validationContext = new ValidationContext(obj);

            List<ValidationResult> validationErrors = new List<ValidationResult>();

            bool validationResult = Validator.TryValidateObject(obj, validationContext, validationErrors, true);

            return validationResult;
        }

        private IEnumerable<TEntity> LoadTableEntities<TEntity>()
            where TEntity : class
        {
            Type entityType = typeof(TEntity);
            string[] columnNames = this.GetEntityColumnNames(entityType);
            string tableName = this.GetTableName(entityType);

            IEnumerable<TEntity> fetchRows = this.connection
                .FetchResultSet<TEntity>(tableName, columnNames);

            return fetchRows;
        }

        private string GetTableName(Type entityType)
        {
            string tableName = ((TableAttribute)
                Attribute.GetCustomAttribute(entityType, typeof(TableAttribute)))
                .Name;

            if (tableName == null)
            {
                tableName = this.dbSetProperties[entityType].Name;
            }

            return tableName;
        }

        private string[] GetEntityColumnNames(Type entityType)
        {
            string tableName = this.GetTableName(entityType);

            string[] dbColumns = this.connection
                .FetchColumnNames(tableName)
                .ToArray();

            string[] columns = entityType
                .GetProperties()
                .Where(pi => dbColumns.Contains(pi.Name)
                    && !pi.HasAttribute<NotMappedAttribute>()
                    && AllowedSqlTypes.Contains(pi.PropertyType))
                .Select(pi => pi.Name)
                .ToArray();

            return columns;
        }
    }
}