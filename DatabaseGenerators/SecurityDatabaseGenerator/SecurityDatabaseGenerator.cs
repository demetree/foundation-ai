using Foundation.CodeGeneration;
using System;
using System.Collections.Generic;

namespace Foundation.Security.Database
{
    public class SecurityDatabaseGenerator : DatabaseGenerator
    {
        private const int SECURITY_READER_PERMISSION_LEVEL = 1;
        private const int SECURITY_USER_WRITER_PERMISSION_LEVEL = 50;
        private const int SECURITY_TENANT_WRITER_PERMISSION_LEVEL = 100;
        private const int SECURITY_MASTER_CONFIG_WRITER_PERMISSION_LEVEL = 150;
        private const int SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL = 255;


        public SecurityDatabaseGenerator() : base("Security", "Security")
        {
            this.database.SetSchemaName("Security");

            /*
             * 
             *  These are the Multi Tenant, and Data Visibility related master tables
             * 
             */
            Database.Table securityTenantTable = database.AddTable("SecurityTenant");
            securityTenantTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_TENANT_WRITER_PERMISSION_LEVEL);
            securityTenantTable.displayNameForTable = "Tenant";
            securityTenantTable.adminAccessNeededToWrite = true;
            securityTenantTable.AddIdField();
            securityTenantTable.AddNameAndDescriptionFields(true, true);
            securityTenantTable.AddTextField("settings").AddScriptComments("To store a JSON object containing arbitrary tenant settings.");
            Database.Table.Field hostNameField = securityTenantTable.AddString250Field("hostName").AddScriptComments("The host name used for HTTP Host header tenant resolution. E.g. 'pettyharbour.example.com'. Used by multi-tenant public-facing apps like Community CMS to determine which tenant's data to serve.");
            securityTenantTable.CreateIndexForField(hostNameField, true, false);            // This will built with an exception to WHERE out null values from the unique requirement constraint.

            securityTenantTable.AddControlFields(true);


            Database.Table.Index securityTenantIdActiveDeletedIndex = securityTenantTable.CreateIndex("I_SecurityTenant_id_active_deleted");
            securityTenantIdActiveDeletedIndex.AddField("id");
            securityTenantIdActiveDeletedIndex.AddField("active");
            securityTenantIdActiveDeletedIndex.AddField("deleted");

            


            securityTenantTable.AddData(new Dictionary<string, string> { { "name", "System Service" },
                                                               { "description", "System Service Tenant - For Administrative purposes or single tenant use." },
                                                               { "active", "1"},
                                                               { "deleted", "0"},
                                                               { "objectGuid", "c017cf97-ccbb-4686-98b3-c59efc1a3f45" }});



            Database.Table securityOrganizationTable = database.AddTable("SecurityOrganization");
            securityOrganizationTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_TENANT_WRITER_PERMISSION_LEVEL);
            securityOrganizationTable.displayNameForTable = "Organization";
            securityOrganizationTable.adminAccessNeededToWrite = true;
            securityOrganizationTable.AddIdField();
            securityOrganizationTable.AddForeignKeyField("securityTenantId", securityTenantTable, false);
            securityOrganizationTable.AddNameAndDescriptionFields(false, true);
            securityOrganizationTable.AddControlFields(true);
            securityOrganizationTable.AddUniqueConstraint("securityTenantId", "name");
            securityOrganizationTable.webAPIGetListDataToBeOverridden = true;       // overridden to have custom version that links with tenant for more meaningful list data

            Database.Table.Index securityOrganizationIdActiveDeletedIndex = securityOrganizationTable.CreateIndex("I_SecurityOrganization_id_active_deleted");
            securityOrganizationIdActiveDeletedIndex.AddField("id");
            securityOrganizationIdActiveDeletedIndex.AddField("active");
            securityOrganizationIdActiveDeletedIndex.AddField("deleted");



            Database.Table securityDepartmentTable = database.AddTable("SecurityDepartment");
            securityDepartmentTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_TENANT_WRITER_PERMISSION_LEVEL);
            securityDepartmentTable.displayNameForTable = "Department";
            securityDepartmentTable.adminAccessNeededToWrite = true;
            securityDepartmentTable.AddIdField();
            securityDepartmentTable.AddForeignKeyField("securityOrganizationId", securityOrganizationTable, false);
            securityDepartmentTable.AddNameAndDescriptionFields(false, true);
            securityDepartmentTable.AddControlFields(true);
            securityDepartmentTable.AddUniqueConstraint("securityOrganizationId", "name");
            securityDepartmentTable.webAPIGetListDataToBeOverridden = true;     // overridden to have custom version that links with tenant and org for more meaningful list data

            Database.Table.Index securityDepartmentIdActiveDeletedIndex = securityDepartmentTable.CreateIndex("I_SecurityDepartment_id_active_deleted");
            securityDepartmentIdActiveDeletedIndex.AddField("id");
            securityDepartmentIdActiveDeletedIndex.AddField("active");
            securityDepartmentIdActiveDeletedIndex.AddField("deleted");


            Database.Table securityTeamTable = database.AddTable("SecurityTeam");
            securityTeamTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_TENANT_WRITER_PERMISSION_LEVEL);
            securityTeamTable.displayNameForTable = "Team";
            securityTeamTable.adminAccessNeededToWrite = true;
            securityTeamTable.AddIdField();
            securityTeamTable.AddForeignKeyField("securityDepartmentId", securityDepartmentTable, false);
            securityTeamTable.AddNameAndDescriptionFields(false, true);
            securityTeamTable.AddControlFields(true);
            securityTeamTable.AddUniqueConstraint("securityDepartmentId", "name");
            securityTeamTable.webAPIGetListDataToBeOverridden = true;           // overridden to have custom version that links with tenant, org and dep, for more meaningful list data

            Database.Table.Index securityTeamIdActiveDeletedIndex = securityTeamTable.CreateIndex("I_SecurityTeam_id_active_deleted");
            securityTeamIdActiveDeletedIndex.AddField("id");
            securityTeamIdActiveDeletedIndex.AddField("active");
            securityTeamIdActiveDeletedIndex.AddField("deleted");


            Database.Table securityUserTitleTable = database.AddTable("SecurityUserTitle");
            securityUserTitleTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_MASTER_CONFIG_WRITER_PERMISSION_LEVEL);
            securityUserTitleTable.displayNameForTable = "User Title";
            securityUserTitleTable.adminAccessNeededToWrite = true;
            securityUserTitleTable.AddIdField();
            securityUserTitleTable.AddNameAndDescriptionFields(true, true);
            securityUserTitleTable.AddControlFields(true);

            Database.Table.Index securityUserTitleIdActiveDeletedIndex = securityUserTitleTable.CreateIndex("I_SecurityUserTitle_id_active_deleted");
            securityUserTitleIdActiveDeletedIndex.AddField("id");
            securityUserTitleIdActiveDeletedIndex.AddField("active");
            securityUserTitleIdActiveDeletedIndex.AddField("deleted");



            Database.Table securityUserTable = database.AddTable("SecurityUser");
            securityUserTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_USER_WRITER_PERMISSION_LEVEL);
            securityUserTable.displayNameForTable = "User";
            securityUserTable.AddIdField();
            Database.Table.Field accountNameField = securityUserTable.AddString250Field("accountName", false);
            accountNameField.unique = true;
            accountNameField.CreateIndex();
            securityUserTable.SetDisplayNameField(accountNameField);
            securityUserTable.AddBoolField("activeDirectoryAccount", false).defaultValue = "0";
            securityUserTable.AddString250Field("password");
            securityUserTable.AddBoolField("canLogin", false, true).AddScriptComments("Whether or not the user can login.  Should be true for people, or API access accounts, and false for internal use service accounts that should never be allowed to login.");
            securityUserTable.AddBoolField("mustChangePassword", false, false).AddScriptComments("True if the user is required to change their password");
            securityUserTable.AddString100Field("firstName");
            securityUserTable.AddString100Field("middleName");
            securityUserTable.AddString100Field("lastName");
            securityUserTable.AddDateTimeField("dateOfBirth");
            securityUserTable.AddString100Field("emailAddress");
            securityUserTable.AddString100Field("cellPhoneNumber");
            securityUserTable.AddString50Field("phoneNumber");
            securityUserTable.AddString50Field("phoneExtension");
            securityUserTable.AddString500Field("description");
            securityUserTable.AddForeignKeyField("securityUserTitleId", securityUserTitleTable, true);
            securityUserTable.AddForeignKeyField("reportsToSecurityUserId", securityUserTable, true);
            securityUserTable.AddString100Field("authenticationDomain");
            securityUserTable.AddIntField("failedLoginCount");
            securityUserTable.AddDateTimeField("lastLoginAttempt");
            securityUserTable.AddDateTimeField("mostRecentActivity");


            Database.Table.Field alternateIdentifierField = securityUserTable.AddString100Field("alternateIdentifier");
            alternateIdentifierField.CreateIndex();

            securityUserTable.AddBinaryField("image");
            securityUserTable.AddTextField("settings");

            securityUserTable.AddForeignKeyField("securityTenantId", securityTenantTable, true).comment = "The tenant that this user is linked to";
            securityUserTable.AddIntField("readPermissionLevel", false, 0);
            securityUserTable.AddIntField("writePermissionLevel", false, 0);
            securityUserTable.AddForeignKeyField("securityOrganizationId", securityOrganizationTable, true).comment = "The default organization to use when creating data, and null is provided as an organization on a data visibility enabled table";
            securityUserTable.AddForeignKeyField("securityDepartmentId", securityDepartmentTable, true).comment = "The default department to use when creating data, and null is provided as a department on a data visibility enabled table";
            securityUserTable.AddForeignKeyField("securityTeamId", securityTeamTable, true).comment = "The default team to use when creating data, and null is provided as a team on a data visibility enabled table";

            Database.Table.Field authenticationTokenField = securityUserTable.AddString100Field("authenticationToken");
            authenticationTokenField.CreateIndex();

            securityUserTable.AddDateTimeField("authenticationTokenExpiry");
            securityUserTable.AddString10Field("twoFactorToken");
            securityUserTable.AddDateTimeField("twoFactorTokenExpiry");
            securityUserTable.AddBoolField("twoFactorSendByEmail");
            securityUserTable.AddBoolField("twoFactorSendBySMS");


            //
            // Add the control fields.  Add an index to the objectGuid field for fast user lookups
            //
            securityUserTable.AddControlFields(true, true, true);

            //
            // Add an index to support the main login query
            //
            securityUserTable.CreateIndexForFields(new List<string>() { "accountName", "activeDirectoryAccount", "active", "deleted" });

            Database.Table.Index securityUserIdActiveDeletedIndex = securityUserTable.CreateIndex("I_SecurityUser_id_active_deleted");
            securityUserIdActiveDeletedIndex.AddField("id");
            securityUserIdActiveDeletedIndex.AddField("active");
            securityUserIdActiveDeletedIndex.AddField("deleted");



            //
            // Set the expected override state for this table.  There are customizations in handling the user table to manage passwords etc..
            //
            securityUserTable.mvcDefineChildenEntitiesToBeOverridden = true;        // custom children list 
            securityUserTable.mvcDefineFieldsToBeOverridden = true;                 // some fields will be conditional on multi tenant and data visibility modes
            securityUserTable.webAPIGetListDataToBeOverridden = true;               // Basically just here to allow the adding of more routes for things like reportsToUser and other such things.  Core logic probably won't need to be changed.
            securityUserTable.webAPIListGetterToBeOverridden = true;                // so passwords can be suppressed
            securityUserTable.webAPIIdGetterToBeOverridden = true;                  // so passwords can be suppressed
            securityUserTable.webAPIPostToBeOverridden = true;                      // so passwords can be encrypted
            securityUserTable.webAPIPutToBeOverridden = true;                       // so passwords can be encrypted
            securityUserTable.webAPIDeleteToBeOverridden = true;                    // so we can fix the name clash with the variables names by the generator


            //
            // Add the admin user with all the possible access for Administrator UI purposes.
            //
            // Password is to be changed after creation, or account locked.  default password is 'Admin'
            //
            // 
            //
            securityUserTable.AddData(new Dictionary<string, string> { { "accountName", "Admin" },
                                                               { "activeDirectoryAccount", "0" },
                                                               { "canLogin", "1" },
                                                               { "mustChangePassword", "1" },
                                                               { "firstName", "Admin" },
                                                               { "lastName", "Account" },
                                                               { "link:SecurityTenant:name:securityTenantId", "System Service" },
                                                               { "password", "$HASH$V1000$10000$7lx52j0Z5CjBUyu8L84pOmsOo+jNH/pVZ1VlI4EBjAftRag+" },    // The default password is 'Admin'
                                                               { "description", "System Aministrator account.  Refer to generator for default password." },
                                                               { "readPermissionLevel", SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL.ToString() },
                                                               { "writePermissionLevel", SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL.ToString() },
                                                               { "objectGuid", "4099226f-cc2f-46d2-9725-29de861c4fa9" },
                                                            });


            // 
            // Add in a system service account for system level job purposes
            //
            // Password is to be changed after creation, or account locked.
            //
            // Default password is d80632a7-b1ff-47cb-9ecd-87f4a4a22763-Service2026!^#
            //
            securityUserTable.AddData(new Dictionary<string, string> { { "accountName", "SystemService" },
                                                               { "activeDirectoryAccount", "0" },
                                                               { "canLogin", "1" },             // Needs this to get token
                                                               { "mustChangePassword", "0" },
                                                               { "firstName", "System" },
                                                               { "middleName", "Service" },
                                                               { "lastName", "Account" },
                                                               { "link:SecurityTenant:name:securityTenantId", "System Service" },
                                                               { "password", "$HASH$V1000$10000$WeuGAJrhrIJWnWZIdyAQKvBEiFM0iMLiS+NJW8ws0YjSCbPq" },    // The default password is ('d80632a7-b1ff-47cb-9ecd-87f4a4a22763-Service2026!^#');
                                                               { "description", "System Service account for job/worker connection purposes.  Refer to generator for default password." },
                                                               { "readPermissionLevel", SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL.ToString() },
                                                               { "writePermissionLevel", SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL.ToString() },
                                                               { "objectGuid", "d80632a7-b1ff-47cb-9ecd-87f4a4a22763" },
                                                            });




            //
            // Keep a record of the users linked to a tenant.  A user record also has a tenant ID to assign the active tenant at any point in time, but this table will allow a user to logically belong in more than 1 tenant at any point in time.
            //
            Database.Table securityTenantUserTable = database.AddTable("SecurityTenantUser");
            securityTenantUserTable.displayNameForTable = "TenantUser";
            securityTenantUserTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_USER_WRITER_PERMISSION_LEVEL);
            securityTenantUserTable.AddIdField();
            securityTenantUserTable.AddForeignKeyField("securityTenantId", securityTenantTable, false);
            securityTenantUserTable.AddForeignKeyField("securityUserId", securityUserTable, false);
            securityTenantUserTable.AddBoolField("isOwner", false, false).AddScriptComments("Whether this user is the owner/creator of the tenant. Only owners can invite/remove members and manage tenant settings.").CreateIndex();
            securityTenantUserTable.AddBoolField("canRead", false, false).AddScriptComments("Whether this user has read access to the tenant's data.").CreateIndex();
            securityTenantUserTable.AddBoolField("canWrite", false, false).AddScriptComments("Whether this user has write access to the tenant's data.").CreateIndex();
            securityTenantUserTable.AddControlFields(true);
            securityTenantUserTable.AddUniqueConstraint("securityTenantId", "securityUserId");

            Database.Table.Index securityTenantUserIdActiveDeletedIndex = securityTenantUserTable.CreateIndex("I_SecurityTenantUser_id_active_deleted");
            securityTenantUserIdActiveDeletedIndex.AddField("id");
            securityTenantUserIdActiveDeletedIndex.AddField("active");
            securityTenantUserIdActiveDeletedIndex.AddField("deleted");




            Database.Table securityOrganizationUserTable = database.AddTable("SecurityOrganizationUser");
            securityOrganizationUserTable.displayNameForTable = "OrganizationUser";
            securityOrganizationUserTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_USER_WRITER_PERMISSION_LEVEL);
            securityOrganizationUserTable.AddIdField();
            securityOrganizationUserTable.AddForeignKeyField("securityOrganizationId", securityOrganizationTable, false);
            securityOrganizationUserTable.AddForeignKeyField("securityUserId", securityUserTable, false);
            securityOrganizationUserTable.AddBoolField("canRead", false, false).CreateIndex();
            securityOrganizationUserTable.AddBoolField("canWrite", false, false).CreateIndex();
            securityOrganizationUserTable.AddBoolField("canChangeHierarchy", false, false).CreateIndex();
            securityOrganizationUserTable.AddBoolField("canChangeOwner", false, false).CreateIndex();
            securityOrganizationUserTable.AddControlFields(true);
            securityOrganizationUserTable.AddUniqueConstraint("securityOrganizationId", "securityUserId");

            Database.Table.Index securityOrganizationUserIdActiveDeletedIndex = securityOrganizationUserTable.CreateIndex("I_SecurityOrganizationUser_id_active_deleted");
            securityOrganizationUserIdActiveDeletedIndex.AddField("id");
            securityOrganizationUserIdActiveDeletedIndex.AddField("active");
            securityOrganizationUserIdActiveDeletedIndex.AddField("deleted");



            Database.Table securityDepartmentUserTable = database.AddTable("SecurityDepartmentUser");
            securityDepartmentUserTable.displayNameForTable = "DepartmentUser";
            securityDepartmentUserTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_USER_WRITER_PERMISSION_LEVEL);
            securityDepartmentUserTable.AddIdField();
            securityDepartmentUserTable.AddForeignKeyField("securityDepartmentId", securityDepartmentTable, false);
            securityDepartmentUserTable.AddForeignKeyField("securityUserId", securityUserTable, false);
            securityDepartmentUserTable.AddBoolField("canRead", false, false).CreateIndex();
            securityDepartmentUserTable.AddBoolField("canWrite", false, false).CreateIndex();
            securityDepartmentUserTable.AddBoolField("canChangeHierarchy", false, false).CreateIndex();
            securityDepartmentUserTable.AddBoolField("canChangeOwner", false, false).CreateIndex();
            securityDepartmentUserTable.AddControlFields(true);
            securityDepartmentUserTable.AddUniqueConstraint("securityDepartmentId", "securityUserId");

            Database.Table.Index securityDepartmentUserIdActiveDeletedIndex = securityDepartmentUserTable.CreateIndex("I_SecurityDepartmentUser_id_active_deleted");
            securityDepartmentUserIdActiveDeletedIndex.AddField("id");
            securityDepartmentUserIdActiveDeletedIndex.AddField("active");
            securityDepartmentUserIdActiveDeletedIndex.AddField("deleted");


            Database.Table securityTeamUserTable = database.AddTable("SecurityTeamUser");
            securityTeamUserTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_USER_WRITER_PERMISSION_LEVEL);
            securityTeamUserTable.displayNameForTable = "TeamUser";
            securityTeamUserTable.AddIdField();
            securityTeamUserTable.AddForeignKeyField("securityTeamId", securityTeamTable, false);
            securityTeamUserTable.AddForeignKeyField("securityUserId", securityUserTable, false);
            securityTeamUserTable.AddBoolField("canRead", false, false).CreateIndex();
            securityTeamUserTable.AddBoolField("canWrite", false, false).CreateIndex();
            securityTeamUserTable.AddBoolField("canChangeHierarchy", false, false).CreateIndex();
            securityTeamUserTable.AddBoolField("canChangeOwner", false, false).CreateIndex();
            securityTeamUserTable.AddControlFields(true);
            securityTeamUserTable.AddUniqueConstraint("securityTeamId", "securityUserId");

            Database.Table.Index securityTeamUserIdActiveDeletedIndex = securityTeamUserTable.CreateIndex("I_SecurityTeamUser_id_active_deleted");
            securityTeamUserIdActiveDeletedIndex.AddField("id");
            securityTeamUserIdActiveDeletedIndex.AddField("active");
            securityTeamUserIdActiveDeletedIndex.AddField("deleted");



            Database.Table securityUserEventTypeTable = database.AddTable("SecurityUserEventType");
            securityUserEventTypeTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            securityUserEventTypeTable.displayNameForTable = "User Event Type";
            securityUserEventTypeTable.isWritable = false;              // this table can NEVER be written to by anybody.  It is a cornerstone set of system values that should never change.

            securityUserEventTypeTable.AddIdField();
            securityUserEventTypeTable.AddNameAndDescriptionFields(true, true);

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "LoginSuccess" },
                                                                    { "description", "Login Success" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "LoginFailure" },
                                                                    { "description", "Login Failure" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "LoginAttemptDuringCooldown" },
                                                                    { "description", "Login Attempt During Cooldown" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "Logout" },
                                                                    { "description", "Logout" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "TwoFactorSend" },
                                                                    { "description", "TwoFactorSend" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "Miscellaneous" },
                                                                    { "description", "Miscellaneous" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "AccountInactivated" },
                                                                    { "description", "AccountInactivated" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "UserInitiatedPasswordResetRequest" },
                                                                    { "description", "UserInitiatedPasswordResetRequest" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "UserInitiatedPasswordResetCompleted" },
                                                                    { "description", "UserInitiatedPasswordResetCompleted" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "SystemInitiatedPasswordResetRequest" },
                                                                    { "description", "SystemInitiatedPasswordResetRequest" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "SystemInitiatedPasswordResetCompleted" },
                                                                    { "description", "SystemInitiatedPasswordResetCompleted" } });

            //
            // AI-Generated: Added for admin user management actions
            //
            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "AdminInitiatedPasswordSet" },
                                                                    { "description", "Admin Initiated Password Set" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "AdminActionLockAccount" },
                                                                    { "description", "Admin Action Lock Account" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "AccountUnlocked" },
                                                                    { "description", "Account Unlocked" } });

            //
            // AI-Generated: Added for session revocation tracking
            //
            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "SessionRevoked" },
                                                                    { "description", "Session Revoked" } });

            securityUserEventTypeTable.AddData(new Dictionary<string, string> { { "name", "SessionRevokedWithAccountLock" },
                                                                    { "description", "Session Revoked With Account Lock" } });



            Database.Table securityUserEventTable = database.AddTable("SecurityUserEvent");
            securityUserEventTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            securityUserEventTable.displayNameForTable = "User Event";
            securityUserEventTable.AddIdField();
            securityUserEventTable.AddForeignKeyField("securityUserId", securityUserTable, false);
            securityUserEventTable.AddForeignKeyField("securityUserEventTypeId", securityUserEventTypeTable, false);
            securityUserEventTable.AddDateTimeField("timeStamp", false).CreateIndex();
            securityUserEventTable.AddString1000Field("comments");
            securityUserEventTable.AddControlFields(false);
            securityUserEventTable.AddSortSequence("timeStamp", true);

            Database.Table.Index securityUserEventIdActiveDeletedIndex = securityUserEventTable.CreateIndex("I_SecurityUserEvent_id_active_deleted");
            securityUserEventIdActiveDeletedIndex.AddField("id");
            securityUserEventIdActiveDeletedIndex.AddField("active");
            securityUserEventIdActiveDeletedIndex.AddField("deleted");



            Database.Table securityUserPasswordResetTokenTable = database.AddTable("SecurityUserPasswordResetToken");
            securityUserPasswordResetTokenTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            securityUserPasswordResetTokenTable.displayNameForTable = "User Password Reset Token";
            securityUserPasswordResetTokenTable.AddIdField();
            securityUserPasswordResetTokenTable.AddForeignKeyField("securityUserId", securityUserTable, false, true);
            securityUserPasswordResetTokenTable.AddString250Field("token", false).AddScriptComments("The token to use for this password reset request").CreateIndex();
            securityUserPasswordResetTokenTable.AddDateTimeField("timeStamp", false).AddScriptComments("The point in time when this request was created.").CreateIndex();
            securityUserPasswordResetTokenTable.AddDateTimeField("expiry", false).AddScriptComments("The expiry time for this password reset request").CreateIndex();
            securityUserPasswordResetTokenTable.AddBoolField("systemInitiated", false, false).AddScriptComments("Whether or not this token reset process was system initiated or not").CreateIndex();
            securityUserPasswordResetTokenTable.AddBoolField("completed", false, false).AddScriptComments("Whether or not this token reset process is completed").CreateIndex();
            securityUserPasswordResetTokenTable.AddString1000Field("comments");
            securityUserPasswordResetTokenTable.AddControlFields(false);
            securityUserPasswordResetTokenTable.AddSortSequence("timeStamp", true);

            Database.Table.Index securityUserPasswordResetTokenIdActiveDeletedIndex = securityUserPasswordResetTokenTable.CreateIndex("I_SecurityUserPasswordResetToken_id_active_deleted");
            securityUserPasswordResetTokenIdActiveDeletedIndex.AddField("id");
            securityUserPasswordResetTokenIdActiveDeletedIndex.AddField("active");
            securityUserPasswordResetTokenIdActiveDeletedIndex.AddField("deleted");



            Database.Table securityGroupTable = database.AddTable("SecurityGroup");
            securityGroupTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_MASTER_CONFIG_WRITER_PERMISSION_LEVEL);
            securityGroupTable.displayNameForTable = "Group";
            securityGroupTable.AddIdField();
            securityGroupTable.AddNameAndDescriptionFields(true, true);
            securityGroupTable.AddControlFields(false);


            Database.Table.Index securityGroupIdActiveDeletedIndex = securityGroupTable.CreateIndex("I_SecurityGroup_id_active_deleted");
            securityGroupIdActiveDeletedIndex.AddField("id");
            securityGroupIdActiveDeletedIndex.AddField("active");
            securityGroupIdActiveDeletedIndex.AddField("deleted");



            Database.Table securityUserSecurityGroupTable = database.AddTable("SecurityUserSecurityGroup");
            securityUserSecurityGroupTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_USER_WRITER_PERMISSION_LEVEL);
            securityUserSecurityGroupTable.displayNameForTable = "User Group";
            securityUserSecurityGroupTable.AddIdField();
            securityUserSecurityGroupTable.AddForeignKeyField("securityUserId", securityUserTable, false);
            securityUserSecurityGroupTable.AddForeignKeyField("securityGroupId", securityGroupTable, false);
            securityUserSecurityGroupTable.AddString1000Field("comments");
            securityUserSecurityGroupTable.AddControlFields(false);
            securityUserSecurityGroupTable.AddUniqueConstraint("securityUserId", "securityGroupId");

            Database.Table.Index securityUserSecurityGroupIdActiveDeletedIndex = securityUserSecurityGroupTable.CreateIndex("I_SecurityUserSecurityGroup_id_active_deleted");
            securityUserSecurityGroupIdActiveDeletedIndex.AddField("id");
            securityUserSecurityGroupIdActiveDeletedIndex.AddField("active");
            securityUserSecurityGroupIdActiveDeletedIndex.AddField("deleted");



            Database.Table privilegeTable = database.AddTable("Privilege");
            privilegeTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            privilegeTable.isWritable = false;          // this table can't be edited by anybody.  It is system level master data.
            privilegeTable.AddIdField();
            privilegeTable.AddNameAndDescriptionFields(true, true);

            privilegeTable.AddData(new Dictionary<string, string> { { "name", "No Access" },
                                                                    { "description", "No Access" } });

            privilegeTable.AddData(new Dictionary<string, string> { { "name", "Anonymous Read Only" },
                                                                    { "description", "Read Only Access, With All Sensitive Data Redacted" } });

            privilegeTable.AddData(new Dictionary<string, string> { { "name", "Read Only" },
                                                                    { "description", "Read Only Access For General Use" } });

            privilegeTable.AddData(new Dictionary<string, string> { { "name", "Read and Write" },
                                                                    { "description", "Read and Write Access" } });

            privilegeTable.AddData(new Dictionary<string, string> { { "name", "Administrative" },
                                                                    { "description", "Complete Administrative Access" } });

            privilegeTable.AddData(new Dictionary<string, string> { { "name", "Custom" },
                                                                    { "description", "Custom Access Level" } });



            Database.Table securityRoleTable = database.AddTable("SecurityRole");
            securityOrganizationTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_MASTER_CONFIG_WRITER_PERMISSION_LEVEL);
            securityRoleTable.AddIdField();
            securityRoleTable.AddForeignKeyField("privilegeId", privilegeTable, false);
            securityRoleTable.AddNameAndDescriptionFields(true, true);
            securityRoleTable.AddString1000Field("comments");
            securityRoleTable.AddControlFields(false);

            Database.Table.Index securityRoleIdActiveDeletedIndex = securityRoleTable.CreateIndex("I_SecurityRole_id_active_deleted");
            securityRoleIdActiveDeletedIndex.AddField("id");
            securityRoleIdActiveDeletedIndex.AddField("active");
            securityRoleIdActiveDeletedIndex.AddField("deleted");


            Database.Table securityUserSecurityRoleTable = database.AddTable("SecurityUserSecurityRole");
            securityUserSecurityRoleTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_USER_WRITER_PERMISSION_LEVEL);
            securityUserSecurityRoleTable.displayNameForTable = "User Security Role";
            securityUserSecurityRoleTable.AddIdField();
            Database.Table.Field userSecurityRoleUserIdField = securityUserSecurityRoleTable.AddForeignKeyField("securityUserId", securityUserTable, false);

            Database.Table.Field userSecurityRoleSecurityRoleIdField = securityUserSecurityRoleTable.AddForeignKeyField("securityRoleId", securityRoleTable, false);

            securityUserSecurityRoleTable.AddString1000Field("comments");
            securityUserSecurityRoleTable.AddControlFields(false);

            securityUserSecurityRoleTable.AddUniqueConstraint(new List<Database.Table.Field> { userSecurityRoleUserIdField, userSecurityRoleSecurityRoleIdField });

            Database.Table.Index securityUserSecurityRoleIdActiveDeletedIndex = securityUserSecurityRoleTable.CreateIndex("I_SecurityUserSecurityRole_id_active_deleted");
            securityUserSecurityRoleIdActiveDeletedIndex.AddField("id");
            securityUserSecurityRoleIdActiveDeletedIndex.AddField("active");
            securityUserSecurityRoleIdActiveDeletedIndex.AddField("deleted");

            // To help with security role queries
            Database.Table.Index securityUserSecurityRoleUserIdActiveDeletedIndex = securityUserSecurityRoleTable.CreateIndex("I_SecurityUserSecurityRole_securityUserId_active_deleted");
            securityUserSecurityRoleUserIdActiveDeletedIndex.AddField("securityUserId");
            securityUserSecurityRoleUserIdActiveDeletedIndex.AddField("active");
            securityUserSecurityRoleUserIdActiveDeletedIndex.AddField("deleted");

            // To help with security role queries
            Database.Table.Index securityUserSecurityRoleRoleIdActiveDeletedIndex = securityUserSecurityRoleTable.CreateIndex("I_SecurityUserSecurityRole_securityRoleId_active_deleted");
            securityUserSecurityRoleRoleIdActiveDeletedIndex.AddField("securityRoleId");
            securityUserSecurityRoleRoleIdActiveDeletedIndex.AddField("active");
            securityUserSecurityRoleRoleIdActiveDeletedIndex.AddField("deleted");



            //
            // Cache clearing done here
            //
            securityUserSecurityRoleTable.webAPIPostToBeOverridden = true;
            securityUserSecurityRoleTable.webAPIPutToBeOverridden = true;
            securityUserSecurityRoleTable.webAPIDeleteToBeOverridden = true;


            Database.Table securityGroupSecurityRoleTable = database.AddTable("SecurityGroupSecurityRole");
            securityGroupSecurityRoleTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_MASTER_CONFIG_WRITER_PERMISSION_LEVEL);
            securityGroupSecurityRoleTable.displayNameForTable = "Group Security Role";
            securityGroupSecurityRoleTable.AddIdField();
            Database.Table.Field groupSecurityRoleUserIdField = securityGroupSecurityRoleTable.AddForeignKeyField("securityGroupId", securityGroupTable, false);


            Database.Table.Field groupSecurityRoleSecurityRoleIdField = securityGroupSecurityRoleTable.AddForeignKeyField("securityRoleId", securityRoleTable, false);
            securityGroupSecurityRoleTable.AddString1000Field("comments");
            securityGroupSecurityRoleTable.AddControlFields(false);
            securityGroupSecurityRoleTable.AddUniqueConstraint(new List<Database.Table.Field> { groupSecurityRoleUserIdField, groupSecurityRoleSecurityRoleIdField });


            Database.Table.Index securityGroupSecurityRoleIdActiveDeletedIndex = securityGroupSecurityRoleTable.CreateIndex("I_SecurityGroupSecurityRole_id_active_deleted");
            securityGroupSecurityRoleIdActiveDeletedIndex.AddField("id");
            securityGroupSecurityRoleIdActiveDeletedIndex.AddField("active");
            securityGroupSecurityRoleIdActiveDeletedIndex.AddField("deleted");


            // To help with security role queries
            Database.Table.Index securityGroupSecurityRoleGroupIdActiveDeletedIndex = securityGroupSecurityRoleTable.CreateIndex("I_SecurityGroupSecurityRole_securityGroupId_active_deleted");
            securityGroupSecurityRoleGroupIdActiveDeletedIndex.AddField("securityGroupId");
            securityGroupSecurityRoleGroupIdActiveDeletedIndex.AddField("active");
            securityGroupSecurityRoleGroupIdActiveDeletedIndex.AddField("deleted");

            // To help with security role queries
            Database.Table.Index securityGroupSecurityRoleRoleIdActiveDeletedIndex = securityGroupSecurityRoleTable.CreateIndex("I_SecurityGroupSecurityRole_securityRoleId_active_deleted");
            securityGroupSecurityRoleRoleIdActiveDeletedIndex.AddField("securityRoleId");
            securityGroupSecurityRoleRoleIdActiveDeletedIndex.AddField("active");
            securityGroupSecurityRoleRoleIdActiveDeletedIndex.AddField("deleted");



            Database.Table moduleTable = database.AddTable("Module");
            moduleTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_MASTER_CONFIG_WRITER_PERMISSION_LEVEL);
            moduleTable.AddIdField();
            moduleTable.AddNameAndDescriptionFields(true, true);
            moduleTable.AddControlFields(false);

            Database.Table.Index moduleIdActiveDeletedIndex = moduleTable.CreateIndex("I_Module_id_active_deleted");
            moduleIdActiveDeletedIndex.AddField("id");
            moduleIdActiveDeletedIndex.AddField("active");
            moduleIdActiveDeletedIndex.AddField("deleted");


            Database.Table moduleSecurityRoleTable = database.AddTable("ModuleSecurityRole");
            moduleSecurityRoleTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_MASTER_CONFIG_WRITER_PERMISSION_LEVEL);
            moduleSecurityRoleTable.AddIdField();
            Database.Table.Field moduleSecurityRoleModuleIdField = moduleSecurityRoleTable.AddForeignKeyField("moduleId", moduleTable, false);

            Database.Table.Field moduleSecurityRoleSecurityRoleIdField = moduleSecurityRoleTable.AddForeignKeyField("securityRoleId", securityRoleTable, false);
            moduleSecurityRoleTable.AddString1000Field("comments");
            moduleSecurityRoleTable.AddControlFields(false);
            moduleSecurityRoleTable.AddUniqueConstraint(new List<Database.Table.Field> { moduleSecurityRoleModuleIdField, moduleSecurityRoleSecurityRoleIdField });

            Database.Table.Index moduleSecurityRoleIdActiveDeletedIndex = moduleSecurityRoleTable.CreateIndex("I_ModuleSecurityRole_id_active_deleted");
            moduleSecurityRoleIdActiveDeletedIndex.AddField("id");
            moduleSecurityRoleIdActiveDeletedIndex.AddField("active");
            moduleSecurityRoleIdActiveDeletedIndex.AddField("deleted");

            // To help with security role queries
            Database.Table.Index moduleSecurityRoleRoleIdActiveDeletedIndex = moduleSecurityRoleTable.CreateIndex("I_ModuleSecurityRole_securityRoleId_active_deleted");
            moduleSecurityRoleRoleIdActiveDeletedIndex.AddField("securityRoleId");
            moduleSecurityRoleRoleIdActiveDeletedIndex.AddField("active");
            moduleSecurityRoleRoleIdActiveDeletedIndex.AddField("deleted");



            Database.Table systemSettingTable = database.AddTable("SystemSetting");
            systemSettingTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            systemSettingTable.AddIdField();
            systemSettingTable.AddNameAndDescriptionFields(true, true);
            systemSettingTable.AddTextField("value");
            systemSettingTable.AddControlFields(false);

            Database.Table.Index systemSettingIdActiveDeletedIndex = systemSettingTable.CreateIndex("I_SystemSetting_id_active_deleted");
            systemSettingIdActiveDeletedIndex.AddField("id");
            systemSettingIdActiveDeletedIndex.AddField("active");
            systemSettingIdActiveDeletedIndex.AddField("deleted");



            /*
             * 
             *  IP Address Location cache table for geolocation data.
             *  Cached lookups from external geolocation providers (e.g. ip-api.com).
             *  One record per unique IP address, referenced by LoginAttempt via FK.
             * 
             */
            Database.Table ipAddressLocationTable = database.AddTable("IpAddressLocation");
            ipAddressLocationTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            ipAddressLocationTable.displayNameForTable = "IP Address Location";
            ipAddressLocationTable.AddIdField();
            Database.Table.Field ipAddressLocationIpField = ipAddressLocationTable.AddString50Field("ipAddress", false);
            ipAddressLocationIpField.unique = true;
            ipAddressLocationIpField.CreateIndex();
            ipAddressLocationTable.SetDisplayNameField(ipAddressLocationIpField);
            ipAddressLocationTable.AddString10Field("countryCode");
            ipAddressLocationTable.AddString100Field("countryName");
            ipAddressLocationTable.AddString100Field("city");
            ipAddressLocationTable.AddDoubleField("latitude", true);
            ipAddressLocationTable.AddDoubleField("longitude", true);
            ipAddressLocationTable.AddDateTimeField("lastLookupDate", false);
            ipAddressLocationTable.AddControlFields(false);
            ipAddressLocationTable.AddSortSequence("lastLookupDate", true);

            Database.Table.Index ipAddressLocationIdActiveDeletedIndex = ipAddressLocationTable.CreateIndex("I_IpAddressLocation_id_active_deleted");
            ipAddressLocationIdActiveDeletedIndex.AddField("id");
            ipAddressLocationIdActiveDeletedIndex.AddField("active");
            ipAddressLocationIdActiveDeletedIndex.AddField("deleted");



            Database.Table loginAttemptTable = database.AddTable("LoginAttempt");
            loginAttemptTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);       // can't edit login attempts..
            loginAttemptTable.AddIdField();
            loginAttemptTable.AddDateTimeField("timeStamp", false);
            loginAttemptTable.AddString250Field("userName", true);
            loginAttemptTable.AddIntField("passwordHash", true);
            loginAttemptTable.AddString500Field("resource", true);
            loginAttemptTable.AddString50Field("sessionId", true);
            loginAttemptTable.AddString50Field("ipAddress", true);
            loginAttemptTable.AddString250Field("userAgent", true);
            loginAttemptTable.AddTextField("value");
            loginAttemptTable.AddBoolField("success", true).AddScriptComments("null = unknown/pending, true = success, false = failure");
            loginAttemptTable.AddForeignKeyField("securityUserId", securityUserTable, true).AddScriptComments("Link to user if identified during login attempt");
            loginAttemptTable.AddForeignKeyField("ipAddressLocationId", ipAddressLocationTable, true).AddScriptComments("Link to cached geolocation data for this IP.  Populated asynchronously by the IpAddressLocationWorker background service.");
            loginAttemptTable.AddControlFields(false);
            loginAttemptTable.AddSortSequence("timeStamp", true);

            Database.Table.Index loginAttemptIdActiveDeletedIndex = loginAttemptTable.CreateIndex("I_loginAttempt_id_active_deleted");
            loginAttemptIdActiveDeletedIndex.AddField("id");
            loginAttemptIdActiveDeletedIndex.AddField("active");
            loginAttemptIdActiveDeletedIndex.AddField("deleted");



            Database.Table entityDataTokenTable = database.AddTable("EntityDataToken");
            entityDataTokenTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_USER_WRITER_PERMISSION_LEVEL);
            entityDataTokenTable.AddIdField();
            entityDataTokenTable.AddForeignKeyField("securityUserId", securityUserTable, false);
            entityDataTokenTable.AddForeignKeyField("moduleId", moduleTable, false);

            entityDataTokenTable.AddString250Field("entity", false);
            entityDataTokenTable.AddString50Field("sessionId", false);
            entityDataTokenTable.AddString50Field("authenticationToken", false).AddScriptComments("This is the authentication token that gets set into the user data of the forms authentication ticket");
            entityDataTokenTable.AddString50Field("token", false).unique = true;
            entityDataTokenTable.AddDateTimeField("timeStamp", false);
            entityDataTokenTable.AddString1000Field("comments");
            entityDataTokenTable.AddControlFields(false);
            entityDataTokenTable.AddSortSequence("timeStamp", true);

            entityDataTokenTable.CreateIndexForField("token");
            entityDataTokenTable.CreateIndexForFields(new List<string> { "securityUserId", "moduleId", "sessionId" });
            entityDataTokenTable.CreateIndexForFields(new List<string> { "securityUserId", "moduleId", "token", "sessionId" });
            entityDataTokenTable.SetDisplayNameField("token");

            Database.Table.Index entityDataTokenIdActiveDeletedIndex = entityDataTokenTable.CreateIndex("I_EntityDataToken_id_active_deleted");
            entityDataTokenIdActiveDeletedIndex.AddField("id");
            entityDataTokenIdActiveDeletedIndex.AddField("active");
            entityDataTokenIdActiveDeletedIndex.AddField("deleted");


            Database.Table entityDataTokenEventTypeTable = database.AddTable("EntityDataTokenEventType");
            entityDataTokenEventTypeTable.isWritable = false;       // This table can't be edited
            entityDataTokenEventTypeTable.AddIdField();
            entityDataTokenEventTypeTable.AddNameAndDescriptionFields(true, true);


            /*
            ReadFromEntity = 1,
            CascadeValidatedReadFromEntity = 2,
            WriteToEntity = 3,
            CascadeValidatedWriteToEntity = 4,
            ReuseExistingToken = 5
            */
            entityDataTokenEventTypeTable.AddData(new Dictionary<string, string> { { "name", "ReadFromEntity" },
                                                                                 { "description", "Read From Entity" } });

            entityDataTokenEventTypeTable.AddData(new Dictionary<string, string> { { "name", "CascadeValidatedReadFromEntity" },
                                                                                 { "description", "Cascade Validated Read From Entity" } });

            entityDataTokenEventTypeTable.AddData(new Dictionary<string, string> { { "name", "WriteToEntity" },
                                                                                 { "description", "Write To Entity" } });

            entityDataTokenEventTypeTable.AddData(new Dictionary<string, string> { { "name", "CascadeValidatedWriteToEntity" },
                                                                                 { "description", "Cascade Validated Write To Entity" } });

            entityDataTokenEventTypeTable.AddData(new Dictionary<string, string> { { "name", "ReuseExistingToken" },
                                                                                 { "description", "Reuse Existing Token" } });


            Database.Table entityDataTokenEventTable = database.AddTable("EntityDataTokenEvent");
            entityDataTokenEventTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            entityDataTokenEventTable.AddIdField();
            entityDataTokenEventTable.AddForeignKeyField("entityDataTokenId", entityDataTokenTable, false);
            entityDataTokenEventTable.AddForeignKeyField("entityDataTokenEventTypeId", entityDataTokenEventTypeTable, false);
            entityDataTokenEventTable.AddDateTimeField("timeStamp", false);
            entityDataTokenEventTable.AddString1000Field("comments");
            entityDataTokenEventTable.AddControlFields(false);
            entityDataTokenEventTable.AddSortSequence("timeStamp", true);

            Database.Table.Index entityDataTokenEventIdActiveDeletedIndex = entityDataTokenEventTable.CreateIndex("I_EntityDataTokenEvent_id_active_deleted");
            entityDataTokenEventIdActiveDeletedIndex.AddField("id");
            entityDataTokenEventIdActiveDeletedIndex.AddField("active");
            entityDataTokenEventIdActiveDeletedIndex.AddField("deleted");


            /*
             * 
             *  This table is for recording the tokens used to send to the other side in OAUTH transactions.
             * 
             */
            Database.Table oauthTokenTable = database.AddTable("OAUTHToken");
            oauthTokenTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_USER_WRITER_PERMISSION_LEVEL);
            oauthTokenTable.AddIdField();
            oauthTokenTable.AddString250Field("token", false).CreateIndex();
            oauthTokenTable.AddDateTimeField("expiryDateTime", false).CreateIndex();
            oauthTokenTable.AddString1000Field("userData", true);
            oauthTokenTable.AddControlFields(false);

            Database.Table.Index oauthTokenIdActiveDeletedIndex = oauthTokenTable.CreateIndex("I_OauthToken_id_active_deleted");
            oauthTokenIdActiveDeletedIndex.AddField("id");
            oauthTokenIdActiveDeletedIndex.AddField("active");
            oauthTokenIdActiveDeletedIndex.AddField("deleted");


            /*
             * 
             *  UserSession table for compliance-grade real-time session tracking.
             *  Records session metadata at token issuance time for accurate audit trails.
             *  Supports session revocation for healthcare/HIPAA compliance.
             * 
             */
            Database.Table userSessionTable = database.AddTable("UserSession");
            userSessionTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_USER_WRITER_PERMISSION_LEVEL);
            userSessionTable.displayNameForTable = "User Session";
            userSessionTable.AddIdField();
            userSessionTable.AddForeignKeyField("securityUserId", securityUserTable, false);
            userSessionTable.AddGuidField("objectGuid", false).AddScriptComments("User's objectGuid for reliable identity resolution").CreateIndex();
            userSessionTable.AddString250Field("tokenId", true).AddScriptComments("OpenIddict token ID for correlation").CreateIndex();
            userSessionTable.AddDateTimeField("sessionStart", false).AddScriptComments("When the token was issued").CreateIndex();
            userSessionTable.AddDateTimeField("expiresAt", false).AddScriptComments("When the token expires").CreateIndex();
            userSessionTable.AddString50Field("ipAddress", true).AddScriptComments("Client IP address at login");
            userSessionTable.AddString500Field("userAgent", true).AddScriptComments("Browser/client user agent");
            userSessionTable.AddString50Field("loginMethod", true).AddScriptComments("Login method: Password, Microsoft, Google, RefreshToken").CreateIndex();
            userSessionTable.AddString100Field("clientApplication", true).AddScriptComments("Client application name");
            userSessionTable.AddBoolField("isRevoked", false, false).AddScriptComments("Whether session has been administratively revoked").CreateIndex();
            userSessionTable.AddDateTimeField("revokedAt", true).AddScriptComments("When session was revoked");
            userSessionTable.AddString100Field("revokedBy", true).AddScriptComments("Who revoked the session (admin username)");
            userSessionTable.AddString500Field("revokedReason", true).AddScriptComments("Reason for revocation");
            userSessionTable.AddControlFields(false);
            userSessionTable.AddSortSequence("sessionStart", true);

            Database.Table.Index userSessionIdActiveDeletedIndex = userSessionTable.CreateIndex("I_UserSession_id_active_deleted");
            userSessionIdActiveDeletedIndex.AddField("id");
            userSessionIdActiveDeletedIndex.AddField("active");
            userSessionIdActiveDeletedIndex.AddField("deleted");

            // Index for active session lookups
            userSessionTable.CreateIndexForFields(new List<string>() { "securityUserId", "isRevoked", "active", "deleted" });

        }

        /* I don't think that I need this table because each data table will have a tenant guid column.  The tenant a user belongs to will come right from the user record, so it can be constained directly
         * without needing this table.  Also, this table could allow a user to have data from multiple tenants if there is no tenant guid constraint, and the user belongs to multiple tenants.
        public static void AddMultiTenantSupportToDatabase(DatabaseScriptGenerator.Database database)
        {
            //
            // This table will be part of each query that is multi tenant enabled to ensure that the object guid belonging to the user executing the query has strict privilege to the tenant guid they are trying to access.  
            //
            // Tenant guid will be drawn from the auth token's user data field, and the user guid will come from the user that we can retrieve in the controller.
            //
            // This will be used for all multi tenant system queries 
            //

            //
            // This table will join to each business table's multi-tenantry enabled tables by way of businessDb.table.tenantGuid = tenantUserTable.tenantGuid where tenantuserTable.userGuid = UserGuidFromProcessingContext
            //
            //
            // Basic workflow for managing this data is that upon user login to the business system, the login handler will do a delete all from this table where userid = current user id to start the slate clean.
            //
            // Then, it will insert into this table the current set of tenants that the logging in user is entitled to so that the queries invoked for this login session have the latest tenant configuration for this user.
            //
            Database.Table tenantUserTable = database.AddTable("TenantUser");
            tenantUserTable.AddIdField();

            tenantUserTable.AddGuidField("tenantGuid", false).CreateIndex();
            tenantUserTable.AddGuidField("userGuid", false).CreateIndex();

            tenantUserTable.AddUniqueConstraint("tenantGuid", "userGuid");
        }
        */


        public static void AddDataVisibilityTablesToDatabase(DatabaseGenerator.Database database)
        {
            //
            // These tables will enable the data visibility logic for business dbs so configured
            //
            // The org table and lower level children will all be read only in the business db side.  Their content will be managed in the security module by a business db super user that has data visibility admin reights to the business' security module.
            // The security module will therefore configure the orgs and children, and also allow the linkage of the users to the org structure.  The UI implication here is that we will need to build a tenant specific screen to allow the admin to only
            // configure their own tenant's org entries, and also not see users belonging to other tenants.  That work is TBD as if November 2021.
            //
            Database.Table organizationTable = database.AddTable("Organization");

            organizationTable.comment = @"This is the local shadow copy of the Security system's SecurityOrganization table, and is a cornerstone part of the Data Visibility system.  

Data in this table is to be exclusively managed by the Data Visibility sync process.  

This will ensure that the Security system's configuration controls the Data Visibility setup in module schemas like this one.";

            organizationTable.isWritable = false;
            organizationTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            organizationTable.AddIdField();
            organizationTable.AddGuidField("tenantGuid", false).CreateIndex();
            organizationTable.AddNameAndDescriptionFields(false, true);
            organizationTable.AddUniqueConstraint("tenantGuid", "name");        // each tenant can only have one org with the same name
            organizationTable.AddControlFields(true);

            Database.Table.Index organizationIdActiveDeletedIndex = organizationTable.CreateIndex("I_Organization_id_active_deleted");
            organizationIdActiveDeletedIndex.AddField("id");
            organizationIdActiveDeletedIndex.AddField("active");
            organizationIdActiveDeletedIndex.AddField("deleted");


            Database.Table userTitleTable = database.AddTable("UserTitle");
            userTitleTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            userTitleTable.comment = @"This is the local shadow copy of the Security system's SecurityUserTitle table, and is a part of the Data Visibility system.  

Data in this table is to be exclusively managed by the Data Visibility sync process.  

This will ensure that the Security system's configuration controls the Data Visibility setup in module schemas like this one.";

            userTitleTable.isWritable = false;
            userTitleTable.displayNameForTable = "User Title";
            userTitleTable.AddIdField();
            userTitleTable.AddNameAndDescriptionFields(true, true);
            userTitleTable.AddControlFields(true);

            Database.Table.Index userTitleIdActiveDeletedIndex = userTitleTable.CreateIndex("I_UserTitle_id_active_deleted");
            userTitleIdActiveDeletedIndex.AddField("id");
            userTitleIdActiveDeletedIndex.AddField("active");
            userTitleIdActiveDeletedIndex.AddField("deleted");



            Database.Table userTable = database.AddTable("User");

            userTable.comment = @"This is the local shadow copy of the Security system's SecurityUser table, and is a cornerstone part of the Data Visibility system.  

Data in this table is to be exclusively managed by the Data Visibility sync process.  

This will ensure that the Security system's configuration controls the Data Visibility setup in module schemas like this one.

**NOTE THAT THE OBJECT GUID FIELD ON THIS TABLE DOES NOT HAVE TO BE UNIQUE **
";

            userTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            userTable.isWritable = false;
            userTable.AddIdField();
            userTable.AddGuidField("tenantGuid", false).CreateIndex();
            userTable.AddString250Field("accountName", false).CreateIndex();
            userTable.SetDisplayNameField("accountName");
            userTable.AddString100Field("firstName");
            userTable.AddString100Field("middleName");
            userTable.AddString100Field("lastName");
            userTable.AddString250Field("displayName");
            userTable.AddForeignKeyField("userTitleId", userTitleTable, true);
            userTable.AddForeignKeyField("reportsToUserId", userTable, true);

            userTable.AddControlFields(true, false);                           // note that object guid is not unique on this table because a user can belong to more than one tenant in this table.


            userTable.AddUniqueConstraint("tenantGuid", "accountName");        // each tenant can only have one user with the same account name.
            userTable.AddUniqueConstraint("tenantGuid", "objectGuid");        // each tenant can only have one user with the same object guid.

            Database.Table.Index userIdActiveDeletedIndex = userTable.CreateIndex("I_User_id_active_deleted");
            userIdActiveDeletedIndex.AddField("id");
            userIdActiveDeletedIndex.AddField("active");
            userIdActiveDeletedIndex.AddField("deleted");


            //
            // This table will be part of the login reset process whereby the login action will review the state of the user's org entitlement, and delete, change, or add as appropriate so the session has up to date org-> user entitlement defined in the db
            //
            //
            // Note - it's not clear if we need a tenantId field on this this table yet.  Can solve with or wihthout by way of join to the tenantUser table.  However, revisit this when writing the queries on the business data side to see what
            // makes the most sense.
            //
            Database.Table organizationUserTable = database.AddTable("OrganizationUser");

            organizationUserTable.comment = @"This is the local shadow copy of the Security system's SecurityOrganizationUser table, and is a part of the Data Visibility system.  

Data in this table is to be exclusively managed by the Data Visibility sync process.  

This will ensure that the Security system's configuration controls the Data Visibility setup in module schemas like this one.";

            organizationUserTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            organizationUserTable.isWritable = false;
            organizationUserTable.AddIdField();
            organizationUserTable.AddGuidField("tenantGuid", false).AddScriptComments("The tenant for this record.").CreateIndex();
            organizationUserTable.AddForeignKeyField("organizationId", organizationTable, false);
            organizationUserTable.AddForeignKeyField("userId", userTable, false);
            organizationUserTable.AddBoolField("canRead", false, false).AddScriptComments("Whether or not the user in this organization can read data.").CreateIndex();
            organizationUserTable.AddBoolField("canWrite", false, false).AddScriptComments("Whether or not the user in this organization can write data.").CreateIndex();
            organizationUserTable.AddBoolField("canChangeHierarchy", false, false).AddScriptComments("Whether or not the user in this organization can change the data visibility hierarchy for data.").CreateIndex();
            organizationUserTable.AddBoolField("canChangeOwner", false, false).AddScriptComments("Whether or not the user in this organization can change the ownership of data.").CreateIndex();

            organizationUserTable.AddControlFields(true);

            Database.Table.Index organizationUserIdActiveDeletedIndex = organizationUserTable.CreateIndex("I_OrganizationUser_id_active_deleted");
            organizationUserIdActiveDeletedIndex.AddField("id");
            organizationUserIdActiveDeletedIndex.AddField("active");
            organizationUserIdActiveDeletedIndex.AddField("deleted");


            //organizationUserTable.AddUniqueConstraint("organizationId", "userId");  Don't do this because if a record changes on the security side, and user ids are re-arranged there correctly, the intermediate state during a data sync could violate this rule.

            Database.Table departmentTable = database.AddTable("Department");

            departmentTable.comment = @"This is the local shadow copy of the Security system's SecurityDepartment table, and is a cornerstone part of the Data Visibility system.  

Data in this table is to be exclusively managed by the Data Visibility sync process.  

This will ensure that the Security system's configuration controls the Data Visibility setup in module schemas like this one.";

            departmentTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            departmentTable.isWritable = false;
            departmentTable.adminAccessNeededToWrite = true;
            departmentTable.AddIdField();
            departmentTable.AddGuidField("tenantGuid", false).CreateIndex();
            departmentTable.AddForeignKeyField("organizationId", organizationTable, false);
            departmentTable.AddNameAndDescriptionFields(false, true);
            departmentTable.AddControlFields(true);

            departmentTable.AddUniqueConstraint("tenantGuid", "organizationId", "name");

            Database.Table.Index departmentIdActiveDeletedIndex = departmentTable.CreateIndex("I_Department_id_active_deleted");
            departmentIdActiveDeletedIndex.AddField("id");
            departmentIdActiveDeletedIndex.AddField("active");
            departmentIdActiveDeletedIndex.AddField("deleted");


            Database.Table departmentUserTable = database.AddTable("DepartmentUser");
            departmentUserTable.comment = @"This is the local shadow copy of the Security system's SecurityDepartmentUser table, and is a cornerstone part of the Data Visibility system.  

Data in this table is to be exclusively managed by the Data Visibility sync process.  

This will ensure that the Security system's configuration controls the Data Visibility setup in module schemas like this one.";


            departmentUserTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            departmentUserTable.isWritable = false;
            departmentUserTable.AddIdField();
            departmentUserTable.AddGuidField("tenantGuid", false).AddScriptComments("The tenant for this record.").CreateIndex();
            departmentUserTable.AddForeignKeyField("departmentId", departmentTable, false);
            departmentUserTable.AddForeignKeyField("userId", userTable, false);

            departmentUserTable.AddBoolField("canRead", false, false).AddScriptComments("Whether or not the user in this department can read data.").CreateIndex();
            departmentUserTable.AddBoolField("canWrite", false, false).AddScriptComments("Whether or not the user in this department can write data.").CreateIndex();
            departmentUserTable.AddBoolField("canChangeHierarchy", false, false).AddScriptComments("Whether or not the user in this department can change the data visibility hierarchy for data.").CreateIndex();
            departmentUserTable.AddBoolField("canChangeOwner", false, false).AddScriptComments("Whether or not the user in this department can change the ownership of data.").CreateIndex();

            //departmentUserTable.AddUniqueConstraint("departmentId", "userId");            Don't do this because if a record changes on the security side, and user ids are re-arranged there correctly, the intermediate state during a data sync could violate this rule.
            departmentUserTable.AddControlFields(true);

            Database.Table.Index departmentUserIdActiveDeletedIndex = departmentUserTable.CreateIndex("I_DepartmentUser_id_active_deleted");
            departmentUserIdActiveDeletedIndex.AddField("id");
            departmentUserIdActiveDeletedIndex.AddField("active");
            departmentUserIdActiveDeletedIndex.AddField("deleted");



            Database.Table teamTable = database.AddTable("Team");

            teamTable.comment = @"This is the local shadow copy of the Security system's SecurityTeam table, and is a cornerstone part of the Data Visibility system.  

Data in this table is to be exclusively managed by the Data Visibility sync process.  

This will ensure that the Security system's configuration controls the Data Visibility setup in module schemas like this one.";


            teamTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            teamTable.isWritable = false;
            teamTable.adminAccessNeededToWrite = true;
            teamTable.AddIdField();
            teamTable.AddGuidField("tenantGuid", false).CreateIndex();
            teamTable.AddForeignKeyField("departmentId", departmentTable, false);
            teamTable.AddNameAndDescriptionFields(false, true);

            teamTable.AddUniqueConstraint("tenantGuid", "departmentId", "name");
            teamTable.AddControlFields(true);

            Database.Table.Index teamIdActiveDeletedIndex = teamTable.CreateIndex("I_Team_id_active_deleted");
            teamIdActiveDeletedIndex.AddField("id");
            teamIdActiveDeletedIndex.AddField("active");
            teamIdActiveDeletedIndex.AddField("deleted");


            Database.Table teamUserTable = database.AddTable("TeamUser");
            teamUserTable.comment = @"This is the local shadow copy of the Security system's SecurityTeamUser table, and is a cornerstone part of the Data Visibility system.  

Data in this table is to be exclusively managed by the Data Visibility sync process.  

This will ensure that the Security system's configuration controls the Data Visibility setup in module schemas like this one.";

            teamUserTable.SetMinimumPermissionLevels(SECURITY_READER_PERMISSION_LEVEL, SECURITY_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            teamUserTable.isWritable = false;
            teamUserTable.AddIdField();
            teamUserTable.AddGuidField("tenantGuid", false).AddScriptComments("The tenant for this record.").CreateIndex();
            teamUserTable.AddForeignKeyField("teamId", teamTable, false);
            teamUserTable.AddForeignKeyField("userId", userTable, false);
            teamUserTable.AddBoolField("canRead", false, false).AddScriptComments("Whether or not the user in this team can read data.").CreateIndex();
            teamUserTable.AddBoolField("canWrite", false, false).AddScriptComments("Whether or not the user in this team can write data.").CreateIndex();
            teamUserTable.AddBoolField("canChangeHierarchy", false, false).AddScriptComments("Whether or not the user in this team can change the data visibility hierarchy for data.").CreateIndex();
            teamUserTable.AddBoolField("canChangeOwner", false, false).AddScriptComments("Whether or not the user in this team can change the ownership of data.").CreateIndex();

            teamUserTable.AddControlFields(true);

            Database.Table.Index teamUserIdActiveDeletedIndex = teamUserTable.CreateIndex("I_TeamUser_id_active_deleted");
            teamUserIdActiveDeletedIndex.AddField("id");
            teamUserIdActiveDeletedIndex.AddField("active");
            teamUserIdActiveDeletedIndex.AddField("deleted");


            //teamUserTable.AddUniqueConstraint("teamId", "userId");                Don't do this because if a record changes on the security side, and user ids are re-arranged there correctly, the intermediate state during a data sync could violate this rule.

            database.dataVisibilityEnabled = true;

            return;
        }


        /*
         * 
         *  This adds tables for a notification system.  It depends on the multi tenancy to exist first.
         *  
         *  The notification table will not have foreign key fields to any other entity.
         *  
         *  Instead, it has an entity name field, and an entity ID field to make it very general.
         *  
         *  This is done on purpose because notification data is not intended to be filed alongside real data.
         *  
         *  It is a transient system of sending a message to a user, not to add anything to the database in and of itself.  
         *  As such, these tables sit slightly outside the model, with the exception of linking to the user table, and other data visibility tables, but
         *  it only does that to figure out which users to distribute the notifications to.
         * 
         * The basic data flow for this is as follows:
         * 
         * 1.) A situation that should generate a notification arises, and the code calls a utility method  in the application base class called 'GenerateNotification'
         *     The notification would be intended to go to one user, one team, one department, one organization, or the whole tenant 
         *     
         *     a record is added to the notification table with the message details, the entity to link to and the id on a click, and an external URL if need be to inform
         *     the UI of what do to.
         *     
         *     If all of the 4 data visibility field are left null would imply distribution to all users in the tenant.  fields are (orgid, depid, teamid, userid),
         *     
         *          If a userId is provided the distribution is meant only to that user
         *          
         *     The distributed field is set to false, until the distribution process is done, and then it can flip it to true.  If the distribution is built into the creation process
         *     then the distributed flag can be created with a value of true.
         *          
         *     
         *      The distribution logic will do this:
         *      
         *      for any non distributed notifications, it will decide where to distribute to, based on which of the 4 fields are set.  List of user ids are determined.
         *      
         *      For each user that needs the notification, a new record is created in notificationDistribution.  
         *      
         *      The end user UI calls a method to get the current user's notifications, and the notificationDistributions for that usr that have not been acknowledge are to
         *      be returned.
         *      
         * 
         * */
        public static void AddNotificationTablesToDatabase(DatabaseGenerator.Database database)
        {

            //
            // Note: This method no longer requires Data Visibility tables.
            // User references are plain int columns resolved at runtime by IMessagingUserResolver.
            // Only multi-tenancy (AddMultiTenantSupport) is required.
            //


            //
            // This is static and spans tenants.
            //
            Database.Table notificationTypeTable = database.AddTable("NotificationType");
            notificationTypeTable.comment = @"This table defines the types of notifications that are available.  It is part of the Foundation Notification system.";

            notificationTypeTable.isWritable = false;
            notificationTypeTable.SetMinimumPermissionLevels(100, 100);
            notificationTypeTable.AddIdField();
            notificationTypeTable.AddNameAndDescriptionFields(true, true);
            notificationTypeTable.AddControlFields();

            Database.Table.Index notificationTypeIdActiveDeletedIndex = notificationTypeTable.CreateIndex("I_NotificationType_id_active_deleted");
            notificationTypeIdActiveDeletedIndex.AddField("id");
            notificationTypeIdActiveDeletedIndex.AddField("active");
            notificationTypeIdActiveDeletedIndex.AddField("deleted");



            notificationTypeTable.AddData(new Dictionary<string, string> { { "name", "Informative" },
                                                                    { "description", "Informative" },
                                                                    { "objectGuid", "065c2b74-dae1-4450-b8ee-bc5500ce64eb" } });


            notificationTypeTable.AddData(new Dictionary<string, string> { { "name", "Regular" },
                                                                    { "description", "Regular" },
                                                                    { "objectGuid", "e7c40dde-461f-41d1-8a9b-32e128179baa" } });


            notificationTypeTable.AddData(new Dictionary<string, string> { { "name", "Urgent" },
                                                                    { "description", "Urgent" },
                                                                    { "objectGuid", "825c0094-f3bb-45da-aa22-da459f4593b4" } });


            //
            // The idea here is that a notification is created in here, and is assigned to 1 of the 4 data visibility fields.  That one level indicates the group to which
            // the notification is distributed.  It could also work if more than one value is provided on the org/dep/team/user fields, but in that case then only the most
            // specific of the levels would be used.  (for org/dep/team at least - user is different here  user, as if there is only one user, then nobody else should be able to see it )
            //
            Database.Table notificationTable = database.AddTable("Notification");
            notificationTable.comment = @"This table store Notifications.  It is part of the Foundation Notification system.";
            notificationTable.minimumReadPermissionLevel = 50;
            notificationTable.minimumWritePermissionLevel = 50;
            notificationTable.adminAccessNeededToWrite = true;
            notificationTable.AddIdField();
            notificationTable.AddMultiTenantSupport();
            notificationTable.AddForeignKeyField("notificationTypeId", notificationTypeTable, true);
            notificationTable.AddIntField("createdByUserId", true).AddScriptComments("The user that created this notification.  Nullable so that the 'system' can create them too.  Resolved by IMessagingUserResolver.");
            notificationTable.AddTextField("message", false);
            notificationTable.AddIntField("priority", false, 100).AddScriptComments("The intent here is that the lower the priority number, the more urgent the notification is.");
            notificationTable.AddString250Field("entity", true).comment = "The name of the entity that this notification is about.";
            notificationTable.AddIntField("entityId", true).comment = "The ID for the entity that this notification is about.";
            notificationTable.AddString1000Field("externalURL", true).comment = "Ad-hoc external URL to be used if helpful.";
            notificationTable.AddDateTimeField("dateTimeCreated", false).comment = "When the notification was created.";
            notificationTable.AddDateTimeField("dateTimeDistributed", true).comment = "When the notification was distributed.";
            notificationTable.AddBoolField("distributionCompleted", false, false).AddScriptComments("Control flag to mark whether or not this notification has been distributed to the notificationUser table or not");
            notificationTable.AddIntField("userId", true).AddScriptComments("Optional target user for this notification.  Resolved by IMessagingUserResolver.");
            notificationTable.AddVersionControl();
            notificationTable.AddControlFields();

            Database.Table.Index notificationIdActiveDeletedIndex = notificationTable.CreateIndex("I_Notification_id_active_deleted");
            notificationIdActiveDeletedIndex.AddField("id");
            notificationIdActiveDeletedIndex.AddField("active");
            notificationIdActiveDeletedIndex.AddField("deleted");


            Database.Table notificationAttachmentTable = database.AddTable("NotificationAttachment");
            notificationAttachmentTable.comment = @"This table stores attachments for notifications.  It is part of the Foundation Notification system.";
            notificationAttachmentTable.minimumReadPermissionLevel = 50;
            notificationAttachmentTable.minimumWritePermissionLevel = 50;
            notificationAttachmentTable.adminAccessNeededToWrite = true;
            notificationAttachmentTable.AddIdField();
            notificationAttachmentTable.AddMultiTenantSupport();
            notificationAttachmentTable.AddForeignKeyField("notificationId", notificationTable, false).comment = "The notification for this attachment.";
            notificationAttachmentTable.AddIntField("userId", false).comment = "The user that created this attachment.  Resolved by IMessagingUserResolver.";
            notificationAttachmentTable.AddDateTimeField("dateTimeCreated", false).AddScriptComments("When this notification attachment was created.");
            notificationAttachmentTable.AddBinaryDataFields("content", false);
            notificationAttachmentTable.AddVersionControl();
            notificationAttachmentTable.AddControlFields();

            Database.Table.Index notificationAttachmentIdActiveDeletedIndex = notificationAttachmentTable.CreateIndex("I_NotificationAttachment_id_active_deleted");
            notificationAttachmentIdActiveDeletedIndex.AddField("id");
            notificationAttachmentIdActiveDeletedIndex.AddField("active");
            notificationAttachmentIdActiveDeletedIndex.AddField("deleted");




            Database.Table notificationDistributionTable = database.AddTable("NotificationDistribution");
            notificationDistributionTable.comment = @"This table defines the distribution for a notification.  It is part of the Foundation Notification system.";
            notificationDistributionTable.minimumReadPermissionLevel = 50;
            notificationDistributionTable.minimumWritePermissionLevel = 50;
            notificationDistributionTable.adminAccessNeededToWrite = true;
            notificationDistributionTable.AddIdField();
            notificationDistributionTable.AddMultiTenantSupport();
            notificationDistributionTable.AddForeignKeyField("notificationId", notificationTable, false).comment = "The notification that is being distributed.";
            notificationDistributionTable.AddIntField("userId", false).comment = "The user to distribute the notification to.  Resolved by IMessagingUserResolver.";
            notificationDistributionTable.AddBoolField("acknowledged", false, false).comment = "Whether or not the notification has been acknowledged.";
            notificationDistributionTable.AddDateTimeField("dateTimeAcknowledged", true).comment = "When the notification was acknowledged.";
            notificationDistributionTable.AddControlFields();

            Database.Table.Index notificationDistributionIdActiveDeletedIndex = notificationDistributionTable.CreateIndex("I_NotificationDistribution_id_active_deleted");
            notificationDistributionIdActiveDeletedIndex.AddField("id");
            notificationDistributionIdActiveDeletedIndex.AddField("active");
            notificationDistributionIdActiveDeletedIndex.AddField("deleted");

        }


        public static void AddConversationTablesToDatabase(DatabaseGenerator.Database database)
        {

            //
            // Note: This method no longer requires Data Visibility tables.
            // User references are plain int columns resolved at runtime by IMessagingUserResolver.
            // Only multi-tenancy (AddMultiTenantSupport) is required.
            //


            //
            // This is static and spans tenants.
            //
            Database.Table conversationTypeTable = database.AddTable("ConversationType");
            conversationTypeTable.comment = @"This is the main Conversation Type table.  It provides the types of conversations that can be created.

It is part of the Foundation's Conversation/Messaging system.";
            conversationTypeTable.isWritable = false;
            conversationTypeTable.SetMinimumPermissionLevels(100, 100);
            conversationTypeTable.AddIdField();
            conversationTypeTable.AddNameAndDescriptionFields(true, true);
            conversationTypeTable.AddControlFields();

            Database.Table.Index conversationTypeIdActiveDeletedIndex = conversationTypeTable.CreateIndex("I_ConversationType_id_active_deleted");
            conversationTypeIdActiveDeletedIndex.AddField("id");
            conversationTypeIdActiveDeletedIndex.AddField("active");
            conversationTypeIdActiveDeletedIndex.AddField("deleted");



            conversationTypeTable.AddData(new Dictionary<string, string> { { "name", "Regular" },
                                                                    { "description", "Regular" },
                                                                    { "objectGuid", "70174fce-f8de-4c44-b11f-db68b314204b" } });


            conversationTypeTable.AddData(new Dictionary<string, string> { { "name", "Urgent" },
                                                                    { "description", "Urgent" },
                                                                    { "objectGuid", "987ea6eb-155a-44ed-ac57-15a72ad2ae27" } });

            conversationTypeTable.AddData(new Dictionary<string, string> { { "name", "Channel" },
                                                                    { "description", "A persistent named conversation (like a Teams/Slack channel)" },
                                                                    { "objectGuid", "54883c38-7860-40bf-a6e4-ce5535db7ed4" } });

            //
            //
            //
            Database.Table conversationTable = database.AddTable("Conversation");
            conversationTable.comment = "This is the main Conversation table.  It is part of the Foundation's Conversation/Messaging system.";
            conversationTable.minimumReadPermissionLevel = 50;
            conversationTable.minimumWritePermissionLevel = 50;
            conversationTable.adminAccessNeededToWrite = true;

            conversationTable.AddIdField();
            conversationTable.AddMultiTenantSupport();
            conversationTable.AddIntField("createdByUserId", true).AddScriptComments("The user that started the conversation.  Resolved by IMessagingUserResolver.  Nullable for system-started conversations.");
            conversationTable.AddForeignKeyField("conversationTypeId", conversationTypeTable, true);
            conversationTable.AddIntField("priority", false, 100).AddScriptComments("The intent here is that the lower the priority number, the more urgent the conversation is.");
            conversationTable.AddDateTimeField("dateTimeCreated", false).AddScriptComments("When the conversation was created.");
            conversationTable.AddString250Field("entity", true).AddScriptComments("The in case the conversation is to do with an entity, it is named here");
            conversationTable.AddIntField("entityId", true).AddScriptComments("The id of the entity that the conversation is about");
            conversationTable.AddString1000Field("externalURL", true);

            conversationTable.AddString250Field("name", true, null).AddScriptComments("The name of the conversation.  A named conversation is a channel.  Optional because not all conversations will be channels.");
            conversationTable.AddString1000Field("description", true, null).AddScriptComments("The description of the channel conversation.  Optional because not all conversations need to have a description, but if it does have one, this is where it goes.");
            conversationTable.AddBooleanField("isPublic", true, null).AddScriptComments("Whether or not the conversation is public");
            /*
             *         public string name { get; set; }
        public string description { get; set; }
        public bool isPublic { get; set; }
             * 
             */


            conversationTable.AddIntField("userId", true).AddScriptComments("Optional target user for this conversation.  Resolved by IMessagingUserResolver.");
            conversationTable.AddControlFields();

            Database.Table.Index conversationIdActiveDeletedIndex = conversationTable.CreateIndex("I_Conversation_id_active_deleted");
            conversationIdActiveDeletedIndex.AddField("id");
            conversationIdActiveDeletedIndex.AddField("active");
            conversationIdActiveDeletedIndex.AddField("deleted");



            Database.Table conversationUserTable = database.AddTable("ConversationUser");
            conversationUserTable.comment = "This is the ConversationUser table.  It tracks the users that belong to a conversation.  It is part of the Foundation's Conversation/Messaging system.";
            conversationUserTable.minimumReadPermissionLevel = 50;
            conversationUserTable.minimumWritePermissionLevel = 50;
            conversationUserTable.adminAccessNeededToWrite = true;
            conversationUserTable.AddIdField();
            conversationUserTable.AddMultiTenantSupport();
            conversationUserTable.AddForeignKeyField("conversationId", conversationTable, false);
            conversationUserTable.AddIntField("userId", false).AddScriptComments("The user in this conversation.  Resolved by IMessagingUserResolver.");
            conversationUserTable.AddDateTimeField("dateTimeAdded", false).AddScriptComments("When this user was added to the conversation.");
            conversationUserTable.AddControlFields();

            Database.Table.Index conversationUserIdActiveDeletedIndex = conversationUserTable.CreateIndex("I_ConversationUser_id_active_deleted");
            conversationUserIdActiveDeletedIndex.AddField("id");
            conversationUserIdActiveDeletedIndex.AddField("active");
            conversationUserIdActiveDeletedIndex.AddField("deleted");


            //
            // ConversationChannel - Extends conversations to support persistent named channels.
            // A channel is a specialized conversation with a name, topic, and optional scope to an organization, department, or team.
            //
            Database.Table conversationChannelTable = database.AddTable("ConversationChannel");
            conversationChannelTable.comment = @"This table extends a Conversation record to be a named Channel.  It is part of the Foundation's Messaging system.

A channel is a persistent, named conversation that users can browse and join.  It links to the base Conversation record 
and adds channel-specific fields like name, topic, and privacy.";

            conversationChannelTable.minimumReadPermissionLevel = 50;
            conversationChannelTable.minimumWritePermissionLevel = 50;
            conversationChannelTable.adminAccessNeededToWrite = true;
            conversationChannelTable.AddIdField();
            conversationChannelTable.AddMultiTenantSupport();
            conversationChannelTable.AddForeignKeyField("conversationId", conversationTable, false).AddScriptComments("The base conversation record that this channel extends.");
            conversationChannelTable.AddString250Field("name", false).AddScriptComments("The display name for the channel.");
            conversationChannelTable.AddString1000Field("topic", true).AddScriptComments("The current topic or description of the channel.  Can be changed over time.");
            conversationChannelTable.AddBoolField("isPrivate", false, false).AddScriptComments("Whether or not this channel is private.  Private channels are invitation-only and do not appear in the channel browser.");
            conversationChannelTable.AddBoolField("isPinned", false, false).AddScriptComments("Whether or not this channel is pinned in the UI.  Pinned channels appear at the top of the channel list.");

            conversationChannelTable.AddVersionControl();
            conversationChannelTable.AddControlFields();

            Database.Table.Index conversationChannelIdActiveDeletedIndex = conversationChannelTable.CreateIndex("I_ConversationChannel_id_active_deleted");
            conversationChannelIdActiveDeletedIndex.AddField("id");
            conversationChannelIdActiveDeletedIndex.AddField("active");
            conversationChannelIdActiveDeletedIndex.AddField("deleted");



            Database.Table conversationMessageTable = database.AddTable("ConversationMessage");
            conversationMessageTable.comment = "This is the ConversationMessage table.  It tracks the messages that belong to a conversation.  It is part of the Foundation's Conversation/Messaging system.";
            conversationMessageTable.minimumReadPermissionLevel = 50;
            conversationMessageTable.minimumWritePermissionLevel = 50;
            conversationMessageTable.adminAccessNeededToWrite = true;
            conversationMessageTable.AddIdField();
            conversationMessageTable.AddMultiTenantSupport();
            conversationMessageTable.AddForeignKeyField("conversationId", conversationTable, false);
            conversationMessageTable.AddIntField("userId", false).AddScriptComments("The user that sent this message.  Resolved by IMessagingUserResolver.");
            conversationMessageTable.AddForeignKeyField("parentConversationMessageId", conversationMessageTable, true);
            //
            // Add conversationChannelId FK to ConversationMessage so messages can be
            // scoped to a specific channel within a conversation.  Nullable — NULL means
            // the message is at the conversation level (no channel assignment).
            //
            conversationMessageTable.AddForeignKeyField("conversationChannelId", conversationChannelTable, true).AddScriptComments("Optional channel that this message belongs to.  NULL = conversation-level (no channel).");
            conversationMessageTable.AddDateTimeField("dateTimeCreated", false).AddScriptComments("When this message was created.");
            conversationMessageTable.AddTextField("message", false);
            conversationMessageTable.AddString250Field("entity", true).AddScriptComments("The in case the conversation message is to do with an entity, it is named here");
            conversationMessageTable.AddIntField("entityId", true).AddScriptComments("The id of the entity that the message is about");
            conversationMessageTable.AddString1000Field("externalURL", true);
            conversationMessageTable.AddVersionControl();       // so message records can be edited, and the changes are recorded.
            conversationMessageTable.AddControlFields();

            Database.Table.Index conversationMessageIdActiveDeletedIndex = conversationMessageTable.CreateIndex("I_ConversationMessage_id_active_deleted");
            conversationMessageIdActiveDeletedIndex.AddField("id");
            conversationMessageIdActiveDeletedIndex.AddField("active");
            conversationMessageIdActiveDeletedIndex.AddField("deleted");



            Database.Table conversationMessageAttachmentTable = database.AddTable("ConversationMessageAttachment");
            conversationMessageAttachmentTable.comment = "This is the ConversationMessageAttachment table.  It tracks the attachments that belong to a message in a conversation.  It is part of the Foundation's Conversation/Messaging system.";
            conversationMessageAttachmentTable.minimumReadPermissionLevel = 50;
            conversationMessageAttachmentTable.minimumWritePermissionLevel = 50;
            conversationMessageAttachmentTable.adminAccessNeededToWrite = true;
            conversationMessageAttachmentTable.AddIdField();
            conversationMessageAttachmentTable.AddMultiTenantSupport();
            conversationMessageAttachmentTable.AddForeignKeyField("conversationMessageId", conversationMessageTable, false);
            conversationMessageAttachmentTable.AddIntField("userId", false).AddScriptComments("The user that uploaded this attachment.  Resolved by IMessagingUserResolver.");
            conversationMessageAttachmentTable.AddDateTimeField("dateTimeCreated", false).AddScriptComments("When this conversation message attachment was created.");
            conversationMessageAttachmentTable.AddBinaryDataFields("content", false);
            conversationMessageAttachmentTable.AddVersionControl();
            conversationMessageAttachmentTable.AddControlFields();

            Database.Table.Index conversationMessageAttachmentIdActiveDeletedIndex = conversationMessageAttachmentTable.CreateIndex("I_ConversationMessageAttachment_id_active_deleted");
            conversationMessageAttachmentIdActiveDeletedIndex.AddField("id");
            conversationMessageAttachmentIdActiveDeletedIndex.AddField("active");
            conversationMessageAttachmentIdActiveDeletedIndex.AddField("deleted");



            Database.Table conversationMessageUserTable = database.AddTable("ConversationMessageUser");
            conversationMessageUserTable.comment = "This is the ConversationMessageUser table.  It tracks the users that belong to a message in a conversation.  It is part of the Foundation's Conversation/Messaging system.";
            conversationMessageUserTable.minimumReadPermissionLevel = 50;
            conversationMessageUserTable.minimumWritePermissionLevel = 50;
            conversationMessageUserTable.adminAccessNeededToWrite = true;
            conversationMessageUserTable.AddIdField();
            conversationMessageUserTable.AddMultiTenantSupport();
            conversationMessageUserTable.AddForeignKeyField("conversationMessageId", conversationMessageTable, false);
            conversationMessageUserTable.AddIntField("userId", false).AddScriptComments("The target user for this message.  Resolved by IMessagingUserResolver.");
            conversationMessageUserTable.AddDateTimeField("dateTimeCreated", false).AddScriptComments("When this conversation message user was created.");
            conversationMessageUserTable.AddBoolField("acknowledged", false, false);
            conversationMessageUserTable.AddDateTimeField("dateTimeAcknowledged", false).AddScriptComments("When this conversation message user was acknowledge by the user.  For messages, this may be auto acknowledged once the data is read and shown.  Up to the UI to decide when to mark it as acknowledged..");
            conversationMessageUserTable.AddControlFields();

            Database.Table.Index conversationMessageUserIdActiveDeletedIndex = conversationMessageUserTable.CreateIndex("I_ConversationMessageUser_id_active_deleted");
            conversationMessageUserIdActiveDeletedIndex.AddField("id");
            conversationMessageUserIdActiveDeletedIndex.AddField("active");
            conversationMessageUserIdActiveDeletedIndex.AddField("deleted");

        }


        /*
         * 
         *  This adds tables for enhanced messaging features on top of the conversation system.
         *  It depends on the conversation tables to exist first.
         *  
         *  These tables extend the conversation system with:
         *  - Channels (persistent named conversations, like Teams/Slack channels)
         *  - Message reactions (emoji reactions to reduce message noise)
         *  - Pinned messages (bookmark important messages within a conversation)
         *  - User presence (online/offline/away status tracking)
         *  
         *  It also adds new ConversationType seed data for "Channel" and "Direct Message" types.
         * 
         * */
        public static void AddMessagingTablesToDatabase(DatabaseGenerator.Database database)
        {

            Database.Table conversationTable = database.GetTable("Conversation");
            Database.Table conversationMessageTable = database.GetTable("ConversationMessage");
            Database.Table conversationTypeTable = database.GetTable("ConversationType");

            if (conversationTable == null || conversationMessageTable == null || conversationTypeTable == null)
            {
                throw new Exception("Conversation tables must be in schema before messaging tables are added");
            }

            //
            // Additional ConversationType seed data for the messaging system
            //
            conversationTypeTable.AddData(new Dictionary<string, string> { { "name", "Direct Message" },
                                                                    { "description", "A 1:1 or small group direct message conversation" },
                                                                    { "objectGuid", "d45cfb49-0dbc-4c05-a8b7-f17a6e71a926" } });




            //
            // ConversationMessageReaction - Lightweight emoji reactions to messages.
            // Reduces noise compared to sending separate "thumbs up" messages.
            //
            Database.Table conversationMessageReactionTable = database.AddTable("ConversationMessageReaction");
            conversationMessageReactionTable.comment = @"This table stores emoji reactions to conversation messages.  It is part of the Foundation's Messaging system.

Reactions provide a lightweight way for users to respond to messages without creating additional message records.  
Each reaction is a short string representing an emoji code or shortname.";

            conversationMessageReactionTable.minimumReadPermissionLevel = 50;
            conversationMessageReactionTable.minimumWritePermissionLevel = 50;
            conversationMessageReactionTable.AddIdField();
            conversationMessageReactionTable.AddMultiTenantSupport();
            conversationMessageReactionTable.AddForeignKeyField("conversationMessageId", conversationMessageTable, false).AddScriptComments("The message that this reaction is for.");
            conversationMessageReactionTable.AddIntField("userId", false).AddScriptComments("The user who reacted.  Resolved by IMessagingUserResolver.");
            conversationMessageReactionTable.AddString50Field("reaction", false).AddScriptComments("The emoji code or shortname for the reaction, for example 'thumbsup', 'heart', 'laughing'.");
            conversationMessageReactionTable.AddDateTimeField("dateTimeCreated", false).AddScriptComments("When this reaction was created.");
            conversationMessageReactionTable.AddControlFields();

            Database.Table.Index conversationMessageReactionIdActiveDeletedIndex = conversationMessageReactionTable.CreateIndex("I_ConversationMessageReaction_id_active_deleted");
            conversationMessageReactionIdActiveDeletedIndex.AddField("id");
            conversationMessageReactionIdActiveDeletedIndex.AddField("active");
            conversationMessageReactionIdActiveDeletedIndex.AddField("deleted");

            // Composite index to efficiently query reactions for a message
            conversationMessageReactionTable.CreateIndexForFields(new List<string> { "conversationMessageId", "active", "deleted" });



            //
            // ConversationPin - Pin important messages within a conversation for easy reference.
            //
            Database.Table conversationPinTable = database.AddTable("ConversationPin");
            conversationPinTable.comment = @"This table tracks pinned messages within a conversation.  It is part of the Foundation's Messaging system.

Pinned messages are highlighted in the conversation and can be browsed separately, providing a way to bookmark 
important messages, decisions, or reference information within a conversation.";

            conversationPinTable.minimumReadPermissionLevel = 50;
            conversationPinTable.minimumWritePermissionLevel = 50;
            conversationPinTable.AddIdField();
            conversationPinTable.AddMultiTenantSupport();
            conversationPinTable.AddForeignKeyField("conversationId", conversationTable, false).AddScriptComments("The conversation that this pin belongs to.");
            conversationPinTable.AddForeignKeyField("conversationMessageId", conversationMessageTable, false).AddScriptComments("The message that is pinned.");
            conversationPinTable.AddIntField("pinnedByUserId", false).AddScriptComments("The user who pinned this message.  Resolved by IMessagingUserResolver.");
            conversationPinTable.AddDateTimeField("dateTimePinned", false).AddScriptComments("When this message was pinned.");
            conversationPinTable.AddControlFields();

            Database.Table.Index conversationPinIdActiveDeletedIndex = conversationPinTable.CreateIndex("I_ConversationPin_id_active_deleted");
            conversationPinIdActiveDeletedIndex.AddField("id");
            conversationPinIdActiveDeletedIndex.AddField("active");
            conversationPinIdActiveDeletedIndex.AddField("deleted");

            // Composite index to efficiently query pins for a conversation
            conversationPinTable.CreateIndexForFields(new List<string> { "conversationId", "active", "deleted" });



            //
            // UserPresence - Track user online/offline status and activity.
            // This table is updated on SignalR hub connect/disconnect events.
            //
            Database.Table userPresenceTable = database.AddTable("UserPresence");
            userPresenceTable.comment = @"This table tracks user online/offline status and activity for the messaging system.  It is part of the Foundation's Messaging system.

Presence records are updated when users connect to or disconnect from the MessagingHub.  The connectionCount field supports 
multi-device presence (a user connected from both a browser and a mobile app would have connectionCount = 2).  
When connectionCount drops to 0, the user is considered offline.";

            userPresenceTable.minimumReadPermissionLevel = 50;
            userPresenceTable.minimumWritePermissionLevel = 50;
            userPresenceTable.AddIdField();
            userPresenceTable.AddMultiTenantSupport();
            userPresenceTable.AddIntField("userId", false).AddScriptComments("The user whose presence is being tracked.  Resolved by IMessagingUserResolver.");
            userPresenceTable.AddString50Field("status", false).AddScriptComments("The current status: 'online', 'away', 'busy', 'offline', 'doNotDisturb'.");
            userPresenceTable.AddString250Field("customStatusMessage", true).AddScriptComments("Optional custom status message, for example 'In a meeting until 3pm'.");
            userPresenceTable.AddDateTimeField("lastSeenDateTime", false).AddScriptComments("The last time this user was seen connected.");
            userPresenceTable.AddDateTimeField("lastActivityDateTime", false).AddScriptComments("The last time this user performed an action (sent a message, reacted, etc).");
            userPresenceTable.AddIntField("connectionCount", false, 0).AddScriptComments("The number of active connections for this user.  Supports multi-device presence.");
            userPresenceTable.AddControlFields();

            Database.Table.Index userPresenceIdActiveDeletedIndex = userPresenceTable.CreateIndex("I_UserPresence_id_active_deleted");
            userPresenceIdActiveDeletedIndex.AddField("id");
            userPresenceIdActiveDeletedIndex.AddField("active");
            userPresenceIdActiveDeletedIndex.AddField("deleted");

            // Index to quickly look up presence by user
            userPresenceTable.CreateIndexForFields(new List<string> { "userId", "active", "deleted" });

        }

    }
}