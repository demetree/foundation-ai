//
// RecurrenceExpansionService.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// This service provides the core logic for expanding a recurring ScheduledEvent's RecurrenceRule
// into a list of virtual event instances within a given date range.  These virtual instances are
// in-memory only and are not persisted to the database.
//
// The service is designed to be reusable across the system — it can be called from API controllers
// for calendar display, from background workers for notifications, and from report generators.
//
// Frequency support:
//   - Daily:   Every N days
//   - Weekly:  Every N weeks on selected days (via dayOfWeekMask bitmask)
//   - Monthly: By day of month (e.g., the 15th) or by Nth weekday (e.g., 2nd Tuesday)
//   - Yearly:  By month and day of month, using the master event's start month
//
// The service also handles RecurrenceException records, which allow individual occurrences
// to be skipped or moved to a different date.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Foundation.Scheduler.Database;

namespace Scheduler.Server.Services
{
    /// <summary>
    ///
    /// Service responsible for expanding RecurrenceRule data into a list of virtual ScheduledEvent instances.
    ///
    /// The expansion is done in-memory and produces lightweight clones of the master event with adjusted
    /// start/end times and synthetic IDs.  No database writes occur.
    ///
    /// </summary>
    public class RecurrenceExpansionService
    {
        //
        // Constants
        //
        private const int MAXIMUM_OCCURRENCES_PER_RULE = 500;

        //
        // Frequency ID constants matching the RecurrenceFrequency seed data
        //
        private const int FREQ_ONCE = 1;
        private const int FREQ_DAILY = 2;
        private const int FREQ_WEEKLY = 3;
        private const int FREQ_MONTHLY = 4;
        private const int FREQ_YEARLY = 5;

        //
        // Day of week bitmask constants (matching client-side convention)
        //
        private const int DAY_SUNDAY = 1;
        private const int DAY_MONDAY = 2;
        private const int DAY_TUESDAY = 4;
        private const int DAY_WEDNESDAY = 8;
        private const int DAY_THURSDAY = 16;
        private const int DAY_FRIDAY = 32;
        private const int DAY_SATURDAY = 64;

        private readonly ILogger<RecurrenceExpansionService> _logger;


        /// <summary>
        ///
        /// Constructs the RecurrenceExpansionService with a logger for diagnostic output.
        ///
        /// </summary>
        /// <param name="logger">Logger instance for this service</param>
        public RecurrenceExpansionService(ILogger<RecurrenceExpansionService> logger)
        {
            _logger = logger;
        }


        /// <summary>
        ///
        /// Expands a recurring ScheduledEvent into a list of virtual event instances within the specified date range.
        ///
        /// The master event must have its RecurrenceRule navigation property loaded.
        /// RecurrenceException records are used to skip or move individual occurrences.
        ///
        /// Each returned event is a clone of the master with adjusted times, a synthetic negative ID,
        /// and the parentScheduledEventId set to the master's ID.
        ///
        /// </summary>
        /// <param name="masterEvent">The master recurring event with RecurrenceRule loaded</param>
        /// <param name="exceptions">RecurrenceException records for this master event</param>
        /// <param name="rangeStart">Start of the date range window (UTC)</param>
        /// <param name="rangeEnd">End of the date range window (UTC)</param>
        /// <returns>A list of virtual ScheduledEvent instances representing individual occurrences</returns>
        public List<ScheduledEvent> ExpandOccurrences(ScheduledEvent masterEvent,
                                                      List<RecurrenceException> exceptions,
                                                      DateTime rangeStart,
                                                      DateTime rangeEnd)
        {
            List<ScheduledEvent> expandedEventList = new List<ScheduledEvent>();

            //
            // Guard: must have a recurrence rule loaded
            //
            if (masterEvent.recurrenceRule == null)
            {
                _logger.LogWarning("ExpandOccurrences called for event {EventId} but RecurrenceRule is null. Returning empty list.", masterEvent.id);
                return expandedEventList;
            }

            RecurrenceRule rule = masterEvent.recurrenceRule;

            //
            // Guard: interval must be at least 1
            //
            if (rule.interval < 1)
            {
                _logger.LogWarning("RecurrenceRule {RuleId} has interval < 1 ({Interval}). Defaulting to 1.", rule.id, rule.interval);
                rule.interval = 1;
            }

            //
            // Calculate the duration of the master event so we can apply it to each occurrence
            //
            TimeSpan eventDuration = masterEvent.endDateTime - masterEvent.startDateTime;

            //
            // Generate the raw occurrence dates based on the recurrence rule
            //
            List<DateTime> occurrenceDateList = GenerateOccurrenceDates(rule: rule,
                                                                       anchorDate: masterEvent.startDateTime,
                                                                       rangeStart: rangeStart,
                                                                       rangeEnd: rangeEnd);

            //
            // Build a set of exception dates for fast lookup, and a map for moved dates
            //
            HashSet<DateTime> skippedDateSet = new HashSet<DateTime>();
            Dictionary<DateTime, DateTime> movedDateMap = new Dictionary<DateTime, DateTime>();

            if (exceptions != null)
            {
                foreach (RecurrenceException exception in exceptions)
                {
                    DateTime exceptionDate = exception.exceptionDateTime.Date;

                    if (exception.movedToDateTime.HasValue == true)
                    {
                        //
                        // This occurrence was moved to a different date, not deleted
                        //
                        movedDateMap[exceptionDate] = exception.movedToDateTime.Value;
                    }
                    else
                    {
                        //
                        // This occurrence was skipped entirely
                        //
                        skippedDateSet.Add(exceptionDate);
                    }
                }
            }

            //
            // Build virtual event instances from the occurrence dates, applying exceptions
            //
            int instanceIndex = 0;

            foreach (DateTime occurrenceDate in occurrenceDateList)
            {
                DateTime occurrenceDateOnly = occurrenceDate.Date;

                //
                // Skip this occurrence if it has been deleted via a RecurrenceException
                //
                if (skippedDateSet.Contains(occurrenceDateOnly) == true)
                {
                    continue;
                }

                //
                // If this occurrence was moved, use the moved-to date instead
                //
                DateTime actualOccurrenceDate = occurrenceDate;

                if (movedDateMap.TryGetValue(occurrenceDateOnly, out DateTime movedTo) == true)
                {
                    actualOccurrenceDate = movedTo;
                }

                //
                // Build a virtual event instance by copying fields from the master event
                //
                ScheduledEvent virtualEvent = CreateVirtualInstance(masterEvent: masterEvent,
                                                                    syntheticId: -(masterEvent.id * 100000 + instanceIndex),
                                                                    instanceDate: occurrenceDate,
                                                                    actualStartDate: actualOccurrenceDate,
                                                                    eventDuration: eventDuration);

                expandedEventList.Add(virtualEvent);
                instanceIndex++;
            }

            return expandedEventList;
        }


        /// <summary>
        ///
        /// Creates a virtual ScheduledEvent instance by copying fields from the master event
        /// and overriding instance-specific values (ID, parent link, dates).
        ///
        /// This replaces a Clone() call since ScheduledEvent doesn't have a Clone method.
        /// Only fields needed for ToOutputDTO() serialization are copied.
        ///
        /// </summary>
        private ScheduledEvent CreateVirtualInstance(ScheduledEvent masterEvent,
                                                     int syntheticId,
                                                     DateTime instanceDate,
                                                     DateTime actualStartDate,
                                                     TimeSpan eventDuration)
        {
            DateTime instanceStartDateTime = new DateTime(actualStartDate.Year,
                                                           actualStartDate.Month,
                                                           actualStartDate.Day,
                                                           masterEvent.startDateTime.Hour,
                                                           masterEvent.startDateTime.Minute,
                                                           masterEvent.startDateTime.Second,
                                                           DateTimeKind.Utc);

            ScheduledEvent virtualEvent = new ScheduledEvent
            {
                //
                // Synthetic negative ID to distinguish from persisted events
                //
                id = syntheticId,

                //
                // Core tenant and ownership
                //
                tenantGuid = masterEvent.tenantGuid,
                objectGuid = masterEvent.objectGuid,

                //
                // Link back to the master event
                //
                parentScheduledEventId = masterEvent.id,
                recurrenceInstanceDate = instanceDate,

                //
                // Copy the recurrence rule reference (used by client for display)
                //
                recurrenceRuleId = masterEvent.recurrenceRuleId,

                //
                // Instance-specific dates
                //
                startDateTime = instanceStartDateTime,
                endDateTime = instanceStartDateTime + eventDuration,

                //
                // Copy all other value fields from the master
                //
                name = masterEvent.name,
                description = masterEvent.description,
                isAllDay = masterEvent.isAllDay,
                location = masterEvent.location,
                notes = masterEvent.notes,
                color = masterEvent.color,
                externalId = masterEvent.externalId,
                attributes = masterEvent.attributes,
                officeId = masterEvent.officeId,
                clientId = masterEvent.clientId,
                scheduledEventTemplateId = masterEvent.scheduledEventTemplateId,
                schedulingTargetId = masterEvent.schedulingTargetId,
                timeZoneId = masterEvent.timeZoneId,
                eventStatusId = masterEvent.eventStatusId,
                resourceId = masterEvent.resourceId,
                crewId = masterEvent.crewId,
                priorityId = masterEvent.priorityId,
                bookingSourceTypeId = masterEvent.bookingSourceTypeId,
                partySize = masterEvent.partySize,
                versionNumber = masterEvent.versionNumber,
                active = masterEvent.active,
                deleted = masterEvent.deleted,

                //
                // Copy navigation properties so ToOutputDTO() can serialize them
                //
                bookingSourceType = masterEvent.bookingSourceType,
                client = masterEvent.client,
                crew = masterEvent.crew,
                eventStatus = masterEvent.eventStatus,
                office = masterEvent.office,
                priority = masterEvent.priority,
                recurrenceRule = masterEvent.recurrenceRule,
                resource = masterEvent.resource,
                scheduledEventTemplate = masterEvent.scheduledEventTemplate,
                schedulingTarget = masterEvent.schedulingTarget,
                timeZone = masterEvent.timeZone
            };

            return virtualEvent;
        }


        /// <summary>
        ///
        /// Generates a list of occurrence dates based on a RecurrenceRule and an anchor date.
        ///
        /// The anchor date is typically the master event's startDateTime.  Occurrences are generated
        /// starting from the anchor date and continuing until one of the termination conditions is met:
        ///   - The generated date exceeds rangeEnd
        ///   - The generated date exceeds the rule's untilDateTime
        ///   - The rule's count limit is reached
        ///   - The safety cap (MAXIMUM_OCCURRENCES_PER_RULE) is hit
        ///
        /// Only dates that fall within [rangeStart, rangeEnd] are included in the output.
        ///
        /// </summary>
        /// <param name="rule">The recurrence rule defining the pattern</param>
        /// <param name="anchorDate">The starting point for recurrence calculation (master event start)</param>
        /// <param name="rangeStart">Only include occurrences on or after this date</param>
        /// <param name="rangeEnd">Stop generating occurrences after this date</param>
        /// <returns>A list of DateTime values representing occurrence dates within the range</returns>
        private List<DateTime> GenerateOccurrenceDates(RecurrenceRule rule,
                                                       DateTime anchorDate,
                                                       DateTime rangeStart,
                                                       DateTime rangeEnd)
        {
            List<DateTime> occurrenceDateList = new List<DateTime>();
            int frequencyId = rule.recurrenceFrequencyId;

            //
            // Dispatch to the appropriate frequency handler
            //
            if (frequencyId == FREQ_DAILY)
            {
                occurrenceDateList = GenerateDailyOccurrences(rule: rule,
                                                              anchorDate: anchorDate,
                                                              rangeStart: rangeStart,
                                                              rangeEnd: rangeEnd);
            }
            else if (frequencyId == FREQ_WEEKLY)
            {
                occurrenceDateList = GenerateWeeklyOccurrences(rule: rule,
                                                               anchorDate: anchorDate,
                                                               rangeStart: rangeStart,
                                                               rangeEnd: rangeEnd);
            }
            else if (frequencyId == FREQ_MONTHLY)
            {
                occurrenceDateList = GenerateMonthlyOccurrences(rule: rule,
                                                                anchorDate: anchorDate,
                                                                rangeStart: rangeStart,
                                                                rangeEnd: rangeEnd);
            }
            else if (frequencyId == FREQ_YEARLY)
            {
                occurrenceDateList = GenerateYearlyOccurrences(rule: rule,
                                                               anchorDate: anchorDate,
                                                               rangeStart: rangeStart,
                                                               rangeEnd: rangeEnd);
            }
            else if (frequencyId == FREQ_ONCE)
            {
                //
                // "Once" means no recurrence — the master event itself is the only occurrence.
                // We include it if it falls within the range.
                //
                if (anchorDate >= rangeStart && anchorDate <= rangeEnd)
                {
                    occurrenceDateList.Add(anchorDate);
                }
            }
            else
            {
                _logger.LogWarning("Unrecognized recurrence frequency ID: {FrequencyId} on rule {RuleId}.", frequencyId, rule.id);
            }

            return occurrenceDateList;
        }


        /// <summary>
        ///
        /// Generates daily occurrences.  Every N days from the anchor date.
        ///
        /// </summary>
        private List<DateTime> GenerateDailyOccurrences(RecurrenceRule rule,
                                                        DateTime anchorDate,
                                                        DateTime rangeStart,
                                                        DateTime rangeEnd)
        {
            List<DateTime> resultList = new List<DateTime>();
            DateTime currentDate = anchorDate;
            int occurrenceCount = 0;
            int maxCount = rule.count ?? int.MaxValue;

            while (currentDate <= rangeEnd &&
                   occurrenceCount < maxCount &&
                   occurrenceCount < MAXIMUM_OCCURRENCES_PER_RULE)
            {
                //
                // Check the untilDateTime termination condition
                //
                if (rule.untilDateTime.HasValue == true && currentDate > rule.untilDateTime.Value)
                {
                    break;
                }

                //
                // Only include if within the requested range
                //
                if (currentDate >= rangeStart)
                {
                    resultList.Add(currentDate);
                }

                occurrenceCount++;
                currentDate = currentDate.AddDays(rule.interval);
            }

            return resultList;
        }


        /// <summary>
        ///
        /// Generates weekly occurrences.  Every N weeks, on the days specified by the dayOfWeekMask bitmask.
        ///
        /// The bitmask uses: Sunday=1, Monday=2, Tuesday=4, Wednesday=8, Thursday=16, Friday=32, Saturday=64.
        ///
        /// If no dayOfWeekMask is set, falls back to the anchor date's day of week.
        ///
        /// </summary>
        private List<DateTime> GenerateWeeklyOccurrences(RecurrenceRule rule,
                                                         DateTime anchorDate,
                                                         DateTime rangeStart,
                                                         DateTime rangeEnd)
        {
            List<DateTime> resultList = new List<DateTime>();
            int dayMask = rule.dayOfWeekMask ?? ConvertDayOfWeekToBitmask(anchorDate.DayOfWeek);
            int occurrenceCount = 0;
            int maxCount = rule.count ?? int.MaxValue;

            //
            // Start from the beginning of the anchor week (Sunday)
            //
            DateTime weekStart = anchorDate.AddDays(-(int)anchorDate.DayOfWeek);

            while (weekStart <= rangeEnd &&
                   occurrenceCount < maxCount &&
                   occurrenceCount < MAXIMUM_OCCURRENCES_PER_RULE)
            {
                //
                // Walk through each day of this week and check the bitmask
                //
                for (int dayIndex = 0; dayIndex < 7; dayIndex++)
                {
                    DateTime candidateDate = weekStart.AddDays(dayIndex);
                    int dayBit = 1 << dayIndex;

                    //
                    // Check if this day of week is selected in the mask
                    //
                    if ((dayMask & dayBit) == dayBit)
                    {
                        //
                        // Skip dates before the anchor (the series starts at the anchor)
                        //
                        if (candidateDate < anchorDate)
                        {
                            continue;
                        }

                        //
                        // Check termination conditions
                        //
                        if (candidateDate > rangeEnd)
                        {
                            break;
                        }

                        if (rule.untilDateTime.HasValue == true && candidateDate > rule.untilDateTime.Value)
                        {
                            break;
                        }

                        if (occurrenceCount >= maxCount || occurrenceCount >= MAXIMUM_OCCURRENCES_PER_RULE)
                        {
                            break;
                        }

                        //
                        // Only include if within the requested range
                        //
                        if (candidateDate >= rangeStart)
                        {
                            resultList.Add(candidateDate);
                        }

                        occurrenceCount++;
                    }
                }

                //
                // Advance to the next week (by interval)
                //
                weekStart = weekStart.AddDays(7 * rule.interval);
            }

            return resultList;
        }


        /// <summary>
        ///
        /// Generates monthly occurrences.  Two modes:
        ///
        /// 1. By day of month (dayOfMonth is set): e.g., the 15th of every N months
        /// 2. By Nth weekday (dayOfWeekInMonth + dayOfWeekMask are set): e.g., 2nd Tuesday of every N months
        ///
        /// </summary>
        private List<DateTime> GenerateMonthlyOccurrences(RecurrenceRule rule,
                                                          DateTime anchorDate,
                                                          DateTime rangeStart,
                                                          DateTime rangeEnd)
        {
            List<DateTime> resultList = new List<DateTime>();
            int occurrenceCount = 0;
            int maxCount = rule.count ?? int.MaxValue;

            //
            // Start from the anchor date's month
            //
            DateTime currentMonth = new DateTime(anchorDate.Year, anchorDate.Month, 1, anchorDate.Hour, anchorDate.Minute, anchorDate.Second, DateTimeKind.Utc);

            while (currentMonth <= rangeEnd &&
                   occurrenceCount < maxCount &&
                   occurrenceCount < MAXIMUM_OCCURRENCES_PER_RULE)
            {
                DateTime candidateDate;
                bool validDate = false;

                if (rule.dayOfMonth.HasValue == true)
                {
                    //
                    // Mode 1: Specific day of month.  Clamp to last day if month is shorter.
                    //
                    int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
                    int targetDay = Math.Min(rule.dayOfMonth.Value, daysInMonth);

                    candidateDate = new DateTime(currentMonth.Year, currentMonth.Month, targetDay,
                                                 anchorDate.Hour, anchorDate.Minute, anchorDate.Second, DateTimeKind.Utc);
                    validDate = true;
                }
                else if (rule.dayOfWeekInMonth.HasValue == true && rule.dayOfWeekMask.HasValue == true)
                {
                    //
                    // Mode 2: Nth weekday of the month (e.g., 2nd Tuesday)
                    //
                    DayOfWeek targetDayOfWeek = ConvertBitmaskToDayOfWeek(rule.dayOfWeekMask.Value);

                    validDate = TryGetNthWeekdayOfMonth(year: currentMonth.Year,
                                                        month: currentMonth.Month,
                                                        nthOccurrence: rule.dayOfWeekInMonth.Value,
                                                        targetDay: targetDayOfWeek,
                                                        result: out candidateDate);

                    if (validDate == true)
                    {
                        //
                        // Apply the time of day from the anchor event
                        //
                        candidateDate = new DateTime(candidateDate.Year, candidateDate.Month, candidateDate.Day,
                                                     anchorDate.Hour, anchorDate.Minute, anchorDate.Second, DateTimeKind.Utc);
                    }
                }
                else
                {
                    //
                    // Fallback: use the anchor date's day of month
                    //
                    int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
                    int targetDay = Math.Min(anchorDate.Day, daysInMonth);

                    candidateDate = new DateTime(currentMonth.Year, currentMonth.Month, targetDay,
                                                 anchorDate.Hour, anchorDate.Minute, anchorDate.Second, DateTimeKind.Utc);
                    validDate = true;
                }

                if (validDate == true)
                {
                    //
                    // Skip dates before the anchor
                    //
                    if (candidateDate >= anchorDate)
                    {
                        //
                        // Check termination conditions
                        //
                        if (candidateDate > rangeEnd)
                        {
                            break;
                        }

                        if (rule.untilDateTime.HasValue == true && candidateDate > rule.untilDateTime.Value)
                        {
                            break;
                        }

                        //
                        // Only include if within the requested range
                        //
                        if (candidateDate >= rangeStart)
                        {
                            resultList.Add(candidateDate);
                        }

                        occurrenceCount++;
                    }
                }

                //
                // Advance to the next month (by interval)
                //
                currentMonth = currentMonth.AddMonths(rule.interval);
            }

            return resultList;
        }


        /// <summary>
        ///
        /// Generates yearly occurrences.  Uses the anchor event's month combined with the rule's dayOfMonth.
        ///
        /// If dayOfMonth is not set, falls back to the anchor date's day.
        ///
        /// </summary>
        private List<DateTime> GenerateYearlyOccurrences(RecurrenceRule rule,
                                                         DateTime anchorDate,
                                                         DateTime rangeStart,
                                                         DateTime rangeEnd)
        {
            List<DateTime> resultList = new List<DateTime>();
            int occurrenceCount = 0;
            int maxCount = rule.count ?? int.MaxValue;
            int targetMonth = anchorDate.Month;
            int targetDay = rule.dayOfMonth ?? anchorDate.Day;

            //
            // Start from the anchor year
            //
            int currentYear = anchorDate.Year;

            while (occurrenceCount < maxCount &&
                   occurrenceCount < MAXIMUM_OCCURRENCES_PER_RULE)
            {
                //
                // Clamp dayOfMonth to the actual days in the target month for this year (handles Feb 29)
                //
                int daysInMonth = DateTime.DaysInMonth(currentYear, targetMonth);
                int actualDay = Math.Min(targetDay, daysInMonth);

                DateTime candidateDate = new DateTime(currentYear, targetMonth, actualDay,
                                                      anchorDate.Hour, anchorDate.Minute, anchorDate.Second, DateTimeKind.Utc);

                //
                // Terminate if we've passed the range end
                //
                if (candidateDate > rangeEnd)
                {
                    break;
                }

                //
                // Check untilDateTime termination
                //
                if (rule.untilDateTime.HasValue == true && candidateDate > rule.untilDateTime.Value)
                {
                    break;
                }

                //
                // Skip dates before the anchor
                //
                if (candidateDate >= anchorDate)
                {
                    //
                    // Only include if within the requested range
                    //
                    if (candidateDate >= rangeStart)
                    {
                        resultList.Add(candidateDate);
                    }

                    occurrenceCount++;
                }

                //
                // Advance to the next year (by interval)
                //
                currentYear += rule.interval;
            }

            return resultList;
        }


        /// <summary>
        ///
        /// Converts a .NET DayOfWeek enum value to the bitmask value used by the recurrence system.
        ///
        /// </summary>
        /// <param name="dayOfWeek">The .NET DayOfWeek value</param>
        /// <returns>The bitmask integer (1=Sunday, 2=Monday, 4=Tuesday, etc.)</returns>
        private int ConvertDayOfWeekToBitmask(DayOfWeek dayOfWeek)
        {
            return 1 << (int)dayOfWeek;
        }


        /// <summary>
        ///
        /// Converts a single-day bitmask value back to a .NET DayOfWeek enum.
        ///
        /// If the mask contains multiple days, returns the first (lowest) day found.
        ///
        /// </summary>
        /// <param name="mask">The bitmask value</param>
        /// <returns>The corresponding DayOfWeek</returns>
        private DayOfWeek ConvertBitmaskToDayOfWeek(int mask)
        {
            for (int i = 0; i < 7; i++)
            {
                if ((mask & (1 << i)) == (1 << i))
                {
                    return (DayOfWeek)i;
                }
            }

            //
            // Default to Sunday if mask is zero or invalid
            //
            return DayOfWeek.Sunday;
        }


        /// <summary>
        ///
        /// Finds the Nth occurrence of a specific day of the week in a given month.
        ///
        /// For example, the 2nd Tuesday of March 2026.
        ///
        /// When nthOccurrence is 5, it means "last" — the last occurrence of that weekday in the month.
        ///
        /// </summary>
        /// <param name="year">The year</param>
        /// <param name="month">The month (1-12)</param>
        /// <param name="nthOccurrence">Which occurrence (1=first, 2=second, ... 5=last)</param>
        /// <param name="targetDay">The day of week to find</param>
        /// <param name="result">Output: the resulting date</param>
        /// <returns>True if a valid date was found, false otherwise</returns>
        private bool TryGetNthWeekdayOfMonth(int year,
                                              int month,
                                              int nthOccurrence,
                                              DayOfWeek targetDay,
                                              out DateTime result)
        {
            result = DateTime.MinValue;

            if (nthOccurrence == 5)
            {
                //
                // Special case: "Last" occurrence — start from the end of the month and walk backwards
                //
                int daysInMonth = DateTime.DaysInMonth(year, month);
                DateTime lastDay = new DateTime(year, month, daysInMonth, 0, 0, 0, DateTimeKind.Utc);

                while (lastDay.Day >= 1)
                {
                    if (lastDay.DayOfWeek == targetDay)
                    {
                        result = lastDay;
                        return true;
                    }

                    lastDay = lastDay.AddDays(-1);
                }

                return false;
            }

            //
            // Standard case: find the Nth occurrence from the start of the month
            //
            DateTime firstOfMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            int matchCount = 0;

            for (int dayIndex = 0; dayIndex < DateTime.DaysInMonth(year, month); dayIndex++)
            {
                DateTime candidateDate = firstOfMonth.AddDays(dayIndex);

                if (candidateDate.DayOfWeek == targetDay)
                {
                    matchCount++;

                    if (matchCount == nthOccurrence)
                    {
                        result = candidateDate;
                        return true;
                    }
                }
            }

            //
            // The requested Nth occurrence doesn't exist in this month (e.g., 5th Monday in a month with only 4)
            //
            return false;
        }
    }
}
