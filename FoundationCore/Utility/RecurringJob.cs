using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Cronos;

namespace Foundation
{
    public class RecurringJob
    {
        public const string CRON_MINUTELY = "* * * * *";
        public const string CRON_HOURLY = "0 * * * *";
        public const string CRON_DAILY = "0 0 * * *";
        public const string CRON_WEEKLY = "0 0 * * 0";
        public const string CRON_MONTHLY = "0 0 1 * *";
        public const string CRON_YEARLY = "0 0 1 1 *";

        private static readonly ConcurrentDictionary<string, (Timer Timer, Action JobAction)> _jobs = new ConcurrentDictionary<string, (Timer Timer, Action JobAction)>();

        /// <summary>
        /// Adds or updates a recurring job based on a cron expression.
        /// </summary>
        /// <param name="jobId">The unique identifier for the job.</param>
        /// <param name="jobAction">The action to perform when the job is triggered.</param>
        /// <param name="cronExpression">A cron expression defining the schedule.</param>
        public static void AddOrUpdate(string jobId, Action jobAction, string cronExpression)
        {
            // Parse the cron expression
            var cron = CronExpression.Parse(cronExpression);
            var nextOccurrence = cron.GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo.Utc);

            if (nextOccurrence == null)
            {
                throw new ArgumentException("Invalid cron expression", nameof(cronExpression));
            }

            var dueTime = (int)(nextOccurrence.Value - DateTime.UtcNow).TotalMilliseconds;

            // Remove existing timer if any
            if (_jobs.TryGetValue(jobId, out var existingJob))
            {
                existingJob.Timer.Dispose();
            }

            // Add new timer
            var timer = new Timer(
                state =>
                {
                    try
                    {
                        jobAction.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Job {jobId} execution failed: {ex.Message}");
                    }
                    finally
                    {
                        // Schedule the next occurrence
                        AddOrUpdate(jobId, jobAction, cronExpression);
                    }
                },
                null,
                dueTime,
                Timeout.Infinite // One shot, we'll reschedule ourselves
            );

            _jobs[jobId] = (timer, jobAction);
        }

        /// <summary>
        /// Manually triggers a recurring job by its identifier.
        /// </summary>
        /// <param name="jobId">The identifier of the job to trigger.</param>
        public static bool TriggerJob(string jobId)
        {
            if (_jobs.TryGetValue(jobId, out var job))
            {
                try
                {
                    job.JobAction.Invoke();

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                //throw new ArgumentException($"Job with ID {jobId} not found.", nameof(jobId));

                return false;
            }
        }

        /// <summary>
        /// Removes a recurring job if it exists.
        /// </summary>
        /// <param name="jobId">The identifier of the job to remove.</param>
        public static void RemoveIfExists(string jobId)
        {
            if (_jobs.TryRemove(jobId, out var job))
            {
                job.Timer.Dispose();
                //Console.WriteLine($"Job {jobId} has been removed.");
            }
            else
            {
                //Console.WriteLine($"Job {jobId} was not found to remove.");
            }
        }
    }
}


