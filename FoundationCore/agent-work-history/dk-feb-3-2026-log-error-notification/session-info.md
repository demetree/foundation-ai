# Session Information

- **Conversation ID:** dd8b30c7-cddc-4511-858b-01bc22213e01
- **Date:** 2026-02-03
- **Time:** 23:54 NST (-03:30)
- **Duration:** ~3 hours

## Summary

Implemented a comprehensive Log Error Notification System for the Foundation framework that captures Exception/Error level logs and sends batched notifications via email and/or the Alerting system. The system supports global consumer registration (applies to all loggers), early startup initialization (before DI), and standalone alerting via direct HTTP calls.

## Key Features

- **First-Failure Immediate Notification with Suppression:** Sends immediate notification on first error, then batches subsequent errors for a configurable window (default 10 minutes)
- **Global Logger Consumer:** Uses `Logger.AddGlobalConsumer()` to receive logs from ALL logger instances
- **Pre-DI Initialization:** `InitializeFromConfiguration()` method reads from appsettings.json and works before DI container builds
- **Standalone Alerting:** Direct HTTP calls to Alerting API without requiring DI service, with API key retrieved from SystemSettings

## Files Created

- `FoundationCore/Utility/LogErrorNotificationOptions.cs` - Configuration options class
- `FoundationCore/Utility/LogErrorNotificationConsumer.cs` - Core consumer with static initialization
- `FoundationCore.Web/Services/LogErrorNotificationExtensions.cs` - DI extensions and config-based initialization

## Files Modified

- `FoundationCore/Utility/Logger.cs` - Added `AddGlobalConsumer()` and `RemoveGlobalConsumer()` static methods
- `Scheduler/Scheduler.Server/Program.cs` - Added early initialization call and Alerting integration
- `Scheduler/Scheduler.Server/appsettings.json` - Added LogErrorNotification configuration section

## Related Sessions

This is part of the ongoing Foundation framework enhancement effort, building on the Alerting and Incident Management module.
