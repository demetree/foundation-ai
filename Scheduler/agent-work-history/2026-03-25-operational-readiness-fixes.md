# Operational Readiness Fixes — Findings 1, 2, 4, 5, 6

**Date:** 2026-03-25

## Summary

Implemented five fixes identified during the Scheduler operational readiness review to harden the system for production deployment.

## Changes Made

- **SchedulerMetricsProvider.cs** — Refactored to use `IServiceScopeFactory` for DI-resolved `SchedulerContext` instead of direct `new SchedulerContext()` construction. Added `Microsoft.Extensions.DependencyInjection` import. Added clarifying comment that cross-tenant metrics are intentional for the admin dashboard. Added AI-developed header label.
- **FileManagerController.cs** — Three changes:
  1. Added null-check with error logging on scratchpad reflection `SetValue` call (~L1601) to surface FK property typos immediately.
  2. Added `[EnableRateLimiting("ShareLinkPolicy")]` attribute to 3 anonymous share link endpoints (`GetShareLinkInfo`, `VerifyShareLinkPassword`, `DownloadSharedFile`). Added `using Microsoft.AspNetCore.RateLimiting`.
  3. Changed `EmailDocument` sender from `securityUser.emailAddress` to `null` so `SendGridEmailService` uses the system-configured `EmailFromAddress`, avoiding SendGrid sender verification issues.
- **Program.cs** — Added `AddRateLimiter` service configuration with `ShareLinkPolicy` (fixed window: 30 requests/minute per IP) and `UseRateLimiter()` middleware after `UseAuthorization`. Added `using System.Threading.RateLimiting` and `using Microsoft.AspNetCore.RateLimiting`.

## Key Decisions

- Rate limiting uses a fixed-window policy at 30 req/min/IP — conservative enough for legitimate use, protective against automated abuse on the unauthenticated endpoints.
- Email sender fix leverages `SendGridEmailService`'s existing fallback logic (null senderEmail → config `EmailFromAddress`) rather than duplicating config resolution.
- Cross-tenant metrics in `SchedulerMetricsProvider` confirmed as intentional — admin System Health dashboard requires global operational totals.

## Testing / Verification

- `dotnet build Scheduler.Server` — **0 errors**, 145 pre-existing nullable annotation warnings (from auto-generated code).
- No functional tests run — manual verification needed for rate limiting (429 responses), email sender address, and System Health metrics loading.
