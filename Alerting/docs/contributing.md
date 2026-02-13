# Alerting — Contributing

Contribution patterns specific to the Alerting project. For general code style, see the root [README.md](file:///d:/source/repos/scheduler/README.md).

---

## What Not to Edit

> [!CAUTION]
> The following are auto-generated and must not be edited manually:

| Folder | Generator |
|--------|-----------|
| `Alerting.Server/DataControllers/` (37 files) | `AlertingTools` via `WebAPICodeGenerator` |
| `Alerting.Client/src/app/alerting-data-services/` | `AlertingTools` via `AngularServiceGenerator` |
| `Alerting.Client/src/app/alerting-data-components/` | `AlertingTools` via `AngularComponentGenerator` |
| `AlertingDatabase/Database/` | EF Core Power Tools |
| `AlertingDatabase/EntityExtensions/` | `AlertingTools` via `EntityExtensionGenerator` |

---

## Adding a New Custom Controller

1. Create `Alerting.Server/Controllers/MyController.cs` inheriting from `FoundationControllerBase`
2. Register in `Program.cs`:
   ```csharp
   controllers.Add(typeof(MyController));
   ```
3. Follow the pattern of existing controllers (`IncidentManagementController`, `AlertsController`)

---

## Adding a New Notification Provider

The notification system uses a provider pattern:

1. Create `Alerting.Server/Services/Notifications/MyNotificationProvider.cs`
2. Implement `INotificationProvider`
3. Register in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<INotificationProvider, MyNotificationProvider>();
   ```
4. Add any required configuration to `NotificationEngineOptions`
5. The `NotificationDispatcher` will automatically include it in multi-channel dispatch

---

## Adding a New Service

1. Create the interface in `Services/IMyService.cs`
2. Create the implementation in `Services/MyService.cs`
3. Register in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<IMyService, MyService>();
   ```
4. Inject via constructor in controllers

---

## Database Changes

When the Alerting database schema needs to change:

1. Update the schema definition in `DatabaseGenerators/AlertingDatabaseGenerator/`
2. Run `AlertingTools` to regenerate SQL scripts and code
3. Apply SQL scripts to the database
4. Run EF Core Power Tools to update `AlertingDatabase/Database/`
5. Never edit `DataControllers/` or `EntityExtensions/` manually

---

## AI-Developed Code

Follow the conventions in the root README for labeling AI-assisted code. Mark files with `// AI-Developed` or `// AI-Assisted` headers as appropriate.
