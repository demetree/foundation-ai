using Foundation.CodeGeneration;
using System.Collections.Generic;

namespace Foundation.Auditor.Database
{
    public class AuditorDatabaseGenerator : DatabaseGenerator
    {
        public AuditorDatabaseGenerator() : base("Auditor", "Auditor")
        {
            this.database.SetSchemaName("Auditor");

            Database.Table auditTypeTable = database.AddTable("AuditType");
            auditTypeTable.isWritable = false;      // this is a code table that nobody should be able to edit.
            auditTypeTable.AddIdField();
            auditTypeTable.AddNameAndDescriptionFields(true, true);
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Login" }, { "description", "Log in to the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Logout" }, { "description", "Log out of the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Read List" }, { "description", "Read entity list from the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Read List (Redacted)" }, { "description", "Read of redacted entity list from the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Read Entity" }, { "description", "Read entity from the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Read Entity (Redacted)" }, { "description", "Read of redacted entity from the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Create Entity" }, { "description", "Create entity in the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Update Entity" }, { "description", "Update entity in the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Delete Entity" }, { "description", "Delete entity from the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Write List" }, { "description", "Write list of entities to the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Load Page" }, { "description", "Load page in the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Confirmation Requested" }, { "description", "Confirmation for operation was requested of the user by the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Confirmation Granted" }, { "description", "Confirmation for operation was granted to the system by the user" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Confirmation Denied" }, { "description", "Confirmation for operation was denied to the system by the user" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Search" }, { "description", "Search of the system" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Context Set" }, { "description", "Application context was set by the user" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Unauthorized Access Attempt" }, { "description", "An attempt was made to access an unauthorized resource" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Error" }, { "description", "An error was encountered" } });
            auditTypeTable.AddData(new Dictionary<string, string> { { "name", "Miscellaneous" }, { "description", "Miscellaneous event" } });



            Database.Table auditAccessTypeTable = database.AddTable("AuditAccessType");
            auditAccessTypeTable.isWritable = false;
            auditAccessTypeTable.AddIdField();
            auditAccessTypeTable.AddNameAndDescriptionFields(true, true);
            auditAccessTypeTable.AddData(new Dictionary<string, string> { { "name", "Web Browser" }, { "description", "User connecting with a web browser to access the system." } });
            auditAccessTypeTable.AddData(new Dictionary<string, string> { { "name", "API Request" }, { "description", "Request made by other software to access the system." } });
            auditAccessTypeTable.AddData(new Dictionary<string, string> { { "name", "Ambiguous" }, { "description", "Ambiguous access type.  Could be end user or other software." } });

            Database.Table auditUserTable = database.AddTable("AuditUser");
            auditUserTable.isWritable = true;
            auditUserTable.adminAccessNeededToWrite = true;

            auditUserTable.AddIdField();
            auditUserTable.AddString500Field("name", false).EnforceUniqueness().CreateIndex();
            auditUserTable.AddString1000Field("comments");
            auditUserTable.AddDateTimeField("firstAccess");

            Database.Table auditSessionTable = database.AddTable("AuditSession");
            auditSessionTable.isWritable = true;
            auditSessionTable.adminAccessNeededToWrite = true;

            auditSessionTable.AddIdField();
            auditSessionTable.AddString500Field("name", false).EnforceUniqueness().CreateIndex();
            auditSessionTable.AddString1000Field("comments");
            auditSessionTable.AddDateTimeField("firstAccess");


            Database.Table auditSourceTable = database.AddTable("AuditSource");
            auditSourceTable.isWritable = true;
            auditSourceTable.adminAccessNeededToWrite = true;

            auditSourceTable.AddIdField();
            auditSourceTable.AddString500Field("name", false).EnforceUniqueness().CreateIndex();
            auditSourceTable.AddString1000Field("comments");
            auditSourceTable.AddDateTimeField("firstAccess");


            Database.Table auditUserAgentTable = database.AddTable("AuditUserAgent");
            auditUserAgentTable.isWritable = true;
            auditUserAgentTable.adminAccessNeededToWrite = true;

            auditUserAgentTable.AddIdField();
            auditUserAgentTable.AddString500Field("name", false).EnforceUniqueness().CreateIndex();
            auditUserAgentTable.AddString1000Field("comments");
            auditUserAgentTable.AddDateTimeField("firstAccess");

            Database.Table auditHostSystemTable = database.AddTable("AuditHostSystem");
            auditHostSystemTable.isWritable = true;
            auditHostSystemTable.adminAccessNeededToWrite = true;

            auditHostSystemTable.AddIdField();
            auditHostSystemTable.AddString500Field("name", false).EnforceUniqueness().CreateIndex();
            auditHostSystemTable.AddString1000Field("comments");
            auditHostSystemTable.AddDateTimeField("firstAccess");


            Database.Table auditResourceTable = database.AddTable("AuditResource");
            auditResourceTable.isWritable = true;
            auditResourceTable.adminAccessNeededToWrite = true;

            auditResourceTable.AddIdField();
            auditResourceTable.AddString850Field("name", false).EnforceUniqueness().CreateIndex();
            auditResourceTable.AddString1000Field("comments");
            auditResourceTable.AddDateTimeField("firstAccess");


            Database.Table auditModuleTable = database.AddTable("AuditModule");
            auditModuleTable.isWritable = true;
            auditModuleTable.adminAccessNeededToWrite = true;
            auditModuleTable.AddIdField();
            auditModuleTable.AddString500Field("name", false).EnforceUniqueness().CreateIndex();
            auditModuleTable.AddString1000Field("comments");
            auditModuleTable.AddDateTimeField("firstAccess");

            Database.Table auditModuleEntityTable = database.AddTable("AuditModuleEntity");
            auditModuleEntityTable.isWritable = true;
            auditModuleEntityTable.adminAccessNeededToWrite = true;
            auditModuleEntityTable.AddIdField();
            Database.Table.Field auditModuleLinkField = auditModuleEntityTable.AddForeignKeyField("auditModuleId", auditModuleTable, false);
            Database.Table.Field nameField = auditModuleEntityTable.AddString500Field("name", false);    // AddNameField(false, false);     // no index just on name.  It'll be added with module ID
            auditModuleEntityTable.AddString1000Field("comments");
            auditModuleEntityTable.AddDateTimeField("firstAccess");
            auditModuleEntityTable.CreateIndexForFields(new Database.Table.Field[] { auditModuleLinkField, nameField });


            Database.Table auditEventTable = database.AddTable("AuditEvent");
            auditEventTable.isWritable = true;
            auditEventTable.adminAccessNeededToWrite = true;
            auditEventTable.AddIdField();
            Database.Table.Field auditEventStartTimeField = auditEventTable.AddDateTimeField("startTime", false);  // when this event started occurred
            auditEventStartTimeField.CreateIndex();
            auditEventTable.AddDateTimeField("stopTime", false).CreateIndex();                                                  // when this event finished
            auditEventTable.AddBoolField("completedSuccessfully", false).defaultValue = "1";                    // whether or not this event was completed as intended.
            Database.Table.Field auditEventUserLinkField = auditEventTable.AddForeignKeyField("auditUserId", auditUserTable, false);

            Database.Table.Field auditEventSessionLinkField = auditEventTable.AddForeignKeyField("auditSessionId", auditSessionTable, false);
            Database.Table.Field auditEventTypeLinkField = auditEventTable.AddForeignKeyField("auditTypeId", auditTypeTable, false);
            Database.Table.Field auditEventAccessTypeLinkField = auditEventTable.AddForeignKeyField("auditAccessTypeId", auditAccessTypeTable, false);
            Database.Table.Field auditEventSourceLinkField = auditEventTable.AddForeignKeyField("auditSourceId", auditSourceTable, false);
            Database.Table.Field auditEventUserAgentLinkField = auditEventTable.AddForeignKeyField("auditUserAgentId", auditUserAgentTable, false);
            Database.Table.Field auditEventModuleLinkField = auditEventTable.AddForeignKeyField("auditModuleId", auditModuleTable, false);
            Database.Table.Field auditEventModuleEntityLinkField = auditEventTable.AddForeignKeyField("auditModuleEntityId", auditModuleEntityTable, false);
            Database.Table.Field auditEventResourceLinkField = auditEventTable.AddForeignKeyField("auditResourceId", auditResourceTable, false);
            Database.Table.Field auditEventHostSystemLinkField = auditEventTable.AddForeignKeyField("auditHostSystemId", auditHostSystemTable, false);
            auditEventTable.AddString250Field("primaryKey", true);                                               // primary key of entity, if relevent.
            auditEventTable.AddIntField("threadId", true);
            auditEventTable.AddTextField("message", false);                                                     // message to record
            auditEventTable.AddSortSequence("id", true);
            auditEventTable.SetDisplayNameField("message");

            //
            // Composite indexes for analytics aggregate queries (User Activity Insights Dashboard)
            //
            auditEventTable.CreateIndexForFields(new Database.Table.Field[] { auditEventStartTimeField, auditEventUserLinkField });      // Activity by day per user, top users
            auditEventTable.CreateIndexForFields(new Database.Table.Field[] { auditEventStartTimeField, auditEventModuleLinkField });    // Top modules, module usage over time
            auditEventTable.CreateIndexForFields(new Database.Table.Field[] { auditEventStartTimeField, auditEventTypeLinkField });      // Event type breakdown by time range

            //
            // Set the expected override state for this table.  
            //
            auditEventTable.mvcDefineFieldsToBeOverridden = true;
            auditEventTable.webAPIListGetterToBeOverridden = true;      // List getter logic custom written to help with performance



            // Tracks the before and after state for events that effect the state of an entity
            Database.Table auditEventEntityStateTable = database.AddTable("AuditEventEntityState");
            auditEventEntityStateTable.isWritable = true;
            auditEventEntityStateTable.adminAccessNeededToWrite = true;
            auditEventEntityStateTable.AddIdField();
            auditEventEntityStateTable.AddForeignKeyField("auditEventId", auditEventTable, false);
            auditEventEntityStateTable.AddTextField("beforeState", true);
            auditEventEntityStateTable.AddTextField("afterState", true);

            //
            // Set the expected override state for this table.  There are customizations in handling the user table to manage passwords etc..
            //
            auditEventEntityStateTable.mvcDefineFieldsToBeOverridden = true;
            auditEventEntityStateTable.webAPIListGetterToBeOverridden = true;       // custom wrote the main list getter to be faster and have more parameters


            Database.Table auditEventErrorMessageTable = database.AddTable("AuditEventErrorMessage", true, true);
            auditEventErrorMessageTable.AddIdField();
            auditEventErrorMessageTable.AddForeignKeyField("auditEventId", auditEventTable, false);
            auditEventErrorMessageTable.AddTextField("errorMessage", false);

            //
            // Set the expected override state for this table.  There are customizations in handling for new filter params and such.
            //
            auditEventErrorMessageTable.mvcDefineFieldsToBeOverridden = true;
            auditEventErrorMessageTable.webAPIListGetterToBeOverridden = true;                // custom wrote the main list getter to be faster and have more parameters


            Database.Table auditPlanBTable = database.AddTable("AuditPlanB", true, true);
            auditPlanBTable.AddIdField();
            auditPlanBTable.AddDateTimeField("startTime", false).CreateIndex();
            auditPlanBTable.AddDateTimeField("stopTime", false).CreateIndex();
            auditPlanBTable.AddBoolField("completedSuccessfully", false).defaultValue = "1";
            auditPlanBTable.AddString100Field("user");
            auditPlanBTable.AddString100Field("session");
            auditPlanBTable.AddString100Field("type");
            auditPlanBTable.AddString100Field("accessType");
            auditPlanBTable.AddString50Field("source");
            auditPlanBTable.AddString100Field("userAgent");
            auditPlanBTable.AddString100Field("module");
            auditPlanBTable.AddString100Field("moduleEntity");
            auditPlanBTable.AddString500Field("resource");
            auditPlanBTable.AddString50Field("hostSystem");
            auditPlanBTable.AddString250Field("primaryKey");
            auditPlanBTable.AddIntField("threadId", true);
            auditPlanBTable.AddTextField("message");
            auditPlanBTable.AddTextField("beforeState");
            auditPlanBTable.AddTextField("afterState");
            auditPlanBTable.AddTextField("errorMessage");
            auditPlanBTable.AddTextField("exceptionText");          // the reason that that the auditEvent write failed
            auditPlanBTable.AddSortSequence("id", true);
            auditPlanBTable.SetDisplayNameField("message");


            //
            // Set the expected override state for this table.  There are customizations in handling the user table to manage passwords etc..
            //
            auditPlanBTable.mvcDefineFieldsToBeOverridden = true;
            auditPlanBTable.webAPIListGetterToBeOverridden = true;      //  custom wrote the main list getter to be faster for this type of data


            Database.Table externalCommunicationTable = database.AddTable("ExternalCommunication", true, true);
            externalCommunicationTable.AddIdField();
            externalCommunicationTable.AddDateTimeField("timeStamp");
            externalCommunicationTable.AddForeignKeyField("auditUserId", auditUserTable, true);
            externalCommunicationTable.AddString100Field("communicationType");
            externalCommunicationTable.AddString2000Field("subject");
            externalCommunicationTable.AddTextField("message");
            externalCommunicationTable.AddBoolField("completedSuccessfully", false).defaultValue = "1";
            externalCommunicationTable.AddTextField("responseMessage");
            externalCommunicationTable.AddTextField("exceptionText");
            externalCommunicationTable.AddSortSequence("timeStamp", true);

            Database.Table externalCommunicationRecipientTable = database.AddTable("ExternalCommunicationRecipient", true, true);
            externalCommunicationRecipientTable.AddIdField();
            externalCommunicationRecipientTable.AddForeignKeyField("externalCommunicationId", externalCommunicationTable, true);
            externalCommunicationRecipientTable.AddString100Field("recipient");
            externalCommunicationRecipientTable.AddString50Field("type");
        }
    }
}