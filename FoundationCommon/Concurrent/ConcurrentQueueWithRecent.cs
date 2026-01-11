using System.Collections.Concurrent;

namespace Foundation.Concurrent
{
    public class ConcurrentQueueWithRecent<T> where T : class
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private T _mostRecentItem;
        private readonly object _lock = new object(); // For thread safety when updating _mostRecentItem

        // Method to add an item to the queue
        public void Enqueue(T item)
        {
            _queue.Enqueue(item);

            // Update the most recent item in a thread-safe manner
            lock (_lock)
            {
                _mostRecentItem = item;
            }
        }

        /// <summary>
        /// 
        /// This gets the most recently enqueued item.  The idea here is that we track the most recent item so that we don't need to do a .Last on the set to get the same value.
        ///     
        /// running .Last does a full enumeration of the contents of the queue and is terribly slow when there is a lot of data in the set.  
        /// 
        /// Whereas, this should be near immediate.
        /// 
        /// </summary>
        /// <returns></returns>
        public T GetMostRecentItem()
        {
            lock (_lock)
            {
                return _mostRecentItem;
            }
        }


        // Method to get the oldest item
        public T GetOldestItem()
        {
            if (_queue.TryPeek(out T result) == true)
            {
                return result;
            }
            else
            {
                return null;
            }

        }


        // Method to dequeue an item from the queue
        public bool TryDequeue(out T result)
        {
            return _queue.TryDequeue(out result);
        }

        public bool TryPeek(out T result)
        {
            return _queue.TryPeek(out result);
        }

        public bool IsEmpty
        {
            get { return _queue.IsEmpty; }
        }

        public int Count
        {
            get { return _queue.Count; }
        }
    }
}

