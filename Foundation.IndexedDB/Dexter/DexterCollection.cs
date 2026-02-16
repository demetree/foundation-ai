using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foundation.IndexedDB.Dexter
{
    /// <summary>
    /// Represents a collection of entities resulting from a query, allowing further operations.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the primary key.</typeparam>
    public class DexterCollection<T, TKey> where T : class
    {
        private readonly IDBIndex _index;
        private readonly object _query; // Can be a single value or an IDBKeyRange
        private int? _limitCount; // For .Limit()

        internal DexterCollection(IDBIndex index, object query)
        {
            _index = index ?? throw new ArgumentNullException(nameof(index));
            _query = query;
        }

        /// <summary>
        /// Retrieves the first matching entity in the collection.
        /// </summary>
        /// <returns>The first entity, or null if no match is found.</returns>
        public async Task<T> First()
        {
            // If query is a single value, use Get<T>. If it's a range, use GetAll<T> with limit 1.
            if (!(_query is IDBKeyRange))
            {
                return await _index.GetAsync<T>(_query).ConfigureAwait(false);
            }
            else
            {
                var results = await _index.GetAllAsync<T>(_query, 1).ConfigureAwait(false);
                return results.FirstOrDefault();
            }
        }

        /// <summary>
        /// Retrieves all matching entities in the collection as a list.
        /// </summary>
        /// <returns>A list of all matching entities.</returns>
        public async Task<List<T>> ToArray()
        {
            var results = await _index.GetAllAsync<T>(_query, _limitCount).ConfigureAwait(false);

            return results.ToList();
        }

        /// <summary>
        /// Limits the number of results returned by subsequent operations.
        /// </summary>
        /// <param name="count">The maximum number of records to return.</param>
        /// <returns>The current <see cref="DexterCollection{T, TKey}"/> instance for chaining.</returns>
        public DexterCollection<T, TKey> Limit(int count)
        {
            _limitCount = count;
            return this;
        }

        /// <summary>
        /// Counts the number of matching records in the collection.
        /// </summary>
        /// <returns>The number of matching records.</returns>
        public async Task<long> Count()
        {
            // Your IDBIndex.GetAll<T> is already fetching all then counting.
            // We need to use IDBObjectStore.Count with a range constructed from _query.
            // This is a bit tricky because IDBIndex only has GetAll with object query.
            // For now, we'll fetch all and count in memory if _query is a simple value.
            // A more efficient implementation would involve the IDBIndex also having a Count method.

            if (_query is IDBKeyRange range)
            {
                // If it's a range, we can pass it directly to the object store's count
                // (Assuming index can also apply ranges for counting - which your IDBObjectStore.Count does)
                return await _index.Store.CountAsync(range).ConfigureAwait(false); // Direct access to internal _store.
            }
            else
            {
                // For a single value query, we fetch and count
                return (await _index.GetAllAsync<T>(_query, _limitCount).ConfigureAwait(false)).Count;
            }
        }

        // --- Additional Dexie-like methods can be added here ---
        // For example: `each`, `modify`, `delete` for the current collection.
    }
}
