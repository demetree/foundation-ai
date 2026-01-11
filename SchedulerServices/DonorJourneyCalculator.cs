using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Foundation.Scheduler.Database;

namespace Foundation.Scheduler.Services
{
    public class DonorJourneyCalculator
    {
        private readonly SchedulerContext _context;

        public DonorJourneyCalculator(SchedulerContext context)
        {
            _context = context;
        }

        public ConstituentJourneyStage CalculateStage(Constituent constituent)
        {
            // Load all active stages ordered by sequence (descending preference typically, but let's assume higher sequence = "advanced")
            // Actually, usually we process rules from "highest" stage down to "lowest"? 
            // Or we check specific specific flags.
            // Let's assume Sequence implies progression.
            
            var stages = _context.ConstituentJourneyStages
                .Where(s => s.active && !s.deleted)
                .OrderByDescending(s => s.sequence)
                .ToList();

            // Calculate metrics for the constituent
            // Check if Gifts are already loaded (e.g. via Include) to avoid N+1
            List<Gift> gifts;
            if (constituent.Gifts != null)
            {
                 gifts = constituent.Gifts.Where(g => g.active && !g.deleted).ToList();
            }
            else
            {
                // Fallback to lazy/explicit loading query
                gifts = _context.Gifts
                    .Where(g => g.constituentId == constituent.id && g.active && !g.deleted)
                    .ToList();
            }

            decimal lifetimeGiving = gifts.Sum(g => g.amount);
            decimal annualGiving = gifts.Where(g => g.receivedDate >= DateTime.UtcNow.AddYears(-1)).Sum(g => g.amount);
            int giftCount = gifts.Count;
            DateTime? lastGiftDate = gifts.OrderByDescending(g => g.receivedDate).FirstOrDefault()?.receivedDate;
            int daysSinceLastGift = lastGiftDate.HasValue ? (int)(DateTime.UtcNow - lastGiftDate.Value).TotalDays : int.MaxValue;

            foreach (var stage in stages)
            {
                bool match = true;

                // 1. Min Annual Giving
                if (stage.minAnnualGiving.HasValue && annualGiving < stage.minAnnualGiving.Value)
                    match = false;

                // 2. Max Days Since Last Gift (Recency)
                // If stage requires "Recent" (e.g. < 365 days), and user is 500 days, FAIL.
                if (match && stage.maxDaysSinceLastGift.HasValue && daysSinceLastGift > stage.maxDaysSinceLastGift.Value)
                    match = false;

                // 3. Min Gift Count
                if (match && stage.minGiftCount.HasValue && giftCount < stage.minGiftCount.Value)
                    match = false;
                
                // 4. Min Lifetime Giving (Existing)
                if (match && stage.minLifetimeGiving.HasValue && lifetimeGiving < stage.minLifetimeGiving.Value)
                    match = false;

                if (match)
                {
                    // Found the highest sequence stage that matches all criteria
                    return stage;
                }
            }

            // Fallback to default if no match found
            return stages.FirstOrDefault(s => s.isDefault) ?? stages.LastOrDefault(); // Fallback to lowest sequence/default
        }
    }
}
