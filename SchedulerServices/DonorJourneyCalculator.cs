using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Foundation.Scheduler.Database;

namespace Foundation.Scheduler.Services
{
    public class DonorJourneyCalculator
    {
        private readonly DatabaseContext _context;

        public DonorJourneyCalculator(DatabaseContext context)
        {
            _context = context;
        }

        public ConstituentJourneyStage CalculateStage(Constituent constituent)
        {
            // Load all active stages ordered by sequence (descending preference typically, but let's assume higher sequence = "advanced")
            // Actually, usually we process rules from "highest" stage down to "lowest"? 
            // Or we check specific specific flags.
            // Let's assume Sequence implies progression.
            
            var stages = _context.ConstituentJourneyStage
                .Where(s => s.Active && !s.Deleted)
                .OrderByDescending(s => s.Sequence)
                .ToList();

            // Calculate metrics for the constituent
            // We need gifts. Assuming they are loaded or we load them.
            // If they are not loaded, we should load them.
            
            // Note: This assumes Gift table links to Constituent. checking schema would be good but standard convention applies.
            // Let's safe-guard lazy loading or explicit loading.
            var gifts = _context.Gift
                .Where(g => g.ConstituentId == constituent.Id && g.Active && !g.Deleted)
                .ToList();

            decimal lifetimeGiving = gifts.Sum(g => g.Amount);
            decimal annualGiving = gifts.Where(g => g.Date >= DateTime.UtcNow.AddYears(-1)).Sum(g => g.Amount);
            int giftCount = gifts.Count;
            DateTime? lastGiftDate = gifts.OrderByDescending(g => g.Date).FirstOrDefault()?.Date;
            int daysSinceLastGift = lastGiftDate.HasValue ? (int)(DateTime.UtcNow - lastGiftDate.Value).TotalDays : int.MaxValue;

            foreach (var stage in stages)
            {
                bool match = true;

                // 1. Min Annual Giving
                if (stage.MinAnnualGiving.HasValue && annualGiving < stage.MinAnnualGiving.Value)
                    match = false;

                // 2. Max Days Since Last Gift (Recency)
                // If stage requires "Recent" (e.g. < 365 days), and user is 500 days, FAIL.
                if (match && stage.MaxDaysSinceLastGift.HasValue && daysSinceLastGift > stage.MaxDaysSinceLastGift.Value)
                    match = false;

                // 3. Min Gift Count
                if (match && stage.MinGiftCount.HasValue && giftCount < stage.MinGiftCount.Value)
                    match = false;
                
                // 4. Min Lifetime Giving (Existing)
                if (match && stage.MinLifetimeGiving.HasValue && lifetimeGiving < stage.MinLifetimeGiving.Value)
                    match = false;

                if (match)
                {
                    // Found the highest sequence stage that matches all criteria
                    return stage;
                }
            }

            // Fallback to default if no match found
            return stages.FirstOrDefault(s => s.IsDefault) ?? stages.LastOrDefault(); // Fallback to lowest sequence/default
        }
    }
}
