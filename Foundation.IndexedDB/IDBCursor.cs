using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Foundation.IndexedDB
{
    /// <summary>
    /// 
    /// This class models a cursor for iterating over records in an object store or index.
    /// 
    /// </summary>
    public class IDBCursor<T> : IDisposable
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
            if (await _enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                EnumeratorEntry entry = _enumerator.Current;
                _currentKey = JsonSerializer.Deserialize<object>(entry.Key, IDBCommon.JsonOptions);
                _currentValue = JsonSerializer.Deserialize<T>(entry.Value, IDBCommon.JsonOptions);
                return true;
            }
            return false;
        }

        // TODO: Advance, Update, Delete (in transaction)

        public void Dispose()
        {
            _enumerator.DisposeAsync().AsTask().Wait();
        }
    }
}