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
        public bool Configured { get; protected set; }
        private IMongoCollection<TEntity> _collection;
        private DbConfiguration _dbConfiguration;
        
        public void Configure(DbConfiguration dbConfiguration)
        {
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

            Configured = true;
        }

        private void CheckIsConfigured()
        {
            if (!Configured)
            {
                throw new Exception("Repository is not conofigured!");
            }
        }

        public virtual void Dispose()
        {
            //No need to dispose MongoDB client manually
        }

        public virtual IEnumerable<TEntity> All(bool includeDeleted = false)
        {
            CheckIsConfigured();

            if (includeDeleted)
            {
                return _collection.AsQueryable().Where(x => !x.Deleted).ToEnumerable();
            }

            return _collection.AsQueryable().ToEnumerable();
        }

        public virtual async Task<IEnumerable<TEntity>> AllAsync(bool includeDeleted = false)
        {
            CheckIsConfigured();

            return await Task.Run(() =>
            {
                if (includeDeleted)
                {
                    return _collection.AsQueryable().Where(x => !x.Deleted).ToEnumerable();
                }

                return _collection.AsQueryable().ToEnumerable();
            });
        }

        public virtual IEnumerable<TEntity> Query(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false)
        {
            CheckIsConfigured();

            if (includeDeleted)
            {
                return _collection.AsQueryable().Where(x => !x.Deleted).Where(filter);
            }

            return _collection.AsQueryable().Where(filter);
        }

        public virtual async Task<IEnumerable<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false)
        {
            CheckIsConfigured();

            return await Task.Run(() =>
            {
                if (includeDeleted)
                {
                    return _collection.AsQueryable().Where(x => !x.Deleted).Where(filter);
                }

                return _collection.AsQueryable().Where(filter);
            });
        }

        public virtual TEntity Get(string entityId, bool includeDeleted = false)
        {
            CheckIsConfigured();

            if (includeDeleted)
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
                if (includeDeleted)
                {
                    return _collection.AsQueryable().FirstOrDefault(x => !x.Deleted && x._id == entityId);
                }

                return _collection.AsQueryable().FirstOrDefault(x => x._id == entityId);
            });
        }

        public virtual long Count(bool includeDeleted = false)
        {
            CheckIsConfigured();

            if (includeDeleted)
            {
                return _collection.CountDocuments(x => true);
            }

            return _collection.AsQueryable().Where(x => true).Count(x => !x.Deleted);
        }

        public virtual async Task<long> CountAsync(bool includeDeleted = false)
        {
            CheckIsConfigured();

            return await Task.Run(() => {
                if (includeDeleted)
                {
                    return _collection.CountDocuments(x => true);
                }

                return _collection.AsQueryable().Where(x => true).Count(x => !x.Deleted);
            });
        }

        public virtual long Count(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false)
        {
            CheckIsConfigured();

            if (includeDeleted)
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
                if (includeDeleted)
                {
                    return _collection.CountDocuments(filter);
                }

                return _collection.AsQueryable().Where(filter).Count(x => !x.Deleted);
            });
        }

        public virtual bool Any(bool includeDeleted = false)
        {
            CheckIsConfigured();

            if (includeDeleted)
            {
                return _collection.AsQueryable().FirstOrDefault() != null;
            }

            return _collection.AsQueryable().FirstOrDefault(x => !x.Deleted) != null;
        }

        public virtual async Task<bool> AnyAsync(bool includeDeleted = false)
        {
            CheckIsConfigured();

            return await Task.Run(() => {
                if (includeDeleted)
                {
                    return _collection.AsQueryable().FirstOrDefault() != null;
                }

                return _collection.AsQueryable().FirstOrDefault(x => !x.Deleted) != null;
            });
        }

        public virtual bool Any(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false)
        {
            CheckIsConfigured();

            if (includeDeleted)
            {
                return _collection.AsQueryable().FirstOrDefault(filter) != null;
            }

            return _collection.AsQueryable().Where(x => !x.Deleted).FirstOrDefault(filter) != null;
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false)
        {
            CheckIsConfigured();

            return await Task.Run(() =>
            {
                if (includeDeleted)
                {
                    return _collection.AsQueryable().FirstOrDefault(filter) != null;
                }

                return _collection.AsQueryable().Where(x => !x.Deleted).FirstOrDefault(filter) != null;
            });
        }

        public virtual void Insert(TEntity entity)
        {
            CheckIsConfigured();

            entity.CreatedOn = DateTime.UtcNow;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.Deleted = false;

            if (_dbConfiguration.AutoGenerateIds)
            {
                entity._id = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            }

            _collection.InsertOne(entity);
        }

        public virtual async Task InsertAsync(TEntity entity)
        {
            CheckIsConfigured();

            entity.CreatedOn = DateTime.UtcNow;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.Deleted = false;

            if (_dbConfiguration.AutoGenerateIds)
            {
                entity._id = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            }

            await _collection.InsertOneAsync(entity);
        }

        public virtual void Insert(IEnumerable<TEntity> entities)
        {
            CheckIsConfigured();

            for (int i = 0; i < entities.Count(); i++)
            {
                entities.ElementAt(i).CreatedOn = DateTime.UtcNow;
                entities.ElementAt(i).ModifiedOn = DateTime.UtcNow;
                entities.ElementAt(i).Deleted = false;

                if (_dbConfiguration.AutoGenerateIds)
                {
                    entities.ElementAt(i)._id = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
                }
            }

            _collection.InsertMany(entities);
        }

        public virtual async Task InsertAsync(IEnumerable<TEntity> entities)
        {
            CheckIsConfigured();

            for (int i = 0; i < entities.Count(); i++)
            {
                entities.ElementAt(i).CreatedOn = DateTime.UtcNow;
                entities.ElementAt(i).ModifiedOn = DateTime.UtcNow;
                entities.ElementAt(i).Deleted = false;

                if (_dbConfiguration.AutoGenerateIds)
                {
                    entities.ElementAt(i)._id = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
                }
            }

            await _collection.InsertManyAsync(entities);
        }

        public virtual void Update(TEntity entity)
        {
            CheckIsConfigured();

            if (entity == null)
            {
                throw new NullReferenceException("Entity cannot be null.");
            }

            entity.ModifiedOn = DateTime.UtcNow;
            _collection.ReplaceOne(x => x._id == entity._id, entity);
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            CheckIsConfigured();

            if (entity == null)
            {
                throw new NullReferenceException("Entity cannot be null.");
            }

            entity.ModifiedOn = DateTime.UtcNow;
            await _collection.ReplaceOneAsync(x => x._id == entity._id, entity);
        }

        public virtual void Upsert(TEntity entity)
        {
            CheckIsConfigured();

            if (entity == null)
            {
                throw new NullReferenceException("Entity cannot be null.");
            }

            if (entity._id == null)
            {
                Insert(entity);
            }
            else
            {
                var item = Get(entity._id);

                if (Any(x => x._id == entity._id))
                {
                    Update(entity);
                }
                else
                {
                    Insert(entity);
                }
            }
        }

        public virtual async void UpsertAsync(TEntity entity)
        {
            CheckIsConfigured();

            if (entity == null)
            {
                throw new NullReferenceException("Entity cannot be null.");
            }

            if (entity._id == null)
            {
                await InsertAsync(entity);
            }
            else
            {
                if (await AnyAsync(x => x._id == entity._id))
                {
                    await UpdateAsync(entity);
                }
                else
                {
                    await InsertAsync(entity);
                }
            }
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
                var item = Get(entityId);

                if (item != null)
                {
                    item.Deleted = true;
                    Update(item);
                }
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
                var item = await GetAsync(entityId);

                if (item != null)
                {
                    item.Deleted = true;
                    await UpdateAsync(item);
                }
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
                for (int i = 0; i < entityIds.Count(); i++)
                {
                    var item = Get(entityIds.ElementAt(i));

                    if (item != null)
                    {
                        item.Deleted = true;
                        Update(item);
                    }
                }
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
                var items = await Task.Run(() =>
                {
                    return _collection.AsQueryable().Where(x => entityIds.Contains(x._id));
                });

                for (int i = 0; i < entityIds.Count(); i++)
                {
                    var item = items.FirstOrDefault(x => x._id == entityIds.ElementAt(i));

                    if (item != null)
                    {
                        item.Deleted = true;
                        await UpdateAsync(item);
                    }
                }
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
    }
}
