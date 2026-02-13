# Scheduler.Worker — Overview

Scheduler.Worker is a **.NET 10** background worker service for running long-lived or periodic tasks outside the Scheduler web server process.

---

## Current State

> [!NOTE]
> This project is in **early development**. The `Worker` class is a stub `BackgroundService` that logs a heartbeat every second. Real processing logic is under development.

---

## Contents

| File | Purpose |
|------|---------|
| `Program.cs` | Configures and starts the hosted worker service |
| `Worker.cs` | `BackgroundService` implementation (currently a stub loop) |
| `DonorJourneyProcessor.cs` | Planned background processor for running `DonorJourneyCalculator` periodically |

---

## Architecture

The worker runs as a standalone process (not inside the web server). It is intended to handle:
- Periodic donor journey recalculations
- Other batch processing tasks that shouldn't block the web API

---

## Dependencies

- `SchedulerDatabase` — database access
- `SchedulerServices` — `DonorJourneyCalculator`
