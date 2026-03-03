---
description: How to get up to speed with the codebase
---

# Onboarding Workflow

Read the following documentation files in order to understand the project structure, architecture, coding standards, and development workflow.

## Step 1 — Read the Project Guidelines

Read the root-level README which contains all coding standards, style rules, and development process guidelines:

- `README.md` — coding style, naming conventions, commenting standards, white space rules, AI usage policy, branching strategy

## Step 2 — Read the Scheduler Documentation

Read these files in the `Scheduler/docs/` folder:

1. `Scheduler/docs/architecture.md` — system overview, server/client architecture, controller pattern, database contexts, authentication
2. `Scheduler/docs/key-concepts.md` — core domain entities (ScheduledEvent, Resource, Office, Client, etc.), entity relationships, key workflows (recurrence, conflict detection, rate resolution)
3. `Scheduler/docs/getting-started.md` — prerequisites, database setup, server/client setup, running locally
4. `Scheduler/docs/contributing.md` — what not to edit (auto-generated folders), how to add entities, custom component/controller/service templates

## Step 3 — Read the Foundation Documentation

Read these files to understand the platform layer:

1. `Foundation/docs/architecture.md` — Foundation system overview, admin hub, controller sources, client components
2. `FoundationCore/docs/key-classes.md` — core library classes
3. `FoundationCore.Web/docs/key-classes.md` — base controllers, security/auditor controllers, SignalR hubs, startup helpers

## Step 4 — Read Supporting Project Docs

Read these files for tooling and supporting systems:

1. `SchedulerTools/docs/overview.md` — code generation runner, what it produces, usage
2. `AlertingDatabase/docs/overview.md` — Alerting EF Core model
3. `FoundationCommon/docs/overview.md` — cross-platform utilities
4. `SchedulerDatabase/docs/overview.md` — Scheduler EF Core model

## Step 5 — Familiarize with Key Directories

Scan these directories to understand the project layout:

- `Scheduler/Scheduler.Server/Controllers/` — custom business logic controllers
- `Scheduler/Scheduler.Server/Services/` — custom services
- `Scheduler/Scheduler.Client/src/app/components/` — custom UI components (look at `resource-custom/` or `office-custom/` as examples)
- `Scheduler/Scheduler.Client/src/app/services/` — custom Angular services

## Key Reminders

- **Do not edit** files in `DataControllers/`, `scheduler-data-services/`, or `scheduler-data-components/` — these are auto-generated
- All DateTime values must be **UTC**
- Follow the explicit comparison style: `if (x == true)` not `if (x)`
- Always use braces, even for single-line `if` statements
- Two blank lines between functions
- AI-developed code must be clearly labeled
