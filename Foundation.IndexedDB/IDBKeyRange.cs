using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Foundation.IndexedDB
{
    /// <summary>
    /// 
    /// This class models a key range for querying records in an object store or index.
    /// 
    /// </summary>
    public class IDBKeyRange
    {
        public object Lower { get; }
        public object Upper { get; }
        public bool LowerOpen { get; }
        public bool UpperOpen { get; }

        private IDBKeyRange(object lower, object upper, bool lowerOpen, bool upperOpen)
        {
            Lower = lower;
            Upper = upper;
            LowerOpen = lowerOpen;
            UpperOpen = upperOpen;
        }

        public static IDBKeyRange Only(object value) => new(value, value, false, false);
        public static IDBKeyRange LowerBound(object lower, bool open = false) => new(lower, null, open, true);
        public static IDBKeyRange UpperBound(object upper, bool open = false) => new(null, upper, true, open);
        public static IDBKeyRange Bound(object lower, object upper, bool lowerOpen = false, bool upperOpen = false) => new(lower, upper, lowerOpen, upperOpen);

        /// <summary>
        /// Gets a character that is lexicographically "after" any other character for a given type,
        /// useful for constructing upper bounds in "starts with" queries.
        /// </summary>
        internal static object GetMaxCharForType(Type type)
        {
            if (type == typeof(string))
            {
                // In JavaScript, '\uffff' is often used. In C#, we can use the largest char value.
                // For strings, appending the max char allows for a range that includes all strings starting with the prefix.
                return char.MaxValue.ToString();
            }

            // Add other types if needed, though startsWith is typically for strings.

            return null;
        }
    }
}