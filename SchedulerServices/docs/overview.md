# SchedulerServices — Overview

SchedulerServices is a **.NET 10** class library containing shared business logic services used by the Scheduler application.

---

## Contents

| File | Purpose |
|------|---------|
| `DonorJourneyCalculator.cs` | Calculates and updates donor journey stages based on giving history and engagement metrics |
| `QuickBooksIntegrator.cs` | Integration hooks for syncing event charges with QuickBooks (journal entries, invoices) |
| `ServiceThoughts.cs` | Design notes and pseudocode for future service implementations (not compiled — entirely commented out) |

---

## Registration

Services from this library are registered in `Scheduler.Server/Program.cs`:

```csharp
builder.Services.AddScoped<Foundation.Scheduler.Services.DonorJourneyCalculator>();
```

---

## Dependencies

- `SchedulerDatabase` — database entity access
- `FoundationCore` — Foundation base classes

---

## Notes

This project is in active development. `ServiceThoughts.cs` contains design sketches for:
- Template-based event creation with automatic charge dropping
- Batch QuickBooks export of pending charges
- Excel export of event charges
