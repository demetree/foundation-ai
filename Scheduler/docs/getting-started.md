# Scheduler — Getting Started

This guide walks you through setting up a local development environment for the Scheduler Server and Client projects.

---

## Prerequisites

| Requirement | Version | Notes |
|-------------|---------|-------|
| **.NET SDK** | 10.0+ | Required for the Server project |
| **Node.js** | 18+ (LTS recommended) | Required for the Angular Client |
| **npm** | 9+ | Comes with Node.js |
| **Angular CLI** | 17.x | Install globally: `npm install -g @angular/cli` |
| **SQL Server** | 2019+ or LocalDB | Three databases are required |
| **Git** | Latest | Must have long paths enabled (see below) |
| **Visual Studio** or **VS Code** | Latest | VS recommended for debugging the Server |

### Git Long Path Support

> [!IMPORTANT]
> This codebase contains deeply nested paths that exceed Windows' default 260-character limit. You **must** enable long path support before cloning or working with the repository.

Run these once on your machine:

```powershell
# Enable Git long paths
git config --global core.longpaths true
```

Also enable long paths in the Windows registry:
1. Press `Win + R`, type `regedit`, press Enter
2. Navigate to `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem`
3. Set the `LongPathsEnabled` DWORD to `1` (create it if it doesn't exist)

---

## Repository Structure (Key Folders)

```
scheduler/
├── Scheduler/
│   ├── Scheduler.Server/        ← .NET 10 Web API
│   └── Scheduler.Client/        ← Angular 17 SPA
├── SchedulerDatabase/           ← EF Core entity model
├── SchedulerDatabaseGenerator/  ← Database script generator definitions
├── SchedulerTools/              ← Code generation runner
├── SchedulerServices/           ← Shared business logic
├── FoundationCommon/            ← Foundation utilities (cross-platform)
├── FoundationCore/              ← Foundation core (Security, Auditor, EF base classes)
├── FoundationCore.Web/          ← Foundation web (controllers, SignalR, Kestrel)
├── Foundation/                  ← Foundation merged client/server
├── Scheduler.sln                ← Solution file
└── README.md                    ← Code style and project guidelines
```

---

## Database Setup

The Scheduler requires **three SQL Server databases**:

| Database | Purpose |
|----------|---------|
| `Scheduler` | Application data (events, resources, offices, etc.) |
| `Security` | User accounts, roles, OIDC clients |
| `Auditor` | Access audit logs |

> [!NOTE]
> Database creation scripts are produced by running `SchedulerTools` against the `SchedulerDatabaseGenerator` definitions. This is a **manual step** performed by the project owner — ask a team member for a current database backup or scripts if you're setting up for the first time.

Connection strings default to `LOCALHOST` with Windows Integrated Security. If your SQL Server instance uses a different name, you'll update it in `appsettings.development.json` (see below).


---

## Server Setup

### 1. Create `appsettings.development.json`

Copy the example file and customize for your environment:

```powershell
cd Scheduler\Scheduler.Server
copy appsettings.development.json.example appsettings.development.json
```

Key settings to review:
- **`ConnectionStrings`** — update `Server=LOCALHOST` if your SQL Server uses a named instance
- **`Kestrel.Endpoints`** — defaults to HTTP `10100` and HTTPS `10101`
- **`OIDC.Certificate` / `OIDC.Key`** — PEM certificate paths for JWT signing (defaults point to `Certificates/` folder)


### 2. Build and Run

**Via Visual Studio:**
1. Open `Scheduler.sln`
2. Set `Scheduler.Server` as the startup project
3. Press `F5` (or `Ctrl+F5` for without debugger)

**Via command line:**
```powershell
cd Scheduler\Scheduler.Server
dotnet run
```

The server will:
1. Validate all three database schemas against EF models
2. Register OIDC client applications
3. Start listening on the configured Kestrel endpoints
4. Swagger UI is available at `https://localhost:10101/swagger` in Development mode


---

## Client Setup

### 1. Install Dependencies

```powershell
cd Scheduler\Scheduler.Client
npm install
```

### 2. Run the Dev Server

```powershell
npx ng serve
```

The Angular dev server starts at **`http://localhost:4300/`** with live reload.

### 3. Build for Production

```powershell
npx ng build
```

Build output goes to `dist/`.  During a server `dotnet publish`, the Angular build is automatically triggered and the output is copied to `wwwroot/` (see the `PublishSPA` MSBuild target in `Scheduler.Server.csproj`).

---

## Running Both Together

For local development, the recommended workflow is:

1. Start the **Server** first (Visual Studio or `dotnet run`)
2. Start the **Client** dev server (`npx ng serve`)
3. The Angular app makes API calls to `https://localhost:10101`

The `proxy.conf.js` or environment configuration in the Angular app should point to the server's HTTPS endpoint.


---

## Common Issues

| Issue | Solution |
|-------|----------|
| **Path too long** errors on clone or build | Enable Git long paths and Windows long path registry key (see Prerequisites) |
| **Certificate errors** on HTTPS | Ensure the PEM files exist in `Scheduler.Server/Certificates/`. For local dev, the included self-signed certs work. |
| **Database connection failures** | Verify SQL Server is running and the connection string in `appsettings.development.json` matches your instance name |
| **Schema validation failure on startup** | The database is out of sync with the EF model. Ask the project owner to provide updated database scripts or a fresh backup. |
| **`ng serve` port conflict** | Check if port 4300 is in use. You can change it: `ng serve --port 4301` |
| **CORS errors in browser** | The server allows all origins in Development mode. If you still see errors, ensure the server is running and reachable. |
| **`npm install` fails** | Try deleting `node_modules/` and `package-lock.json`, then run `npm install` again |

---

## Useful Commands

```powershell
# Server
dotnet build                      # Build server project
dotnet run                        # Run server
dotnet publish                    # Publish (includes Angular build)

# Client
npx ng serve                     # Dev server with live reload
npx ng build                     # Production build
npx ng build --watch             # Watch mode build
npx ng generate component <name> # Scaffold a new component
npx ng test                      # Run unit tests
```
