using Foundation.Scheduler.Database;
using Foundation.Scheduler.Services;
using Microsoft.EntityFrameworkCore;

namespace Scheduler.Worker
{
    public class DonorJourneyProcessor : BackgroundService
    {
        private readonly ILogger<DonorJourneyProcessor> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DonorJourneyProcessor(ILogger<DonorJourneyProcessor> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("DonorJourneyProcessor running at: {time}", DateTimeOffset.Now);

                try
                {
                        int pageSize = 1000;
                        int pageNumber = 0;
                        bool hasMore = true;

                        while (hasMore && !stoppingToken.IsCancellationRequested)
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
                                var calculator = scope.ServiceProvider.GetRequiredService<DonorJourneyCalculator>();

                                var constituents = await context.Constituents
                                    .Include(c => c.Gifts)
                                    .Where(c => !c.deleted && c.active)
                                    .OrderBy(c => c.id)
                                    .Skip(pageNumber * pageSize)
                                    .Take(pageSize)
                                    .AsSplitQuery()
                                    .ToListAsync(stoppingToken);

                                if (constituents.Count == 0)
                                {
                                    hasMore = false;
                                    break;
                                }

                                foreach (var constituent in constituents)
                                {
                                    try 
                                    {
                                        var stage = calculator.CalculateStage(constituent);
                                        if (stage != null && constituent.constituentJourneyStageId != stage.id)
                                        {
                                            _logger.LogInformation("Updating stage for Constituent {Id}: {OldStage} -> {NewStage}", 
                                                constituent.id, constituent.constituentJourneyStageId, stage.name);
                                                
                                            constituent.constituentJourneyStageId = stage.id;
                                            constituent.dateEnteredCurrentStage = DateTime.UtcNow;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error processing constituent {Id}", constituent.id);
                                    }
                                }

                                await context.SaveChangesAsync(stoppingToken);
                            }
                            pageNumber++;
                        }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing Donor Journey Job");
                }

                // Run daily (or every X hours)
                // For demo/testing, let's say every 24 hours.
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
