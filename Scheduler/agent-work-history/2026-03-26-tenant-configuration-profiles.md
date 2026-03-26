# Tenant Configuration Profiles for SchedulerTools

**Date:** 2026-03-26

## Summary

Added a "Configure Tenant with Default Profile" feature to SchedulerTools (menu option 5) that seeds sensible default configuration records for any tenant based on a chosen business vertical. Three profiles are available: Small Town Operations & Recreation Committee, Healthcare Clinic / HomeCare Scheduling, and Construction / Technical Services.

## Changes Made

- **[NEW] `SchedulerTools/Profiles/ITenantConfigurationProfile.cs`** — Interface defining the profile contract (`Name`, `Description`, `Apply()`)
- **[NEW] `SchedulerTools/Profiles/TenantConfigurationContext.cs`** — Shared context holding tenant GUID, geography IDs, and currency ID
- **[NEW] `SchedulerTools/Profiles/SmallTownProfile.cs`** — Seeds ~28 records (priorities, calendars, resource types, assignment roles, client types, scheduling target types, rate types, tax codes, charge types, event types, crews, document types, tenant profile)
- **[NEW] `SchedulerTools/Profiles/HealthcareProfile.cs`** — Seeds ~35 records for clinic/home-care scheduling workflows
- **[NEW] `SchedulerTools/Profiles/ConstructionProfile.cs`** — Seeds ~37 records for construction/technical services dispatch
- **[NEW] `SchedulerTools/Profiles/TenantConfigurationRunner.cs`** — Interactive orchestrator: tenant picker from Security DB, geography/currency prompts (pick existing or create new), profile selection, execution with progress output
- **[MOD] `SchedulerTools/Program.cs`** — Added `using`, menu option 5, and `case D5/NumPad5` wiring

## Key Decisions

- **Hardcoded C# profiles** over JSON config files — compile-time safety, simpler for a dev tool
- **Interactive geography/currency prompts** — user picks or creates Country, StateProvince, TimeZone, and Currency during setup
- **Configuration data only** — no sample/operational data (contacts, clients, scheduling targets); users create those via the app
- **Existing PHMC code untouched** — `ConfigurePettyHarbour()` and `LoadPettyHarbourData()` remain as options 3 and 4
- **Idempotent seeding** — all profiles use check-before-insert pattern, safe to re-run
- **DocumentType is global** (no `tenantGuid`) — seeded without tenant scope; all other entities are tenant-scoped

## Testing / Verification

- `dotnet build SchedulerTools\SchedulerTools.csproj` — **Build succeeded** (0 errors)
- Manual verification of the interactive flow is required against a live database with existing SecurityTenant records
