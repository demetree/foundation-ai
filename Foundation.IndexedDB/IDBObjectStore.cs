using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EFCore.BulkExtensions;

namespace Foundation.IndexedDB
{
    /// <summary>
    /// 
    /// This class models an object store within the database, allowing adding and retrieving records.
    /// 
    /// </summary>
    public class IDBObjectStore
    {
        private string _name;

        private readonly IDBDatabase _db;

        internal IDBDatabase DB { get { return _db; } }

        private ObjectStoreConfig _config;

        public string Name 
        { 
            get { return _name; } 
        }

        public string KeyPath 
        {
            get { return _config.KeyPath; }
        }
        public bool AutoIncrement
        { 
            get { return _config.AutoIncrement; } 
        }

        public IEnumerable<string> IndexNames
        {
            get
            {
                return _config.Indexes.Select(i => i.Name);
            }
        }


        public class IndexOptions
        {
            [JsonPropertyName("unique")]
            public bool Unique { get; set; }
        }


        /// <summary>
        /// 
        /// This constructs the object store for the specified database and name.
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="name"></param>
        internal IDBObjectStore(IDBDatabase db, string name)
        {
            _db = db;
            _name = name;
            _config = _db.StoreConfigs[name];
        }


        /// <summary>
        /// 
        /// This adds a new record to the object store, optionally determining the key based on the provided value and options.
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<object> AddAsync<T>(T value, object key = null)
        {
            return await _db.ExecuteWithLockAsync(async () =>
            {
                try
                {
                    object usedKey = await DetermineKeyAsync(value, key).ConfigureAwait(false);

                    string keyJson = JsonSerializer.Serialize(usedKey, IDBCommon.JsonOptions);

                    string valueJson = JsonSerializer.Serialize(value, IDBCommon.JsonOptions);


                    // Check for existing key (constraint)
                    if (await _db._context.Data.AnyAsync(d => d.storeName == Name && d.keyJson == keyJson).ConfigureAwait(false))
                    {
                        throw new ConstraintException($"Key {usedKey.ToString()} already exists.");
                    }

                    _db._context.Data.Add(new Data { storeName = Name, keyJson = keyJson, valueJson = valueJson });


                    await _db._context.SaveChangesAsync().ConfigureAwait(false);

                    return usedKey;
                }
                catch (DbUpdateException dbex)
                {
                    throw new DataException($"Failed to Add record for object of type {typeof(T).Name}.  Database error encountered.", dbex);
                }
                catch (ConstraintException)
                {
                    // pass constraint exceptions up as is-is
                    throw;            
                }
                catch (Exception ex)
                {
                    throw new NotSupportedException($"Unable to add data for object of type {typeof(T).Name}.", ex);
                }
            }).ConfigureAwait(false);
        }


        // generic Put (Upsert)
        public async Task<object> PutAsync<T>(T value, object key = null)
        {
            if (value == null)
            {
                throw new ArgumentException("Value parameter must not be null.");
            }

            return await _db.ExecuteWithLockAsync(async () =>
            {
                try
                {
                    object usedKey = await DetermineKeyAsync(value, key).ConfigureAwait(false);

                    string keyJson = JsonSerializer.Serialize(usedKey, IDBCommon.JsonOptions);
                    string valueJson = JsonSerializer.Serialize(value, IDBCommon.JsonOptions);

                    Data existing = await _db._context.Data.FirstOrDefaultAsync(d => d.storeName == Name && d.keyJson == keyJson).ConfigureAwait(false);

                    if (existing != null)
                    {
                        existing.valueJson = valueJson;

                        _db._context.Data.Update(existing);
                    }
                    else
                    {
                        _db._context.Data.Add(new Data { storeName = Name, keyJson = keyJson, valueJson = valueJson });
                    }

                    await _db._context.SaveChangesAsync().ConfigureAwait(false);

                    return usedKey;
                }
                catch (DbUpdateException dbex)
                {
                    throw new DataException($"Failed to put record for object of type {typeof(T).Name}.  Database error encountered.", dbex);
                }
                catch (ConstraintException)
                {
                    // pass constraint exceptions up as is-is
                    throw;
                }
                catch (Exception ex)
                {
                    throw new NotSupportedException($"Unable to put record for object of type {typeof(T).Name}.", ex);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// Adds a list of objects to the object store database.
        /// 
        /// Optional key list parameter to provide keys to use for items in the value list.  If provided, its count must match the value count
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueList"></param>
        /// <param name="keyList"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DataException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public async Task<List<object>> AddListAsync<T>(List<T> valueList, List<object> keyList = null)
        {
            if (valueList == null || valueList.Count == 0)
            {
                throw new ArgumentException("Value list parameter must not be null and have entries.");
            }

            if (keyList != null && keyList.Count != valueList.Count)
            {
                throw new ArgumentException("If providing a key list, it must have the same number of entries as the value list.");
            }

            return await _db.ExecuteWithLockAsync(async () =>
            {
                // Get all the existing keys
                HashSet<string> preloadedExistingKeys = await _db._context.Data.Where(d => d.storeName == Name).Select(d => d.keyJson).ToHashSetAsync().ConfigureAwait(false);

                try
                {
                    List<Data> bulkAddList = new List<Data>();
                    List<object> keysToReturn = new List<object>();

                    for (int i = 0; i < valueList.Count; i++)
                    {
                        // Get the value
                        T value = valueList[i];

                        // Get the key, if it has been provided
                        object key = null;
                        if (keyList != null)
                        {
                            key = keyList[i];
                        }

                        object usedKey = await DetermineKeyAsync(value, key).ConfigureAwait(false);

                        string keyJson = JsonSerializer.Serialize(usedKey, IDBCommon.JsonOptions);

                        string valueJson = JsonSerializer.Serialize(valueList, IDBCommon.JsonOptions);


                        //
                        // Check for existing key in the pre loaded list.
                        //
                        if (preloadedExistingKeys.Contains(keyJson) == true)
                        {
                            throw new ConstraintException($"Key {usedKey.ToString()} already exists.");
                        }
                        else
                        {
                            // Add the key to the list of existing to handle possible duplicates in input data pre-addition.
                            preloadedExistingKeys.Add(keyJson);
                        }

                        bulkAddList.Add(new Data { storeName = Name, keyJson = keyJson, valueJson = valueJson });

                        keysToReturn.Add(usedKey);
                    }

                    //
                    // Do a bulk insert
                    //
                    await _db._context.BulkInsertAsync(bulkAddList, options =>
                    {
                        options.BatchSize = 5000;
                        options.SetOutputIdentity = false;           // we don't need to to read the ids after the bulk save
                    }).ConfigureAwait(false);


                    await _db._context.SaveChangesAsync().ConfigureAwait(false);

                    return keysToReturn;
                }
                catch (DbUpdateException dbex)
                {
                    throw new DataException($"Failed to add list of records for object of type {typeof(T).Name}.  Database error encountered.", dbex);
                }
                catch (ConstraintException)
                {
                    // pass constraint exceptions up as is-is
                    throw;
                }
                catch (Exception ex)
                {
                    throw new NotSupportedException($"Unable to add list of data for object of type {typeof(T).Name}.", ex);
                }
            }).ConfigureAwait(false);
        }



        public async Task<List<object>> PutListAsync<T>(List<T> valueList, List<object> keyList = null)
        {
            if (valueList == null || valueList.Count == 0)
            {
                throw new ArgumentException("Value list parameter must not be null and have entries.");
            }

            if (keyList != null && keyList.Count != valueList.Count)
            {
                throw new ArgumentException("If providing a key list, it must have the same number of entries as the value list.");
            }

            return await _db.ExecuteWithLockAsync(async () =>
            {
                // Get all the existing keys
                HashSet<string> preloadedExistingKeys = await _db._context.Data.Where(d => d.storeName == Name).Select(d => d.keyJson).ToHashSetAsync().ConfigureAwait(false);

                try
                {
                    List<Data> bulkAddList = new List<Data>();
                    List<object> keysToReturn = new List<object>();

                    for (int i = 0; i < valueList.Count; i++)
                    {
                       try
                        {
                            // Get the value
                            T value = valueList[i];

                            // Get the key, if it has been provided
                            object key = null;
                            if (keyList != null)
                            {
                                key = keyList[i];
                            }

                            object usedKey = await DetermineKeyAsync(value, key).ConfigureAwait(false);

                            string keyJson = JsonSerializer.Serialize(usedKey, IDBCommon.JsonOptions);
                            string valueJson = JsonSerializer.Serialize(value, IDBCommon.JsonOptions);


                            //
                            // Check for existing key in the pre loaded list.
                            //
                            if (preloadedExistingKeys.Contains(keyJson) == true)
                            {
                                Data existing = await _db._context.Data.FirstOrDefaultAsync(d => d.storeName == Name && d.keyJson == keyJson).ConfigureAwait(false);

                                if (existing != null)
                                {
                                    existing.valueJson = valueJson;

                                    // Do an update on the changed entities.

                                    _db._context.Data.Update(existing);
                                }
                                else
                                {
                                    // Shouldn't happen
                                    bulkAddList.Add(new Data { storeName = Name, keyJson = keyJson, valueJson = valueJson });
                                }
                            }
                            else
                            {
                                // directly add a new item to the bulk insert list
                                bulkAddList.Add(new Data { storeName = Name, keyJson = keyJson, valueJson = valueJson });
                            }

                            keysToReturn.Add(usedKey);
                        }
                        catch (DbUpdateException dbex)
                        {
                            throw new DataException($"Failed to put record for object of type {typeof(T).Name}.  Database error encountered.", dbex);
                        }
                        catch (ConstraintException)
                        {
                            // pass constraint exceptions up as is-is
                            throw;
                        }
                        catch (Exception ex)
                        {
                            throw new NotSupportedException($"Unable to put record for object of type {typeof(T).Name}.", ex);
                        }
                    }

                    // First save the queued changes for the updated entities, just to get them flushed and make sure that the bulk insert starts fresh.
                    await _db._context.SaveChangesAsync().ConfigureAwait(false);

                    //
                    // Do a bulk insert for the new records
                    //
                    await _db._context.BulkInsertAsync(bulkAddList, options =>
                    {
                        options.BatchSize = 5000;
                        options.SetOutputIdentity = false;           // we don't need to to read the ids after the bulk save
                    }).ConfigureAwait(false);


                    await _db._context.SaveChangesAsync().ConfigureAwait(false);

                    return keysToReturn;
                }
                catch (DbUpdateException dbex)
                {
                    throw new DataException($"Failed to add list of records for object of type {typeof(T).Name}.  Database error encountered.", dbex);
                }
                catch (ConstraintException)
                {
                    // pass constraint exceptions up as is-is
                    throw;
                }
                catch (Exception ex)
                {
                    throw new NotSupportedException($"Unable to add list of data for object of type {typeof(T).Name}.", ex);
                }
            }).ConfigureAwait(false);
        }


        // Delete by key or range
        public async Task DeleteAsync(object query)
        {
            await _db.ExecuteWithLockAsync(async () =>
            {
                try
                {
                    IQueryable<Data> q = _db._context.Data.Where(d => d.storeName == Name);
                    if (query is IDBKeyRange range)
                    {
                        q = ApplyKeyRange(q, range);
                    }
                    else
                    {
                       var keyJson = JsonSerializer.Serialize(query, IDBCommon.JsonOptions);
                        q = q.Where(d => d.keyJson == keyJson);
                    }

                    _db._context.Data.RemoveRange(await q.ToListAsync().ConfigureAwait(false));
                    await _db._context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateException ex)
                {
                    throw new DataException("Failed to delete record(s).", ex);
                }
            }).ConfigureAwait(false);
        }

        // Clear all records
        public async Task ClearAsync()
        {
            await _db.ExecuteWithLockAsync(async () =>
            {
                try
                {
                    List<Data> toClear = await _db._context.Data.Where(d => d.storeName == _name).ToListAsync().ConfigureAwait(false);

                    _db._context.Data.RemoveRange(toClear);

                    await _db._context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateException ex)
                {
                    throw new DataException($"Failed to clear store {_name}.", ex);
                }
            }).ConfigureAwait(false);
        }

        //
        // Count records (optional range)
        //
        public async Task<long> CountAsync(IDBKeyRange range = null)
        {
            return await _db.ExecuteWithLockAsync(async () =>
            {
                var q = _db._context.Data.Where(d => d.storeName == Name);

                if (range != null)
                {
                    q = ApplyKeyRange(q, range);
                }

                return await q.LongCountAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        // Get Index
        public IDBIndex Index(string name)
        {
            IndexConfig indexConfig = _config.Indexes.FirstOrDefault(i => i.Name == name);

            if (indexConfig == null)
            {
                throw new NotFoundException($"Index '{name}' not found.");
            }

            return new IDBIndex(this, indexConfig);
        }

        // Delete Index
        public void DeleteIndexAsync(string name)
        {
            IndexConfig config = _config.Indexes.FirstOrDefault(i => i.Name == name);

            if (config == null)
            {
                throw new NotFoundException($"Index '{name}' not found.");
            }

            _config.Indexes.Remove(config);

            _db.UpdateMetaAsync($"store_{Name}", JsonSerializer.Serialize(_config, IDBCommon.JsonOptions)).Wait();

            _db.Semaphore.Wait();
            try
            {
                _db._context.Database.ExecuteSqlRawAsync($"DROP INDEX IF EXISTS idx_{Name}_{name}").Wait();
            }
            finally
            {
                _db.Semaphore.Release();
            }
        }


        // Open Cursor (value cursor)
        public IDBCursor<T> OpenCursor<T>(IDBKeyRange range = null, string direction = "next")
        {
            IQueryable<IDBCursor<T>.EnumeratorEntry> q = _db._context.Data.Where(d => d.storeName == Name)
                                                             .Select(d => new IDBCursor<T>.EnumeratorEntry { Key = d.keyJson, Value = d.valueJson });


            if (range != null)
            {
                q = ApplyKeyRange(q, range, true); // Apply to anonymous type
            }

            // Order for direction (simplified; assumes key is comparable)
            q = direction switch
            {
                "next" or "nextunique" => q.OrderBy(d => d.Key),
                "prev" or "prevunique" => q.OrderByDescending(d => d.Key),
                _ => throw new InvalidOperationException("Invalid direction.")
            };

            
            return new IDBCursor<T>(q.AsAsyncEnumerable().GetAsyncEnumerator(), this, direction);
        }


        // Open Key Cursor (keys only)
        public IDBCursor<T> OpenKeyCursor<T>(IDBKeyRange range = null, string direction = "next")
        {
            //var q = _db._context.Data.Where(d => d.storeName == Name)
            //                         .Select(d => new { d.keyJson, valueJson = (string)null }); // No value

            IQueryable<IDBCursor<T>.EnumeratorEntry> q = _db._context.Data.Where(d => d.storeName == Name)
                                                     .Select(d => new IDBCursor<T>.EnumeratorEntry { Key = d.keyJson, Value = null }); // No value

            if (range != null)
            {
                q = ApplyKeyRange(q, range, true);
            }

            q = direction switch
            {
                "next" or "nextunique" => q.OrderBy(d => d.Key),
                "prev" or "prevunique" => q.OrderByDescending(d => d.Key),
                _ => throw new InvalidOperationException("Invalid direction.")
            };

            //return new IDBCursor(q.ToAsyncEnumerable().Select(d => (d.KeyJson, d.ValueJson)), this, direction);

            return new IDBCursor<T>(q.AsAsyncEnumerable().GetAsyncEnumerator(), this, direction);
        }


        /// <summary>
        /// 
        /// Applies a key range filter to an IQueryable, generating SQL conditions for the specified range.
        /// 
        /// </summary>
        /// <typeparam name="T">The queryable type (DataEntry or anonymous type with KeyJson).</typeparam>
        /// <param name="q">The input queryable to filter.</param>
        /// <param name="range">The key range to apply (lower, upper, open/closed bounds).</param>
        /// <param name="isAnon">Indicates if T is an anonymous type with KeyJson property.</param>
        /// <returns>The filtered IQueryable with range conditions applied.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the range is invalid or key types are unsupported.</exception>
        private IQueryable<T> ApplyKeyRange<T>(IQueryable<T> q, IDBKeyRange range, bool isAnon = false) where T : class
        {
            if (range == null)
            {
                return q;
            }

            // Validate range key types
            if ((range.Lower != null && range.Lower is not (string or double or int or long or float or decimal)) ||
                (range.Upper != null && range.Upper is not (string or double or int or long or float or decimal)))
            {
                throw new InvalidOperationException("Key range bounds must be strings or numbers.");
            }

            var conditions = new List<string>();
            var parameters = new List<object>();
            int paramIndex = 0;

            // Determine the key expression (KeyJson for object store
            string keyExpression = isAnon ? "keyJson" : "d.keyJson"; // For DataEntry, use d.KeyJson


            // Apply lower bound
            if (range.Lower != null)
            {
                var lowerJson = JsonSerializer.Serialize(range.Lower, IDBCommon.JsonOptions);
                conditions.Add($"{keyExpression} {(range.LowerOpen ? ">" : ">=")} {{{paramIndex}}}");
                parameters.Add(lowerJson);
                paramIndex++;
            }

            // Apply upper bound
            if (range.Upper != null)
            {
                var upperJson = JsonSerializer.Serialize(range.Upper, IDBCommon.JsonOptions);
                conditions.Add($"{keyExpression} {(range.UpperOpen ? "<" : "<=")} {{{paramIndex}}}");
                parameters.Add(upperJson);
                paramIndex++;
            }

            if (conditions.Count == 0)
                return q;

            // Combine conditions into a single WHERE clause
            var whereClause = string.Join(" AND ", conditions);

            // Use raw SQL to append the WHERE clause
            // Note: EF Core's FromSqlRaw doesn't directly support appending WHERE to an existing IQueryable,
            // so we use a subquery approach or rely on caller to handle ordering
            string sql = $"SELECT * FROM ({q.ToQueryString()}) AS sub WHERE {whereClause}";

            return _db._context.Database
                .SqlQueryRaw<T>(sql, parameters.ToArray())
                .AsQueryable();
        }



        /// <summary>
        /// 
        /// This gets an object based on the provided key.
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<object> GetAsync(object key)
        {
            return await _db.ExecuteWithLockAsync(async () =>
            {
                string  keyJson = JsonSerializer.Serialize(key, IDBCommon.JsonOptions);

                string valueJson = await _db._context.Data.Where(d => d.storeName == Name && d.keyJson == keyJson)
                                                          .Select(d => d.valueJson)
                                                          .FirstOrDefaultAsync()
                                                          .ConfigureAwait(false);

                if (valueJson == null)
                {
                    return null;
                }
                else
                {
                    return JsonSerializer.Deserialize<object>(valueJson, IDBCommon.JsonOptions);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// This gets an object of the specified type based on the provided key.
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<T> GetAsync<T>(object key)
        {
            return await _db.ExecuteWithLockAsync(async () =>
            {
                string keyJson = JsonSerializer.Serialize(key, IDBCommon.JsonOptions);

                string valueJson = await _db._context.Data.Where(d => d.storeName == Name && d.keyJson == keyJson)
                                                          .Select(d => d.valueJson)
                                                          .FirstOrDefaultAsync()
                                                          .ConfigureAwait(false);


                if (valueJson == null)
                {
                    return default;
                }

                try
                {
                    return JsonSerializer.Deserialize<T>(valueJson, IDBCommon.JsonOptions);
                }
                catch (JsonException jex)
                {
                    throw new InvalidOperationException($"Failed to deserialize to type {typeof(T).Name}. Ensure the stored data matches the expected type.", jex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not get object of type {typeof(T).Name} with key of {keyJson}", ex);
                }
            }).ConfigureAwait(false);
        }


        /// <summary>
        /// 
        /// This gets the key value from the value object
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        private async Task<object> DetermineKeyAsync(object value, object key)
        {
            if (_config.AutoIncrement && key == null)
            {
                string nextKeyStr = await _db._context.Metadata.Where(m => m.Key == $"nextKey_{Name}")
                                                               .Select(m => m.Value)
                                                               .FirstOrDefaultAsync().ConfigureAwait(false) ?? "1";

                long nextKey = long.Parse(nextKeyStr);

                //
                // If the object has a keypath, then put the value into the object's key field.
                //
                if (!string.IsNullOrEmpty(_config.KeyPath))
                {
                    Type valueType = value.GetType();

                    PropertyInfo prop = valueType.GetProperty(_config.KeyPath, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    if (prop == null)
                    {
                        throw new InvalidOperationException($"KeyPath '{_config.KeyPath}' not found in value type {valueType.Name}.");
                    }

                    //
                    // Map the key in for long, int, and string data types.
                    //
                    if (prop.PropertyType == typeof(long))
                    {
                        prop.SetValue(value, nextKey);
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        if (nextKey > int.MinValue && nextKey < int.MaxValue)
                        {
                            prop.SetValue(value, (int)nextKey);
                        }
                    }
                    else if (prop.PropertyType == typeof(string)) 
                    {
                        prop.SetValue(value, nextKey.ToString());
                    }
                }

                // Use internal (no-lock) variant — we're already inside ExecuteWithLockAsync
                await _db.UpdateMetaInternalAsync($"nextKey_{Name}", (nextKey + 1).ToString()).ConfigureAwait(false);

                return nextKey;
            }
            else if (key != null)
            {
                if (key is not (string or double or int or long or float or decimal))
                {
                    throw new NotSupportedException("Key must be a number or string.");
                }

                //
                // We have a key provided, but there is also a keypath specified on the object store meta data.  Validate to make sure that the key values are the same.
                //
                // If not, throw an error.
                //
                if (!string.IsNullOrEmpty(_config.KeyPath))
                {
                    object keyFromObject = GetKeyValueFromObject(value);
                    
                    //
                    // Compare the object values for equivalency using the equality comparer
                    //
                    if (!EqualityComparer<object>.Default.Equals(key, keyFromObject))
                    {
                        throw new InvalidOperationException($"Key provided but keyPath defined, and the keys are not the same.  Key provided is {key.ToString()} and key from object is {keyFromObject.ToString()}.");
                    }
                }

                return key;
            }
            else if (!string.IsNullOrEmpty(_config.KeyPath))
            {
                return GetKeyValueFromObject(value);
            }

            throw new InvalidOperationException("Key required.");
        }

        /// <summary>
        /// This retrieves the key value from the provided object according to the configuration's key path
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        private object GetKeyValueFromObject(object value)
        {

            // Extract key from value using reflection
            Type valueType = value.GetType();

            PropertyInfo prop = valueType.GetProperty(_config.KeyPath, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop == null)
            {
                throw new InvalidOperationException($"KeyPath '{_config.KeyPath}' not found in value type {valueType.Name}.");
            }

            object keyValue = prop.GetValue(value);

            if (keyValue == null)
            {
                throw new InvalidOperationException($"KeyPath '{_config.KeyPath}' value is null.");
            }

            if (keyValue is not (string or double or int or long or float or decimal))
            {
                throw new NotSupportedException("Key must be a number or string.");
            }

            return keyValue;
        }

        /// <summary>
        /// 
        /// This creates an index on the specified path within the stored JSON objects.
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task CreateIndexAsync(string name, string path, IndexOptions options = null)
        {

            if (options == null)
            {
                options = new IndexOptions();
            }


            IndexConfig indexConfig = new IndexConfig { Name = name, Path = path, Unique = options.Unique };

            _config.Indexes.Add(indexConfig);


            await _db.UpdateMetaAsync($"store_{Name}", JsonSerializer.Serialize(_config, IDBCommon.JsonOptions)).ConfigureAwait(false);

            string uniqueStr = options.Unique ? "UNIQUE " : "";

            //
            // Note that the SQLite json_extract is used to implement the index on the JSON path on the value in the Data table.
            //
            // https://sqldocs.org/sqlite-database/sqlite-json-data/
            //
            await _db.ExecuteWithLockAsync(async () =>
            {
                await _db._context.Database.ExecuteSqlRawAsync($"CREATE {uniqueStr}INDEX IF NOT EXISTS idx_{Name}_{name} ON Data (json_extract(ValueJson, '$.{path}')) WHERE StoreName = '{Name}'").ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}