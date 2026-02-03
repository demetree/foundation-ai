# Session Information

- **Conversation ID:** 73023cc9-1c67-48b5-a022-fcfcdfa664d5
- **Date:** 2026-02-03
- **Time:** 1:59 PM - 2:11 PM NST (-03:30)
- **Duration:** ~12 minutes

## Summary

Implemented auto-registration with the Alerting system during Foundation startup. When Foundation starts with an Alerting URL configured, it automatically registers itself and stores the API key in SystemSettings.

## Files Modified

### Foundation.Server
- `Program.cs` - Added DI registration for AlertingIntegration and auto-registration logic
- `appsettings.json` - Added `Alerting` configuration section
- `OIDC/OidcTokenHelper.cs` - New utility for obtaining service account OIDC tokens

## Key Features

1. **Configuration-Driven:** Only runs if `Alerting:BaseUrl` is configured
2. **Idempotent:** Skips if API key already exists in SystemSettings
3. **Non-Blocking:** Logs warning but doesn't fail startup if Alerting is unavailable
4. **Service Account Auth:** Uses configured ServiceAccount credentials for OIDC token

## SystemSettings Keys

After registration:
- `Alerting:Integration:Foundation:ApiKey`
- `Alerting:Integration:Foundation:IntegrationId`
- `Alerting:Integration:Foundation:ServiceId`

## Related Sessions

- `dk-feb-3-2026-alerting-integration-library` (FoundationCore.Web) - Created the shared AlertingIntegrationService that this session uses
