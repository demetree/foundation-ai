using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Foundation.IndexedDB
{

    /// <summary>
    /// 
    /// This class models a transaction in the IndexedDB, allowing operations on object stores within the transaction context.
    /// 
    /// It supports a read only mode for querying, and a read write mode when data will change.
    /// 
    /// When using read write mode, the transaction must be either committed or aborted.  If neither is done, the transaction will be automatically aborted when disposed.
    /// 
    /// </summary>
    public class IDBTransaction : IDisposable
    {
        private readonly IDBDatabase _db;
        private IDbContextTransaction _sqliteTransaction;

        private TransactionMode _mode;

        private bool _writeModeTransactionFinalized = false;

        internal bool IsWriteModeTransactionFinalized 
        {
             get { return _writeModeTransactionFinalized; }
        }

        public enum TransactionMode
        {
            ReadOnly,
            ReadWrite
        }


        internal IDBTransaction(IDBDatabase db, string[] storeNames, TransactionMode mode)
        {
            _db = db;

            _mode = mode;


            // Begin a database transaction if in ReadWrite mode.  Readonly mode does not need an actual SQLite transaction.
            if (mode == TransactionMode.ReadWrite)
            {
                _sqliteTransaction = db._context.Database.BeginTransaction();
            }
            else
            {
                _sqliteTransaction = null;
            }


            // Validate stores exist in storeConfigs
            foreach (string storeName in storeNames)
            {
                if (!db.StoreConfigs.ContainsKey(storeName))
                {
                    throw new InvalidOperationException($"Object store '{storeName}' not found.");
                }
            }   
        }

        public IDBObjectStore ObjectStore(string name)
        {
            if (!_db.StoreConfigs.ContainsKey(name))
            {
                throw new InvalidOperationException("Object store not found.");
            }

            return new IDBObjectStore(_db, name);
        }

        public void Commit()
        {
            // No action for read-only transactions.  We could throw an exception here, but it seems more user-friendly to just do nothing.
            if (_mode == TransactionMode.ReadOnly || _writeModeTransactionFinalized == true)
            {
                return;
            }

            _sqliteTransaction.Commit();

            _sqliteTransaction.Dispose();

            _sqliteTransaction = null;

            _writeModeTransactionFinalized = true;
        }

        public void Abort()
        {
            // No action for read-only transactions.  We could throw an exception here, but it seems more user-friendly to just do nothing.
            if (_mode == TransactionMode.ReadOnly || _writeModeTransactionFinalized == true)
            {
                return;
            }

            _sqliteTransaction.Rollback();

            _sqliteTransaction.Dispose();

            _sqliteTransaction = null;

            _writeModeTransactionFinalized = true;
        }

        public void Dispose()
        {
            if (_mode == TransactionMode.ReadWrite && 
                !_writeModeTransactionFinalized && 
                _sqliteTransaction != null)
            {
                // Auto-rollback if not finalized
                _sqliteTransaction.Rollback();
            }

            if (_sqliteTransaction != null)
            {
                _sqliteTransaction.Dispose();
            }
        }
    }
}