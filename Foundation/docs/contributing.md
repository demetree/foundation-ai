# Foundation — Contributing

Contribution patterns specific to the Foundation project. For general code style, see the root [README.md](file:///d:/source/repos/scheduler/README.md).

---

## Project Scope

Foundation is an **administrative console** — it hosts platform modules, not application business logic. Code that belongs here:
- Security module administration UI
- Audit log viewing and management UI
- Telemetry dashboards
- Cross-system health monitoring
- OIDC configuration

Code that does **not** belong here:
- Application-specific business logic (goes in Scheduler, Alerting, etc.)
- Shared platform logic (goes in FoundationCore or FoundationCore.Web)
- Reusable utilities (goes in FoundationCommon)

---

## Adding a New Module View

If a new Foundation module needs a UI dashboard in the Foundation console:

1. Create a component group under `Foundation.Client/src/app/components/<module-name>/`
2. Follow the existing component patterns (listing → table → detail → tabs)
3. Add the route in `app-routing.module.ts`
4. Register components in `app.module.ts`
5. Add sidebar navigation link

---

## Controller Registration

Foundation does not list controllers individually like Scheduler. Instead, it uses **helper methods** from `FoundationCore.Web`:

```csharp
// In Program.cs
Foundation.Web.Utility.StartupBasics.AddSecurityWebAPIControllers(controllers);
Foundation.Web.Utility.StartupBasics.AddAuditorWebAPIControllers(controllers);
```

To add a new custom controller:

1. Create the controller in `Foundation.Server/Controllers/`
2. Register it directly in `Program.cs`: `controllers.Add(typeof(MyController));`
3. Follow the same pattern as `IncidentsController`

---

## Configuration

When adding new integrations, add configuration sections to `appsettings.json` with appropriate environment overrides in `appsettings.development.json`.

Key configuration areas:
- **`MonitoredApplications`** — add new apps here for health monitoring
- **`LogViewer.LogFolders`** — add log paths for cross-app log viewing
- **`Telemetry.Applications`** — add apps for telemetry collection
