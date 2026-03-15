# PHMC Scheduler — UX Polish Audit & Recommendations

**Date:** 2026-03-15

## Summary

Conducted a comprehensive UX audit of the PHMC Scheduler from Denise's (recreation committee coordinator) perspective. Tested all major workflows: dashboard overview, calendar views (week/month/day), event creation and editing, sidebar navigation, and mobile experience across desktop, tablet, and phone viewports.

## Findings

Identified 5 UX polish opportunities, ranked by impact-per-effort:

1. **Calendar auto-scroll to 8am** — Week/day view starts at 6am requiring scroll to see the 9am-4pm booking window (1 line fix)
2. **Color events by booking source** — All events appear as identical purple pills; no visual distinction between event types (config change)
3. **Dashboard widgets for Rec Committee** — Generic terminology ("Active Jobs", "Deployed") doesn't resonate with facility booking context
4. **Mobile auto-switch to Day view** — Week view is too cramped on phones; Day view works beautifully but requires manual switch
5. **Simplified sidebar navigation** — 12+ items, many not relevant for Denise's daily needs

## Key Decisions

- Prioritized by impact-per-effort: items 1, 2, 4 are quick wins for immediate implementation
- Items 3, 5 are pre-launch/post-launch improvements

## Changes Made

- Created `ux-polish-recommendations.md` artifact with detailed analysis and proposed fixes

## Testing / Verification

- Full browser walkthrough on desktop (1280x800), tablet (768x1024), and phone (375x667)
- Screenshots captured for calendar week view, mobile day view, and event modal
- Video recording of complete audit session
