# Fix Landing Page Stats to Show Real Database Counts

**Date:** 2026-03-09

## Summary

Investigated and fixed the BMC landing page stat counters, which were displaying misleading numbers. While the counters were dynamically fetched from the server (not hard-coded), the `totalParts` value was computed as the aggregate sum of part counts across all sets (millions), rather than the count of unique brick parts (~79K+). The landing page also only showed 3 stats vs. the welcome page's 5. Additionally, hard-coded "79,000+" strings appeared in taglines and feature descriptions.

## Changes Made

- **`BMC.Server/Controllers/PublicShowcaseController.cs`** — Injected `BMCContext`, made `GetStats()` async, replaced misleading `sets.Sum(PartCount)` with real `CountAsync` queries against `BrickParts`, `BrickColours`, `LegoMinifigs`, and `LegoThemes` tables. Added `totalColours` and `totalMinifigs` to the response.
- **`BMC.Client/src/app/components/public-landing/public-landing.component.ts`** — Added `displayColours`/`displayMinifigs` counters, mapped new API fields, added counter animations. Replaced hard-coded "79,000+" in taglines and feature descriptions with dynamic values populated from real stats.
- **`BMC.Client/src/app/components/public-landing/public-landing.component.html`** — Expanded from 3 to 5 stat cards (Sets, Unique Parts, Colours, Themes, Minifigs). Changed "Total Parts" label to "Unique Parts". Updated skeleton loader to 5 placeholders.
- **`BMC.Client/src/app/components/welcome/welcome.component.ts`** — Removed hard-coded "79,000+ parts" from feature link description.

## Key Decisions

- Injected `BMCContext` directly into the controller (scoped per-request) rather than using `IServiceScopeFactory`, since controllers already have scoped lifetime.
- Used `CountAsync` with `active && !deleted` filtering to match the same filtering logic used by the authenticated `RowCount` endpoints.
- Results are cached for 1 hour (matching existing cache policy) so the DB count queries only run once per hour.
- Changed the tagline to be dynamically inserted after stats load, using index 0 of the taglines array.

## Testing / Verification

- Server build (`dotnet build BMC.Server.csproj`): ✅ 0 errors
- Manual verification needed: confirm the 5 stat cards render correctly and show matching numbers to the welcome page.
