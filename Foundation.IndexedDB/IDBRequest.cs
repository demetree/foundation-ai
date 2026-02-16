using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace Foundation.IndexedDB
{
    /// <summary>
    /// 
    /// This class models a request to the IndexedDB, providing events for success and error handling, as well as a Task for awaiting the result.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IDBRequest<T>
    {
        public event EventHandler Success;
        public event EventHandler<Exception> Error;
        public T Result { get; protected set; }
        public Exception ErrorException { get; protected set; }

        protected readonly TaskCompletionSource<T> _tcs = new();

        public Task<T> Ready => _tcs.Task;

        public IDBRequest()
        {
        }

        internal void SetResult(T result)
        {
            Result = result;

            Success?.Invoke(this, EventArgs.Empty);

            _tcs.SetResult(result);
        }

        internal void SetError(Exception ex)
        {
            ErrorException = ex;
            Error?.Invoke(this, ex);
            _tcs.SetException(ex);
        }
    }

    public class IDBOpenDBRequest : IDBRequest<IDBDatabase>
    {
        // Note that database object setup should be done in the callback to the factory creating the object store
    }

    public class IDBVersionChangeEventArgs : EventArgs
    {
        public IDBDatabase Database { get; }
        public uint OldVersion { get; }
        public uint NewVersion { get; }

        public IDBVersionChangeEventArgs(IDBDatabase db, uint oldVer, uint newVer)
        {
            Database = db;
            OldVersion = oldVer;
            NewVersion = newVer;
        }
    }
}