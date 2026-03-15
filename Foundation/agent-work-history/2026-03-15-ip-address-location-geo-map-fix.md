# IpAddressLocation Geo Map Fix

**Date:** 2026-03-15

## Summary

Fixed the login geo map visualization in Foundation.Client which was never displaying markers. Three bugs were identified and resolved across client and server layers.

## Changes Made

- **Foundation.Client** `login-attempt-custom-listing.component.ts` — Changed `includeRelations: false` to `true` so the server returns the `ipAddressLocation` nav property with each login attempt.
- **Foundation.Client** `login-geo-map.component.ts` — Fixed `processAttempts()` to read geo fields (`latitude`, `longitude`, `countryCode`, etc.) from the nested `attempt.ipAddressLocation` nav property instead of directly from the attempt object (which doesn't have those fields).
- **Foundation.Server** `Program.cs` — Registered `IIpGeolocationService`, `IpAddressLocationManager`, and `IpAddressLocationWorker` in DI. These services were already registered in BMC and Alerting but were missing from Foundation.Server, meaning the background IP resolution worker never started.

## Key Decisions

- The `IpAddressLocationWorker` resolves external IPs via ip-api.com (free tier, 45 req/min). Localhost IPs (`127.0.0.1`, `::1`) are skipped since they cannot be geo-resolved.
- BMC and Alerting already had the service registrations — only Foundation.Server needed the addition.

## Testing / Verification

- Confirmed via browser screenshot that prior to the fix, the geo map showed "No Geographic Data Available" with "49 attempt(s) without geo data."
- Server-side controller was verified to correctly include `ipAddressLocation` via `.Include()` when `includeRelations=true`.
- After the fix, the worker will resolve the 4 external IP attempts (`103.21.175.246`); the remaining 45 from `127.0.0.1` are correctly unresolvable.
