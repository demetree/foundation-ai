using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Foundation.Scheduler.Database;
using System.Threading.Tasks;

namespace Foundation.Scheduler.Services
{
    public class DonorJourneyCalculator
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<DonorJourneyCalculator> _logger;

        public DonorJourneyCalculator(SchedulerContext context, ILogger<DonorJourneyCalculator> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ConstituentJourneyStage> CalculateStageAsync(Constituent constituent)
        {
            // Load all active stages ordered by sequence
            var stages = await _context.ConstituentJourneyStages
                .Where(s => s.active && !s.deleted)
                .OrderByDescending(s => s.sequence)
                .ToListAsync();

            //
            // Calculate metrics for the constituent
            // Check if Gifts are already loaded (e.g. via Include) to avoid N+1
            //
            // If null or empty list, then attempt to read from the context because we assume lazy loading was not enabled on thr provided constituent.
            //
            // Worst case, this causes an extra 0 record read.
            //
            List<Gift> gifts;
            if (constituent.Gifts != null && constituent.Gifts.Count > 0)
            {
                gifts = constituent.Gifts.Where(g => g.active && !g.deleted).ToList();
            }
            else
            {
                //
                // Fallback to lazy/explicit loading query
                //
                gifts = await _context.Gifts
                    .Where(g => g.constituentId == constituent.id && g.active && !g.deleted)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }

            decimal lifetimeGiving = gifts.Sum(g => g.amount);
            decimal annualGiving = gifts.Where(g => g.receivedDate >= DateTime.UtcNow.AddYears(-1)).Sum(g => g.amount);
            int giftCount = gifts.Count;

            DateTime? lastGiftDate = gifts.OrderByDescending(g => g.receivedDate).FirstOrDefault()?.receivedDate;
            int daysSinceLastGift = lastGiftDate.HasValue ? (int)(DateTime.UtcNow - lastGiftDate.Value).TotalDays : int.MaxValue;

            return StageCalculation(constituent, stages, lifetimeGiving, annualGiving, giftCount, daysSinceLastGift);
        }


        public ConstituentJourneyStage CalculateStage(Constituent constituent)
        {
            // Load all active stages ordered by sequence
            var stages = _context.ConstituentJourneyStages
                .Where(s => s.active && !s.deleted)
                .OrderByDescending(s => s.sequence)
                .ToList();

            //
            // Calculate metrics for the constituent
            // Check if Gifts are already loaded (e.g. via Include) to avoid N+1
            //
            // If null or empty list, then attempt to read from the context because we assume lazy loading was not enabled on thr provided constituent.
            //
            // Worst case, this causes an extra 0 record read.
            //
            List<Gift> gifts;
            if (constituent.Gifts != null && constituent.Gifts.Count > 0)
            {
                gifts = constituent.Gifts.Where(g => g.active && !g.deleted).ToList();
            }
            else
            {
                //
                // Fallback to lazy/explicit loading query
                //
                gifts = _context.Gifts
                    .Where(g => g.constituentId == constituent.id && g.active && !g.deleted)
                    .ToList();
            }

            decimal lifetimeGiving = gifts.Sum(g => g.amount);
            decimal annualGiving = gifts.Where(g => g.receivedDate >= DateTime.UtcNow.AddYears(-1)).Sum(g => g.amount);
            int giftCount = gifts.Count;

            DateTime? lastGiftDate = gifts.OrderByDescending(g => g.receivedDate).FirstOrDefault()?.receivedDate;
            int daysSinceLastGift = lastGiftDate.HasValue ? (int)(DateTime.UtcNow - lastGiftDate.Value).TotalDays : int.MaxValue;

            return StageCalculation(constituent, stages, lifetimeGiving, annualGiving, giftCount, daysSinceLastGift);
        }


        private ConstituentJourneyStage StageCalculation(Constituent constituent, List<ConstituentJourneyStage> stages, decimal lifetimeGiving, decimal annualGiving, int giftCount, int daysSinceLastGift)
        {
            _logger.LogInformation("Calculating Stage for Constituent {Id}. Metrics: Lifetime={Lifetime}, Annual={Annual}, Count={Count}, DaysSinceLast={Days}",
                            constituent.id, lifetimeGiving, annualGiving, giftCount, daysSinceLastGift);

            foreach (var stage in stages)
            {
                bool match = true;

                // 1. Min Annual Giving
                if (stage.minAnnualGiving.HasValue && annualGiving < stage.minAnnualGiving.Value)
                {
                    match = false;
                    _logger.LogDebug("Stage {StageName} (Seq {Seq}) Skipped: AnnualGiving {Annual} < Min {Min}", stage.name, stage.sequence, annualGiving, stage.minAnnualGiving);
                }

                // 2. Max Days Since Last Gift (Recency)
                if (match && stage.maxDaysSinceLastGift.HasValue && daysSinceLastGift > stage.maxDaysSinceLastGift.Value)
                {
                    match = false;
                    _logger.LogDebug("Stage {StageName} (Seq {Seq}) Skipped: DaysSinceLast {Days} > Max {Max}", stage.name, stage.sequence, daysSinceLastGift, stage.maxDaysSinceLastGift);
                }

                // 3. Min Gift Count
                if (match && stage.minGiftCount.HasValue && giftCount < stage.minGiftCount.Value)
                {
                    match = false;
                    _logger.LogDebug("Stage {StageName} (Seq {Seq}) Skipped: GiftCount {Count} < Min {Min}", stage.name, stage.sequence, giftCount, stage.minGiftCount);
                }

                // 4. Min Lifetime Giving (Existing)
                if (match && stage.minLifetimeGiving.HasValue && lifetimeGiving < stage.minLifetimeGiving.Value)
                {
                    match = false;
                    _logger.LogDebug("Stage {StageName} (Seq {Seq}) Skipped: LifetimeGiving {Lifetime} < Min {Min}", stage.name, stage.sequence, lifetimeGiving, stage.minLifetimeGiving);
                }

                if (match)
                {
                    _logger.LogInformation("Matched Stage: {StageName} (Seq {Seq})", stage.name, stage.sequence);
                    return stage;
                }
            }

            // Fallback to default if no match found
            var fallback = stages.FirstOrDefault(s => s.isDefault) ?? stages.LastOrDefault();
            _logger.LogInformation("No specific match found. Fallback to: {StageName}", fallback?.name ?? "None");

            return fallback;
        }
    }
}
