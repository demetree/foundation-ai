using Foundation.CodeGeneration;
using System.Collections.Generic;

namespace Foundation.Alerting.Database
{
    /// <summary>
    /// Database schema generator for the Alerting / Incident Management system.
    /// Independent module similar to PagerDuty, with escalation, on-call schedules,
    /// incident tracking, and webhook integrations.
    /// 
    /// References Security database entities via objectGuid (SecurityUser, SecurityTeam).
    /// </summary>
    public class AlertingDatabaseGenerator : DatabaseGenerator
    {
        public AlertingDatabaseGenerator() : base("Alerting", "Alerting")
        {
            database.comment = @"Alerting and Incident Management database.
Stores services, escalation policies, on-call schedules, incidents, timeline events,
and integration credentials. Designed to be independent while sharing the central
Security database for users and teams.";

            this.database.SetSchemaName("Alerting");

            #region Configuration Tables (admin-only write, soft-delete)

            // Service - the monitored system/application that can trigger alerts
            var serviceTable = database.AddTable("Service");
            serviceTable.comment = "Monitored services/applications that can generate alerts.";
            serviceTable.isWritable = true;
            serviceTable.adminAccessNeededToWrite = true;
            serviceTable.AddIdField();
            serviceTable.AddString100Field("name", false).EnforceUniqueness().CreateIndex();
            serviceTable.AddString500Field("description", true);
            serviceTable.AddGuidField("ownerTeamObjectGuid", true).AddScriptComments("References Security.SecurityTeam.objectGuid - logical owner.");
            serviceTable.AddControlFields(true); // objectGuid, active, deleted

            // EscalationPolicy - defines how an alert escalates
            var escalationPolicyTable = database.AddTable("EscalationPolicy");
            escalationPolicyTable.comment = "Escalation policies assigned to services.";
            escalationPolicyTable.isWritable = true;
            escalationPolicyTable.adminAccessNeededToWrite = true;
            escalationPolicyTable.AddIdField();
            escalationPolicyTable.AddString100Field("name", false);
            escalationPolicyTable.AddString500Field("description", true);
            escalationPolicyTable.AddControlFields(true);

            // Link service → policy (one-to-one for simplicity; can be made many-to-one later)
            serviceTable.AddForeignKeyField("escalationPolicyId", escalationPolicyTable, true);

            // EscalationRule - ordered rules within a policy
            var escalationRuleTable = database.AddTable("EscalationRule");
            escalationRuleTable.comment = "Individual escalation rules (ordered).";
            escalationRuleTable.isWritable = true;
            escalationRuleTable.adminAccessNeededToWrite = true;
            escalationRuleTable.AddIdField();
            escalationRuleTable.AddForeignKeyField("escalationPolicyId", escalationPolicyTable, false);
            escalationRuleTable.AddIntField("ruleOrder", false, 0).CreateIndex(); // lower = earlier
            escalationRuleTable.AddIntField("delayMinutes", false, 0);
            escalationRuleTable.AddString50Field("targetType", false) // User, Team, Schedule
                .AddScriptComments("Valid values: User, Team, Schedule");
            escalationRuleTable.AddGuidField("targetObjectGuid", true).AddScriptComments("References Security.SecurityUser/SecurityTeam or Alerting.OnCallSchedule objectGuid.");
            escalationRuleTable.AddControlFields(true);

            // Composite index for ordered lookup
            escalationRuleTable.CreateIndexForFields(new List<string>() { "escalationPolicyId", "ruleOrder" } );

            // OnCallSchedule - rotation schedule definition
            var onCallScheduleTable = database.AddTable("OnCallSchedule");
            onCallScheduleTable.comment = "On-call rotation schedules (dynamic targets for escalation rules).";
            onCallScheduleTable.isWritable = true;
            onCallScheduleTable.adminAccessNeededToWrite = true;
            onCallScheduleTable.AddIdField();
            onCallScheduleTable.AddString100Field("name", false).EnforceUniqueness();
            onCallScheduleTable.AddString50Field("timeZoneId", false, "UTC");
            onCallScheduleTable.AddString500Field("description", true);
            onCallScheduleTable.AddControlFields(true);

            // ScheduleLayer - parallel layers (primary, backup, etc.)
            var scheduleLayerTable = database.AddTable("ScheduleLayer");
            scheduleLayerTable.comment = "Layers within an on-call schedule (primary, secondary, etc.).";
            scheduleLayerTable.isWritable = true;
            scheduleLayerTable.adminAccessNeededToWrite = true;
            scheduleLayerTable.AddIdField();
            scheduleLayerTable.AddForeignKeyField("onCallScheduleId", onCallScheduleTable, false);
            scheduleLayerTable.AddString100Field("name", true);
            scheduleLayerTable.AddIntField("layerLevel", false, 1); // 1 = primary, higher = later escalation
            scheduleLayerTable.AddDateTimeField("rotationStart", false); // when the rotation cycle began
            scheduleLayerTable.AddIntField("rotationDays", false, 7); // length of one full rotation
            scheduleLayerTable.AddString50Field("handoffTime", false, "09:00"); // HH:mm daily handoff
            scheduleLayerTable.AddControlFields(true);

            // ScheduleLayerMember - ordered users in the rotation
            var layerMemberTable = database.AddTable("ScheduleLayerMember");
            layerMemberTable.comment = "Users in a schedule layer rotation (ordered).";
            layerMemberTable.isWritable = true;
            layerMemberTable.adminAccessNeededToWrite = true;
            layerMemberTable.AddIdField();
            layerMemberTable.AddForeignKeyField("scheduleLayerId", scheduleLayerTable, false);
            layerMemberTable.AddIntField("position", false, 0); // 0 = first in rotation
            layerMemberTable.AddGuidField("securityUserObjectGuid", false)
                .AddScriptComments("References Security.SecurityUser.objectGuid");
            layerMemberTable.AddControlFields(true);
            layerMemberTable.AddUniqueConstraint("scheduleLayerId", "position");

            #endregion


            #region Operational Tables (high volume, no soft-delete)

            // Incident - core incident record
            var incidentTable = database.AddTable("Incident");
            incidentTable.comment = "Active and historical incidents.";
            incidentTable.isWritable = true; // system + UI writes
            incidentTable.AddIdField();
            incidentTable.AddString250Field("incidentKey", false).EnforceUniqueness().CreateIndex(); // deduplication key from source
            incidentTable.AddForeignKeyField("serviceId", serviceTable, false);
            incidentTable.AddString250Field("title", false);
            incidentTable.AddTextField("description", true);
            incidentTable.AddString50Field("severity", false, "medium"); // critical, high, medium, low
            incidentTable.AddString50Field("status", false, "triggered"); // triggered, acknowledged, resolved
            incidentTable.AddDateTimeField("createdAt", false).CreateIndex();
            incidentTable.AddDateTimeField("acknowledgedAt", true);
            incidentTable.AddDateTimeField("resolvedAt", true);
            incidentTable.AddGuidField("currentAssigneeObjectGuid", true); // current user/team
            incidentTable.AddTextField("sourcePayloadJson", true); // original inbound payload for debugging

            // IncidentTimelineEvent - audit trail / timeline
            var timelineEventTable = database.AddTable("IncidentTimelineEvent");
            timelineEventTable.comment = "Timeline events for incidents (trigger, ack, escalate, resolve, note, etc.).";
            timelineEventTable.isWritable = true;
            timelineEventTable.AddIdField();
            timelineEventTable.AddForeignKeyField("incidentId", incidentTable, false);
            timelineEventTable.AddString100Field("eventType", false); // Triggered, Acknowledged, Escalated, Resolved, NoteAdded, ...
            timelineEventTable.AddDateTimeField("timestamp", false).CreateIndex();
            timelineEventTable.AddGuidField("actorObjectGuid", true); // SecurityUser who performed action
            timelineEventTable.AddTextField("detailsJson", true);

            // IncidentNote - free-form notes added by responders
            var noteTable = database.AddTable("IncidentNote");
            noteTable.comment = "Notes added to incidents by responders.";
            noteTable.isWritable = true;
            noteTable.AddIdField();
            noteTable.AddForeignKeyField("incidentId", incidentTable, false);
            noteTable.AddGuidField("authorObjectGuid", false);
            noteTable.AddDateTimeField("createdAt", false);
            noteTable.AddTextField("content", false);

            // Integration - inbound API keys + outbound webhook URLs per service
            var integrationTable = database.AddTable("Integration");
            integrationTable.comment = "API integrations for inbound alerts and outbound status callbacks.";
            integrationTable.isWritable = true;
            integrationTable.adminAccessNeededToWrite = true;
            integrationTable.AddIdField();
            integrationTable.AddForeignKeyField("serviceId", serviceTable, false);
            integrationTable.AddString100Field("name", false);
            integrationTable.AddString250Field("apiKeyHash", false).EnforceUniqueness(); // store hashed
            integrationTable.AddString1000Field("callbackWebhookUrl", true); // where we POST status updates

            #endregion
        }
    }
}