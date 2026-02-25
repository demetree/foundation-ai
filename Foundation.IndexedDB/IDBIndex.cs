using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Foundation.IndexedDB
{
    /// <summary>
    /// 
    /// This class models an index on an object store, allowing efficient querying based on indexed properties.
    /// 
    /// </summary>
    public class IDBIndex
    {
        private readonly IDBObjectStore _store;
        private readonly IndexConfig _config;

        internal IDBIndex(IDBObjectStore store, IndexConfig config)
        {
            _store = store;
            _config = config;
        }

        internal IDBObjectStore Store
        {
            get { return _store; }
        }

        public string Name => _config.Name;
        public string KeyPath => _config.Path;
        public bool Unique => _config.Unique;

        /// <summary>
        /// Retrieves the first record where the indexed property matches the query.
        /// </summary>
        /// <param name="query">The value to match against the indexed property (string or number).</param>
        /// <returns>The deserialized record as an object, or null if no match is found.</returns>
        /// <exception cref="DataException">Thrown if the database query fails.</exception>
        public async Task<object> GetAsync(object query)
        {
            try
            {
                string queryText = null;

                Type queryType = query.GetType();

                if (queryType == typeof(string) || queryType == typeof(int) || queryType == typeof(long) || queryType == typeof(double) || queryType == typeof(float) || queryType == typeof(decimal))
                {
                    // Primitive types can be added directly
                    queryText = query.ToString();
                }
                else
                {
                    // Complex types need to be serialized to JSON
                    string queryJson = JsonSerializer.Serialize(query, IDBCommon.JsonOptions);

                    queryText = queryJson;
                }


                string safePath = _config.GetSafePath();

                //
                // Acquire the concurrency semaphore to protect the DbContext from concurrent access.
                //
                string valueJson = await _store.DB.ExecuteWithLockAsync(async () =>
                {
                    return await _store.DB._context.Database.SqlQueryRaw<string>($"SELECT ValueJson AS Value FROM Data WHERE StoreName = {{0}} AND json_extract(ValueJson, '$.{safePath}') = {{1}} LIMIT 1",
                                                                                          _store.Name, 
                                                                                          queryText)
                                                                         .FirstOrDefaultAsync()
                                                                         .ConfigureAwait(false);
                }).ConfigureAwait(false);

                return valueJson == null ? null : JsonSerializer.Deserialize<object>(valueJson, IDBCommon.JsonOptions);
            }
            catch (Exception ex)
            {
                throw new DataException($"Failed to get record from index '{Name}'.", ex);
            }
        }


        /// <summary>
        /// Retrieves the first record where the indexed property matches the query, deserialized to type T.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the record to.</typeparam>
        /// <param name="query">The value to match against the indexed property (string or number).</param>
        /// <returns>The deserialized record as T, or default(T) if no match is found.</returns>
        /// <exception cref="DataException">Thrown if the database query fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown if deserialization to T fails.</exception>
        public async Task<T> GetAsync<T>(object query)
        {
            try
            {
                string queryText = null;

                Type queryType = query.GetType();

                if (queryType == typeof(string) || queryType == typeof(int) || queryType == typeof(long) || queryType == typeof(double) || queryType == typeof(float) || queryType == typeof(decimal))
                {
                    // Primitive types can be added directly
                    queryText = query.ToString();
                }
                else
                {
                    // Complex types need to be serialized to JSON
                    string queryJson = JsonSerializer.Serialize(query, IDBCommon.JsonOptions);

                    queryText = queryJson;
                }


                string safePath = _config.GetSafePath();

                //
                // Acquire the concurrency semaphore to protect the DbContext from concurrent access.
                //
                string valueJson = await _store.DB.ExecuteWithLockAsync(async () =>
                {
                    return await _store.DB._context.Database.SqlQueryRaw<string>(
                                                                         $"SELECT ValueJson AS Value FROM Data WHERE StoreName = {{0}} AND json_extract(ValueJson, '$.{safePath}') = {{1}} LIMIT 1",
                                                                        _store.Name, 
                                                                        queryText)
                                                                        .FirstOrDefaultAsync()
                                                                        .ConfigureAwait(false);
                }).ConfigureAwait(false);

                if (valueJson == null)
                {
                    return default;
                }

                try
                {
                    return JsonSerializer.Deserialize<T>(valueJson, IDBCommon.JsonOptions);
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"Failed to deserialize to type {typeof(T).Name}. Ensure the stored data matches the expected type.", ex);
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"Failed to get record from index '{Name}'.", ex);
            }
        }


        /// <summary>
        /// Retrieves all records where the indexed property matches the query or falls within the specified range.
        /// </summary>
        /// <param name="query">The value or key range to match against the indexed property.</param>
        /// <param name="count">Optional maximum number of records to return.</param>
        /// <returns>A list of deserialized records as objects.</returns>
        /// <exception cref="DataException">Thrown if the database query fails.</exception>
        public async Task<IList<object>> GetAllAsync(object query, int? count = null)
        {
            try
            {
                string safePath = _config.GetSafePath();

                string sql = $"SELECT ValueJson AS Value FROM Data WHERE StoreName = {{0}}";

                List<Object> parameters = new List<object>();

                // Put in the store name first
                parameters.Add(_store.Name);


                // next add the query or range
                if (query is IDBKeyRange range)
                {
                    (string rangeSql, List<object> rangeParams) = BuildKeyRangeSql(range, safePath);
                    sql += $" AND {rangeSql}";
                    parameters.AddRange(rangeParams);
                }
                else
                {
                    sql += $" AND json_extract(ValueJson, '$.{safePath}') = {{1}}";

                    Type queryType = query.GetType();

                    if (queryType == typeof(string) || queryType == typeof(int) || queryType == typeof(long) || queryType == typeof(double) || queryType == typeof(float) || queryType == typeof(decimal))
                    {
                        // Primitive types can be added directly
                        parameters.Add(query);
                    }
                    else
                    {
                        // Complex types need to be serialized to JSON
                        string queryJson = JsonSerializer.Serialize(query, IDBCommon.JsonOptions);
                        parameters.Add(queryJson);
                    }
                }

                if (count.HasValue)
                {
                    sql += $" LIMIT {{{parameters.Count}}}";
                }

                // Now add count if present
                if (count.HasValue)
                {
                    parameters.Add(count.HasValue);
                }

                //
                // Acquire the concurrency semaphore to protect the DbContext from concurrent access.
                //
                List<string> valueJsons = await _store.DB.ExecuteWithLockAsync(async () =>
                {
                    return await _store.DB._context.Database
                        .SqlQueryRaw<string>(sql, parameters.ToArray())
                        .ToListAsync()
                        .ConfigureAwait(false);
                }).ConfigureAwait(false);


                return valueJsons.Select(v => JsonSerializer.Deserialize<object>(v, IDBCommon.JsonOptions)).ToList();
            }
            catch (Exception ex)
            {
                throw new DataException($"Failed to get records from index '{Name}'.", ex);
            }
        }

        /// <summary>
        /// Retrieves all records where the indexed property matches the query or falls within the specified range, deserialized to type T.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the records to.</typeparam>
        /// <param name="query">The value or key range to match against the indexed property.</param>
        /// <param name="count">Optional maximum number of records to return.</param>
        /// <returns>A list of deserialized records as T.</returns>
        /// <exception cref="DataException">Thrown if the database query fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown if deserialization to T fails.</exception>
        public async Task<IList<T>> GetAllAsync<T>(object query, int? count = null)
        {
            try
            {
                // Construct the base SQL query to select ValueJson from the Data table.
                string sql = $"SELECT ValueJson AS Value FROM Data WHERE StoreName = {{0}}";

                List<object> parameters = new List<object>();

                // Initialize parameters list with the store name.
                parameters.Add(_store.Name); // Store name first


                // Sanitize the index path to get the right path to query the JSON for
                string safePath = _config.GetSafePath();

                // Append conditions based on query type (single value or key range).
                if (query is IDBKeyRange range)
                {
                    (string rangeSql, List<object> rangeParams) = BuildKeyRangeSql(range, safePath);

                    sql += $" AND {rangeSql}";

                    parameters.AddRange(rangeParams);
                }
                else
                {
                    sql += $" AND json_extract(ValueJson, '$.{safePath}') = {{1}}";

                    Type queryType = query.GetType();

                    if (queryType == typeof(string) || queryType == typeof(int) || queryType == typeof(long) || queryType == typeof(double) || queryType == typeof(float) || queryType == typeof(decimal))
                    {
                        // Primitive types can be added directly
                        parameters.Add(query);
                    }
                    else
                    {
                        // Complex types need to be serialized to JSON
                        string queryJson = JsonSerializer.Serialize(query, IDBCommon.JsonOptions);
                        parameters.Add(queryJson);
                    }
                }

                // Add LIMIT clause if count is specified.
                if (count.HasValue)
                {
                    sql += $" LIMIT {{{parameters.Count}}}";
                    parameters.Add(count.Value);
                }

                //
                // Acquire the concurrency semaphore to protect the DbContext from concurrent access.
                //
                List<string> valueJsons = await _store.DB.ExecuteWithLockAsync(async () =>
                {
                    return await _store.DB._context.Database
                                                      .SqlQueryRaw<string>(sql, parameters.ToArray())
                                                      .ToListAsync()
                                                      .ConfigureAwait(false);
                }).ConfigureAwait(false);

                var results = new List<T>();
                foreach (var valueJson in valueJsons)
                {
                    try
                    {
                        results.Add(JsonSerializer.Deserialize<T>(valueJson, IDBCommon.JsonOptions));
                    }
                    catch (JsonException ex)
                    {
                        throw new InvalidOperationException($"Failed to deserialize to type {typeof(T).Name}. Ensure the stored data matches the expected type.", ex);
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                throw new DataException($"Failed to get records from index '{Name}'.", ex);
            }
        }


        /// <summary>
        /// Opens a cursor over records where the indexed property matches the query or falls within the specified range.
        /// </summary>
        /// <param name="query">The value or key range to match against the indexed property.</param>
        /// <param name="direction">The cursor direction ("next", "nextunique", "prev", "prevunique").</param>
        /// <returns>An IDBCursor for iterating over matching records.</returns>
        /// <exception cref="DataException">Thrown if the database query fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the direction is invalid.</exception>
        public IDBCursor<T> OpenCursor<T>(object query = null, string direction = "next")
        {
            try
            {
                string safePath = _config.GetSafePath();

                string sql = $"SELECT KeyJson, ValueJson FROM Data WHERE StoreName = {{0}}";

                List<object> parameters = new List<object> { _store.Name };

                if (query is IDBKeyRange range)
                {
                    (string rangeSql, List<object> rangeParams) = BuildKeyRangeSql(range, safePath);
                    sql += $" AND {rangeSql}";
                    parameters.AddRange(rangeParams);
                }
                else if (query != null)
                {
                    Type queryType = query.GetType();

                    if (queryType == typeof(string) || queryType == typeof(int) || queryType == typeof(long) || queryType == typeof(double) || queryType == typeof(float) || queryType == typeof(decimal))
                    {
                        // Primitive types can be added directly
                        parameters.Add(query);
                    }
                    else
                    {
                        // Complex types need to be serialized to JSON
                        string queryJson = JsonSerializer.Serialize(query, IDBCommon.JsonOptions);
                        parameters.Add(queryJson);
                    }
                }

                // Order for direction (simplified; uses index value for sorting)
                sql += direction switch
                {
                    "next" or "nextunique" => $" ORDER BY json_extract(ValueJson, '$.{safePath}') ASC",
                    "prev" or "prevunique" => $" ORDER BY json_extract(ValueJson, '$.{safePath}') DESC",
                    _ => throw new InvalidOperationException($"Invalid cursor direction: {direction}.")
                };

                //
                // Acquire the concurrency semaphore to protect the DbContext during query construction.
                // Actual query execution happens lazily in IDBCursor.ContinueAsync which acquires its own lock.
                //
                _store.DB.Semaphore.Wait();
                try
                {
                    var queryable = _store.DB._context.Database.SqlQueryRaw<IDBCursor<T>.EnumeratorEntry>(sql, parameters.ToArray());

                    return new IDBCursor<T>(queryable.AsAsyncEnumerable().GetAsyncEnumerator(), _store, direction);
                }
                finally
                {
                    _store.DB.Semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"Failed to open cursor on index '{Name}'.", ex);
            }
        }

        // Helper to build SQL for key range
        private (string Sql, List<object> Parameters) BuildKeyRangeSql(IDBKeyRange range, string path)
        {
            var conditions = new List<string>();
            var parameters = new List<object>();
            int paramIndex = 1; // Start after StoreName

            if (range.Lower != null)
            {
                conditions.Add($"json_extract(ValueJson, '$.{path}') {(range.LowerOpen ? ">" : ">=")} {{{paramIndex}}}");

                // Add the range value directly so its data type is maintained.
                parameters.Add(range.Lower);

                paramIndex++;
            }

            if (range.Upper != null)
            {
                conditions.Add($"json_extract(ValueJson, '$.{path}') {(range.UpperOpen ? "<" : "<=")} {{{paramIndex}}}");

                // Add the range value directly so its data type is maintained.
                parameters.Add(range.Upper);
            }

            var sql = string.Join(" AND ", conditions);

            return (sql, parameters);
        }
    }
}