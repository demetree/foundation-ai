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
        private const int ALERTING_READER_PERMISSION_LEVEL = 1;
        private const int ALERTING_BASIC_WRITER_PERMISSION_LEVEL = 1;
        private const int ALERTING_USER_WRITER_PERMISSION_LEVEL = 50;
        private const int ALERTING_SCHEDULE_WRITER_PERMISSION_LEVEL = 100;
        private const int ALERTING_MASTER_CONFIG_WRITER_PERMISSION_LEVEL = 150;
        private const int ALERTING_SUPER_ADMIN_WRITER_PERMISSION_LEVEL = 255;

        private const string ALERTING_USER_WRITER_CUSTOM_ROLE_NAME = "Alerting User Writer";
        private const string ALERTING_SCHEDULE_WRITER_CUSTOM_ROLE_NAME = "Alerting Schedule Writer";
        private const string ALERTING_MASTER_CONFIG_WRITER_CUSTOM_ROLE_NAME = "Alerting Master Config Writer";


        public AlertingDatabaseGenerator() : base("Alerting", "Alerting")
        {
            database.comment = @"Alerting and Incident Management database.
Stores services, escalation policies, on-call schedules, incidents, timeline events,
notification delivery tracking, webhook delivery tracking, and integration credentials.
Designed to be independent while sharing the central Security database for users and teams.";

            this.database.SetSchemaName("Alerting");

            this.database.AddCustomRole(ALERTING_USER_WRITER_CUSTOM_ROLE_NAME, $"{ALERTING_USER_WRITER_CUSTOM_ROLE_NAME} Role");                        // Can write only to user setting type tables
            this.database.AddCustomRole(ALERTING_SCHEDULE_WRITER_CUSTOM_ROLE_NAME, $"{ALERTING_SCHEDULE_WRITER_CUSTOM_ROLE_NAME} Role");                // Can write to Schedule setup tables
            this.database.AddCustomRole(ALERTING_MASTER_CONFIG_WRITER_CUSTOM_ROLE_NAME, $"{ALERTING_MASTER_CONFIG_WRITER_CUSTOM_ROLE_NAME} Role");      // Can write to service and integration type setup tables

            #region Lookup Tables (static, non-writable)

            //
            // SeverityType - static lookup for incident severity
            //
            Database.Table severityTable = database.AddTable("SeverityType");
            severityTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            severityTable.comment = "Static severity levels for incidents.";
            severityTable.isWritable = false;           // Nobody gets to write to this
            severityTable.AddIdField();
            severityTable.AddNameAndDescriptionFields(true, true, true);
            severityTable.AddSequenceField(false);
            severityTable.AddControlFields(false);

            severityTable.AddData(new Dictionary<string, string> { { "name", "Critical" }, { "description", "Critical" }, { "sequence", "10" } });
            severityTable.AddData(new Dictionary<string, string> { { "name", "High" }, { "description", "High" }, { "sequence", "20" } });
            severityTable.AddData(new Dictionary<string, string> { { "name", "Medium" }, { "description", "Medium" }, { "sequence", "30" } });
            severityTable.AddData(new Dictionary<string, string> { { "name", "Low" }, { "description", "Low" }, { "sequence", "40" } });

            //
            // IncidentStatusType - static lookup for incident status
            //
            Database.Table incidentStatusTypeTable = database.AddTable("IncidentStatusType");
            incidentStatusTypeTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            incidentStatusTypeTable.comment = "Static status values for incidents.";
            incidentStatusTypeTable.isWritable = false;     // Nobody gets to write to this
            incidentStatusTypeTable.AddIdField();
            incidentStatusTypeTable.AddNameAndDescriptionFields(true, true, true);
            incidentStatusTypeTable.AddControlFields(false);

            incidentStatusTypeTable.AddData(new Dictionary<string, string> { { "name", "Triggered" }, { "description", "Newly triggered incident" } });
            incidentStatusTypeTable.AddData(new Dictionary<string, string> { { "name", "Acknowledged" }, { "description", "Acknowledged by a responder" } });
            incidentStatusTypeTable.AddData(new Dictionary<string, string> { { "name", "Resolved" }, { "description", "Incident resolved" } });

            //
            // IncidentEventType - static lookup for timeline event types
            //
            Database.Table incidentEventTypeTable = database.AddTable("IncidentEventType");
            incidentEventTypeTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            incidentEventTypeTable.comment = "Static event types for the incident timeline.";
            incidentEventTypeTable.isWritable = false;              // Nobody gets to write to this
            incidentEventTypeTable.AddIdField();
            incidentEventTypeTable.AddNameAndDescriptionFields(true, true, true);
            incidentEventTypeTable.AddControlFields(false);

            incidentEventTypeTable.AddData(new Dictionary<string, string> { { "name", "Triggered" }, { "description", "Incident was triggered" } });
            incidentEventTypeTable.AddData(new Dictionary<string, string> { { "name", "Escalated" }, { "description", "Escalation rule fired" } });
            incidentEventTypeTable.AddData(new Dictionary<string, string> { { "name", "Acknowledged" }, { "description", "Incident acknowledged" } });
            incidentEventTypeTable.AddData(new Dictionary<string, string> { { "name", "Resolved" }, { "description", "Incident resolved" } });
            incidentEventTypeTable.AddData(new Dictionary<string, string> { { "name", "NoteAdded" }, { "description", "Note added to incident" } });
            incidentEventTypeTable.AddData(new Dictionary<string, string> { { "name", "NotificationSent" }, { "description", "Notification delivery attempted" } });

            //
            // NotificationChannelType - static channels for delivery attempts
            //
            Database.Table notificationChannelTypeTable = database.AddTable("NotificationChannelType");
            notificationChannelTypeTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            notificationChannelTypeTable.comment = "Static notification delivery channels.";
            notificationChannelTypeTable.isWritable = false;        // Nobody gets to write to this
            notificationChannelTypeTable.AddIdField();
            notificationChannelTypeTable.AddNameAndDescriptionFields(true, true, true);
            notificationChannelTypeTable.AddIntField("defaultPriority", false, 0); // lower = higher priority
            notificationChannelTypeTable.AddControlFields(false);

            notificationChannelTypeTable.AddData(new Dictionary<string, string> { { "name", "Email" }, { "description", "Email notification" }, { "defaultPriority", "30" } });
            notificationChannelTypeTable.AddData(new Dictionary<string, string> { { "name", "SMS" }, { "description", "SMS text message" }, { "defaultPriority", "10" } });
            notificationChannelTypeTable.AddData(new Dictionary<string, string> { { "name", "VoiceCall" }, { "description", "Automated voice call" }, { "defaultPriority", "5" } });
            notificationChannelTypeTable.AddData(new Dictionary<string, string> { { "name", "MobilePush" }, { "description", "Mobile app push" }, { "defaultPriority", "20" } });

            #endregion

            #region Configuration Tables (admin-only write, soft-delete)

            Database.Table escalationPolicyTable = database.AddTable("EscalationPolicy");
            escalationPolicyTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_MASTER_CONFIG_WRITER_PERMISSION_LEVEL);
            escalationPolicyTable.comment = "Escalation policies assigned to services.";
            escalationPolicyTable.isWritable = true;

            //
            // Must have Alerting Master Config role to write to this table.
            //
            escalationPolicyTable.customWriteAccessRole = ALERTING_MASTER_CONFIG_WRITER_CUSTOM_ROLE_NAME;

            escalationPolicyTable.AddIdField();
            escalationPolicyTable.AddMultiTenantSupport();
            escalationPolicyTable.AddNameAndDescriptionFields(true, true, true);
            escalationPolicyTable.AddVersionControl();
            escalationPolicyTable.AddControlFields(true);



            Database.Table serviceTable = database.AddTable("Service");
            serviceTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_MASTER_CONFIG_WRITER_PERMISSION_LEVEL);
            serviceTable.comment = "Monitored services/applications that can generate alerts.";
            serviceTable.isWritable = true;

            //
            // Must have Alerting Master Config role to write to this table.
            //
            serviceTable.customWriteAccessRole = ALERTING_MASTER_CONFIG_WRITER_CUSTOM_ROLE_NAME;


            serviceTable.AddIdField();
            serviceTable.AddMultiTenantSupport();
            serviceTable.AddForeignKeyField(escalationPolicyTable, true);
            serviceTable.AddNameAndDescriptionFields(true, true, true);
            serviceTable.AddGuidField("ownerTeamObjectGuid", true).AddScriptComments("References Security.SecurityTeam.objectGuid - logical owner.");
            serviceTable.AddVersionControl();
            serviceTable.AddControlFields(true);

            

            Database.Table escalationRuleTable = database.AddTable("EscalationRule");
            escalationRuleTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_MASTER_CONFIG_WRITER_PERMISSION_LEVEL);
            escalationRuleTable.comment = "Individual escalation rules (ordered). Supports repeat looping until acknowledgment.";
            escalationRuleTable.isWritable = true;

            //
            // Must have Alerting Master Config role to write to this table.
            //
            escalationRuleTable.customWriteAccessRole = ALERTING_MASTER_CONFIG_WRITER_CUSTOM_ROLE_NAME;


            escalationRuleTable.AddIdField();
            escalationRuleTable.AddMultiTenantSupport();
            escalationRuleTable.AddForeignKeyField(escalationPolicyTable, false);
            escalationRuleTable.AddIntField("ruleOrder", false, 0).CreateIndex();
            escalationRuleTable.AddIntField("delayMinutes", false, 0);
            escalationRuleTable.AddIntField("repeatCount", false, 0).AddScriptComments("How many times to repeat notification if no ack (0 = no repeat).");
            escalationRuleTable.AddIntField("repeatDelayMinutes", true).AddScriptComments("Delay between repeat attempts (null = same as delayMinutes).");
            escalationRuleTable.AddString50Field("targetType", false).AddScriptComments("Valid values: User, Team, Schedule");
            escalationRuleTable.AddGuidField("targetObjectGuid", true).AddScriptComments("References Security.SecurityUser/SecurityTeam or Alerting.OnCallSchedule objectGuid.");

            escalationRuleTable.AddVersionControl();
            escalationRuleTable.AddControlFields(true);

            escalationRuleTable.CreateIndexForFields(new List<string>() { "escalationPolicyId", "ruleOrder" });




            Database.Table onCallScheduleTable = database.AddTable("OnCallSchedule");
            onCallScheduleTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_SCHEDULE_WRITER_PERMISSION_LEVEL);
            onCallScheduleTable.comment = "On-call rotation schedules (dynamic targets for escalation rules).";

            //
            // Must have Alerting Schedule writer role to write to this table.
            //
            onCallScheduleTable.customWriteAccessRole = ALERTING_SCHEDULE_WRITER_CUSTOM_ROLE_NAME;

            onCallScheduleTable.AddIdField();
            onCallScheduleTable.AddMultiTenantSupport();
            onCallScheduleTable.AddNameAndDescriptionFields(true, true, true);
            onCallScheduleTable.AddString50Field("timeZoneId", false, "UTC");
            onCallScheduleTable.AddVersionControl();
            onCallScheduleTable.AddControlFields(true);



            Database.Table scheduleLayerTable = database.AddTable("ScheduleLayer");
            scheduleLayerTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_SCHEDULE_WRITER_PERMISSION_LEVEL);
            scheduleLayerTable.comment = "Layers within an on-call schedule (primary, secondary, etc.).";

            //
            // Must have Alerting Schedule writer role to write to this table.
            //
            scheduleLayerTable.customWriteAccessRole = ALERTING_SCHEDULE_WRITER_CUSTOM_ROLE_NAME;

            scheduleLayerTable.AddIdField();
            scheduleLayerTable.AddMultiTenantSupport();
            scheduleLayerTable.AddForeignKeyField(onCallScheduleTable, false);
            scheduleLayerTable.AddNameAndDescriptionFields(true, true, true);
            scheduleLayerTable.AddIntField("layerLevel", false, 1);
            scheduleLayerTable.AddDateTimeField("rotationStart", false);
            scheduleLayerTable.AddIntField("rotationDays", false, 7);
            scheduleLayerTable.AddString50Field("handoffTime", false, "09:00");
            scheduleLayerTable.AddVersionControl();
            scheduleLayerTable.AddControlFields(true);



            Database.Table layerMemberTable = database.AddTable("ScheduleLayerMember");
            layerMemberTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_SCHEDULE_WRITER_PERMISSION_LEVEL);
            layerMemberTable.comment = "Users in a schedule layer rotation (ordered).";

            //
            // Must have Alerting Schedule writer role to write to this table.
            //
            layerMemberTable.customWriteAccessRole = ALERTING_SCHEDULE_WRITER_CUSTOM_ROLE_NAME;


            layerMemberTable.AddIdField();
            layerMemberTable.AddMultiTenantSupport();
            layerMemberTable.AddForeignKeyField(scheduleLayerTable, false);
            layerMemberTable.AddIntField("position", false, 0);
            layerMemberTable.AddGuidField("securityUserObjectGuid", false).AddScriptComments("References Security.SecurityUser.objectGuid");
            layerMemberTable.AddVersionControl();
            layerMemberTable.AddControlFields(true);
            layerMemberTable.AddUniqueConstraint("tenantGuid", "scheduleLayerId", "position");



            Database.Table integrationTable = database.AddTable("Integration");
            integrationTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_MASTER_CONFIG_WRITER_PERMISSION_LEVEL);
            integrationTable.comment = "API integrations for inbound alerts and outbound status callbacks.";
            integrationTable.SetTableToBeReadonlyForControllerCreationPurposes();                   // custom write controller logic to be implemented to handle the hashing of the key server side.

            //
            // Must have Alerting Master Config role to write to this table.
            //
            integrationTable.customWriteAccessRole = ALERTING_MASTER_CONFIG_WRITER_CUSTOM_ROLE_NAME;


            integrationTable.AddIdField();
            integrationTable.AddMultiTenantSupport();
            integrationTable.AddForeignKeyField(serviceTable, false);
            integrationTable.AddNameAndDescriptionFields(true, true, true);
            integrationTable.AddString250Field("apiKeyHash", false).EnforceUniqueness();            // global unique - No two tenants can share the same key hash 
            integrationTable.AddString1000Field("callbackWebhookUrl", true);
            integrationTable.AddIntField("maxRetryAttempts", true, 10).AddScriptComments("How many times to retry failed deliveries");
            integrationTable.AddIntField("retryBackoffSeconds", true, 30).AddScriptComments("Base seconds for backoff (30, 60, 120, 240...)");
            integrationTable.AddString500Field("callbackOnEventTypes", true).AddScriptComments("Comma-separated IncidentEventType names (e.g., 'Acknowledged,Resolved,Escalated')");
            integrationTable.AddDateTimeField("lastCallbackSuccessAt", true);
            integrationTable.AddIntField("consecutiveCallbackFailures", true, null);    
            integrationTable.AddVersionControl();
            integrationTable.AddControlFields(true);



            Database.Table integrationCallbackEventTable = database.AddTable("IntegrationCallbackIncidentEventType");
            integrationCallbackEventTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_MASTER_CONFIG_WRITER_PERMISSION_LEVEL);
            integrationCallbackEventTable.comment = "API integrations incident event types to callback on.";
            integrationCallbackEventTable.SetTableToBeReadonlyForControllerCreationPurposes();                   // custom write controller logic to be implemented to handle the hashing of the key server side.

            //
            // Must have Alerting Master Config role to write to this table.
            //
            integrationCallbackEventTable.customWriteAccessRole = ALERTING_MASTER_CONFIG_WRITER_CUSTOM_ROLE_NAME;

            integrationCallbackEventTable.AddIdField();
            integrationCallbackEventTable.AddMultiTenantSupport();

            integrationCallbackEventTable.AddForeignKeyField(integrationTable, false, true);
            integrationCallbackEventTable.AddForeignKeyField(incidentEventTypeTable, false, true);

            integrationCallbackEventTable.AddVersionControl();
            integrationCallbackEventTable.AddControlFields(true);

            #endregion



            #region User Notification Preferences (user-editable config)

            // UserNotificationPreference - per-user default notification preferences
            Database.Table userPrefTable = database.AddTable("UserNotificationPreference");
            userPrefTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_USER_WRITER_PERMISSION_LEVEL);
            userPrefTable.comment = "Per-user notification preferences (channels, quiet hours, DND, etc.). Users can edit their own preferences.";
            userPrefTable.isWritable = true;

            //
            // Must have Alerting User writer role to write to this table.
            //
            userPrefTable.customWriteAccessRole = ALERTING_USER_WRITER_CUSTOM_ROLE_NAME;


            userPrefTable.AddIdField();
            userPrefTable.AddMultiTenantSupport();
            userPrefTable.AddGuidField("securityUserObjectGuid", false).AddScriptComments("References Security.SecurityUser.objectGuid - one preference row per user.");
            userPrefTable.AddString50Field("timeZoneId", true, "UTC").AddScriptComments("User's preferred timezone for quiet hours scheduling.");
            userPrefTable.AddString10Field("quietHoursStart", true).AddScriptComments("HH:mm format local to timeZoneId - start of quiet hours (null = no quiet hours).");
            userPrefTable.AddString10Field("quietHoursEnd", true).AddScriptComments("HH:mm format local to timeZoneId - end of quiet hours.");
            userPrefTable.AddBoolField("isDoNotDisturb", false, false).AddScriptComments("Global DND override - if true, no notifications except possibly critical overrides.");
            userPrefTable.AddBoolField("isDoNotDisturbPermanent", false, false).AddScriptComments("If true, DND has no scheduled end (until manually cleared).");
            userPrefTable.AddDateTimeField("doNotDisturbUntil", true).AddScriptComments("Temporary DND end time (ignored if isDoNotDisturbPermanent = true).");
            userPrefTable.AddTextField("customSettingsJson", true).AddScriptComments("Flexible JSON for future extensions (e.g., per-severity overrides, custom sounds).");
            userPrefTable.AddVersionControl();
            userPrefTable.AddControlFields(true); // objectGuid, active, deleted, versionNumber

            // Unique constraint: one active preference row per user
            userPrefTable.AddUniqueConstraint("tenantGuid", "securityUserObjectGuid", "active", "deleted"); //  "Ensures only one active preference record per user.")

            // Indexes
            userPrefTable.CreateIndexForFields(new List<string> { "securityUserObjectGuid" });



            // Child table: per-channel enable/disable and priority override
            Database.Table channelPrefTable = database.AddTable("UserNotificationChannelPreference");
            channelPrefTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_USER_WRITER_PERMISSION_LEVEL);
            channelPrefTable.comment = "Per-user, per-channel notification preferences (enable/disable, custom priority).";

            //
            // Must have Alerting User writer role to write to this table.
            //
            channelPrefTable.customWriteAccessRole = ALERTING_USER_WRITER_CUSTOM_ROLE_NAME;


            channelPrefTable.AddIdField();
            channelPrefTable.AddMultiTenantSupport();
            channelPrefTable.AddForeignKeyField(userPrefTable, false, true);
            channelPrefTable.AddForeignKeyField(notificationChannelTypeTable, false, true); // References NotificationChannelType
            channelPrefTable.AddBoolField("isEnabled", false, true).AddScriptComments("If false, this channel is disabled for the user (overrides system defaults).");
            channelPrefTable.AddIntField("priorityOverride", true).AddScriptComments("Optional custom priority (lower = higher urgency) - null = use channel default.");
            channelPrefTable.AddVersionControl();
            channelPrefTable.AddControlFields(true);

            // Composite unique: one row per user per channel
            channelPrefTable.AddUniqueConstraint("tenantGuid", "userNotificationPreferenceId", "notificationChannelTypeId");



            #endregion



            #region Operational Tables (high volume)


            Database.Table incidentTable = database.AddTable("Incident");

            // Limit writing to super admins with 255 write level
            incidentTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            incidentTable.adminAccessNeededToWrite = true;

            incidentTable.comment = "Active and historical incidents.";
            incidentTable.isWritable = true;
            incidentTable.AddIdField();
            incidentTable.AddMultiTenantSupport();
            incidentTable.AddString250Field("incidentKey", false).CreateIndex();
            incidentTable.AddForeignKeyField(serviceTable, false);
            incidentTable.AddString250Field("title", false);
            incidentTable.AddTextField("description", true);
            incidentTable.AddForeignKeyField(severityTable, false);
            incidentTable.AddForeignKeyField(incidentStatusTypeTable, false);
            incidentTable.AddDateTimeField("createdAt", false).CreateIndex();

            // Escalation state tracking - enables efficient background worker processing
            incidentTable.AddForeignKeyField(escalationRuleTable, true).AddScriptComments("Current active escalation rule (null = no active escalation, e.g., acknowledged/resolved).");
            incidentTable.AddIntField("currentRepeatCount", true, 0).AddScriptComments("How many repeat notifications have been sent for the current rule (resets on rule change).");
            incidentTable.AddDateTimeField("nextEscalationAt", true).AddScriptComments("Timestamp when the next escalation/repeat should fire (drives worker query).");

            incidentTable.AddDateTimeField("acknowledgedAt", true);
            incidentTable.AddDateTimeField("resolvedAt", true);
            incidentTable.AddGuidField("currentAssigneeObjectGuid", true);
            incidentTable.AddTextField("sourcePayloadJson", true);

            incidentTable.AddVersionControl();
            incidentTable.AddControlFields();

            incidentTable.AddUniqueConstraint("tenantGuid", "incidentKey");

            // Indexes for dashboards and escalation worker
            incidentTable.CreateIndexForFields(new List<string> { "tenantGuid", "incidentStatusTypeId", "createdAt" });
            incidentTable.CreateIndexForFields(new List<string> { "tenantGuid", "serviceId", "createdAt" });
            incidentTable.CreateIndexForFields(new List<string> { "tenantGuid", "nextEscalationAt", "incidentStatusTypeId" }); // Critical for worker: find due escalations in Triggered state



            Database.Table timelineEventTable = database.AddTable("IncidentTimelineEvent");

            // Limit writing to super admins with 255 write level
            timelineEventTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            timelineEventTable.adminAccessNeededToWrite = true;

            timelineEventTable.comment = "Timeline events for incidents.";
            timelineEventTable.isWritable = true;
            timelineEventTable.AddIdField();
            timelineEventTable.AddMultiTenantSupport();
            timelineEventTable.AddForeignKeyField(incidentTable, false);
            timelineEventTable.AddForeignKeyField(incidentEventTypeTable, false);
            timelineEventTable.AddDateTimeField("timestamp", false).CreateIndex();
            timelineEventTable.AddGuidField("actorObjectGuid", true);
            timelineEventTable.AddTextField("detailsJson", true);
            timelineEventTable.AddControlFields();

            timelineEventTable.CreateIndexForFields(new List<string> { "incidentId", "timestamp" });



            Database.Table noteTable = database.AddTable("IncidentNote");
            noteTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_BASIC_WRITER_PERMISSION_LEVEL);
            // Note no special role needd for writing this table, just the permission level check, so it can be written to by all writers
            noteTable.comment = "Notes added to incidents by responders.";
            noteTable.isWritable = true;
            noteTable.AddIdField();
            noteTable.AddMultiTenantSupport();
            noteTable.AddForeignKeyField(incidentTable, false);
            noteTable.AddGuidField("authorObjectGuid", false);
            noteTable.AddDateTimeField("createdAt", false);
            noteTable.AddTextField("content", false);
            noteTable.AddVersionControl();
            noteTable.AddControlFields();



            Database.Table notificationTable = database.AddTable("IncidentNotification");

            // Limit writing to super admins with 255 write level
            notificationTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);

            notificationTable.comment = "Notifications sent to individual users as part of escalation (teams/schedules are resolved to users at runtime).";
            notificationTable.adminAccessNeededToWrite = true;

            notificationTable.isWritable = true;
            notificationTable.AddIdField();
            notificationTable.AddMultiTenantSupport();
            notificationTable.AddForeignKeyField(incidentTable, false);
            notificationTable.AddForeignKeyField(escalationRuleTable, true);
            notificationTable.AddGuidField("userObjectGuid", false).AddScriptComments("Resolved Security.SecurityUser.objectGuid that was notified.");
            notificationTable.AddDateTimeField("firstNotifiedAt", false);
            notificationTable.AddDateTimeField("lastNotifiedAt", true);
            notificationTable.AddDateTimeField("acknowledgedAt", true);
            notificationTable.AddGuidField("acknowledgedByObjectGuid", true);
            notificationTable.AddControlFields();           // No need for version control on this table

            notificationTable.CreateIndexForFields(new List<string> { "incidentId", "userObjectGuid" });

            Database.Table deliveryAttemptTable = database.AddTable("NotificationDeliveryAttempt");

            // Limit writing to super admins with 255 write level
            deliveryAttemptTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            deliveryAttemptTable.adminAccessNeededToWrite = true;

            deliveryAttemptTable.comment = "Individual delivery attempts per channel for a notification.";
            deliveryAttemptTable.isWritable = true;
            deliveryAttemptTable.AddIdField();
            deliveryAttemptTable.AddMultiTenantSupport();
            deliveryAttemptTable.AddForeignKeyField(notificationTable, false);
            deliveryAttemptTable.AddForeignKeyField(notificationChannelTypeTable, false);
            deliveryAttemptTable.AddIntField("attemptNumber", false, 1);
            deliveryAttemptTable.AddDateTimeField("attemptedAt", false);
            deliveryAttemptTable.AddString50Field("status", false, "Pending"); // Pending, Sent, Delivered, Failed
            deliveryAttemptTable.AddTextField("errorMessage", true);
            deliveryAttemptTable.AddTextField("response", true);
            deliveryAttemptTable.AddControlFields();

            deliveryAttemptTable.CreateIndexForFields(new List<string> { "tenantGuid", "incidentNotificationId", "notificationChannelTypeId" });



            Database.Table webhookDeliveryTable = database.AddTable("WebhookDeliveryAttempt");
            webhookDeliveryTable.SetMinimumPermissionLevels(ALERTING_READER_PERMISSION_LEVEL, ALERTING_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            webhookDeliveryTable.comment = "Outbound webhook delivery attempts for incident status updates.";
            
            webhookDeliveryTable.isWritable = true;
            webhookDeliveryTable.adminAccessNeededToWrite = true;

            webhookDeliveryTable.AddIdField();
            webhookDeliveryTable.AddMultiTenantSupport();
            webhookDeliveryTable.AddForeignKeyField(incidentTable, false);
            webhookDeliveryTable.AddForeignKeyField(integrationTable, false);
            webhookDeliveryTable.AddForeignKeyField(timelineEventTable, true);
            webhookDeliveryTable.AddIntField("attemptNumber", false, 1);
            webhookDeliveryTable.AddDateTimeField("attemptedAt", false);
            webhookDeliveryTable.AddIntField("httpStatusCode", true);
            webhookDeliveryTable.AddBoolField("success", false, false);
            webhookDeliveryTable.AddTextField("payloadJson", true);
            webhookDeliveryTable.AddTextField("responseBody", true);
            webhookDeliveryTable.AddTextField("errorMessage", true);
            webhookDeliveryTable.AddControlFields();

            webhookDeliveryTable.CreateIndexForFields(new List<string> { "tenantGuid", "incidentId", "attemptedAt" });

            #endregion


            /*  example stored procedure
             *  
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Alerting].[usp_ProcessIncidentEscalations]
    @BatchSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Identify incidents due for escalation
    -- We only look at 'Triggered' incidents. 'Acknowledged' or 'Resolved' usually stop escalation.
    SELECT TOP (@BatchSize)
        i.id AS IncidentId,
        i.incidentKey,
        i.currentEscalationRuleId,
        i.currentRepeatCount,
        i.serviceId,
        s.escalationPolicyId,
        i.nextEscalationAt
    INTO #IncidentsToProcess
    FROM [Alerting].[Incident] i
    JOIN [Alerting].[Service] s ON i.serviceId = s.id
    JOIN [Alerting].[IncidentStatusType] st ON i.statusId = st.id
    WHERE i.nextEscalationAt <= SYSUTCDATETIME()
      AND st.name = 'Triggered' 
      AND i.resolvedAt IS NULL
    ORDER BY i.nextEscalationAt ASC;

    -- If nothing to do, exit early
    IF NOT EXISTS (SELECT 1 FROM #IncidentsToProcess) RETURN;

    DECLARE @CurrentIncidentId INT, @CurrentPolicyId INT, @CurrentRuleId INT, @CurrentRepeat INT;
    DECLARE @NextRuleId INT, @NextDelayMinutes INT, @NextRepeatCount INT;
    DECLARE @TargetType NVARCHAR(50), @TargetObjectGuid UNIQUEIDENTIFIER;
    DECLARE @ResolvedUserGuid UNIQUEIDENTIFIER;
    DECLARE @LogJson NVARCHAR(MAX);

    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT IncidentId, escalationPolicyId, currentEscalationRuleId, ISNULL(currentRepeatCount, 0)
        FROM #IncidentsToProcess;

    OPEN cur;
    FETCH NEXT FROM cur INTO @CurrentIncidentId, @CurrentPolicyId, @CurrentRuleId, @CurrentRepeat;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @NextRuleId = NULL;
        SET @NextDelayMinutes = NULL;
        SET @NextRepeatCount = 0;
        SET @ResolvedUserGuid = NULL;
        
        -------------------------------------------------------------------------
        -- LOGIC STEP 1: Determine the Next State (Rule & Timing)
        -------------------------------------------------------------------------
        
        -- SCENARIO A: Incident just started (No current rule)
        IF @CurrentRuleId IS NULL
        BEGIN
            -- Get the first rule in the policy
            SELECT TOP 1 
                @NextRuleId = id,
                @NextDelayMinutes = delayMinutes,
                @TargetType = targetType,
                @TargetObjectGuid = targetObjectGuid
            FROM [Alerting].[EscalationRule]
            WHERE escalationPolicyId = @CurrentPolicyId
              AND active = 1 AND deleted = 0
            ORDER BY ruleOrder ASC;
        END
        ELSE
        BEGIN
            -- We are already on a rule. Check if we should repeat it.
            DECLARE @RuleRepeatLimit INT, @RuleRepeatDelay INT, @CurrentRuleOrder INT;
            
            SELECT 
                @RuleRepeatLimit = repeatCount,
                @RuleRepeatDelay = ISNULL(repeatDelayMinutes, delayMinutes),
                @CurrentRuleOrder = ruleOrder,
                @TargetType = targetType, -- Re-select strictly for target resolution logic below
                @TargetObjectGuid = targetObjectGuid
            FROM [Alerting].[EscalationRule]
            WHERE id = @CurrentRuleId;

            IF @CurrentRepeat < @RuleRepeatLimit
            BEGIN
                -- SCENARIO B: Repeat the current rule
                SET @NextRuleId = @CurrentRuleId;
                SET @NextRepeatCount = @CurrentRepeat + 1;
                SET @NextDelayMinutes = @RuleRepeatDelay;
            END
            ELSE
            BEGIN
                -- SCENARIO C: Repeats exhausted, move to next rule
                SELECT TOP 1 
                    @NextRuleId = id,
                    @NextDelayMinutes = delayMinutes,
                    @TargetType = targetType,
                    @TargetObjectGuid = targetObjectGuid
                FROM [Alerting].[EscalationRule]
                WHERE escalationPolicyId = @CurrentPolicyId
                  AND ruleOrder > @CurrentRuleOrder
                  AND active = 1 AND deleted = 0
                ORDER BY ruleOrder ASC;
                
                -- Reset repeat count for the new rule
                SET @NextRepeatCount = 0;
            END
        END

        -------------------------------------------------------------------------
        -- LOGIC STEP 2: Execute State Change
        -------------------------------------------------------------------------

        BEGIN TRANSACTION;

        IF @NextRuleId IS NOT NULL
        BEGIN
            -- 1. RESOLVE TARGET
            --    Logic to turn a Schedule or Team into a specific User GUID.
            
            IF @TargetType = 'User'
            BEGIN
                SET @ResolvedUserGuid = @TargetObjectGuid;
            END
            ELSE IF @TargetType = 'Schedule'
            BEGIN
                -- TODO: Call a helper function to calculate who is on call right now.
                -- Ideally: SET @ResolvedUserGuid = [Alerting].[fn_GetOnCallUser](@TargetObjectGuid, SYSUTCDATETIME());
                -- For now, we assume NULL to prevent crash, or log an error.
                SET @ResolvedUserGuid = NULL; 
            END
            
            -- 2. CREATE NOTIFICATION (If we found a user)
            IF @ResolvedUserGuid IS NOT NULL
            BEGIN
                INSERT INTO [Alerting].[IncidentNotification]
                (
                    incidentId, escalationRuleId, userObjectGuid, 
                    firstNotifiedAt, lastNotifiedAt -- lastNotifiedAt usually updated on Ack, but initial can be same
                )
                VALUES
                (
                    @CurrentIncidentId, @NextRuleId, @ResolvedUserGuid,
                    SYSUTCDATETIME(), SYSUTCDATETIME()
                );

                -- 3. CREATE TIMELINE EVENT (Notification Sent)
                SET @LogJson = '{"ruleId": ' + CAST(@NextRuleId AS NVARCHAR(20)) + ', "userGuid": "' + CAST(@ResolvedUserGuid AS NVARCHAR(50)) + '"}';
                
                INSERT INTO [Alerting].[IncidentTimelineEvent]
                (incidentId, eventTypeId, timestamp, detailsJson)
                SELECT 
                    @CurrentIncidentId, 
                    id, 
                    SYSUTCDATETIME(), 
                    @LogJson
                FROM [Alerting].[IncidentEventType] WHERE name = 'NotificationSent';
                
                -- 4. QUEUE DELIVERY ATTEMPTS
                --    Insert rows into NotificationDeliveryAttempt for every active channel for this user.
                --    (Assuming default channels for now, e.g., Email & SMS)
                INSERT INTO [Alerting].[NotificationDeliveryAttempt]
                (incidentNotificationId, channelTypeId, attemptNumber, attemptedAt, status)
                SELECT 
                    SCOPE_IDENTITY(), -- The IncidentNotification ID we just inserted
                    ct.id,
                    1,
                    SYSUTCDATETIME(),
                    'Pending'
                FROM [Alerting].[NotificationChannelType] ct
                WHERE ct.active = 1 AND ct.deleted = 0;
            END

            -- 5. UPDATE INCIDENT POINTER
            UPDATE [Alerting].[Incident]
            SET 
                currentEscalationRuleId = @NextRuleId,
                currentRepeatCount = @NextRepeatCount,
                nextEscalationAt = DATEADD(MINUTE, ISNULL(@NextDelayMinutes, 15), SYSUTCDATETIME())
            WHERE id = @CurrentIncidentId;

        END
        ELSE
        BEGIN
            -- SCENARIO D: End of Policy (No more rules).
            -- Mark nextEscalationAt as NULL so the poller stops picking it up.
            
            UPDATE [Alerting].[Incident]
            SET nextEscalationAt = NULL
            WHERE id = @CurrentIncidentId;

            -- Optional: Log "Policy Exhausted" to timeline
             INSERT INTO [Alerting].[IncidentTimelineEvent]
            (incidentId, eventTypeId, timestamp, detailsJson)
            SELECT @CurrentIncidentId, id, SYSUTCDATETIME(), '{"message": "Escalation policy exhausted. No more rules."}'
            FROM [Alerting].[IncidentEventType] WHERE name = 'Escalated'; -- Or a custom 'Exhausted' type
        END

        COMMIT TRANSACTION;

        FETCH NEXT FROM cur INTO @CurrentIncidentId, @CurrentPolicyId, @CurrentRuleId, @CurrentRepeat;
    END

    CLOSE cur;
    DEALLOCATE cur;
END
GO             
             
             */
        }
    }
}