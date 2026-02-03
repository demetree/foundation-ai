# Foundation → Alerting Auto-Registration

## Configuration
- [x] Add `Alerting` section to `appsettings.json`

## Service Registration
- [x] Add `AddAlertingIntegration()` call in Program.cs
- [x] Project reference to Foundation.Web already exists

## Auto-Registration Logic
- [x] Create `OidcTokenHelper.cs` for service account token
- [x] Add `RegisterWithAlertingAsync()` method to Program.cs
- [x] Wire up call after schema validation

## Verification
- [x] Build Foundation.Server - 0 errors
- [ ] Test startup with Alerting running
- [ ] Verify API key stored in SystemSettings
