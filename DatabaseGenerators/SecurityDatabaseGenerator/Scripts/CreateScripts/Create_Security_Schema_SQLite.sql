/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE "OAUTHToken"
-- DROP TABLE "EntityDataTokenEvent"
-- DROP TABLE "EntityDataTokenEventType"
-- DROP TABLE "EntityDataToken"
-- DROP TABLE "LoginAttempt"
-- DROP TABLE "SystemSetting"
-- DROP TABLE "ModuleSecurityRole"
-- DROP TABLE "Module"
-- DROP TABLE "SecurityGroupSecurityRole"
-- DROP TABLE "SecurityUserSecurityRole"
-- DROP TABLE "SecurityRole"
-- DROP TABLE "Privilege"
-- DROP TABLE "SecurityUserSecurityGroup"
-- DROP TABLE "SecurityGroup"
-- DROP TABLE "SecurityUserPasswordResetToken"
-- DROP TABLE "SecurityUserEvent"
-- DROP TABLE "SecurityUserEventType"
-- DROP TABLE "SecurityTeamUser"
-- DROP TABLE "SecurityDepartmentUser"
-- DROP TABLE "SecurityOrganizationUser"
-- DROP TABLE "SecurityTenantUser"
-- DROP TABLE "SecurityUser"
-- DROP TABLE "SecurityUserTitle"
-- DROP TABLE "SecurityTeam"
-- DROP TABLE "SecurityDepartment"
-- DROP TABLE "SecurityOrganization"
-- DROP TABLE "SecurityTenant"

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON "OAUTHToken" DISABLE
-- ALTER INDEX ALL ON "EntityDataTokenEvent" DISABLE
-- ALTER INDEX ALL ON "EntityDataTokenEventType" DISABLE
-- ALTER INDEX ALL ON "EntityDataToken" DISABLE
-- ALTER INDEX ALL ON "LoginAttempt" DISABLE
-- ALTER INDEX ALL ON "SystemSetting" DISABLE
-- ALTER INDEX ALL ON "ModuleSecurityRole" DISABLE
-- ALTER INDEX ALL ON "Module" DISABLE
-- ALTER INDEX ALL ON "SecurityGroupSecurityRole" DISABLE
-- ALTER INDEX ALL ON "SecurityUserSecurityRole" DISABLE
-- ALTER INDEX ALL ON "SecurityRole" DISABLE
-- ALTER INDEX ALL ON "Privilege" DISABLE
-- ALTER INDEX ALL ON "SecurityUserSecurityGroup" DISABLE
-- ALTER INDEX ALL ON "SecurityGroup" DISABLE
-- ALTER INDEX ALL ON "SecurityUserPasswordResetToken" DISABLE
-- ALTER INDEX ALL ON "SecurityUserEvent" DISABLE
-- ALTER INDEX ALL ON "SecurityUserEventType" DISABLE
-- ALTER INDEX ALL ON "SecurityTeamUser" DISABLE
-- ALTER INDEX ALL ON "SecurityDepartmentUser" DISABLE
-- ALTER INDEX ALL ON "SecurityOrganizationUser" DISABLE
-- ALTER INDEX ALL ON "SecurityTenantUser" DISABLE
-- ALTER INDEX ALL ON "SecurityUser" DISABLE
-- ALTER INDEX ALL ON "SecurityUserTitle" DISABLE
-- ALTER INDEX ALL ON "SecurityTeam" DISABLE
-- ALTER INDEX ALL ON "SecurityDepartment" DISABLE
-- ALTER INDEX ALL ON "SecurityOrganization" DISABLE
-- ALTER INDEX ALL ON "SecurityTenant" DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON "OAUTHToken" REBUILD
-- ALTER INDEX ALL ON "EntityDataTokenEvent" REBUILD
-- ALTER INDEX ALL ON "EntityDataTokenEventType" REBUILD
-- ALTER INDEX ALL ON "EntityDataToken" REBUILD
-- ALTER INDEX ALL ON "LoginAttempt" REBUILD
-- ALTER INDEX ALL ON "SystemSetting" REBUILD
-- ALTER INDEX ALL ON "ModuleSecurityRole" REBUILD
-- ALTER INDEX ALL ON "Module" REBUILD
-- ALTER INDEX ALL ON "SecurityGroupSecurityRole" REBUILD
-- ALTER INDEX ALL ON "SecurityUserSecurityRole" REBUILD
-- ALTER INDEX ALL ON "SecurityRole" REBUILD
-- ALTER INDEX ALL ON "Privilege" REBUILD
-- ALTER INDEX ALL ON "SecurityUserSecurityGroup" REBUILD
-- ALTER INDEX ALL ON "SecurityGroup" REBUILD
-- ALTER INDEX ALL ON "SecurityUserPasswordResetToken" REBUILD
-- ALTER INDEX ALL ON "SecurityUserEvent" REBUILD
-- ALTER INDEX ALL ON "SecurityUserEventType" REBUILD
-- ALTER INDEX ALL ON "SecurityTeamUser" REBUILD
-- ALTER INDEX ALL ON "SecurityDepartmentUser" REBUILD
-- ALTER INDEX ALL ON "SecurityOrganizationUser" REBUILD
-- ALTER INDEX ALL ON "SecurityTenantUser" REBUILD
-- ALTER INDEX ALL ON "SecurityUser" REBUILD
-- ALTER INDEX ALL ON "SecurityUserTitle" REBUILD
-- ALTER INDEX ALL ON "SecurityTeam" REBUILD
-- ALTER INDEX ALL ON "SecurityDepartment" REBUILD
-- ALTER INDEX ALL ON "SecurityOrganization" REBUILD
-- ALTER INDEX ALL ON "SecurityTenant" REBUILD

CREATE TABLE "SecurityTenant"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the SecurityTenant table's name field.
CREATE INDEX "I_SecurityTenant_name" ON "SecurityTenant" ("name");

-- Index on the SecurityTenant table's active field.
CREATE INDEX "I_SecurityTenant_active" ON "SecurityTenant" ("active");

-- Index on the SecurityTenant table's deleted field.
CREATE INDEX "I_SecurityTenant_deleted" ON "SecurityTenant" ("deleted");

-- Index on the SecurityTenant table's id,active,deleted fields.
CREATE INDEX "I_SecurityTenant_id_active_deleted" ON "SecurityTenant" ("id", "active", "deleted");


CREATE TABLE "SecurityOrganization"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityTenantId" INTEGER NOT NULL,		-- Link to the SecurityTenant table.
	"name" VARCHAR(100) NOT NULL COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityTenantId") REFERENCES "SecurityTenant"("id"),		-- Foreign key to the SecurityTenant table.
	UNIQUE ( "securityTenantId", "name") 		-- Uniqueness enforced on the SecurityOrganization table's securityTenantId and name fields.
);
-- Index on the SecurityOrganization table's securityTenantId field.
CREATE INDEX "I_SecurityOrganization_securityTenantId" ON "SecurityOrganization" ("securityTenantId");

-- Index on the SecurityOrganization table's name field.
CREATE INDEX "I_SecurityOrganization_name" ON "SecurityOrganization" ("name");

-- Index on the SecurityOrganization table's active field.
CREATE INDEX "I_SecurityOrganization_active" ON "SecurityOrganization" ("active");

-- Index on the SecurityOrganization table's deleted field.
CREATE INDEX "I_SecurityOrganization_deleted" ON "SecurityOrganization" ("deleted");

-- Index on the SecurityOrganization table's id,active,deleted fields.
CREATE INDEX "I_SecurityOrganization_id_active_deleted" ON "SecurityOrganization" ("id", "active", "deleted");


CREATE TABLE "SecurityDepartment"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityOrganizationId" INTEGER NOT NULL,		-- Link to the SecurityOrganization table.
	"name" VARCHAR(100) NOT NULL COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityOrganizationId") REFERENCES "SecurityOrganization"("id"),		-- Foreign key to the SecurityOrganization table.
	UNIQUE ( "securityOrganizationId", "name") 		-- Uniqueness enforced on the SecurityDepartment table's securityOrganizationId and name fields.
);
-- Index on the SecurityDepartment table's securityOrganizationId field.
CREATE INDEX "I_SecurityDepartment_securityOrganizationId" ON "SecurityDepartment" ("securityOrganizationId");

-- Index on the SecurityDepartment table's name field.
CREATE INDEX "I_SecurityDepartment_name" ON "SecurityDepartment" ("name");

-- Index on the SecurityDepartment table's active field.
CREATE INDEX "I_SecurityDepartment_active" ON "SecurityDepartment" ("active");

-- Index on the SecurityDepartment table's deleted field.
CREATE INDEX "I_SecurityDepartment_deleted" ON "SecurityDepartment" ("deleted");

-- Index on the SecurityDepartment table's id,active,deleted fields.
CREATE INDEX "I_SecurityDepartment_id_active_deleted" ON "SecurityDepartment" ("id", "active", "deleted");


CREATE TABLE "SecurityTeam"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityDepartmentId" INTEGER NOT NULL,		-- Link to the SecurityDepartment table.
	"name" VARCHAR(100) NOT NULL COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityDepartmentId") REFERENCES "SecurityDepartment"("id"),		-- Foreign key to the SecurityDepartment table.
	UNIQUE ( "securityDepartmentId", "name") 		-- Uniqueness enforced on the SecurityTeam table's securityDepartmentId and name fields.
);
-- Index on the SecurityTeam table's securityDepartmentId field.
CREATE INDEX "I_SecurityTeam_securityDepartmentId" ON "SecurityTeam" ("securityDepartmentId");

-- Index on the SecurityTeam table's name field.
CREATE INDEX "I_SecurityTeam_name" ON "SecurityTeam" ("name");

-- Index on the SecurityTeam table's active field.
CREATE INDEX "I_SecurityTeam_active" ON "SecurityTeam" ("active");

-- Index on the SecurityTeam table's deleted field.
CREATE INDEX "I_SecurityTeam_deleted" ON "SecurityTeam" ("deleted");

-- Index on the SecurityTeam table's id,active,deleted fields.
CREATE INDEX "I_SecurityTeam_id_active_deleted" ON "SecurityTeam" ("id", "active", "deleted");


CREATE TABLE "SecurityUserTitle"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the SecurityUserTitle table's name field.
CREATE INDEX "I_SecurityUserTitle_name" ON "SecurityUserTitle" ("name");

-- Index on the SecurityUserTitle table's active field.
CREATE INDEX "I_SecurityUserTitle_active" ON "SecurityUserTitle" ("active");

-- Index on the SecurityUserTitle table's deleted field.
CREATE INDEX "I_SecurityUserTitle_deleted" ON "SecurityUserTitle" ("deleted");

-- Index on the SecurityUserTitle table's id,active,deleted fields.
CREATE INDEX "I_SecurityUserTitle_id_active_deleted" ON "SecurityUserTitle" ("id", "active", "deleted");


CREATE TABLE "SecurityUser"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"accountName" VARCHAR(250) NOT NULL UNIQUE COLLATE NOCASE,
	"activeDirectoryAccount" BIT NOT NULL DEFAULT 0,
	"password" VARCHAR(250) NULL COLLATE NOCASE,
	"canLogin" BIT NOT NULL DEFAULT 1,		-- Whether or not the user can login.  Should be true for people, or API access accounts, and false for internal use service accounts that should never be allowed to login.
	"mustChangePassword" BIT NOT NULL DEFAULT 0,		-- True if the user is required to change their password
	"firstName" VARCHAR(100) NULL COLLATE NOCASE,
	"middleName" VARCHAR(100) NULL COLLATE NOCASE,
	"lastName" VARCHAR(100) NULL COLLATE NOCASE,
	"dateOfBirth" DATETIME NULL,
	"emailAddress" VARCHAR(100) NULL COLLATE NOCASE,
	"cellPhoneNumber" VARCHAR(100) NULL COLLATE NOCASE,
	"phoneNumber" VARCHAR(50) NULL COLLATE NOCASE,
	"phoneExtension" VARCHAR(50) NULL COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"securityUserTitleId" INTEGER NULL,		-- Link to the SecurityUserTitle table.
	"reportsToSecurityUserId" INTEGER NULL,		-- Link to the SecurityUser table.
	"authenticationDomain" VARCHAR(100) NULL COLLATE NOCASE,
	"failedLoginCount" INTEGER NULL,
	"lastLoginAttempt" DATETIME NULL,
	"mostRecentActivity" DATETIME NULL,
	"alternateIdentifier" VARCHAR(100) NULL COLLATE NOCASE,
	"image" BLOB NULL,
	"settings" TEXT NULL COLLATE NOCASE,
	"securityTenantId" INTEGER NULL,		-- The tenant that this user is linked to
	"readPermissionLevel" INTEGER NOT NULL DEFAULT 0,
	"writePermissionLevel" INTEGER NOT NULL DEFAULT 0,
	"securityOrganizationId" INTEGER NULL,		-- The default organization to use when creating data, and null is provided as an organization on a data visibility enabled table
	"securityDepartmentId" INTEGER NULL,		-- The default department to use when creating data, and null is provided as a department on a data visibility enabled table
	"securityTeamId" INTEGER NULL,		-- The default team to use when creating data, and null is provided as a team on a data visibility enabled table
	"authenticationToken" VARCHAR(100) NULL COLLATE NOCASE,
	"authenticationTokenExpiry" DATETIME NULL,
	"twoFactorToken" VARCHAR(10) NULL COLLATE NOCASE,
	"twoFactorTokenExpiry" DATETIME NULL,
	"twoFactorSendByEmail" BIT NULL,
	"twoFactorSendBySMS" BIT NULL,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityUserTitleId") REFERENCES "SecurityUserTitle"("id"),		-- Foreign key to the SecurityUserTitle table.
	FOREIGN KEY ("reportsToSecurityUserId") REFERENCES "SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	FOREIGN KEY ("securityTenantId") REFERENCES "SecurityTenant"("id"),		-- Foreign key to the SecurityTenant table.
	FOREIGN KEY ("securityOrganizationId") REFERENCES "SecurityOrganization"("id"),		-- Foreign key to the SecurityOrganization table.
	FOREIGN KEY ("securityDepartmentId") REFERENCES "SecurityDepartment"("id"),		-- Foreign key to the SecurityDepartment table.
	FOREIGN KEY ("securityTeamId") REFERENCES "SecurityTeam"("id")		-- Foreign key to the SecurityTeam table.
);
-- Index on the SecurityUser table's accountName field.
CREATE INDEX "I_SecurityUser_accountName" ON "SecurityUser" ("accountName");

-- Index on the SecurityUser table's securityUserTitleId field.
CREATE INDEX "I_SecurityUser_securityUserTitleId" ON "SecurityUser" ("securityUserTitleId");

-- Index on the SecurityUser table's reportsToSecurityUserId field.
CREATE INDEX "I_SecurityUser_reportsToSecurityUserId" ON "SecurityUser" ("reportsToSecurityUserId");

-- Index on the SecurityUser table's alternateIdentifier field.
CREATE INDEX "I_SecurityUser_alternateIdentifier" ON "SecurityUser" ("alternateIdentifier");

-- Index on the SecurityUser table's securityTenantId field.
CREATE INDEX "I_SecurityUser_securityTenantId" ON "SecurityUser" ("securityTenantId");

-- Index on the SecurityUser table's securityOrganizationId field.
CREATE INDEX "I_SecurityUser_securityOrganizationId" ON "SecurityUser" ("securityOrganizationId");

-- Index on the SecurityUser table's securityDepartmentId field.
CREATE INDEX "I_SecurityUser_securityDepartmentId" ON "SecurityUser" ("securityDepartmentId");

-- Index on the SecurityUser table's securityTeamId field.
CREATE INDEX "I_SecurityUser_securityTeamId" ON "SecurityUser" ("securityTeamId");

-- Index on the SecurityUser table's authenticationToken field.
CREATE INDEX "I_SecurityUser_authenticationToken" ON "SecurityUser" ("authenticationToken");

-- Index on the SecurityUser table's objectGuid field.
CREATE INDEX "I_SecurityUser_objectGuid" ON "SecurityUser" ("objectGuid");

-- Index on the SecurityUser table's active field.
CREATE INDEX "I_SecurityUser_active" ON "SecurityUser" ("active");

-- Index on the SecurityUser table's deleted field.
CREATE INDEX "I_SecurityUser_deleted" ON "SecurityUser" ("deleted");

-- Index on the SecurityUser table's accountName,activeDirectoryAccount,active,deleted fields.
CREATE INDEX "I_Scurtysr_ccuntNm_ctvDrctryccunt_ctv_dltd" ON "SecurityUser" ("accountName", "activeDirectoryAccount", "active", "deleted");

-- Index on the SecurityUser table's id,active,deleted fields.
CREATE INDEX "I_SecurityUser_id_active_deleted" ON "SecurityUser" ("id", "active", "deleted");

INSERT INTO "SecurityUser" ( "accountName", "activeDirectoryAccount", "canLogin", "mustChangePassword", "firstName", "lastName", "password", "description", "readPermissionLevel", "writePermissionLevel", "objectGuid" ) VALUES  ( 'Admin', 0, 1, 1, 'Admin', 'User', '$HASH$V1000$10000$7lx52j0Z5CjBUyu8L84pOmsOo+jNH/pVZ1VlI4EBjAftRag+', 'Refer to generator for default password.', 255, 255, '3b1cabc6-472c-4cef-b831-b4b2052e4c10' );


CREATE TABLE "SecurityTenantUser"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityTenantId" INTEGER NOT NULL,		-- Link to the SecurityTenant table.
	"securityUserId" INTEGER NOT NULL,		-- Link to the SecurityUser table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityTenantId") REFERENCES "SecurityTenant"("id"),		-- Foreign key to the SecurityTenant table.
	FOREIGN KEY ("securityUserId") REFERENCES "SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	UNIQUE ( "securityTenantId", "securityUserId") 		-- Uniqueness enforced on the SecurityTenantUser table's securityTenantId and securityUserId fields.
);
-- Index on the SecurityTenantUser table's securityTenantId field.
CREATE INDEX "I_SecurityTenantUser_securityTenantId" ON "SecurityTenantUser" ("securityTenantId");

-- Index on the SecurityTenantUser table's securityUserId field.
CREATE INDEX "I_SecurityTenantUser_securityUserId" ON "SecurityTenantUser" ("securityUserId");

-- Index on the SecurityTenantUser table's active field.
CREATE INDEX "I_SecurityTenantUser_active" ON "SecurityTenantUser" ("active");

-- Index on the SecurityTenantUser table's deleted field.
CREATE INDEX "I_SecurityTenantUser_deleted" ON "SecurityTenantUser" ("deleted");

-- Index on the SecurityTenantUser table's id,active,deleted fields.
CREATE INDEX "I_SecurityTenantUser_id_active_deleted" ON "SecurityTenantUser" ("id", "active", "deleted");


CREATE TABLE "SecurityOrganizationUser"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityOrganizationId" INTEGER NOT NULL,		-- Link to the SecurityOrganization table.
	"securityUserId" INTEGER NOT NULL,		-- Link to the SecurityUser table.
	"canRead" BIT NOT NULL DEFAULT 0,
	"canWrite" BIT NOT NULL DEFAULT 0,
	"canChangeHierarchy" BIT NOT NULL DEFAULT 0,
	"canChangeOwner" BIT NOT NULL DEFAULT 0,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityOrganizationId") REFERENCES "SecurityOrganization"("id"),		-- Foreign key to the SecurityOrganization table.
	FOREIGN KEY ("securityUserId") REFERENCES "SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	UNIQUE ( "securityOrganizationId", "securityUserId") 		-- Uniqueness enforced on the SecurityOrganizationUser table's securityOrganizationId and securityUserId fields.
);
-- Index on the SecurityOrganizationUser table's securityOrganizationId field.
CREATE INDEX "I_SecurityOrganizationUser_securityOrganizationId" ON "SecurityOrganizationUser" ("securityOrganizationId");

-- Index on the SecurityOrganizationUser table's securityUserId field.
CREATE INDEX "I_SecurityOrganizationUser_securityUserId" ON "SecurityOrganizationUser" ("securityUserId");

-- Index on the SecurityOrganizationUser table's canRead field.
CREATE INDEX "I_SecurityOrganizationUser_canRead" ON "SecurityOrganizationUser" ("canRead");

-- Index on the SecurityOrganizationUser table's canWrite field.
CREATE INDEX "I_SecurityOrganizationUser_canWrite" ON "SecurityOrganizationUser" ("canWrite");

-- Index on the SecurityOrganizationUser table's canChangeHierarchy field.
CREATE INDEX "I_SecurityOrganizationUser_canChangeHierarchy" ON "SecurityOrganizationUser" ("canChangeHierarchy");

-- Index on the SecurityOrganizationUser table's canChangeOwner field.
CREATE INDEX "I_SecurityOrganizationUser_canChangeOwner" ON "SecurityOrganizationUser" ("canChangeOwner");

-- Index on the SecurityOrganizationUser table's active field.
CREATE INDEX "I_SecurityOrganizationUser_active" ON "SecurityOrganizationUser" ("active");

-- Index on the SecurityOrganizationUser table's deleted field.
CREATE INDEX "I_SecurityOrganizationUser_deleted" ON "SecurityOrganizationUser" ("deleted");

-- Index on the SecurityOrganizationUser table's id,active,deleted fields.
CREATE INDEX "I_SecurityOrganizationUser_id_active_deleted" ON "SecurityOrganizationUser" ("id", "active", "deleted");


CREATE TABLE "SecurityDepartmentUser"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityDepartmentId" INTEGER NOT NULL,		-- Link to the SecurityDepartment table.
	"securityUserId" INTEGER NOT NULL,		-- Link to the SecurityUser table.
	"canRead" BIT NOT NULL DEFAULT 0,
	"canWrite" BIT NOT NULL DEFAULT 0,
	"canChangeHierarchy" BIT NOT NULL DEFAULT 0,
	"canChangeOwner" BIT NOT NULL DEFAULT 0,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityDepartmentId") REFERENCES "SecurityDepartment"("id"),		-- Foreign key to the SecurityDepartment table.
	FOREIGN KEY ("securityUserId") REFERENCES "SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	UNIQUE ( "securityDepartmentId", "securityUserId") 		-- Uniqueness enforced on the SecurityDepartmentUser table's securityDepartmentId and securityUserId fields.
);
-- Index on the SecurityDepartmentUser table's securityDepartmentId field.
CREATE INDEX "I_SecurityDepartmentUser_securityDepartmentId" ON "SecurityDepartmentUser" ("securityDepartmentId");

-- Index on the SecurityDepartmentUser table's securityUserId field.
CREATE INDEX "I_SecurityDepartmentUser_securityUserId" ON "SecurityDepartmentUser" ("securityUserId");

-- Index on the SecurityDepartmentUser table's canRead field.
CREATE INDEX "I_SecurityDepartmentUser_canRead" ON "SecurityDepartmentUser" ("canRead");

-- Index on the SecurityDepartmentUser table's canWrite field.
CREATE INDEX "I_SecurityDepartmentUser_canWrite" ON "SecurityDepartmentUser" ("canWrite");

-- Index on the SecurityDepartmentUser table's canChangeHierarchy field.
CREATE INDEX "I_SecurityDepartmentUser_canChangeHierarchy" ON "SecurityDepartmentUser" ("canChangeHierarchy");

-- Index on the SecurityDepartmentUser table's canChangeOwner field.
CREATE INDEX "I_SecurityDepartmentUser_canChangeOwner" ON "SecurityDepartmentUser" ("canChangeOwner");

-- Index on the SecurityDepartmentUser table's active field.
CREATE INDEX "I_SecurityDepartmentUser_active" ON "SecurityDepartmentUser" ("active");

-- Index on the SecurityDepartmentUser table's deleted field.
CREATE INDEX "I_SecurityDepartmentUser_deleted" ON "SecurityDepartmentUser" ("deleted");

-- Index on the SecurityDepartmentUser table's id,active,deleted fields.
CREATE INDEX "I_SecurityDepartmentUser_id_active_deleted" ON "SecurityDepartmentUser" ("id", "active", "deleted");


CREATE TABLE "SecurityTeamUser"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityTeamId" INTEGER NOT NULL,		-- Link to the SecurityTeam table.
	"securityUserId" INTEGER NOT NULL,		-- Link to the SecurityUser table.
	"canRead" BIT NOT NULL DEFAULT 0,
	"canWrite" BIT NOT NULL DEFAULT 0,
	"canChangeHierarchy" BIT NOT NULL DEFAULT 0,
	"canChangeOwner" BIT NOT NULL DEFAULT 0,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityTeamId") REFERENCES "SecurityTeam"("id"),		-- Foreign key to the SecurityTeam table.
	FOREIGN KEY ("securityUserId") REFERENCES "SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	UNIQUE ( "securityTeamId", "securityUserId") 		-- Uniqueness enforced on the SecurityTeamUser table's securityTeamId and securityUserId fields.
);
-- Index on the SecurityTeamUser table's securityTeamId field.
CREATE INDEX "I_SecurityTeamUser_securityTeamId" ON "SecurityTeamUser" ("securityTeamId");

-- Index on the SecurityTeamUser table's securityUserId field.
CREATE INDEX "I_SecurityTeamUser_securityUserId" ON "SecurityTeamUser" ("securityUserId");

-- Index on the SecurityTeamUser table's canRead field.
CREATE INDEX "I_SecurityTeamUser_canRead" ON "SecurityTeamUser" ("canRead");

-- Index on the SecurityTeamUser table's canWrite field.
CREATE INDEX "I_SecurityTeamUser_canWrite" ON "SecurityTeamUser" ("canWrite");

-- Index on the SecurityTeamUser table's canChangeHierarchy field.
CREATE INDEX "I_SecurityTeamUser_canChangeHierarchy" ON "SecurityTeamUser" ("canChangeHierarchy");

-- Index on the SecurityTeamUser table's canChangeOwner field.
CREATE INDEX "I_SecurityTeamUser_canChangeOwner" ON "SecurityTeamUser" ("canChangeOwner");

-- Index on the SecurityTeamUser table's active field.
CREATE INDEX "I_SecurityTeamUser_active" ON "SecurityTeamUser" ("active");

-- Index on the SecurityTeamUser table's deleted field.
CREATE INDEX "I_SecurityTeamUser_deleted" ON "SecurityTeamUser" ("deleted");

-- Index on the SecurityTeamUser table's id,active,deleted fields.
CREATE INDEX "I_SecurityTeamUser_id_active_deleted" ON "SecurityTeamUser" ("id", "active", "deleted");


CREATE TABLE "SecurityUserEventType"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE
);
-- Index on the SecurityUserEventType table's name field.
CREATE INDEX "I_SecurityUserEventType_name" ON "SecurityUserEventType" ("name");

INSERT INTO "SecurityUserEventType" ( "name", "description" ) VALUES  ( 'LoginSuccess', 'Login Success' );

INSERT INTO "SecurityUserEventType" ( "name", "description" ) VALUES  ( 'LoginFailure', 'Login Failure' );

INSERT INTO "SecurityUserEventType" ( "name", "description" ) VALUES  ( 'LoginAttemptDuringCooldown', 'Login Attempt During Cooldown' );

INSERT INTO "SecurityUserEventType" ( "name", "description" ) VALUES  ( 'Logout', 'Logout' );

INSERT INTO "SecurityUserEventType" ( "name", "description" ) VALUES  ( 'TwoFactorSend', 'TwoFactorSend' );

INSERT INTO "SecurityUserEventType" ( "name", "description" ) VALUES  ( 'Miscellaneous', 'Miscellaneous' );

INSERT INTO "SecurityUserEventType" ( "name", "description" ) VALUES  ( 'AccountInactivated', 'AccountInactivated' );

INSERT INTO "SecurityUserEventType" ( "name", "description" ) VALUES  ( 'UserInitiatedPasswordResetRequest', 'UserInitiatedPasswordResetRequest' );

INSERT INTO "SecurityUserEventType" ( "name", "description" ) VALUES  ( 'UserInitiatedPasswordResetCompleted', 'UserInitiatedPasswordResetCompleted' );

INSERT INTO "SecurityUserEventType" ( "name", "description" ) VALUES  ( 'SystemInitiatedPasswordResetRequest', 'SystemInitiatedPasswordResetRequest' );

INSERT INTO "SecurityUserEventType" ( "name", "description" ) VALUES  ( 'SystemInitiatedPasswordResetCompleted', 'SystemInitiatedPasswordResetCompleted' );


CREATE TABLE "SecurityUserEvent"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityUserId" INTEGER NOT NULL,		-- Link to the SecurityUser table.
	"securityUserEventTypeId" INTEGER NOT NULL,		-- Link to the SecurityUserEventType table.
	"timeStamp" DATETIME NOT NULL,
	"comments" VARCHAR(1000) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityUserId") REFERENCES "SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	FOREIGN KEY ("securityUserEventTypeId") REFERENCES "SecurityUserEventType"("id")		-- Foreign key to the SecurityUserEventType table.
);
-- Index on the SecurityUserEvent table's securityUserId field.
CREATE INDEX "I_SecurityUserEvent_securityUserId" ON "SecurityUserEvent" ("securityUserId");

-- Index on the SecurityUserEvent table's securityUserEventTypeId field.
CREATE INDEX "I_SecurityUserEvent_securityUserEventTypeId" ON "SecurityUserEvent" ("securityUserEventTypeId");

-- Index on the SecurityUserEvent table's timeStamp field.
CREATE INDEX "I_SecurityUserEvent_timeStamp" ON "SecurityUserEvent" ("timeStamp");

-- Index on the SecurityUserEvent table's active field.
CREATE INDEX "I_SecurityUserEvent_active" ON "SecurityUserEvent" ("active");

-- Index on the SecurityUserEvent table's deleted field.
CREATE INDEX "I_SecurityUserEvent_deleted" ON "SecurityUserEvent" ("deleted");

-- Index on the SecurityUserEvent table's id,active,deleted fields.
CREATE INDEX "I_SecurityUserEvent_id_active_deleted" ON "SecurityUserEvent" ("id", "active", "deleted");


CREATE TABLE "SecurityUserPasswordResetToken"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityUserId" INTEGER NOT NULL,		-- Link to the SecurityUser table.
	"token" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- The token to use for this password reset request
	"timeStamp" DATETIME NOT NULL,		-- The point in time when this request was created.
	"expiry" DATETIME NOT NULL,		-- The expiry time for this password reset request
	"systemInitiated" BIT NOT NULL DEFAULT 0,		-- Whether or not this token reset process was system initiated or not
	"completed" BIT NOT NULL DEFAULT 0,		-- Whether or not this token reset process is completed
	"comments" VARCHAR(1000) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityUserId") REFERENCES "SecurityUser"("id")		-- Foreign key to the SecurityUser table.
);
-- Index on the SecurityUserPasswordResetToken table's securityUserId field.
CREATE INDEX "I_SecurityUserPasswordResetToken_securityUserId" ON "SecurityUserPasswordResetToken" ("securityUserId");

-- Index on the SecurityUserPasswordResetToken table's token field.
CREATE INDEX "I_SecurityUserPasswordResetToken_token" ON "SecurityUserPasswordResetToken" ("token");

-- Index on the SecurityUserPasswordResetToken table's timeStamp field.
CREATE INDEX "I_SecurityUserPasswordResetToken_timeStamp" ON "SecurityUserPasswordResetToken" ("timeStamp");

-- Index on the SecurityUserPasswordResetToken table's expiry field.
CREATE INDEX "I_SecurityUserPasswordResetToken_expiry" ON "SecurityUserPasswordResetToken" ("expiry");

-- Index on the SecurityUserPasswordResetToken table's systemInitiated field.
CREATE INDEX "I_SecurityUserPasswordResetToken_systemInitiated" ON "SecurityUserPasswordResetToken" ("systemInitiated");

-- Index on the SecurityUserPasswordResetToken table's completed field.
CREATE INDEX "I_SecurityUserPasswordResetToken_completed" ON "SecurityUserPasswordResetToken" ("completed");

-- Index on the SecurityUserPasswordResetToken table's active field.
CREATE INDEX "I_SecurityUserPasswordResetToken_active" ON "SecurityUserPasswordResetToken" ("active");

-- Index on the SecurityUserPasswordResetToken table's deleted field.
CREATE INDEX "I_SecurityUserPasswordResetToken_deleted" ON "SecurityUserPasswordResetToken" ("deleted");

-- Index on the SecurityUserPasswordResetToken table's id,active,deleted fields.
CREATE INDEX "I_SecurityUserPasswordResetToken_id_active_deleted" ON "SecurityUserPasswordResetToken" ("id", "active", "deleted");


CREATE TABLE "SecurityGroup"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the SecurityGroup table's name field.
CREATE INDEX "I_SecurityGroup_name" ON "SecurityGroup" ("name");

-- Index on the SecurityGroup table's active field.
CREATE INDEX "I_SecurityGroup_active" ON "SecurityGroup" ("active");

-- Index on the SecurityGroup table's deleted field.
CREATE INDEX "I_SecurityGroup_deleted" ON "SecurityGroup" ("deleted");

-- Index on the SecurityGroup table's id,active,deleted fields.
CREATE INDEX "I_SecurityGroup_id_active_deleted" ON "SecurityGroup" ("id", "active", "deleted");


CREATE TABLE "SecurityUserSecurityGroup"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityUserId" INTEGER NOT NULL,		-- Link to the SecurityUser table.
	"securityGroupId" INTEGER NOT NULL,		-- Link to the SecurityGroup table.
	"comments" VARCHAR(1000) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityUserId") REFERENCES "SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	FOREIGN KEY ("securityGroupId") REFERENCES "SecurityGroup"("id"),		-- Foreign key to the SecurityGroup table.
	UNIQUE ( "securityUserId", "securityGroupId") 		-- Uniqueness enforced on the SecurityUserSecurityGroup table's securityUserId and securityGroupId fields.
);
-- Index on the SecurityUserSecurityGroup table's securityUserId field.
CREATE INDEX "I_SecurityUserSecurityGroup_securityUserId" ON "SecurityUserSecurityGroup" ("securityUserId");

-- Index on the SecurityUserSecurityGroup table's securityGroupId field.
CREATE INDEX "I_SecurityUserSecurityGroup_securityGroupId" ON "SecurityUserSecurityGroup" ("securityGroupId");

-- Index on the SecurityUserSecurityGroup table's active field.
CREATE INDEX "I_SecurityUserSecurityGroup_active" ON "SecurityUserSecurityGroup" ("active");

-- Index on the SecurityUserSecurityGroup table's deleted field.
CREATE INDEX "I_SecurityUserSecurityGroup_deleted" ON "SecurityUserSecurityGroup" ("deleted");

-- Index on the SecurityUserSecurityGroup table's id,active,deleted fields.
CREATE INDEX "I_SecurityUserSecurityGroup_id_active_deleted" ON "SecurityUserSecurityGroup" ("id", "active", "deleted");


CREATE TABLE "Privilege"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE
);
-- Index on the Privilege table's name field.
CREATE INDEX "I_Privilege_name" ON "Privilege" ("name");

INSERT INTO "Privilege" ( "name", "description" ) VALUES  ( 'No Access', 'No Access' );

INSERT INTO "Privilege" ( "name", "description" ) VALUES  ( 'Anonymous Read Only', 'Read Only Access, With All Sensitive Data Redacted' );

INSERT INTO "Privilege" ( "name", "description" ) VALUES  ( 'Read Only', 'Read Only Access For General Use' );

INSERT INTO "Privilege" ( "name", "description" ) VALUES  ( 'Read and Write', 'Read and Write Access' );

INSERT INTO "Privilege" ( "name", "description" ) VALUES  ( 'Administrative', 'Complete Administrative Access' );

INSERT INTO "Privilege" ( "name", "description" ) VALUES  ( 'Custom', 'Custom Access Level' );


CREATE TABLE "SecurityRole"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"privilegeId" INTEGER NOT NULL,		-- Link to the Privilege table.
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"comments" VARCHAR(1000) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("privilegeId") REFERENCES "Privilege"("id")		-- Foreign key to the Privilege table.
);
-- Index on the SecurityRole table's privilegeId field.
CREATE INDEX "I_SecurityRole_privilegeId" ON "SecurityRole" ("privilegeId");

-- Index on the SecurityRole table's name field.
CREATE INDEX "I_SecurityRole_name" ON "SecurityRole" ("name");

-- Index on the SecurityRole table's active field.
CREATE INDEX "I_SecurityRole_active" ON "SecurityRole" ("active");

-- Index on the SecurityRole table's deleted field.
CREATE INDEX "I_SecurityRole_deleted" ON "SecurityRole" ("deleted");

-- Index on the SecurityRole table's id,active,deleted fields.
CREATE INDEX "I_SecurityRole_id_active_deleted" ON "SecurityRole" ("id", "active", "deleted");


CREATE TABLE "SecurityUserSecurityRole"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityUserId" INTEGER NOT NULL,		-- Link to the SecurityUser table.
	"securityRoleId" INTEGER NOT NULL,		-- Link to the SecurityRole table.
	"comments" VARCHAR(1000) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityUserId") REFERENCES "SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	FOREIGN KEY ("securityRoleId") REFERENCES "SecurityRole"("id"),		-- Foreign key to the SecurityRole table.
	UNIQUE ( "securityUserId", "securityRoleId") 		-- Uniqueness enforced on the SecurityUserSecurityRole table's securityUserId and securityRoleId fields.
);
-- Index on the SecurityUserSecurityRole table's securityUserId field.
CREATE INDEX "I_SecurityUserSecurityRole_securityUserId" ON "SecurityUserSecurityRole" ("securityUserId");

-- Index on the SecurityUserSecurityRole table's securityRoleId field.
CREATE INDEX "I_SecurityUserSecurityRole_securityRoleId" ON "SecurityUserSecurityRole" ("securityRoleId");

-- Index on the SecurityUserSecurityRole table's active field.
CREATE INDEX "I_SecurityUserSecurityRole_active" ON "SecurityUserSecurityRole" ("active");

-- Index on the SecurityUserSecurityRole table's deleted field.
CREATE INDEX "I_SecurityUserSecurityRole_deleted" ON "SecurityUserSecurityRole" ("deleted");

-- Index on the SecurityUserSecurityRole table's id,active,deleted fields.
CREATE INDEX "I_SecurityUserSecurityRole_id_active_deleted" ON "SecurityUserSecurityRole" ("id", "active", "deleted");

-- Index on the SecurityUserSecurityRole table's securityUserId,active,deleted fields.
CREATE INDEX "I_SecurityUserSecurityRole_securityUserId_active_deleted" ON "SecurityUserSecurityRole" ("securityUserId", "active", "deleted");

-- Index on the SecurityUserSecurityRole table's securityRoleId,active,deleted fields.
CREATE INDEX "I_SecurityUserSecurityRole_securityRoleId_active_deleted" ON "SecurityUserSecurityRole" ("securityRoleId", "active", "deleted");


CREATE TABLE "SecurityGroupSecurityRole"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityGroupId" INTEGER NOT NULL,		-- Link to the SecurityGroup table.
	"securityRoleId" INTEGER NOT NULL,		-- Link to the SecurityRole table.
	"comments" VARCHAR(1000) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityGroupId") REFERENCES "SecurityGroup"("id"),		-- Foreign key to the SecurityGroup table.
	FOREIGN KEY ("securityRoleId") REFERENCES "SecurityRole"("id"),		-- Foreign key to the SecurityRole table.
	UNIQUE ( "securityGroupId", "securityRoleId") 		-- Uniqueness enforced on the SecurityGroupSecurityRole table's securityGroupId and securityRoleId fields.
);
-- Index on the SecurityGroupSecurityRole table's securityGroupId field.
CREATE INDEX "I_SecurityGroupSecurityRole_securityGroupId" ON "SecurityGroupSecurityRole" ("securityGroupId");

-- Index on the SecurityGroupSecurityRole table's securityRoleId field.
CREATE INDEX "I_SecurityGroupSecurityRole_securityRoleId" ON "SecurityGroupSecurityRole" ("securityRoleId");

-- Index on the SecurityGroupSecurityRole table's active field.
CREATE INDEX "I_SecurityGroupSecurityRole_active" ON "SecurityGroupSecurityRole" ("active");

-- Index on the SecurityGroupSecurityRole table's deleted field.
CREATE INDEX "I_SecurityGroupSecurityRole_deleted" ON "SecurityGroupSecurityRole" ("deleted");

-- Index on the SecurityGroupSecurityRole table's id,active,deleted fields.
CREATE INDEX "I_SecurityGroupSecurityRole_id_active_deleted" ON "SecurityGroupSecurityRole" ("id", "active", "deleted");

-- Index on the SecurityGroupSecurityRole table's securityGroupId,active,deleted fields.
CREATE INDEX "I_SecurityGroupSecurityRole_securityGroupId_active_deleted" ON "SecurityGroupSecurityRole" ("securityGroupId", "active", "deleted");

-- Index on the SecurityGroupSecurityRole table's securityRoleId,active,deleted fields.
CREATE INDEX "I_SecurityGroupSecurityRole_securityRoleId_active_deleted" ON "SecurityGroupSecurityRole" ("securityRoleId", "active", "deleted");


CREATE TABLE "Module"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the Module table's name field.
CREATE INDEX "I_Module_name" ON "Module" ("name");

-- Index on the Module table's active field.
CREATE INDEX "I_Module_active" ON "Module" ("active");

-- Index on the Module table's deleted field.
CREATE INDEX "I_Module_deleted" ON "Module" ("deleted");

-- Index on the Module table's id,active,deleted fields.
CREATE INDEX "I_Module_id_active_deleted" ON "Module" ("id", "active", "deleted");


CREATE TABLE "ModuleSecurityRole"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"moduleId" INTEGER NOT NULL,		-- Link to the Module table.
	"securityRoleId" INTEGER NOT NULL,		-- Link to the SecurityRole table.
	"comments" VARCHAR(1000) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("moduleId") REFERENCES "Module"("id"),		-- Foreign key to the Module table.
	FOREIGN KEY ("securityRoleId") REFERENCES "SecurityRole"("id"),		-- Foreign key to the SecurityRole table.
	UNIQUE ( "moduleId", "securityRoleId") 		-- Uniqueness enforced on the ModuleSecurityRole table's moduleId and securityRoleId fields.
);
-- Index on the ModuleSecurityRole table's moduleId field.
CREATE INDEX "I_ModuleSecurityRole_moduleId" ON "ModuleSecurityRole" ("moduleId");

-- Index on the ModuleSecurityRole table's securityRoleId field.
CREATE INDEX "I_ModuleSecurityRole_securityRoleId" ON "ModuleSecurityRole" ("securityRoleId");

-- Index on the ModuleSecurityRole table's active field.
CREATE INDEX "I_ModuleSecurityRole_active" ON "ModuleSecurityRole" ("active");

-- Index on the ModuleSecurityRole table's deleted field.
CREATE INDEX "I_ModuleSecurityRole_deleted" ON "ModuleSecurityRole" ("deleted");

-- Index on the ModuleSecurityRole table's id,active,deleted fields.
CREATE INDEX "I_ModuleSecurityRole_id_active_deleted" ON "ModuleSecurityRole" ("id", "active", "deleted");

-- Index on the ModuleSecurityRole table's securityRoleId,active,deleted fields.
CREATE INDEX "I_ModuleSecurityRole_securityRoleId_active_deleted" ON "ModuleSecurityRole" ("securityRoleId", "active", "deleted");


CREATE TABLE "SystemSetting"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"value" TEXT NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the SystemSetting table's name field.
CREATE INDEX "I_SystemSetting_name" ON "SystemSetting" ("name");

-- Index on the SystemSetting table's active field.
CREATE INDEX "I_SystemSetting_active" ON "SystemSetting" ("active");

-- Index on the SystemSetting table's deleted field.
CREATE INDEX "I_SystemSetting_deleted" ON "SystemSetting" ("deleted");

-- Index on the SystemSetting table's id,active,deleted fields.
CREATE INDEX "I_SystemSetting_id_active_deleted" ON "SystemSetting" ("id", "active", "deleted");


CREATE TABLE "LoginAttempt"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"timeStamp" DATETIME NOT NULL,
	"userName" VARCHAR(250) NULL COLLATE NOCASE,
	"passwordHash" INTEGER NULL,
	"resource" VARCHAR(500) NULL COLLATE NOCASE,
	"sessionId" VARCHAR(50) NULL COLLATE NOCASE,
	"ipAddress" VARCHAR(50) NULL COLLATE NOCASE,
	"userAgent" VARCHAR(250) NULL COLLATE NOCASE,
	"value" TEXT NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the LoginAttempt table's active field.
CREATE INDEX "I_LoginAttempt_active" ON "LoginAttempt" ("active");

-- Index on the LoginAttempt table's deleted field.
CREATE INDEX "I_LoginAttempt_deleted" ON "LoginAttempt" ("deleted");

-- Index on the LoginAttempt table's id,active,deleted fields.
CREATE INDEX "I_loginAttempt_id_active_deleted" ON "LoginAttempt" ("id", "active", "deleted");


CREATE TABLE "EntityDataToken"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"securityUserId" INTEGER NOT NULL,		-- Link to the SecurityUser table.
	"moduleId" INTEGER NOT NULL,		-- Link to the Module table.
	"entity" VARCHAR(250) NOT NULL COLLATE NOCASE,
	"sessionId" VARCHAR(50) NOT NULL COLLATE NOCASE,
	"authenticationToken" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- This is the authentication token that gets set into the user data of the forms authentication ticket
	"token" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,
	"timeStamp" DATETIME NOT NULL,
	"comments" VARCHAR(1000) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("securityUserId") REFERENCES "SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	FOREIGN KEY ("moduleId") REFERENCES "Module"("id")		-- Foreign key to the Module table.
);
-- Index on the EntityDataToken table's securityUserId field.
CREATE INDEX "I_EntityDataToken_securityUserId" ON "EntityDataToken" ("securityUserId");

-- Index on the EntityDataToken table's moduleId field.
CREATE INDEX "I_EntityDataToken_moduleId" ON "EntityDataToken" ("moduleId");

-- Index on the EntityDataToken table's active field.
CREATE INDEX "I_EntityDataToken_active" ON "EntityDataToken" ("active");

-- Index on the EntityDataToken table's deleted field.
CREATE INDEX "I_EntityDataToken_deleted" ON "EntityDataToken" ("deleted");

-- Index on the EntityDataToken table's token field.
CREATE INDEX "I_EntityDataToken_token" ON "EntityDataToken" ("token");

-- Index on the EntityDataToken table's securityUserId,moduleId,sessionId fields.
CREATE INDEX "I_EntityDataToken_securityUserId_moduleId_sessionId" ON "EntityDataToken" ("securityUserId", "moduleId", "sessionId");

-- Index on the EntityDataToken table's securityUserId,moduleId,token,sessionId fields.
CREATE INDEX "I_EntityDataToken_securityUserId_moduleId_token_sessionId" ON "EntityDataToken" ("securityUserId", "moduleId", "token", "sessionId");

-- Index on the EntityDataToken table's id,active,deleted fields.
CREATE INDEX "I_EntityDataToken_id_active_deleted" ON "EntityDataToken" ("id", "active", "deleted");


CREATE TABLE "EntityDataTokenEventType"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE
);
-- Index on the EntityDataTokenEventType table's name field.
CREATE INDEX "I_EntityDataTokenEventType_name" ON "EntityDataTokenEventType" ("name");

INSERT INTO "EntityDataTokenEventType" ( "name", "description" ) VALUES  ( 'ReadFromEntity', 'Read From Entity' );

INSERT INTO "EntityDataTokenEventType" ( "name", "description" ) VALUES  ( 'CascadeValidatedReadFromEntity', 'Cascade Validated Read From Entity' );

INSERT INTO "EntityDataTokenEventType" ( "name", "description" ) VALUES  ( 'WriteToEntity', 'Write To Entity' );

INSERT INTO "EntityDataTokenEventType" ( "name", "description" ) VALUES  ( 'CascadeValidatedWriteToEntity', 'Cascade Validated Write To Entity' );

INSERT INTO "EntityDataTokenEventType" ( "name", "description" ) VALUES  ( 'ReuseExistingToken', 'Reuse Existing Token' );


CREATE TABLE "EntityDataTokenEvent"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"entityDataTokenId" INTEGER NOT NULL,		-- Link to the EntityDataToken table.
	"entityDataTokenEventTypeId" INTEGER NOT NULL,		-- Link to the EntityDataTokenEventType table.
	"timeStamp" DATETIME NOT NULL,
	"comments" VARCHAR(1000) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("entityDataTokenId") REFERENCES "EntityDataToken"("id"),		-- Foreign key to the EntityDataToken table.
	FOREIGN KEY ("entityDataTokenEventTypeId") REFERENCES "EntityDataTokenEventType"("id")		-- Foreign key to the EntityDataTokenEventType table.
);
-- Index on the EntityDataTokenEvent table's entityDataTokenId field.
CREATE INDEX "I_EntityDataTokenEvent_entityDataTokenId" ON "EntityDataTokenEvent" ("entityDataTokenId");

-- Index on the EntityDataTokenEvent table's entityDataTokenEventTypeId field.
CREATE INDEX "I_EntityDataTokenEvent_entityDataTokenEventTypeId" ON "EntityDataTokenEvent" ("entityDataTokenEventTypeId");

-- Index on the EntityDataTokenEvent table's active field.
CREATE INDEX "I_EntityDataTokenEvent_active" ON "EntityDataTokenEvent" ("active");

-- Index on the EntityDataTokenEvent table's deleted field.
CREATE INDEX "I_EntityDataTokenEvent_deleted" ON "EntityDataTokenEvent" ("deleted");

-- Index on the EntityDataTokenEvent table's id,active,deleted fields.
CREATE INDEX "I_EntityDataTokenEvent_id_active_deleted" ON "EntityDataTokenEvent" ("id", "active", "deleted");


CREATE TABLE "OAUTHToken"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"token" VARCHAR(250) NOT NULL COLLATE NOCASE,
	"expiryDateTime" DATETIME NOT NULL,
	"userData" VARCHAR(1000) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the OAUTHToken table's token field.
CREATE INDEX "I_OAUTHToken_token" ON "OAUTHToken" ("token");

-- Index on the OAUTHToken table's expiryDateTime field.
CREATE INDEX "I_OAUTHToken_expiryDateTime" ON "OAUTHToken" ("expiryDateTime");

-- Index on the OAUTHToken table's active field.
CREATE INDEX "I_OAUTHToken_active" ON "OAUTHToken" ("active");

-- Index on the OAUTHToken table's deleted field.
CREATE INDEX "I_OAUTHToken_deleted" ON "OAUTHToken" ("deleted");

-- Index on the OAUTHToken table's id,active,deleted fields.
CREATE INDEX "I_OauthToken_id_active_deleted" ON "OAUTHToken" ("id", "active", "deleted");


