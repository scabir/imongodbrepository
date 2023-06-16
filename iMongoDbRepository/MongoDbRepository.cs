using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace iMongoDbRepository
{
    public abstract class MongoDbRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IMongoDbItem
    {
        public bool Configured => _collection != null && _dbConfiguration != null;
        protected IMongoCollection<TEntity> _collection;
        private DbConfiguration _dbConfiguration;
        private const int MaxNumberOfRows = 100000;

        public void Configure(DbConfiguration dbConfiguration)
        {
            if (dbConfiguration == null)
            {
                throw new ArgumentNullException(nameof(dbConfiguration));
            }

            _dbConfiguration = dbConfiguration;
            //Set up MongoDb client
            var client = new MongoClient(_dbConfiguration.ConnectionString);
            var database = client.GetDatabase(_dbConfiguration.DbName);
            _collection = database.GetCollection<TEntity>(_dbConfiguration.Collection);
            
            //Create the collection if doesn't exist
            if (database.ListCollections().ToList().All(x => x.GetElement("name").Value.ToString() != _dbConfiguration.Collection))
            {
                database.CreateCollection(_dbConfiguration.Collection);
                _collection = database.GetCollection<TEntity>(_dbConfiguration.Collection);
            }

            CheckIsConfigured();
        }

        public virtual void Dispose()
        {
            //No need to dispose MongoDB client manually
        }

        public virtual IEnumerable<TEntity> All(int maxNumberOfRows = MaxNumberOfRows, bool includeDeleted = false)
        {
            CheckIsConfigured();

            var result = new List<TEntity>();
            var cursor = includeDeleted
                ? _collection.FindAsync(x => true, new FindOptions<TEntity>()).Result
                : _collection.FindAsync(x => !x.Deleted, new FindOptions<TEntity>()).Result;

            using (cursor)
            {
                while (cursor.MoveNextAsync().Result)
                {
                    if (result.Count() + cursor.Current.Count() >= maxNumberOfRows)
                    {
                        result.AddRange(cursor.Current.Take(maxNumberOfRows - result.Count));
                        break;
                    }

                    result.AddRange(cursor.Current);
                }
            }

            return result;
        }

        public virtual async Task<IEnumerable<TEntity>> AllAsync(int maxNumberOfRows = MaxNumberOfRows, bool includeDeleted = false)
        {
            CheckIsConfigured();

            var result = new List<TEntity>();

            var cursor = includeDeleted
                ? await _collection.FindAsync(x => true, new FindOptions<TEntity>())
                : await _collection.FindAsync(x => !x.Deleted, new FindOptions<TEntity>());
            
            using (cursor)
            {
                while (await cursor.MoveNextAsync())
                {
                    if (result.Count() + cursor.Current.Count() >= maxNumberOfRows)
                    {
                        result.AddRange(cursor.Current.Take(maxNumberOfRows - result.Count));
                        break;
                    }

                    result.AddRange(cursor.Current);
                }
            }

            return result;
        }

        public virtual IEnumerable<TEntity> Query(Expression<Func<TEntity, bool>> filter, int maxNumberOfRows = MaxNumberOfRows, bool includeDeleted = false)
        {
            CheckIsConfigured();

            var result = new List<TEntity>();

            using (var cursor = _collection.FindAsync(filter, new FindOptions<TEntity>()).Result)
            {
                while (cursor.MoveNextAsync().Result)
                {
                    var selectedPart = includeDeleted
                        ? cursor.Current
                        : cursor.Current.Where(x => !x.Deleted);

                    if (result.Count() + selectedPart.Count() >= maxNumberOfRows)
                    {
                        result.AddRange(selectedPart.Take(maxNumberOfRows - result.Count));
                        break;
                    }

                    result.AddRange(selectedPart);
                }
            }

            return result;
        }

        public virtual async Task<IEnumerable<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> filter, int maxNumberOfRows = MaxNumberOfRows, bool includeDeleted = false)
        {
            CheckIsConfigured();

            var result = new List<TEntity>();

            using (var cursor = await _collection.FindAsync(filter, new FindOptions<TEntity>()))
            {
                while (await cursor.MoveNextAsync())
                {
                    var selectedPart = includeDeleted
                        ? cursor.Current
                        : cursor.Current.Where(x => !x.Deleted);

                    if (result.Count() + selectedPart.Count() >= maxNumberOfRows)
                    {
                        result.AddRange(selectedPart.Take(maxNumberOfRows - result.Count));
                        break;
                    }

                    result.AddRange(selectedPart);
                }
            }

            return result;
        }

        public virtual TEntity Get(string entityId, bool includeDeleted = false)
        {
            CheckIsConfigured();

            if (!includeDeleted)
            {
                return _collection.AsQueryable().FirstOrDefault(x => !x.Deleted && x._id == entityId);
            }

            return _collection.AsQueryable().FirstOrDefault(x => x._id == entityId);
        }

        public virtual async Task<TEntity> GetAsync(string entityId, bool includeDeleted = false)
        {
            CheckIsConfigured();

            return await Task.Run(() =>
            {
                if (!includeDeleted)
                {
                    return _collection.AsQueryable().FirstOrDefault(x => !x.Deleted && x._id == entityId);
                }

                return _collection.AsQueryable().FirstOrDefault(x => x._id == entityId);
            });
        }

        public virtual long Count(bool includeDeleted = false)
        {
            CheckIsConfigured();

            if (!includeDeleted)
            {
                return _collection.CountDocuments(x => true);
            }

            return _collection.AsQueryable().Where(x => true).Count(x => !x.Deleted);
        }

        public virtual async Task<long> CountAsync(bool includeDeleted = false)
        {
            CheckIsConfigured();

            return await Task.Run(() => {
                if (!includeDeleted)
                {
                    return _collection.CountDocuments(x => true);
                }

                return _collection.AsQueryable().Where(x => true).Count(x => !x.Deleted);
            });
        }

        public virtual long Count(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false)
        {
            CheckIsConfigured();

            if (!includeDeleted)
            {
                return _collection.CountDocuments(filter);
            }

            return _collection.AsQueryable().Where(filter).Count(x => !x.Deleted);
        }

        public virtual async Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false)
        {
            CheckIsConfigured();

            return await Task.Run(() =>
            {
                if (!includeDeleted)
                {
                    return _collection.CountDocuments(filter);
                }

                return _collection.AsQueryable().Where(filter).Count(x => !x.Deleted);
            });
        }

        public virtual bool Any(bool includeDeleted = false)
        {
            CheckIsConfigured();

            if (!includeDeleted)
            {
                return _collection.AsQueryable().Any();
            }

            return _collection.AsQueryable().Any(x => !x.Deleted);
        }

        public virtual async Task<bool> AnyAsync(bool includeDeleted = false)
        {
            CheckIsConfigured();

            if (!includeDeleted)
            {
                return await _collection.AsQueryable().AnyAsync();
            }

            return await _collection.AsQueryable().AnyAsync(x => !x.Deleted);
        }

        public virtual bool Any(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false)
        {
            CheckIsConfigured();

            var filterBuilder = Builders<TEntity>.Filter;
            var finalFilter = includeDeleted ? filter : filterBuilder.And(filter, filterBuilder.Eq(x => x.Deleted, false));

            return _collection.Find(finalFilter).Any();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false)
        {
            CheckIsConfigured();

            var filterBuilder = Builders<TEntity>.Filter;
            var finalFilter = includeDeleted ? filter : filterBuilder.And(filter, filterBuilder.Eq(x => x.Deleted, false));

            return await _collection.Find(finalFilter).AnyAsync();
        }

        public virtual void Insert(TEntity entity)
        {
            CheckIsConfigured();
            _collection.InsertOne(PrepareForInsert(entity));
        }

        public virtual async Task InsertAsync(TEntity entity)
        {
            CheckIsConfigured();
            await _collection.InsertOneAsync(PrepareForInsert(entity));
        }

        public virtual void Insert(IEnumerable<TEntity> entities)
        {
            CheckIsConfigured();
            _collection.InsertMany(PrepareForInsert(entities));
        }

        public virtual async Task InsertAsync(IEnumerable<TEntity> entities)
        {
            CheckIsConfigured();
            await _collection.InsertManyAsync(PrepareForInsert(entities));
        }

        public virtual void Update(TEntity entity)
        {
            CheckIsConfigured();
            entity = PrepareForUpdate(entity);
            _collection.ReplaceOne(x => x._id == entity._id, entity);
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            CheckIsConfigured();
            entity = PrepareForUpdate(entity);

            await _collection.ReplaceOneAsync(x => x._id == entity._id, entity);
        }

        public virtual void Upsert(TEntity entity)
        {
            CheckIsConfigured();

            if (entity == null)
            {
                throw new NullReferenceException("Entity cannot be null.");
            }

            entity = entity._id == null ? PrepareForInsert(entity) : PrepareForUpdate(entity);

            var filter = Builders<TEntity>.Filter.Eq(x => x._id, entity._id);
            _collection.ReplaceOne(filter, entity, new ReplaceOptions { IsUpsert = true });
        }

        public virtual async Task UpsertAsync(TEntity entity)
        {
            CheckIsConfigured();

            if (entity == null)
            {
                throw new NullReferenceException("Entity cannot be null.");
            }

            entity = entity._id == null ? PrepareForInsert(entity) : PrepareForUpdate(entity);

            var filter = Builders<TEntity>.Filter.Eq(x => x._id, entity._id);
            await _collection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = true });
        }

        public virtual void Delete(string entityId, bool hardDelete = false)
        {
            CheckIsConfigured();

            if (hardDelete)
            {
                _collection.DeleteOne(x => x._id == entityId);
            }
            else
            {
                var update = Builders<TEntity>.Update.Set(x => x.Deleted, true);

                _collection.UpdateOne(x => x._id == entityId, update);
            }
        }

        public virtual async Task DeleteAsync(string entityId, bool hardDelete = false)
        {
            CheckIsConfigured();

            if (hardDelete)
            {
                await _collection.DeleteOneAsync(x => x._id == entityId);
            }
            else
            {
                var update = Builders<TEntity>.Update.Set(x => x.Deleted, true);

                await _collection.UpdateOneAsync(x => x._id == entityId, update);
            }
        }
        public virtual void Delete(IEnumerable<string> entityIds, bool hardDelete = false)
        {
            CheckIsConfigured();

            if (hardDelete)
            {
                _collection.DeleteMany(x => entityIds.Contains(x._id));
            }
            else
            {
                var filter = Builders<TEntity>.Filter.In(x => x._id, entityIds);
                var update = Builders<TEntity>.Update.Set(x => x.Deleted, true);

                _collection.UpdateMany(filter, update);
            }
        }

        public virtual async Task DeleteAsync(IEnumerable<string> entityIds, bool hardDelete = false)
        {
            CheckIsConfigured();

            if (hardDelete)
            {
                await _collection.DeleteManyAsync(x => entityIds.Contains(x._id));
            }
            else
            {
                var filter = Builders<TEntity>.Filter.In(x => x._id, entityIds);
                var update = Builders<TEntity>.Update.Set(x => x.Deleted, true);

                await _collection.UpdateManyAsync(filter, update);
            }
        }

        public virtual void UnDelete(string entityId)
        {
            CheckIsConfigured();

            var item = Get(entityId);

            if (item != null)
            {
                item.Deleted = false;
                Update(item);
            }
        }

        public virtual async Task UnDeleteAsync(string entityId)
        {
            CheckIsConfigured();

            var item = await GetAsync(entityId);

            if (item != null)
            {
                item.Deleted = false;
                await UpdateAsync(item);
            }
        }

        private void CheckIsConfigured()
        {
            if (!Configured)
            {
                throw new Exception("The repository is not configured. Make sure the configuration is done properly before using the repository.");

                // Hypothetical additional check and message
                if (_dbConfiguration.ConnectionString == null || string.IsNullOrEmpty(_dbConfiguration.ConnectionString))
                {
                    throw new Exception("The repository is not configured. The connection string is null or empty. Make sure the connection string is set up correctly in the configuration.");
                }
            }
        }


        private TEntity PrepareForInsert(TEntity entity)
        {
            entity.CreatedOn = DateTime.UtcNow;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.Deleted = false;

            if (_dbConfiguration.AutoGenerateIds)
            {
                entity._id = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            }

            return entity;
        }

        private IEnumerable<TEntity> PrepareForInsert(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                yield return PrepareForInsert(entity);
            }
        }

        private TEntity PrepareForUpdate(TEntity entity)
        {
            if (entity == null)
            {
                throw new NullReferenceException("Entity cannot be null.");
            }

            entity.ModifiedOn = DateTime.UtcNow;

            return entity;
        }
    }
}
