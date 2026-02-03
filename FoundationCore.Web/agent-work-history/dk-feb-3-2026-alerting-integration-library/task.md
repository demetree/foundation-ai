# Alerting Integration Library

## Phase 1: Alerting API - Registration Endpoint
- [x] Create `IntegrationRegistrationController` 
- [x] Add OIDC-authenticated registration endpoint
- [x] Return API key on successful registration

## Phase 2: FoundationCore.Web Client Library
- [x] Create `AlertingIntegrationOptions` config class
- [x] Create `IAlertingIntegrationService` interface
- [x] Create `AlertingIntegrationService` implementation
- [x] Add `AddAlertingIntegration()` extension method
- [x] Add shared DTOs

## Phase 3: Webhook Handler Base Class
- [x] Create `AlertingWebhookControllerBase`
- [x] Handle all incident event types

## Verification
- [x] Build Alerting.Server (0 errors)
- [x] Build FoundationCore.Web (0 errors)
