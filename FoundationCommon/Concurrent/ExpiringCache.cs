using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using MessagePack;
using K4os.Compression.LZ4;
using System.Collections.Generic;

namespace Foundation.Concurrent
{
    /// <summary>
    /// 
    /// This is an expiring cache class that allows for self management of the data cache that will expire after the defined time interval.  It has these optional features:
    /// 
    /// - Sliding expiry, so upon access the clock will restart for the cache item
    /// - Compression of data to save memory, but increase processing load.  B
    ///     - if using these, be mindful of these things which could blow up in your face:
    ///         1.) Message Pack serialization is used, so if circular references are in your object, the fields must be decorated with [IgnoreMember] or you will have problems
    ///         2.) Further to the above, the fact that circular references, such as those to parents, won't be restored upon reconstruction because they weren't serialized.
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ExpiringCache<TKey, TValue> : IDisposable
    {
        private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> _cache;
        private readonly int _expirationInSeconds;
        private readonly bool _useSlidingExpiration;
        private readonly bool _useCompression;
        private readonly Timer _timer;

        public ExpiringCache(int expirationInSeconds, bool useSlidingExpiration = false, bool useCompression = false)
        {
            if (expirationInSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expirationInSeconds), "Expiration must be positive.");
            }

            _expirationInSeconds = expirationInSeconds;
            _useSlidingExpiration = useSlidingExpiration;
            _useCompression = useCompression;

            _cache = new ConcurrentDictionary<TKey, CacheItem<TValue>>();

            //
            // Start a timer to check for expired items periodically  
            //
            _timer = new Timer(CheckForExpiredItems, null, TimeSpan.Zero, TimeSpan.FromSeconds(Math.Max(1, expirationInSeconds / 10)));
        }

        public int Count { get { return _cache.Count; } }

        /// <summary>
        ///  This is here to make interface similar to concurrent dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void TryAdd(TKey key, TValue value)
        {
            Add(key, value);

            return;
        }

        public void Add(TKey key, TValue value)
        {
            CacheItem<TValue> cacheItem;

            if (_useCompression)
            {
                //
                // Serialize and compress the value
                //
                byte[] serialized = MessagePackSerializer.Serialize(value);
                byte[] compressed = new byte[LZ4Codec.MaximumOutputSize(serialized.Length)];
                int compressedLength = LZ4Codec.Encode(serialized, 0, serialized.Length, compressed, 0, compressed.Length, LZ4Level.L00_FAST);
                if (compressedLength < 0)
                {
                    throw new InvalidOperationException("Compression failed.");
                }

                //
                // Resize buffer to exact size
                //
                Array.Resize(ref compressed, compressedLength);
                cacheItem = new CacheItem<TValue>(compressed, serialized.Length, DateTime.UtcNow.AddSeconds(_expirationInSeconds), true);
            }
            else
            {
                cacheItem = new CacheItem<TValue>(value, 0, DateTime.UtcNow.AddSeconds(_expirationInSeconds), false);
            }

            _cache.AddOrUpdate(key,
                               cacheItem,
                               (k, existingVal) => cacheItem);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;

            if (_cache.TryGetValue(key, out var cacheItem))
            {
                if (cacheItem.ExpirationTime > DateTime.UtcNow)
                {
                    value = cacheItem.GetValue();

                    if (_useSlidingExpiration)
                    {
                        //
                        // Update expiration time atomically
                        //
                        _cache.AddOrUpdate(key,
                            cacheItem, // Shouldn't hit this path
                            (k, existing) => new CacheItem<TValue>(
                                existing.IsCompressed ? existing.CompressedValue : existing.Value,
                                existing.DecompressedSize,
                                DateTime.UtcNow.AddSeconds(_expirationInSeconds),
                                existing.IsCompressed));

                    }
                    return true;
                }
                else
                {
                    //
                    // Item has expired, remove it
                    //
                    _cache.TryRemove(key, out _);
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// Returns true if the cache contains the provided key.  The sliding expiry time for items in the cache is updated when this is accessed, if running in sliding expiry mode.
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            if (_cache.TryGetValue(key, out var cacheItem))
            {
                if (cacheItem.ExpirationTime > DateTime.UtcNow)
                {
                    if (_useSlidingExpiration)
                    {
                        //
                        // Update expiration time on access atomically
                        //
                        _cache.AddOrUpdate(key,
                            cacheItem, // Shouldn't hit this path
                            (k, existing) => new CacheItem<TValue>(
                                existing.IsCompressed ? existing.CompressedValue : existing.Value,
                                existing.DecompressedSize,
                                DateTime.UtcNow.AddSeconds(_expirationInSeconds),
                                existing.IsCompressed));

                    }

                    return true;
                }
                else
                {
                    // Item has expired, remove it
                    _cache.TryRemove(key, out _);
                }
            }

            return false;
        }

        public List<TKey> KeyList()
        {
            return _cache.Keys.ToList();
        }


        private void CheckForExpiredItems(object state)
        {
            foreach (var key in _cache.Keys.ToList())
            {
                if (_cache.TryGetValue(key, out var item) && item.ExpirationTime <= DateTime.UtcNow)
                {
                    _cache.TryRemove(key, out _);
                }
            }
        }

        private class CacheItem<T>
        {
            public object Value { get; } // Store either TValue or byte[] (compressed)  
            public byte[] CompressedValue { get; } // Store compressed data if applicable  
            public int DecompressedSize { get; }   // Store original size for decompression
            public DateTime ExpirationTime { get; }
            public bool IsCompressed { get; }

            public CacheItem(object value, int decompressedSize, DateTime expirationTime, bool isCompressed)
            {
                if (isCompressed)
                {
                    CompressedValue = (byte[])value;
                    Value = null;
                }
                else
                {
                    Value = value;
                    CompressedValue = null;
                }
                DecompressedSize = decompressedSize;
                ExpirationTime = expirationTime;
                IsCompressed = isCompressed;
            }

            /// <summary>
            /// This gets the value from the cache item, either directly, or via decompression and deserialization
            /// </summary>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"></exception>
            public TValue GetValue()
            {
                if (this.IsCompressed)
                {
                    //
                    // Decompress and deserialize
                    //
                    byte[] decompressed = new byte[this.DecompressedSize];

                    int decodedLength = LZ4Codec.Decode(this.CompressedValue,
                                                        0,
                                                        this.CompressedValue.Length,
                                                        decompressed,
                                                        0,
                                                        decompressed.Length);

                    if (decodedLength != this.DecompressedSize)
                    {
                        throw new InvalidOperationException("Decompression failed.");
                    }

                    return MessagePackSerializer.Deserialize<TValue>(decompressed);
                }
                else
                {
                    return (TValue)this.Value;
                }
            }
        }




        public void Dispose()
        {
            _timer?.Dispose();
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public bool Remove(TKey key, out TValue value)
        {
            if (_cache.TryRemove(key, out CacheItem<TValue> cacheItem) == true)
            {
                value = cacheItem.GetValue();

                return true;
            }
            else
            {
                value = default;

                return false;
            }
        }
    }
}