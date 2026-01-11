using Microsoft.EntityFrameworkCore.Diagnostics;
using System;

namespace Foundation
{
    /// <summary>
    /// 
    /// This class intercepts EF core actions, and converts datetime properties to be kinded 'UTC'.
    /// 
    /// It is a singleton on purpose so that the repeated IServiceProvider creating warnings don't trigger, and one instance of this class can serve all.
    /// 
    /// To use this, it should be registered during the ASP.Net DI setup, and also in the 'OnConfiguring' method in the context custom extensions so it is registered for
    /// both DI and manual usage.
    /// 
    /// </summary>
    public class UtcDateTimeInterceptor : IMaterializationInterceptor
    {
        // Singleton instance
        public static readonly UtcDateTimeInterceptor Instance = new UtcDateTimeInterceptor();

        private UtcDateTimeInterceptor() { } // Private constructor to enforce singleton

        public object InitializedInstance(MaterializationInterceptionData materializationData, object instance)
        {
            foreach (var property in instance.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(DateTime) && property.CanRead && property.CanWrite)
                {
                    var dateTime = (DateTime)property.GetValue(instance);
                    if (dateTime.Kind != DateTimeKind.Utc)
                    {
                        property.SetValue(instance, DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
                    }
                }
                else if (property.PropertyType == typeof(DateTime?) && property.CanRead && property.CanWrite)
                {
                    var nullableDateTime = (DateTime?)property.GetValue(instance);
                    if (nullableDateTime.HasValue && nullableDateTime.Value.Kind != DateTimeKind.Utc)
                    {
                        property.SetValue(instance, DateTime.SpecifyKind(nullableDateTime.Value, DateTimeKind.Utc));
                    }
                }
            }
            return instance;
        }
    }
}