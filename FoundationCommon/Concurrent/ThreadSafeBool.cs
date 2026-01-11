using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Foundation.Concurrent
{
    public class ThreadSafeBool
    {
        private int _value;

        public ThreadSafeBool(bool initialValue = false)
        {
            _value = initialValue ? 1 : 0;
        }

        /// <summary>
        /// Sets the boolean value in a thread-safe manner.
        /// </summary>
        public void Set(bool value)
        {
            Interlocked.Exchange(ref _value, value ? 1 : 0);
        }

        /// <summary>
        /// Gets the current boolean value in a thread-safe manner.
        /// </summary>
        public bool Get()
        {
            return Interlocked.CompareExchange(ref _value, 0, 0) == 1;
        }

        /// <summary>
        /// Attempts to set the value from false to true if it's currently false. 
        /// Returns true if the operation succeeded (i.e., it was false and now is true).
        /// </summary>
        public bool TrySetTrue()
        {
            return Interlocked.CompareExchange(ref _value, 1, 0) == 0;
        }

        /// <summary>
        /// Attempts to set the value from true to false if it's currently true. 
        /// Returns true if the operation succeeded (i.e., it was true and now is false).
        /// </summary>
        public bool TrySetFalse()
        {
            return Interlocked.CompareExchange(ref _value, 0, 1) == 1;
        }

        // Implicit conversion to bool
        public static implicit operator bool(ThreadSafeBool tsb)
        {
            return Interlocked.CompareExchange(ref tsb._value, 0, 0) == 1;
        }
    }
}
