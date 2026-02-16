using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Foundation.IndexedDB.Dexter
{
    /// <summary>
    /// 
    /// Represents a Dexie-style table (object store) providing a fluent API for data operations.
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the entity stored in the table.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for the entity.</typeparam>
    public class DexterTable<T, TKey> where T : class
    {
        private readonly IDBObjectStore _objectStore;
        private readonly DexterDatabase _db; // Reference to the parent DexterDatabase

        internal DexterTable(IDBObjectStore objectStore, DexterDatabase db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (objectStore == null)
            {
                throw new ArgumentNullException("objectStore");
            }

            _objectStore = objectStore;

            _db = db;
        }


        /// <summary>
        /// Adds a new entity to the object store with a specified key
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="key">Optional explicit key for the entity. If null and AutoIncrement is true, a key will be generated.</param>
        /// <returns>The primary key of the added entity.</returns>
        public async Task<TKey> AddAsync(T entity, TKey key)
        {
            // Note: IDBObjectStore.Add returns 'object'. We need to cast it to TKey.
            // This might require a convention for how auto-incremented keys map to TKey.
            // For simplicity, we assume TKey is compatible with the generated key.
            object resultKey = await _objectStore.AddAsync(entity, key).ConfigureAwait(false);

            return (TKey)Convert.ChangeType(resultKey, typeof(TKey));
        }

        /// <summary>
        /// Adds a new entity to the object store without specifying the key.  Key can be drawn from the object itself, depending on the keyPath property of the object store
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The primary key of the added entity.</returns>
        public async Task<TKey> AddAsync(T entity)
        {
            // Note: IDBObjectStore.Add returns 'object'. We need to cast it to TKey.
            // This might require a convention for how auto-incremented keys map to TKey.
            // For simplicity, we assume TKey is compatible with the generated key.
            object resultKey = await _objectStore.AddAsync(entity).ConfigureAwait(false);

            return (TKey)Convert.ChangeType(resultKey, typeof(TKey));
        }


        /// <summary>
        /// Adds a list of entities to the object store without specifying the key.  Key can be drawn from the object itself, depending on the keyPath property of the object store
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The list of primary keys for the added entities.</returns>
        public async Task<List<TKey>> AddListAsync(List<T> entityList)
        {
            List<object> resultObjectKeyList = await _objectStore.AddListAsync(entityList).ConfigureAwait(false);

            List<TKey> resultList = new List<TKey>();

            foreach (object objectKey in resultObjectKeyList)
            { 
                resultList.Add((TKey)Convert.ChangeType(objectKey, typeof(TKey)));
            }

            return resultList;
        }


        /// <summary>
        /// Puts (inserts or updates) an entity in the object store with the specified key.
        /// </summary>
        /// <param name="entity">The entity to put.</param>
        /// <param name="key">Optional explicit key for the entity. If null, the key will be determined from KeyPath or auto-incremented.</param>
        /// <returns>The primary key of the put entity.</returns>
        public async Task<TKey> PutAsync(T entity, TKey key)
        {
            object resultKey = await _objectStore.PutAsync(entity, key).ConfigureAwait(false);
            return (TKey)Convert.ChangeType(resultKey, typeof(TKey));
        }


        /// <summary>
        /// Puts (inserts or updates) an entity in the object store without specifying the key.  Key can be drawn from the object itself, depending on the keyPath property of the object store
        /// </summary>
        /// <param name="entity">The entity to put.</param>
        /// <param name="key">Optional explicit key for the entity. If null, the key will be determined from KeyPath or auto-incremented.</param>
        /// <returns>The primary key of the put entity.</returns>
        public async Task<TKey> PutAsync(T entity)
        {
            object resultKey = await _objectStore.PutAsync(entity).ConfigureAwait(false);
            return (TKey)Convert.ChangeType(resultKey, typeof(TKey));
        }




        /// <summary>
        /// Puts a list of entities to the object store without specifying the key.  Key can be drawn from the object itself, depending on the keyPath property of the object store
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The list of primary keys for the added entities.</returns>
        public async Task<List<TKey>> PutListAsync(List<T> entityList)
        {
            List<object> resultObjectKeyList = await _objectStore.PutListAsync(entityList).ConfigureAwait(false);

            List<TKey> resultList = new List<TKey>();

            foreach (object objectKey in resultObjectKeyList)
            {
                resultList.Add((TKey)Convert.ChangeType(objectKey, typeof(TKey)));
            }

            return resultList;
        }



        /// <summary>
        /// Retrieves an entity by its primary key.  
        /// </summary>
        /// <param name="key">The primary key of the entity to retrieve.</param>
        /// <returns>The entity, or null if not found.</returns>
        public async Task<T> GetAsync(TKey key)
        {
            return await _objectStore.GetAsync<T>(key).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an entity by its primary key.
        /// </summary>
        /// <param name="key">The primary key of the entity to delete.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(TKey key)
        {
            await _objectStore.DeleteAsync(key).ConfigureAwait(false);
        }

        /// <summary>
        /// Clears all records from the object store.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task ClearAsync()
        {
            await _objectStore.ClearAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all entities from the object store.
        /// </summary>
        /// <returns>A list of all entities.</returns>
        public async Task<List<T>> ToListAsync()
        {
            // Your IDBObjectStore.OpenCursor returns an IDBCursor<T>.
            // We need to iterate it to get all values.
            var cursor = _objectStore.OpenCursor<T>();
            var results = new List<T>();
            while (await cursor.ContinueAsync().ConfigureAwait(false))
            {
                results.Add(cursor.Value);
            }
            return results;
        }

        /// <summary>
        /// Initiates a query chain for filtering records based on a property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property to query.</typeparam>
        /// <param name="propertySelector">An expression selecting the property to query.</param>
        /// <returns>A <see cref="DexterWhereClause{T, TKey, TProperty}"/> for further filtering.</returns>
        public DexterWhereClause<T, TKey, TProperty> Where<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            // Extract the property name from the expression
            if (propertySelector.Body is MemberExpression memberExpression)
            {
                string propertyName = memberExpression.Member.Name;
                return new DexterWhereClause<T, TKey, TProperty>(_objectStore, propertyName);
            }
            else
            {
                throw new ArgumentException("Property selector must be a direct property access expression.", nameof(propertySelector));
            }
        }

        /// <summary>
        /// Counts the number of records in the object store.
        /// </summary>
        /// <param name="range">Optional key range to count within.</param>
        /// <returns>The number of records.</returns>
        public async Task<long> CountAsync(IDBKeyRange range = null)
        {
            return await _objectStore.CountAsync(range).ConfigureAwait(false);
        }

        // --- Additional Dexie-like methods can be added here ---
        // For example:
        // public async Task BulkAdd(IEnumerable<T> entities) { ... }
        // public async Task BulkPut(IEnumerable<T> entities) { ... }
        // public async Task BulkDelete(IEnumerable<TKey> keys) { ... }
        // public async Task Update(TKey key, Dictionary<string, object> changes) { ... }
    }
}
