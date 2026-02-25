using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.IndexedDB
{
    /// <summary>
    /// 
    /// This class models a cursor for iterating over records in an object store or index.
    /// 
    /// </summary>
    public class IDBCursor<T> : IDisposable, IAsyncDisposable
    {
        private readonly IAsyncEnumerator<EnumeratorEntry> _enumerator;

        private readonly IDBObjectStore _store;

        private readonly string _direction;
        private object _currentKey;
        private T _currentValue;

        internal class EnumeratorEntry
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }


        internal IDBCursor(IAsyncEnumerator<EnumeratorEntry> enumerator, IDBObjectStore store, string direction = "next")
        {
            _enumerator = enumerator;
            _store = store;
            _direction = direction;
        }


        public object Key => _currentKey;
        public T Value => _currentValue;


        public async Task<bool> ContinueAsync()
        {
            // Acquire the database semaphore to prevent concurrent DbContext access.
            // Each MoveNextAsync call executes a query against the DbContext.
            SemaphoreSlim semaphore = _store.DB.Semaphore;
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (await _enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    EnumeratorEntry entry = _enumerator.Current;
                    _currentKey = JsonSerializer.Deserialize<object>(entry.Key, IDBCommon.JsonOptions);
                    _currentValue = JsonSerializer.Deserialize<T>(entry.Value, IDBCommon.JsonOptions);
                    return true;
                }
                return false;
            }
            finally
            {
                semaphore.Release();
            }
        }


        // TODO: Advance, Update, Delete (in transaction)


        /// <summary>
        /// 
        /// Asynchronous disposal. Preferred over Dispose() in async contexts
        /// to avoid blocking the thread while waiting on the semaphore and enumerator.
        /// 
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            SemaphoreSlim semaphore = _store.DB.Semaphore;
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await _enumerator.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }


        /// <summary>
        /// 
        /// Synchronous disposal fallback. Prefer DisposeAsync() via 'await using' in async code.
        /// 
        /// </summary>
        public void Dispose()
        {
            SemaphoreSlim semaphore = _store.DB.Semaphore;
            semaphore.Wait();
            try
            {
                _enumerator.DisposeAsync().AsTask().Wait();
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
