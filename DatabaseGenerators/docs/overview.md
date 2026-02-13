# DatabaseGenerators — Overview

DatabaseGenerators is a **container directory** housing the database schema definition projects for the Foundation platform modules.

---

## Sub-Generators

| Project | Purpose |
|---------|---------|
| `SecurityDatabaseGenerator/` | Defines the Security module database (users, roles, tenants, OIDC clients, sessions) — 18 files |
| `AuditorDatabaseGenerator/` | Defines the Auditor module database (audit events, entity states, error messages) — 11 files |
| `AlertingDatabaseGenerator/` | Defines the Alerting module database (incidents, rules, notification channels) — 11 files |
| `TelemetryDatabaseGenerator/` | Defines the Telemetry module database (metrics, snapshots, app registrations) — 11 files |
| `HangFireDatabaseGenerator/` | Defines HangFire background job database tables — 3 files |

---

## Architecture

Each sub-generator contains a single C# class that extends `CodeGenerationCommon.DatabaseGenerator` and defines all tables for its module using the fluent schema API.

These are consumed by the tool projects:
- `FoundationCoreTools` consumes Security + Auditor generators
- `AlertingTools` consumes the Alerting generator
- `SchedulerTools` does **not** reference these — it uses `SchedulerDatabaseGenerator/` directly

---

## Notes

The **SchedulerDatabaseGenerator** project lives outside this folder (at the repository root) because it is application-specific rather than a platform module.
