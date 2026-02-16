using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Foundation.IndexedDB.Dexter
{
    /// <summary>
    /// Represents a Dexie-style where clause for filtering records in a table or index.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the primary key.</typeparam>
    /// <typeparam name="TProperty">The type of the property being filtered.</typeparam>
    public class DexterWhereClause<T, TKey, TProperty> where T : class
    {
        private readonly IDBObjectStore _objectStore;
        private readonly string _indexName; // This will correspond to your KeyPath for the index

        internal DexterWhereClause(IDBObjectStore objectStore, string indexName)
        {
            if (objectStore == null)
            {
                throw new ArgumentNullException(nameof(objectStore));
            }

            if (indexName == null)
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            _objectStore = objectStore;
            _indexName = indexName;
        }

        private IDBIndex GetIndex()
        {
            try
            {
                return _objectStore.Index(_indexName);
            }
            catch (NotFoundException)
            {
                // If an index doesn't exist for the property, it's a runtime error in Dexie too if you try to query it.
                // Alternatively, you could fall back to an unfiltered query and then filter in memory,
                // but that defeats the purpose of an index.
                throw new InvalidOperationException($"No index defined for property '{_indexName}'. " +
                                                    "Ensure an index is created for this property during database upgrade.");
            }
        }

        /// <summary>
        /// Filters records where the indexed property equals the specified value.
        /// </summary>
        /// <param name="value">The value to match.</param>
        /// <returns>A <see cref="DexterCollection{T, TKey}"/> containing matching records.</returns>
        public DexterCollection<T, TKey> Equals(TProperty value)
        {
            // IDBIndex.Get<T> only returns the first match.
            // For 'Equals' that can return multiple results, we should use GetAll.
            return new DexterCollection<T, TKey>(GetIndex(), value);
        }

        /// <summary>
        /// Filters records where the indexed property is strictly greater than the specified value.
        /// </summary>
        /// <param name="value">The lower bound value.</param>
        /// <returns>A <see cref="DexterCollection{T, TKey}"/> containing matching records.</returns>
        public DexterCollection<T, TKey> Above(TProperty value)
        {
            return new DexterCollection<T, TKey>(GetIndex(), IDBKeyRange.LowerBound(value, open: true));
        }

        /// <summary>
        /// Filters records where the indexed property is greater than or equal to the specified value.
        /// </summary>
        /// <param name="value">The lower bound value.</param>
        /// <returns>A <see cref="DexterCollection{T, TKey}"/> containing matching records.</returns>
        public DexterCollection<T, TKey> AboveOrEqual(TProperty value)
        {
            return new DexterCollection<T, TKey>(GetIndex(), IDBKeyRange.LowerBound(value, open: false));
        }

        /// <summary>
        /// Filters records where the indexed property is strictly less than the specified value.
        /// </summary>
        /// <param name="value">The upper bound value.</param>
        /// <returns>A <see cref="DexterCollection{T, TKey}"/> containing matching records.</returns>
        public DexterCollection<T, TKey> Below(TProperty value)
        {
            return new DexterCollection<T, TKey>(GetIndex(), IDBKeyRange.UpperBound(value, open: true));
        }

        /// <summary>
        /// Filters records where the indexed property is less than or equal to the specified value.
        /// </summary>
        /// <param name="value">The upper bound value.</param>
        /// <returns>A <see cref="DexterCollection{T, TKey}"/> containing matching records.</returns>
        public DexterCollection<T, TKey> BelowOrEqual(TProperty value)
        {
            return new DexterCollection<T, TKey>(GetIndex(), IDBKeyRange.UpperBound(value, open: false));
        }

        /// <summary>
        /// Filters records where the indexed property is within the specified range.
        /// </summary>
        /// <param name="lower">The lower bound of the range.</param>
        /// <param name="upper">The upper bound of the range.</param>
        /// <param name="includeLower">True to include the lower bound, false otherwise.</param>
        /// <param name="includeUpper">True to include the upper bound, false otherwise.</param>
        /// <returns>A <see cref="DexterCollection{T, TKey}"/> containing matching records.</returns>
        public DexterCollection<T, TKey> Between(TProperty lower, TProperty upper, bool includeLower = true, bool includeUpper = true)
        {
            return new DexterCollection<T, TKey>(GetIndex(), IDBKeyRange.Bound(lower, upper, !includeLower, !includeUpper));
        }

        /// <summary>
        /// Filters records where the indexed string property starts with the specified prefix.
        /// (Requires the property to be of type string).
        /// </summary>
        /// <param name="prefix">The prefix to match.</param>
        /// <returns>A <see cref="DexterCollection{T, TKey}"/> containing matching records.</returns>
        public DexterCollection<T, TKey> StartsWith(string prefix)
        {
            if (typeof(TProperty) != typeof(string))
            {
                throw new InvalidOperationException("StartsWith can only be used on string properties.");
            }
            // For 'StartsWith', IndexedDB typically uses a key range.
            // Lower bound is `prefix`, upper bound is `prefix + \uFFFF` (or similar for the largest possible char)
            // This assumes lexicographical ordering.
            object lower = prefix;
            object upper = prefix + IDBKeyRange.GetMaxCharForType(typeof(string)); // Helper needed
            return new DexterCollection<T, TKey>(GetIndex(), IDBKeyRange.Bound(lower, upper, false, true)); // Inclusive lower, exclusive upper
        }

        // --- Add more Dexie-like query methods here ---
        // Examples: `anyOf`, `notEqual`, `noneOf`, `or`, `and` etc.
        // Many of these might require more complex logic involving cursors or combining multiple queries.
    }
}
