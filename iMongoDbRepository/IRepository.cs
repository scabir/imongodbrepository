using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace iMongoDbRepository
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        bool Configured { get; }

        void Configure(DbConfiguration dbConfiguration);

        /// <summary>
        ///     Returns all item on the database
        /// </summary>
        /// <param name="maxNumberOfRows"></param>
        /// <param name="includeDeleted">Includes soft deleted items</param>
        /// <returns></returns>
        IEnumerable<TEntity> All(int maxNumberOfRows = 100000, bool includeDeleted = false);

        Task<IEnumerable<TEntity>> AllAsync(int maxNumberOfRows = 100000, bool includeDeleted = false);

        /// <summary>
        /// Pass your query in as a parameter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="maxNumberOfRows"></param>
        /// <param name="includeDeleted">Includes soft deleted items</param>
        /// <returns></returns>
        IEnumerable<TEntity> Query(Expression<Func<TEntity, bool>> filter, int maxNumberOfRows = 100000, bool includeDeleted = false);

        Task<IEnumerable<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> filter, int maxNumberOfRows = 100000, bool includeDeleted = false);

        /// <summary>
        ///     Get a single item
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="includeDeleted">Includes soft deleted items</param>
        /// <returns></returns>
        TEntity Get(string entityId, bool includeDeleted = false);

        Task<TEntity> GetAsync(string entityId, bool includeDeleted = false);

        /// <summary>
        /// Counts filtered documents.
        /// </summary>
        /// <returns></returns>
        long Count(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false);

        Task<long> CountAsync(bool includeDeleted = false);

        /// <summary>
        /// Counts all documents
        /// </summary>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        long Count(bool includeDeleted = false);

        Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false);

        bool Any(bool includeDeleted = false);

        Task<bool> AnyAsync(bool includeDeleted = false);

        bool Any(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, bool includeDeleted = false);

        /// <summary>
        ///     Insert an item
        /// </summary>
        /// <param name="entity"></param>
        void Insert(TEntity entity);

        /// <summary>
        ///     Insert an item
        /// </summary>
        /// <param name="entity"></param>
        Task InsertAsync(TEntity entity);

        /// <summary>
        ///     Insert multiple items
        /// </summary>
        /// <param name="entities"></param>
        void Insert(IEnumerable<TEntity> entities);

        /// <summary>
        ///     Insert multiple items
        /// </summary>
        /// <param name="entities"></param>
        Task InsertAsync(IEnumerable<TEntity> entities);

        /// <summary>
        ///     Delete the item with the given Id
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="hardDelete">Soft delete will mark it as deleted, hard delete will actually delete.</param>
        void Delete(string entityId, bool hardDelete = false);

        /// <summary>
        ///     Delete the item with the given Id
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="hardDelete">Soft delete will mark it as deleted, hard delete will actually delete.</param>
        Task DeleteAsync(string entityId, bool hardDelete = false);

        /// <summary>
        /// Delete given ids.
        /// </summary>
        /// <param name="entityIds"></param>
        /// <param name="hardDelete"></param>
        void Delete(IEnumerable<string> entityIds, bool hardDelete = false);

        /// <summary>
        /// Delete given ids.
        /// </summary>
        /// <param name="entityIds"></param>
        /// <param name="hardDelete"></param>
        Task DeleteAsync(IEnumerable<string> entityIds, bool hardDelete = false);

        /// <summary>
        /// Recovers a soft deleted item.
        /// </summary>
        /// <param name="entityId"></param>
        void UnDelete(string entityId);

        /// <summary>
        /// Recovers a soft deleted item.
        /// </summary>
        /// <param name="entityId"></param>
        Task UnDeleteAsync(string entityId);

        /// <summary>
        ///     Update an item
        /// </summary>
        /// <param name="entity"></param>
        void Update(TEntity entity);

        /// <summary>
        ///     Update an item
        /// </summary>
        /// <param name="entity"></param>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// Deletes all hard-deleted records from the database that are older than the specified number of days.
        /// </summary>
        /// <param name="days">The number of days to consider when deleting records. Defaults to 30.</param>
        void CleanHardDeleted(int days = 30);

        /// <summary>
        /// Deletes all hard-deleted records from the database that are older than the specified number of days asynchronously.
        /// </summary>
        /// <param name="days">The number of days to consider when deleting records. Defaults to 30.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        Task CleanHardDeletedAsync(int days = 30);
    }
}
