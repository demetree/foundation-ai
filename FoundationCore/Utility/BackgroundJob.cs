using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Foundation
{
    public class BackgroundJob
    {
        // A simple queue to hold the actions to be executed
        private static readonly ConcurrentQueue<Action> _jobQueue = new ConcurrentQueue<Action>();

        // Thread to run background jobs
        private static readonly Task _workerTask;

        static BackgroundJob()
        {
            // Initialize the worker task
            _workerTask = Task.Run(() => RunJobsAsync());
        }

        /// <summary>
        /// Enqueues a job to be run in the background. 
        /// </summary>
        /// <param name="jobAction">The action to be executed as a job.</param>
        public static void Enqueue(Action jobAction)
        {
            _jobQueue.Enqueue(jobAction);
        }

        // This method runs in a background thread to process jobs from the queue
        private static async Task RunJobsAsync()
        {
            while (true)
            {
                if (_jobQueue.TryDequeue(out Action job))
                {
                    try
                    {
                        job.Invoke();
                    }
                    catch (Exception ex)
                    {
                        // Log the exception or handle it as necessary
                        Console.WriteLine($"Job execution failed: {ex.Message}");
                    }
                }
                else
                {
                    // If no jobs are available, wait before checking again to avoid tight looping
                    await Task.Delay(100);
                }
            }
        }
    }
}
