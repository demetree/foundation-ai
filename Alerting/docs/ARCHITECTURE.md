# Foundation Alerting and Incident Management
## Architecture Documentation

> **Version**: 1.0 | **Status**: Production Ready | **Last Updated**: February 2026

---

# Executive Summary

**Foundation Alerting** is a production-ready, multi-tenant incident management platform integrated into the Foundation framework. It provides complete operational alerting from detection to resolution.

## Core Capabilities

| Capability | Description |
|------------|-------------|
| **Incident Lifecycle** | Full state machine (Triggered → Acknowledged → Resolved) with forensic audit trails |
| **Multi-Channel Notifications** | Email, SMS, Voice, Push, and Microsoft Teams delivery |
| **On-Call Scheduling** | Rotational participant management with layers and overrides |
| **Escalation Policies** | Configurable routing rules with timing delays and repeat logic |
| **Real-Time Dashboards** | Command center, flight control, and responder consoles |

## Technology Stack

- **Backend**: ASP.NET Core Web API with Entity Framework Core
- **Frontend**: Angular 18+ with Premium UI patterns
- **Database**: SQL Server with multi-tenant isolation
- **Integrations**: SendGrid (Email), Twilio (SMS/Voice), Firebase (Push)

## Value Proposition

```mermaid
mindmap
  root((Alerting))
    Detection
      API Inbound
      Log Bridge
      Webhooks
    Response
      Multi-Channel
      Escalation
      On-Call
    Resolution
      Acknowledge
      Notes
      Audit Trail
    Visibility
      Command Center
      Flight Control
      Health Matrix
```

---

# 1. System Architecture

## 1.1 Context Diagram

The Alerting module operates as an independent Foundation module while integrating with the broader ecosystem.

```mermaid
C4Context
    title System Context - Foundation Alerting

    Person(responder, "Responder", "Receives and responds to incidents")
    Person(admin, "Administrator", "Configures policies and schedules")
    
    System(alerting, "Alerting Server", "Incident management and notification orchestration")
    
    System_Ext(sources, "Alert Sources", "Monitoring systems, Foundation apps, Log bridges")
    System_Ext(channels, "Notification Channels", "SendGrid, Twilio, Firebase, Teams")
    System(security, "Security Module", "User identity and authentication")
    
    Rel(sources, alerting, "Trigger alerts", "REST API + API Key")
    Rel(alerting, channels, "Send notifications", "Provider SDKs")
    Rel(alerting, security, "Resolve identities", "Cross-DB query")
    Rel(responder, alerting, "Ack/Resolve", "Web UI")
    Rel(admin, alerting, "Configure", "Admin UI")
```

## 1.2 Container Diagram

```mermaid
C4Container
    title Container Diagram - Alerting Module

    Container(client, "Alerting Client", "Angular 18", "Premium admin UI and responder console")
    Container(server, "Alerting Server", "ASP.NET Core", "API, business logic, background workers")
    ContainerDb(db, "Alerting Database", "SQL Server", "Incidents, policies, schedules, preferences")
    ContainerDb(security_db, "Security Database", "SQL Server", "Users, teams, tenants")
    
    Rel(client, server, "HTTPS/REST", "OIDC Auth")
    Rel(server, db, "EF Core", "SQL")
    Rel(server, security_db, "Cross-DB Query", "Identity resolution")
```

## 1.3 Component Diagram (Server)

```mermaid
flowchart TB
    subgraph Controllers["API Layer"]
        AC[AlertsController]
        IC[IncidentManagementController]
        DC[DashboardController]
        IMC[IntegrationManagementController]
        NFC[NotificationFlightControlController]
        NAC[NotificationAuditController]
    end
    
    subgraph Services["Business Logic"]
        AS[AlertingService]
        ES[EscalationService]
        DS[DashboardService]
        US[UserService]
        NAS[NotificationAuditService]
    end
    
    subgraph Workers["Background Processing"]
        EW[EscalationWorker]
        NRW[NotificationRetryWorker]
    end
    
    subgraph Notifications["Notification Engine"]
        ND[NotificationDispatcher]
        EP[EmailProvider]
        SP[SmsProvider]
        VP[VoiceProvider]
        PP[PushProvider]
        TP[TeamsProvider]
    end
    
    Controllers --> Services
    Services --> Notifications
    Workers --> Services
    ES --> ND
    ND --> EP & SP & VP & PP & TP
```

---

# 2. Database Architecture

## 2.1 Table Groupings

The database is organized into logical groupings that reflect their purpose in the system.

```mermaid
erDiagram
    %% Configuration Tables
    EscalationPolicy ||--o{ EscalationRule : "contains"
    EscalationRule }o--|| OnCallSchedule : "targets"
    Service ||--o{ Integration : "has"
    Service }o--|| EscalationPolicy : "uses"
    
    %% Scheduling Tables
    OnCallSchedule ||--o{ ScheduleLayer : "contains"
    ScheduleLayer ||--o{ ScheduleLayerMember : "has"
    OnCallSchedule ||--o{ ScheduleOverride : "has"
    
    %% Operational Tables
    Incident }o--|| Service : "belongs to"
    Incident ||--o{ IncidentTimelineEvent : "has"
    Incident ||--o{ IncidentNotification : "generates"
    IncidentNotification ||--o{ NotificationDeliveryAttempt : "tracks"
    
    %% Preference Tables
    UserNotificationPreference ||--o{ UserNotificationChannelPreference : "contains"
```

## 2.2 Core Entity Groups

| Group | Tables | Purpose |
|-------|--------|---------|
| **Configuration** | `EscalationPolicy`, `EscalationRule`, `Service`, `Integration` | Defines routing and API access |
| **Scheduling** | `OnCallSchedule`, `ScheduleLayer`, `ScheduleLayerMember`, `ScheduleOverride` | Manages rotational on-call |
| **Operational** | `Incident`, `IncidentTimelineEvent`, `IncidentNotification`, `NotificationDeliveryAttempt` | Active incident data |
| **Preferences** | `UserNotificationPreference`, `UserNotificationChannelPreference`, `UserPushToken` | Per-user notification settings |
| **Static Types** | `SeverityType`, `IncidentStatusType`, `IncidentEventType`, `NotificationChannelType` | Lookup/reference data |

## 2.3 Multi-Tenancy

All configuration and operational tables include `tenantGuid` for complete data isolation. Identity references use `objectGuid` pointing to the central Security database.

---

# 3. Incident Lifecycle

## 3.1 State Machine

```mermaid
stateDiagram-v2
    [*] --> Triggered: Alert received
    
    Triggered --> Acknowledged: Responder claims
    Triggered --> Triggered: Escalation fires
    Triggered --> Resolved: Auto-resolve / API
    
    Acknowledged --> Resolved: Issue fixed
    
    Resolved --> [*]
    
    note right of Triggered
        nextEscalationAt drives
        background worker
    end note
    
    note right of Acknowledged
        Escalation timers
        halted
    end note
```

## 3.2 Escalation Flow

```mermaid
sequenceDiagram
    participant Source as Alert Source
    participant API as Alerting API
    participant DB as Database
    participant Worker as EscalationWorker
    participant Dispatcher as NotificationDispatcher
    participant Channels as Notification Channels
    
    Source->>API: POST /api/alerts/trigger
    API->>DB: Create Incident (Triggered)
    API->>DB: Set nextEscalationAt
    
    loop Every 15 seconds
        Worker->>DB: Query due escalations
        Worker->>DB: Load EscalationRule
        Worker->>DB: Resolve on-call user(s)
        Worker->>Dispatcher: Send notification
        Dispatcher->>Channels: Deliver via preference order
        Worker->>DB: Update nextEscalationAt
    end
    
    Note over Worker,DB: Continues until Acknowledged or Resolved
```

## 3.3 Timeline Events

Every state change is captured in `IncidentTimelineEvent`:

| Event ID | Type | Source |
|----------|------|--------|
| 1 | Triggered | system/api |
| 2 | Escalated | system |
| 3 | Acknowledged | user |
| 4 | Resolved | user/api |
| 5 | NoteAdded | user |
| 6 | NotificationSent | system |

---

# 4. Notification Engine

## 4.1 Multi-Channel Architecture

```mermaid
flowchart LR
    subgraph Orchestration
        ES[EscalationService]
        ND[NotificationDispatcher]
    end
    
    subgraph Preferences["User Preferences"]
        DND[Do Not Disturb]
        QH[Quiet Hours]
        CP[Channel Priority]
    end
    
    subgraph Providers["Channel Providers"]
        Email[Email<br/>SendGrid]
        SMS[SMS<br/>Twilio]
        Voice[Voice<br/>Twilio TTS]
        Push[Push<br/>Firebase FCM]
        Teams[Teams<br/>Webhooks]
    end
    
    subgraph Resilience
        NRW[RetryWorker]
        DB[(Delivery<br/>Attempts)]
    end
    
    ES --> ND
    ND --> Preferences
    Preferences --> Providers
    Providers --> DB
    DB --> NRW
    NRW -->|Retry failed| Providers
```

## 4.2 Channel Priority Resolution

```mermaid
flowchart TD
    Start([Notification Request]) --> CheckDND{DND Active?}
    CheckDND -->|Yes - Permanent| Skip[Skip Notification]
    CheckDND -->|Yes - Timed| CheckExpiry{Expired?}
    CheckExpiry -->|No| Skip
    CheckExpiry -->|Yes| CheckQH
    CheckDND -->|No| CheckQH{Quiet Hours?}
    
    CheckQH -->|In quiet window| LowPriOnly[Low Priority Channels Only]
    CheckQH -->|Outside window| AllChannels[All Enabled Channels]
    
    AllChannels --> Sort[Sort by Priority]
    LowPriOnly --> Sort
    Sort --> Deliver[Deliver in Order]
```

## 4.3 Provider Configuration

| Channel | Provider | Configuration Key |
|---------|----------|-------------------|
| Email | SendGrid | `Notifications:SendGrid:ApiKey` |
| SMS | Twilio | `Notifications:Twilio:AccountSid`, `AuthToken` |
| Voice | Twilio | Same as SMS, uses Polly neural TTS |
| Push | Firebase | `Notifications:Firebase:ProjectId`, `ServiceAccountJson` |
| Teams | Incoming Webhook | Per-integration webhook URL |

## 4.4 Retry Strategy

Failed deliveries are retried by `NotificationRetryWorker` with exponential backoff:
- **Attempt 1**: Immediate
- **Attempt 2**: +1 minute
- **Attempt 3**: +5 minutes
- **Attempt 4**: +15 minutes (final)

---

# 5. On-Call Scheduling

## 5.1 Rotation Algorithm

```mermaid
flowchart TD
    Input[/"Target DateTime"/] --> Calc["Calculate elapsed time<br/>from rotation anchor"]
    Calc --> Index["rotationIndex = elapsed ÷ rotationDays"]
    Index --> Member["memberIndex = rotationIndex mod memberCount"]
    Member --> Override{Override exists?}
    Override -->|REPLACE| UseReplacement[Use replacement user]
    Override -->|REMOVE| Gap[No coverage - Gap]
    Override -->|No override| UseBase[Use base member]
```

## 5.2 Schedule Structure

```mermaid
erDiagram
    OnCallSchedule {
        guid objectGuid
        string name
        string timeZoneId
    }
    
    ScheduleLayer {
        int layerLevel
        datetime rotationStart
        int rotationDays
        time handoffTime
    }
    
    ScheduleLayerMember {
        int position
        guid securityUserObjectGuid
    }
    
    ScheduleOverride {
        int overrideTypeId
        datetime startDateTime
        datetime endDateTime
        guid originalUserObjectGuid
        guid replacementUserObjectGuid
    }
    
    OnCallSchedule ||--o{ ScheduleLayer : "has layers"
    ScheduleLayer ||--o{ ScheduleLayerMember : "has members"
    OnCallSchedule ||--o{ ScheduleOverride : "has overrides"
```

## 5.3 Override Types

| Type | ID | Behavior |
|------|-----|----------|
| **Swap** | 1 | Reciprocal exchange between two users |
| **Replace** | 2 | Substitute one user with another |
| **Remove** | 3 | Creates a coverage gap |

---

# 6. Client UI Architecture

## 6.1 Component Hierarchy

```mermaid
flowchart TB
    subgraph Admin["Administrative Consoles"]
        AO[Alerting Overview<br/>Command Center]
        SM[Service Management]
        IM[Integration Management]
        EPM[Escalation Policy Management]
        SchM[Schedule Management]
        SchE[Schedule Editor]
    end
    
    subgraph Operations["Operational Dashboards"]
        ID[Incident Dashboard]
        IV[Incident Viewer]
        NFC[Notification Flight Control]
        SHM[Service Health Matrix]
        NA[Notification Audit]
        CH[Configuration Health]
    end
    
    subgraph Responder["Responder Interfaces"]
        RC[Responder Console]
        MS[My Shift]
        NPE[Notification Preferences]
    end
    
    AO --> Operations
    AO --> Admin
    ID --> IV
    MS --> RC
```

## 6.2 Key Premium UI Patterns

| Pattern | Component | Description |
|---------|-----------|-------------|
| **87** | Hero Headers | Gradient glassmorphism with bouncy icons |
| **23** | Flight Control | Real-time pipeline monitoring |
| **105** | My Shift | Personal on-call HUD |
| **135** | Title-Icon-Wrapper | Gold standard header consistency |
| **136** | Identity Proxy | Client-side GUID→name resolution |
| **139** | Notification Audit | Forensic content inspection |

---

# 7. API Surface

## 7.1 Custom Controllers

| Controller | Endpoints | Purpose |
|------------|-----------|---------|
| `AlertsController` | `POST /trigger`, `POST /resolve` | External alert ingestion |
| `IncidentManagementController` | Full CRUD + actions | Incident lifecycle management |
| `DashboardController` | `GET /stats`, `GET /incidents` | Real-time dashboard data |
| `IntegrationManagementController` | CRUD + API key ops | Integration configuration |
| `NotificationFlightControlController` | Pipeline metrics | Notification engine visibility |
| `NotificationAuditController` | Delivery content | Forensic content retrieval |
| `PushTokenController` | Token registration | FCM device management |
| `UsersController` | Identity proxy | Cross-module user resolution |

## 7.2 Authentication Patterns

| Pattern | Use Case | Mechanism |
|---------|----------|-----------|
| **OIDC** | Human users | Bearer token via Security module |
| **API Key** | Automated sources | `X-Api-Key` header with SHA-256 hash |
| **Integration Proxy** | Foundation apps | Service-to-service OIDC |

## 7.3 Inbound Alert Format

```json
{
  "incidentKey": "unique-deduplication-key",
  "title": "CPU Critical on Server-01",
  "description": "CPU utilization exceeded 95% for 5 minutes",
  "severityTypeId": 1,
  "sourcePayloadJson": { /* optional raw data */ }
}
```

---

# 8. Integration Points

## 8.1 Foundation Integration Library

The `FoundationCore.Web` assembly provides:
- `IAlertingIntegrationService` - Typed client for alert operations
- `AlertingIntegrationOptions` - Configuration binding
- OIDC auto-registration at startup

## 8.2 Log-to-Alerting Bridge

Foundation's `LogErrorNotificationConsumer` provides automatic alerting:
- Monitors all log files for ERROR-level entries
- "First-strike" immediate notification
- Temporal suppression to prevent alert fatigue

---

# Appendix: Quick Reference

## Status Codes

| Status | ID | Description |
|--------|-----|-------------|
| Triggered | 1 | New incident, escalation active |
| Acknowledged | 2 | Claimed by responder |
| Resolved | 3 | Issue fixed |

## Severity Levels

| Severity | ID | Sequence |
|----------|-----|----------|
| Critical | 1 | 10 |
| High | 2 | 20 |
| Medium | 3 | 30 |
| Low | 4 | 40 |

## Notification Channels

| Channel | ID | Default Priority |
|---------|-----|-----------------|
| Voice | 3 | 5 (highest) |
| SMS | 2 | 10 |
| Push | 4 | 20 |
| WebPush | 5 | 25 |
| Email | 1 | 30 |
| Teams | 6 | 40 (lowest) |

---

*Documentation generated by AI assistant - February 2026*
