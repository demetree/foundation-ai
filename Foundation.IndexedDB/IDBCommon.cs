using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foundation.IndexedDB
{

    // Custom exceptions
    public class DataException : Exception { public DataException(string msg, Exception inner) : base(msg, inner) { } }
    public class ConstraintException : Exception { public ConstraintException(string msg) : base(msg) { } }
    public class NotFoundException : Exception { public NotFoundException(string msg) : base(msg) { } }

    /// <summary>
    /// 
    /// This class contains common utilities and settings for the IndexedDB implementation.
    /// 
    /// </summary>
    internal static class IDBCommon
    {
        /// <summary>
        /// 
        /// Standard JSON serialization options used library-wide. Configured for camelCase naming,
        /// 
        /// ignoring cycles/nulls, and handling special numbers.
        /// 
        /// </summary>
        internal static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
        };
    }
}