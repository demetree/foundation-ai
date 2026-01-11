using System;
using System.Collections.Generic;
using System.Reflection;

namespace Foundation
{
    public class DateTimeUtility
    {
        private static void Test()
        {
            var tester = new DateTimeUTCConversionTester();
            Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(tester, true);

            var tester2 = new DateTimeUTCConversionTester();
            Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(tester2, false);
        }

        public class DateTimeUTCConversionTester
        {
            public DateTime Regular { get; set; }
            public DateTime? Nullable { get; set; }

            public DateTime? NullableWithoutValue { get; set; }


            public DateTimeUTCConversionTester tester { get; set; }
            public List<DateTimeUTCConversionTester> testerList { get; set; }


            public DateTimeUTCConversionTester()
            {
                Regular = new DateTime();
                Nullable = new DateTime();
                NullableWithoutValue = null;

                // The child object and object list should't build their own children.
                tester = new DateTimeUTCConversionTester(true);
                testerList = new List<DateTimeUTCConversionTester>();
                testerList.Add(new DateTimeUTCConversionTester(true));
            }

            private DateTimeUTCConversionTester(bool noChildrenListsOrObjects)
            {
                Regular = new DateTime();
                Nullable = new DateTime();
                NullableWithoutValue = null;

                if (noChildrenListsOrObjects == false)
                {
                    tester = new DateTimeUTCConversionTester(true);
                    testerList = new List<DateTimeUTCConversionTester>();
                    testerList.Add(new DateTimeUTCConversionTester(true));
                }
            }
        }




        public static void ConvertAllDateTimePropertiesToUTC(object obj, bool databaseStoresDatesWithTimeZone)
        {
            //
            // SQL Server DateTime fields do not store the any kind of indication about the time zone when storing a DateTime, but SQLite does.
            // This affects the way that the resultant DateTime objects when read from the database are 'kinded', and that varies between reads from SQL Server and SQLite.
            //
            // This means that SQL Server DATETIME fields don't store their original time zone nor the UTC identifier, and come back with a local date kind.
            // SQLite stores UTC dates as ISO date strings, including the Z suffix, so they are read into local time, after converting from the UTC stored time if there is a Z suffix..
            // 
            // We want all date/time fields to serialize out as UTC dates.
            // 

            //
            // Dates are stored as UTC in the database, but EF reads them as being of kind 'Unspecified'.  The problem with this is that they serialize to JSON without a 'Z' UTC indicator.
            // To fix this, change the kind of all of the the date objects to be UTC.
            //

            //
            // We want the 'Kind' of all dates on the return object to be of UTC, so that they serialize with the Z suffix.
            // 
            // For SQLite, and any other databases that store dates with the time zone (ie. in ISO 8601 string format, like SQLite), the date that comes back is of 'kind' local, and the time is already adjusted from the UTC raw storage format.
            // For these, do a direct conversion back into Universal time so that it serializes out with the UTC time and Z suffix.
            //
            //
            // NOTE THAT THIS FUNCTION WILL NOT WORK ON ANONYMOUS TYPES BECAUSE THEY HAVE NO SETTER FOR THEIR PROPERTIES. 
            // If using anonymous types, then construct the appropriate datetime values as part of their initialization instead.
            // 
            //
            // This will convert the following properties to UTC
            //
            // All DateTime properties
            // All Nullable DateTime properties
            // 
            // All DateTime and Nullable DateTime properties belonging to objects in children lists.  One level only
            //
            // Children objects will be recursed to have the same rules apply.  There is no circular reference checking, so if one exists, there will be a stack overflow
            //
            if (obj != null)
            {
                foreach (var property in obj.GetType().GetProperties())
                {
                    // This is DateTime test is the primary purpose of this function.  The next 2 conditions are to drill in deeper to find more DateTimes to fix.
                    if (property.PropertyType == typeof(DateTime))
                    {
                        ConvertDateTimePropertyToUTC(obj, property, databaseStoresDatesWithTimeZone);
                    }

                    //
                    // Is this a nullable DateTime?
                    //
                    else if (property.PropertyType.FullName.StartsWith("System.Nullable") && Nullable.GetUnderlyingType(property.PropertyType) == typeof(DateTime))
                    {
                        ConvertNullableDateTimePropertyToUTC(obj, property, databaseStoresDatesWithTimeZone);
                    }

                    //
                    // Check if this is a collection type.  We are testing for this, and will handle first leve datetimes, but won't drill in deeper by recursing back into this function.
                    // It is likely that a child object in a sub list object list has a parent reference property that could result in an endless loop.
                    //
                    // Also, LazyLoading must be on for these items to get any data, and the expectation for this framework is that LazyLoading is off, so all of this might be irrelevant in practice
                    //
                    // The data here is expected to be a list of custom objects, like children lists for navigation properties.
                    //
                    else if (property.PropertyType.FullName.StartsWith("System.Collections.Generic"))
                    {
                        //
                        // Can't just call this function back with each object item because one of the child objects of that object could be this object, and if that's populated then this would endless loop.
                        // 
                        // Because of that, we will only fix date types directly on properties of child object lists.  We won't drill in deeper than this to ensure we avoid parent references on the objects.
                        //
                        //
                        // This will fail if Lazy Loading is on, but the failure is caught and written to the diagnostics console.
                        //
                        // Turn lazy loading off with 'this.Configuration.LazyLoadingEnabled = false;' in the constructor in the context class.
                        //
                        try
                        {
                            IEnumerable<object> enumerable = (IEnumerable<object>)property.GetValue(obj);
                            if (enumerable != null)
                            {
                                foreach (object listItem in enumerable)
                                {
                                    foreach (var listItemProperty in listItem.GetType().GetProperties())
                                    {
                                        // This is DateTime test is the primary purpose of this function.  The next 2 conditions are to drill in deeper to find more DateTimes to fix.
                                        if (listItemProperty.PropertyType == typeof(DateTime))
                                        {
                                            ConvertDateTimePropertyToUTC(listItem, listItemProperty, databaseStoresDatesWithTimeZone);
                                        }
                                        else if (listItemProperty.PropertyType.FullName.StartsWith("System.Nullable") && Nullable.GetUnderlyingType(listItemProperty.PropertyType) == typeof(DateTime))
                                        {
                                            ConvertNullableDateTimePropertyToUTC(listItem, listItemProperty, databaseStoresDatesWithTimeZone);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Error caught processing dates while trying to adjust for UTC storage.  This shouldn't happen.  Is LazyLoading disabled in the context class?  If not, turn it off.  Error is: " + ex.ToString());
                        }
                    }
                    //
                    // Check if this is a non system base type that has properties.
                    // Expected to be custom object types link foreign key relation objects on the 'one' side of navigate properties
                    //
                    else if (property.PropertyType.FullName.StartsWith("System.") == false &&
                        property.PropertyType.GetProperties().Length > 0)
                    {
                        //System.Diagnostics.Debug.WriteLine("Recursing down into object property named: " + property.Name + " to check for and fix dates.");

                        object propertyValue = property.GetValue(obj, null);

                        if (propertyValue != null)
                        {
                            //
                            // Make sure that none of the properties on this object are of the same type as the object we are processing to mitigate the risk of stack overflow via infinite looping.. 
                            //
                            // This will work in some case, but could fail in deeper nesting than one level.
                            //
                            bool selfTypeReferenceFound = false;

                            string objectTypeName = obj.GetType().Name;

                            foreach (var subProperty in propertyValue.GetType().GetProperties())
                            {
                                if (subProperty.PropertyType.Name == objectTypeName)
                                {
                                    selfTypeReferenceFound = true;
                                    break;
                                }
                            }

                            if (selfTypeReferenceFound == false)
                            {
                                ConvertAllDateTimePropertiesToUTC(propertyValue, databaseStoresDatesWithTimeZone);
                            }
                        }
                    }
                }
            }

            return;
        }


        private static void ConvertDateTimePropertyToUTC(object obj, PropertyInfo property, bool databaseStoresDatesWithTimeZone)
        {
            if (databaseStoresDatesWithTimeZone == false)
            {
                //
                // Change the kind of the date time to be UTC.  This doesn't change the hours/minutes/seconds, but makes the kind UTC.
                //
                DateTime date = (DateTime)property.GetValue(obj, null);

                if (date.Kind != DateTimeKind.Utc)
                {
                    date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

                    property.SetValue(obj, date);
                }
            }
            else
            {
                //
                // Translate the date time to be UTC.  This does change the hours/minutes/seconds if the kind is local time, and it will make a new date time object of kind UTC
                //
                DateTime date = (DateTime)property.GetValue(obj, null);

                if (date.Kind != DateTimeKind.Utc)
                {
                    date = date.ToUniversalTime();

                    property.SetValue(obj, date);
                }
            }
        }


        private static void ConvertNullableDateTimePropertyToUTC(object obj, PropertyInfo property, bool databaseStoresDatesWithTimeZone)
        {
            if (databaseStoresDatesWithTimeZone == false)
            {
                //
                // Change the kind of the date time to be UTC.  This doesn't change the hours/minutes/seconds, but makes the kind UTC.
                //
                DateTime? date = (DateTime?)property.GetValue(obj, null);

                if (date.HasValue == true)
                {
                    if (date.Value.Kind != DateTimeKind.Utc)
                    {
                        date = DateTime.SpecifyKind(date.Value, DateTimeKind.Utc);

                        property.SetValue(obj, date);
                    }
                }
            }
            else
            {
                //
                // Translate the date time to be UTC.  This does change the hours/minutes/seconds if the kind is local time, and it will make a new date time object of kind UTC
                //
                DateTime? date = (DateTime?)property.GetValue(obj, null);

                if (date.HasValue == true)
                {
                    if (date.Value.Kind != DateTimeKind.Utc)
                    {
                        date = date.Value.ToUniversalTime();

                        property.SetValue(obj, date);
                    }
                }
            }
        }
    }
}
