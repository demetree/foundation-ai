using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Foundation.Concurrent
{
    /// <summary>
    /// This class implements a thread safe object list that can be interchanged with a standard List.  Internally, it uses a ConcurrentDictionary as its data store.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentList<T> : IEnumerable<T>
    {
        private readonly ConcurrentDictionary<T, T> _items = new ConcurrentDictionary<T, T>();
        private readonly List<T> _orderedItems = new List<T>();

        // Adds an object to the collection
        public void Add(T item)
        {
            if (_items.TryAdd(item, item))
            {
                lock (_orderedItems) // Ensure thread-safe access to the list
                {
                    _orderedItems.Add(item);
                }
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (_items.TryAdd(item, item))
                {
                    lock (_orderedItems) // Ensure thread-safe access to the list
                    {
                        _orderedItems.Add(item);
                    }
                }
            }
        }

        // Removes the specific object from the collection
        public bool Remove(T item)
        {
            if (_items.TryRemove(item, out _))
            {
                lock (_orderedItems) // Ensure thread-safe access to the list
                {
                    _orderedItems.Remove(item);
                }
                return true;
            }
            return false;
        }

        // Check if an object exists
        public bool Contains(T item)
        {
            return _items.ContainsKey(item);
        }

        public void Clear()
        {
            lock (_orderedItems) // Ensure thread-safe access to the list
            {
                _items.Clear();

                _orderedItems.Clear();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            // Take a snapshot of the collection to avoid modification during iteration
            List<T> snapshot;
            lock (_orderedItems)
            {
                snapshot = new List<T>(_orderedItems);
            }
            return snapshot.GetEnumerator();
        }

        // Explicit implementation for non-generic IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // Integer indexer to access items by their insertion order
        public T this[int index]
        {
            get
            {
                lock (_orderedItems) // Ensure thread-safe access to the list
                {
                    if (index < 0 || index >= _orderedItems.Count)
                    {
                        throw new IndexOutOfRangeException("Index is out of range.");
                    }
                    return _orderedItems[index];
                }
            }
        }

        // Optional: get the total count of items
        public int Count
        {
            get
            {
                lock (_orderedItems)
                {
                    return _orderedItems.Count;
                }
            }
        }

        // Sort method compatible with List<T>.Sort()
        public void Sort()
        {
            lock (_orderedItems)
            {
                _orderedItems.Sort();
            }
        }

        // Sort method with Comparison<T>
        public void Sort(Comparison<T> comparison)
        {
            lock (_orderedItems)
            {
                _orderedItems.Sort(comparison);
            }
        }

        // Sort method with IComparer<T>
        public void Sort(IComparer<T> comparer)
        {
            lock (_orderedItems)
            {
                _orderedItems.Sort(comparer);
            }
        }

        // Sort method with start index and count
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            lock (_orderedItems)
            {
                _orderedItems.Sort(index, count, comparer);
            }
        }

    }
}
