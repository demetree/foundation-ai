CREATE DATABASE "Security"
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    TEMPLATE=template0;

\connect "Security"   -- Run the create database first, then connect to it, and run the rest if running in a query tool rather than through psql scripting.



CREATE SCHEMA "Security"

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE "Security"."UserSession"
-- DROP TABLE "Security"."OAUTHToken"
-- DROP TABLE "Security"."EntityDataTokenEvent"
-- DROP TABLE "Security"."EntityDataTokenEventType"
-- DROP TABLE "Security"."EntityDataToken"
-- DROP TABLE "Security"."LoginAttempt"
-- DROP TABLE "Security"."SystemSetting"
-- DROP TABLE "Security"."ModuleSecurityRole"
-- DROP TABLE "Security"."Module"
-- DROP TABLE "Security"."SecurityGroupSecurityRole"
-- DROP TABLE "Security"."SecurityUserSecurityRole"
-- DROP TABLE "Security"."SecurityRole"
-- DROP TABLE "Security"."Privilege"
-- DROP TABLE "Security"."SecurityUserSecurityGroup"
-- DROP TABLE "Security"."SecurityGroup"
-- DROP TABLE "Security"."SecurityUserPasswordResetToken"
-- DROP TABLE "Security"."SecurityUserEvent"
-- DROP TABLE "Security"."SecurityUserEventType"
-- DROP TABLE "Security"."SecurityTeamUser"
-- DROP TABLE "Security"."SecurityDepartmentUser"
-- DROP TABLE "Security"."SecurityOrganizationUser"
-- DROP TABLE "Security"."SecurityTenantUser"
-- DROP TABLE "Security"."SecurityUser"
-- DROP TABLE "Security"."SecurityUserTitle"
-- DROP TABLE "Security"."SecurityTeam"
-- DROP TABLE "Security"."SecurityDepartment"
-- DROP TABLE "Security"."SecurityOrganization"
-- DROP TABLE "Security"."SecurityTenant"

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON "UserSession" DISABLE
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
-- ALTER INDEX ALL ON "UserSession" REBUILD
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

CREATE TABLE "Security"."SecurityTenant"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL,
	"settings" TEXT NULL,		-- To store a JSON object containing arbitrary tenant settings.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the SecurityTenant table's name field.
CREATE INDEX "I_SecurityTenant_name" ON "Security"."SecurityTenant" ("name")
;

-- Index on the SecurityTenant table's active field.
CREATE INDEX "I_SecurityTenant_active" ON "Security"."SecurityTenant" ("active")
;

-- Index on the SecurityTenant table's deleted field.
CREATE INDEX "I_SecurityTenant_deleted" ON "Security"."SecurityTenant" ("deleted")
;

-- Index on the SecurityTenant table's id,active,deleted fields.
CREATE INDEX "I_SecurityTenant_id_active_deleted" ON "Security"."SecurityTenant" ("id", "active", "deleted")
;

INSERT INTO "Security"."SecurityTenant" ( "name", "description", "active", "deleted", "objectGuid" ) VALUES  ( 'System Service', 'System Service Tenant - For Administrative purposes or single tenant use.', true, false, 'c017cf97-ccbb-4686-98b3-c59efc1a3f45' );


CREATE TABLE "Security"."SecurityOrganization"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityTenantId" INT NOT NULL,		-- Link to the SecurityTenant table.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityTenantId" FOREIGN KEY ("securityTenantId") REFERENCES "Security"."SecurityTenant"("id"),		-- Foreign key to the SecurityTenant table.
	CONSTRAINT "UC_SecurityOrganization_securityTenantId_name" UNIQUE ( "securityTenantId", "name") 		-- Uniqueness enforced on the SecurityOrganization table's securityTenantId and name fields.
);
-- Index on the SecurityOrganization table's securityTenantId field.
CREATE INDEX "I_SecurityOrganization_securityTenantId" ON "Security"."SecurityOrganization" ("securityTenantId")
;

-- Index on the SecurityOrganization table's name field.
CREATE INDEX "I_SecurityOrganization_name" ON "Security"."SecurityOrganization" ("name")
;

-- Index on the SecurityOrganization table's active field.
CREATE INDEX "I_SecurityOrganization_active" ON "Security"."SecurityOrganization" ("active")
;

-- Index on the SecurityOrganization table's deleted field.
CREATE INDEX "I_SecurityOrganization_deleted" ON "Security"."SecurityOrganization" ("deleted")
;

-- Index on the SecurityOrganization table's id,active,deleted fields.
CREATE INDEX "I_SecurityOrganization_id_active_deleted" ON "Security"."SecurityOrganization" ("id", "active", "deleted")
;


CREATE TABLE "Security"."SecurityDepartment"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityOrganizationId" INT NOT NULL,		-- Link to the SecurityOrganization table.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityOrganizationId" FOREIGN KEY ("securityOrganizationId") REFERENCES "Security"."SecurityOrganization"("id"),		-- Foreign key to the SecurityOrganization table.
	CONSTRAINT "UC_SecurityDepartment_securityOrganizationId_name" UNIQUE ( "securityOrganizationId", "name") 		-- Uniqueness enforced on the SecurityDepartment table's securityOrganizationId and name fields.
);
-- Index on the SecurityDepartment table's securityOrganizationId field.
CREATE INDEX "I_SecurityDepartment_securityOrganizationId" ON "Security"."SecurityDepartment" ("securityOrganizationId")
;

-- Index on the SecurityDepartment table's name field.
CREATE INDEX "I_SecurityDepartment_name" ON "Security"."SecurityDepartment" ("name")
;

-- Index on the SecurityDepartment table's active field.
CREATE INDEX "I_SecurityDepartment_active" ON "Security"."SecurityDepartment" ("active")
;

-- Index on the SecurityDepartment table's deleted field.
CREATE INDEX "I_SecurityDepartment_deleted" ON "Security"."SecurityDepartment" ("deleted")
;

-- Index on the SecurityDepartment table's id,active,deleted fields.
CREATE INDEX "I_SecurityDepartment_id_active_deleted" ON "Security"."SecurityDepartment" ("id", "active", "deleted")
;


CREATE TABLE "Security"."SecurityTeam"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityDepartmentId" INT NOT NULL,		-- Link to the SecurityDepartment table.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityDepartmentId" FOREIGN KEY ("securityDepartmentId") REFERENCES "Security"."SecurityDepartment"("id"),		-- Foreign key to the SecurityDepartment table.
	CONSTRAINT "UC_SecurityTeam_securityDepartmentId_name" UNIQUE ( "securityDepartmentId", "name") 		-- Uniqueness enforced on the SecurityTeam table's securityDepartmentId and name fields.
);
-- Index on the SecurityTeam table's securityDepartmentId field.
CREATE INDEX "I_SecurityTeam_securityDepartmentId" ON "Security"."SecurityTeam" ("securityDepartmentId")
;

-- Index on the SecurityTeam table's name field.
CREATE INDEX "I_SecurityTeam_name" ON "Security"."SecurityTeam" ("name")
;

-- Index on the SecurityTeam table's active field.
CREATE INDEX "I_SecurityTeam_active" ON "Security"."SecurityTeam" ("active")
;

-- Index on the SecurityTeam table's deleted field.
CREATE INDEX "I_SecurityTeam_deleted" ON "Security"."SecurityTeam" ("deleted")
;

-- Index on the SecurityTeam table's id,active,deleted fields.
CREATE INDEX "I_SecurityTeam_id_active_deleted" ON "Security"."SecurityTeam" ("id", "active", "deleted")
;


CREATE TABLE "Security"."SecurityUserTitle"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the SecurityUserTitle table's name field.
CREATE INDEX "I_SecurityUserTitle_name" ON "Security"."SecurityUserTitle" ("name")
;

-- Index on the SecurityUserTitle table's active field.
CREATE INDEX "I_SecurityUserTitle_active" ON "Security"."SecurityUserTitle" ("active")
;

-- Index on the SecurityUserTitle table's deleted field.
CREATE INDEX "I_SecurityUserTitle_deleted" ON "Security"."SecurityUserTitle" ("deleted")
;

-- Index on the SecurityUserTitle table's id,active,deleted fields.
CREATE INDEX "I_SecurityUserTitle_id_active_deleted" ON "Security"."SecurityUserTitle" ("id", "active", "deleted")
;


CREATE TABLE "Security"."SecurityUser"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"accountName" VARCHAR(250) NOT NULL UNIQUE,
	"activeDirectoryAccount" BOOLEAN NOT NULL DEFAULT false,
	"password" VARCHAR(250) NULL,
	"canLogin" BOOLEAN NOT NULL DEFAULT true,		-- Whether or not the user can login.  Should be true for people, or API access accounts, and false for internal use service accounts that should never be allowed to login.
	"mustChangePassword" BOOLEAN NOT NULL DEFAULT false,		-- True if the user is required to change their password
	"firstName" VARCHAR(100) NULL,
	"middleName" VARCHAR(100) NULL,
	"lastName" VARCHAR(100) NULL,
	"dateOfBirth" TIMESTAMP NULL,
	"emailAddress" VARCHAR(100) NULL,
	"cellPhoneNumber" VARCHAR(100) NULL,
	"phoneNumber" VARCHAR(50) NULL,
	"phoneExtension" VARCHAR(50) NULL,
	"description" VARCHAR(500) NULL,
	"securityUserTitleId" INT NULL,		-- Link to the SecurityUserTitle table.
	"reportsToSecurityUserId" INT NULL,		-- Link to the SecurityUser table.
	"authenticationDomain" VARCHAR(100) NULL,
	"failedLoginCount" INT NULL,
	"lastLoginAttempt" TIMESTAMP NULL,
	"mostRecentActivity" TIMESTAMP NULL,
	"alternateIdentifier" VARCHAR(100) NULL,
	"image" BYTEA NULL,
	"settings" TEXT NULL,
	"securityTenantId" INT NULL,		-- The tenant that this user is linked to
	"readPermissionLevel" INT NOT NULL DEFAULT 0,
	"writePermissionLevel" INT NOT NULL DEFAULT 0,
	"securityOrganizationId" INT NULL,		-- The default organization to use when creating data, and null is provided as an organization on a data visibility enabled table
	"securityDepartmentId" INT NULL,		-- The default department to use when creating data, and null is provided as a department on a data visibility enabled table
	"securityTeamId" INT NULL,		-- The default team to use when creating data, and null is provided as a team on a data visibility enabled table
	"authenticationToken" VARCHAR(100) NULL,
	"authenticationTokenExpiry" TIMESTAMP NULL,
	"twoFactorToken" VARCHAR(10) NULL,
	"twoFactorTokenExpiry" TIMESTAMP NULL,
	"twoFactorSendByEmail" BOOLEAN NULL,
	"twoFactorSendBySMS" BOOLEAN NULL,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityUserTitleId" FOREIGN KEY ("securityUserTitleId") REFERENCES "Security"."SecurityUserTitle"("id"),		-- Foreign key to the SecurityUserTitle table.
	CONSTRAINT "reportsToSecurityUserId" FOREIGN KEY ("reportsToSecurityUserId") REFERENCES "Security"."SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	CONSTRAINT "securityTenantId" FOREIGN KEY ("securityTenantId") REFERENCES "Security"."SecurityTenant"("id"),		-- Foreign key to the SecurityTenant table.
	CONSTRAINT "securityOrganizationId" FOREIGN KEY ("securityOrganizationId") REFERENCES "Security"."SecurityOrganization"("id"),		-- Foreign key to the SecurityOrganization table.
	CONSTRAINT "securityDepartmentId" FOREIGN KEY ("securityDepartmentId") REFERENCES "Security"."SecurityDepartment"("id"),		-- Foreign key to the SecurityDepartment table.
	CONSTRAINT "securityTeamId" FOREIGN KEY ("securityTeamId") REFERENCES "Security"."SecurityTeam"("id")		-- Foreign key to the SecurityTeam table.
);
-- Index on the SecurityUser table's accountName field.
CREATE INDEX "I_SecurityUser_accountName" ON "Security"."SecurityUser" ("accountName")
;

-- Index on the SecurityUser table's securityUserTitleId field.
CREATE INDEX "I_SecurityUser_securityUserTitleId" ON "Security"."SecurityUser" ("securityUserTitleId")
;

-- Index on the SecurityUser table's reportsToSecurityUserId field.
CREATE INDEX "I_SecurityUser_reportsToSecurityUserId" ON "Security"."SecurityUser" ("reportsToSecurityUserId")
;

-- Index on the SecurityUser table's alternateIdentifier field.
CREATE INDEX "I_SecurityUser_alternateIdentifier" ON "Security"."SecurityUser" ("alternateIdentifier")
;

-- Index on the SecurityUser table's securityTenantId field.
CREATE INDEX "I_SecurityUser_securityTenantId" ON "Security"."SecurityUser" ("securityTenantId")
;

-- Index on the SecurityUser table's securityOrganizationId field.
CREATE INDEX "I_SecurityUser_securityOrganizationId" ON "Security"."SecurityUser" ("securityOrganizationId")
;

-- Index on the SecurityUser table's securityDepartmentId field.
CREATE INDEX "I_SecurityUser_securityDepartmentId" ON "Security"."SecurityUser" ("securityDepartmentId")
;

-- Index on the SecurityUser table's securityTeamId field.
CREATE INDEX "I_SecurityUser_securityTeamId" ON "Security"."SecurityUser" ("securityTeamId")
;

-- Index on the SecurityUser table's authenticationToken field.
CREATE INDEX "I_SecurityUser_authenticationToken" ON "Security"."SecurityUser" ("authenticationToken")
;

-- Index on the SecurityUser table's objectGuid field.
CREATE INDEX "I_SecurityUser_objectGuid" ON "Security"."SecurityUser" ("objectGuid")
;

-- Index on the SecurityUser table's active field.
CREATE INDEX "I_SecurityUser_active" ON "Security"."SecurityUser" ("active")
;

-- Index on the SecurityUser table's deleted field.
CREATE INDEX "I_SecurityUser_deleted" ON "Security"."SecurityUser" ("deleted")
;

-- Index on the SecurityUser table's accountName,activeDirectoryAccount,active,deleted fields.
CREATE INDEX "I_SecurityUser_accountName_activeDirectoryAccount_active_delete" ON "Security"."SecurityUser" ("accountName", "activeDirectoryAccount", "active", "deleted")
;

-- Index on the SecurityUser table's id,active,deleted fields.
CREATE INDEX "I_SecurityUser_id_active_deleted" ON "Security"."SecurityUser" ("id", "active", "deleted")
;

INSERT INTO "Security"."SecurityUser" ( "accountName", "activeDirectoryAccount", "canLogin", "mustChangePassword", "firstName", "lastName", "securityTenantId", "password", "description", "readPermissionLevel", "writePermissionLevel", "objectGuid" ) VALUES  ( 'Admin', false, true, true, 'Admin', 'Account', ( SELECT id FROM "SecurityTenant" WHERE "name" = 'System Service' LIMIT 1), '$HASH$V1000$10000$7lx52j0Z5CjBUyu8L84pOmsOo+jNH/pVZ1VlI4EBjAftRag+', 'System Aministrator account.  Refer to generator for default password.', 255, 255, '4099226f-cc2f-46d2-9725-29de861c4fa9' );

INSERT INTO "Security"."SecurityUser" ( "accountName", "activeDirectoryAccount", "canLogin", "mustChangePassword", "firstName", "middleName", "lastName", "securityTenantId", "password", "description", "readPermissionLevel", "writePermissionLevel", "objectGuid" ) VALUES  ( 'SystemService', false, true, false, 'System', 'Service', 'Account', ( SELECT id FROM "SecurityTenant" WHERE "name" = 'System Service' LIMIT 1), '$HASH$V1000$10000$WeuGAJrhrIJWnWZIdyAQKvBEiFM0iMLiS+NJW8ws0YjSCbPq', 'System Service account for job/worker connection purposes.  Refer to generator for default password.', 255, 255, 'd80632a7-b1ff-47cb-9ecd-87f4a4a22763' );


CREATE TABLE "Security"."SecurityTenantUser"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityTenantId" INT NOT NULL,		-- Link to the SecurityTenant table.
	"securityUserId" INT NOT NULL,		-- Link to the SecurityUser table.
	"isOwner" BOOLEAN NOT NULL DEFAULT false,		-- Whether this user is the owner/creator of the tenant. Only owners can invite/remove members and manage tenant settings.
	"canRead" BOOLEAN NOT NULL DEFAULT false,		-- Whether this user has read access to the tenant's data.
	"canWrite" BOOLEAN NOT NULL DEFAULT false,		-- Whether this user has write access to the tenant's data.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityTenantId" FOREIGN KEY ("securityTenantId") REFERENCES "Security"."SecurityTenant"("id"),		-- Foreign key to the SecurityTenant table.
	CONSTRAINT "securityUserId" FOREIGN KEY ("securityUserId") REFERENCES "Security"."SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	CONSTRAINT "UC_SecurityTenantUser_securityTenantId_securityUserId" UNIQUE ( "securityTenantId", "securityUserId") 		-- Uniqueness enforced on the SecurityTenantUser table's securityTenantId and securityUserId fields.
);
-- Index on the SecurityTenantUser table's securityTenantId field.
CREATE INDEX "I_SecurityTenantUser_securityTenantId" ON "Security"."SecurityTenantUser" ("securityTenantId")
;

-- Index on the SecurityTenantUser table's securityUserId field.
CREATE INDEX "I_SecurityTenantUser_securityUserId" ON "Security"."SecurityTenantUser" ("securityUserId")
;

-- Index on the SecurityTenantUser table's isOwner field.
CREATE INDEX "I_SecurityTenantUser_isOwner" ON "Security"."SecurityTenantUser" ("isOwner")
;

-- Index on the SecurityTenantUser table's canRead field.
CREATE INDEX "I_SecurityTenantUser_canRead" ON "Security"."SecurityTenantUser" ("canRead")
;

-- Index on the SecurityTenantUser table's canWrite field.
CREATE INDEX "I_SecurityTenantUser_canWrite" ON "Security"."SecurityTenantUser" ("canWrite")
;

-- Index on the SecurityTenantUser table's active field.
CREATE INDEX "I_SecurityTenantUser_active" ON "Security"."SecurityTenantUser" ("active")
;

-- Index on the SecurityTenantUser table's deleted field.
CREATE INDEX "I_SecurityTenantUser_deleted" ON "Security"."SecurityTenantUser" ("deleted")
;

-- Index on the SecurityTenantUser table's id,active,deleted fields.
CREATE INDEX "I_SecurityTenantUser_id_active_deleted" ON "Security"."SecurityTenantUser" ("id", "active", "deleted")
;


CREATE TABLE "Security"."SecurityOrganizationUser"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityOrganizationId" INT NOT NULL,		-- Link to the SecurityOrganization table.
	"securityUserId" INT NOT NULL,		-- Link to the SecurityUser table.
	"canRead" BOOLEAN NOT NULL DEFAULT false,
	"canWrite" BOOLEAN NOT NULL DEFAULT false,
	"canChangeHierarchy" BOOLEAN NOT NULL DEFAULT false,
	"canChangeOwner" BOOLEAN NOT NULL DEFAULT false,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityOrganizationId" FOREIGN KEY ("securityOrganizationId") REFERENCES "Security"."SecurityOrganization"("id"),		-- Foreign key to the SecurityOrganization table.
	CONSTRAINT "securityUserId" FOREIGN KEY ("securityUserId") REFERENCES "Security"."SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	CONSTRAINT "UC_SecurityOrganizationUser_securityOrganizationId_securityUserId" UNIQUE ( "securityOrganizationId", "securityUserId") 		-- Uniqueness enforced on the SecurityOrganizationUser table's securityOrganizationId and securityUserId fields.
);
-- Index on the SecurityOrganizationUser table's securityOrganizationId field.
CREATE INDEX "I_SecurityOrganizationUser_securityOrganizationId" ON "Security"."SecurityOrganizationUser" ("securityOrganizationId")
;

-- Index on the SecurityOrganizationUser table's securityUserId field.
CREATE INDEX "I_SecurityOrganizationUser_securityUserId" ON "Security"."SecurityOrganizationUser" ("securityUserId")
;

-- Index on the SecurityOrganizationUser table's canRead field.
CREATE INDEX "I_SecurityOrganizationUser_canRead" ON "Security"."SecurityOrganizationUser" ("canRead")
;

-- Index on the SecurityOrganizationUser table's canWrite field.
CREATE INDEX "I_SecurityOrganizationUser_canWrite" ON "Security"."SecurityOrganizationUser" ("canWrite")
;

-- Index on the SecurityOrganizationUser table's canChangeHierarchy field.
CREATE INDEX "I_SecurityOrganizationUser_canChangeHierarchy" ON "Security"."SecurityOrganizationUser" ("canChangeHierarchy")
;

-- Index on the SecurityOrganizationUser table's canChangeOwner field.
CREATE INDEX "I_SecurityOrganizationUser_canChangeOwner" ON "Security"."SecurityOrganizationUser" ("canChangeOwner")
;

-- Index on the SecurityOrganizationUser table's active field.
CREATE INDEX "I_SecurityOrganizationUser_active" ON "Security"."SecurityOrganizationUser" ("active")
;

-- Index on the SecurityOrganizationUser table's deleted field.
CREATE INDEX "I_SecurityOrganizationUser_deleted" ON "Security"."SecurityOrganizationUser" ("deleted")
;

-- Index on the SecurityOrganizationUser table's id,active,deleted fields.
CREATE INDEX "I_SecurityOrganizationUser_id_active_deleted" ON "Security"."SecurityOrganizationUser" ("id", "active", "deleted")
;


CREATE TABLE "Security"."SecurityDepartmentUser"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityDepartmentId" INT NOT NULL,		-- Link to the SecurityDepartment table.
	"securityUserId" INT NOT NULL,		-- Link to the SecurityUser table.
	"canRead" BOOLEAN NOT NULL DEFAULT false,
	"canWrite" BOOLEAN NOT NULL DEFAULT false,
	"canChangeHierarchy" BOOLEAN NOT NULL DEFAULT false,
	"canChangeOwner" BOOLEAN NOT NULL DEFAULT false,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityDepartmentId" FOREIGN KEY ("securityDepartmentId") REFERENCES "Security"."SecurityDepartment"("id"),		-- Foreign key to the SecurityDepartment table.
	CONSTRAINT "securityUserId" FOREIGN KEY ("securityUserId") REFERENCES "Security"."SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	CONSTRAINT "UC_SecurityDepartmentUser_securityDepartmentId_securityUserId" UNIQUE ( "securityDepartmentId", "securityUserId") 		-- Uniqueness enforced on the SecurityDepartmentUser table's securityDepartmentId and securityUserId fields.
);
-- Index on the SecurityDepartmentUser table's securityDepartmentId field.
CREATE INDEX "I_SecurityDepartmentUser_securityDepartmentId" ON "Security"."SecurityDepartmentUser" ("securityDepartmentId")
;

-- Index on the SecurityDepartmentUser table's securityUserId field.
CREATE INDEX "I_SecurityDepartmentUser_securityUserId" ON "Security"."SecurityDepartmentUser" ("securityUserId")
;

-- Index on the SecurityDepartmentUser table's canRead field.
CREATE INDEX "I_SecurityDepartmentUser_canRead" ON "Security"."SecurityDepartmentUser" ("canRead")
;

-- Index on the SecurityDepartmentUser table's canWrite field.
CREATE INDEX "I_SecurityDepartmentUser_canWrite" ON "Security"."SecurityDepartmentUser" ("canWrite")
;

-- Index on the SecurityDepartmentUser table's canChangeHierarchy field.
CREATE INDEX "I_SecurityDepartmentUser_canChangeHierarchy" ON "Security"."SecurityDepartmentUser" ("canChangeHierarchy")
;

-- Index on the SecurityDepartmentUser table's canChangeOwner field.
CREATE INDEX "I_SecurityDepartmentUser_canChangeOwner" ON "Security"."SecurityDepartmentUser" ("canChangeOwner")
;

-- Index on the SecurityDepartmentUser table's active field.
CREATE INDEX "I_SecurityDepartmentUser_active" ON "Security"."SecurityDepartmentUser" ("active")
;

-- Index on the SecurityDepartmentUser table's deleted field.
CREATE INDEX "I_SecurityDepartmentUser_deleted" ON "Security"."SecurityDepartmentUser" ("deleted")
;

-- Index on the SecurityDepartmentUser table's id,active,deleted fields.
CREATE INDEX "I_SecurityDepartmentUser_id_active_deleted" ON "Security"."SecurityDepartmentUser" ("id", "active", "deleted")
;


CREATE TABLE "Security"."SecurityTeamUser"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityTeamId" INT NOT NULL,		-- Link to the SecurityTeam table.
	"securityUserId" INT NOT NULL,		-- Link to the SecurityUser table.
	"canRead" BOOLEAN NOT NULL DEFAULT false,
	"canWrite" BOOLEAN NOT NULL DEFAULT false,
	"canChangeHierarchy" BOOLEAN NOT NULL DEFAULT false,
	"canChangeOwner" BOOLEAN NOT NULL DEFAULT false,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityTeamId" FOREIGN KEY ("securityTeamId") REFERENCES "Security"."SecurityTeam"("id"),		-- Foreign key to the SecurityTeam table.
	CONSTRAINT "securityUserId" FOREIGN KEY ("securityUserId") REFERENCES "Security"."SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	CONSTRAINT "UC_SecurityTeamUser_securityTeamId_securityUserId" UNIQUE ( "securityTeamId", "securityUserId") 		-- Uniqueness enforced on the SecurityTeamUser table's securityTeamId and securityUserId fields.
);
-- Index on the SecurityTeamUser table's securityTeamId field.
CREATE INDEX "I_SecurityTeamUser_securityTeamId" ON "Security"."SecurityTeamUser" ("securityTeamId")
;

-- Index on the SecurityTeamUser table's securityUserId field.
CREATE INDEX "I_SecurityTeamUser_securityUserId" ON "Security"."SecurityTeamUser" ("securityUserId")
;

-- Index on the SecurityTeamUser table's canRead field.
CREATE INDEX "I_SecurityTeamUser_canRead" ON "Security"."SecurityTeamUser" ("canRead")
;

-- Index on the SecurityTeamUser table's canWrite field.
CREATE INDEX "I_SecurityTeamUser_canWrite" ON "Security"."SecurityTeamUser" ("canWrite")
;

-- Index on the SecurityTeamUser table's canChangeHierarchy field.
CREATE INDEX "I_SecurityTeamUser_canChangeHierarchy" ON "Security"."SecurityTeamUser" ("canChangeHierarchy")
;

-- Index on the SecurityTeamUser table's canChangeOwner field.
CREATE INDEX "I_SecurityTeamUser_canChangeOwner" ON "Security"."SecurityTeamUser" ("canChangeOwner")
;

-- Index on the SecurityTeamUser table's active field.
CREATE INDEX "I_SecurityTeamUser_active" ON "Security"."SecurityTeamUser" ("active")
;

-- Index on the SecurityTeamUser table's deleted field.
CREATE INDEX "I_SecurityTeamUser_deleted" ON "Security"."SecurityTeamUser" ("deleted")
;

-- Index on the SecurityTeamUser table's id,active,deleted fields.
CREATE INDEX "I_SecurityTeamUser_id_active_deleted" ON "Security"."SecurityTeamUser" ("id", "active", "deleted")
;


CREATE TABLE "Security"."SecurityUserEventType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL
);
-- Index on the SecurityUserEventType table's name field.
CREATE INDEX "I_SecurityUserEventType_name" ON "Security"."SecurityUserEventType" ("name")
;

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'LoginSuccess', 'Login Success' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'LoginFailure', 'Login Failure' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'LoginAttemptDuringCooldown', 'Login Attempt During Cooldown' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'Logout', 'Logout' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'TwoFactorSend', 'TwoFactorSend' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'Miscellaneous', 'Miscellaneous' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'AccountInactivated', 'AccountInactivated' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'UserInitiatedPasswordResetRequest', 'UserInitiatedPasswordResetRequest' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'UserInitiatedPasswordResetCompleted', 'UserInitiatedPasswordResetCompleted' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'SystemInitiatedPasswordResetRequest', 'SystemInitiatedPasswordResetRequest' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'SystemInitiatedPasswordResetCompleted', 'SystemInitiatedPasswordResetCompleted' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'AdminInitiatedPasswordSet', 'Admin Initiated Password Set' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'AdminActionLockAccount', 'Admin Action Lock Account' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'AccountUnlocked', 'Account Unlocked' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'SessionRevoked', 'Session Revoked' );

INSERT INTO "Security"."SecurityUserEventType" ( "name", "description" ) VALUES  ( 'SessionRevokedWithAccountLock', 'Session Revoked With Account Lock' );


CREATE TABLE "Security"."SecurityUserEvent"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityUserId" INT NOT NULL,		-- Link to the SecurityUser table.
	"securityUserEventTypeId" INT NOT NULL,		-- Link to the SecurityUserEventType table.
	"timeStamp" TIMESTAMP NOT NULL,
	"comments" VARCHAR(1000) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityUserId" FOREIGN KEY ("securityUserId") REFERENCES "Security"."SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	CONSTRAINT "securityUserEventTypeId" FOREIGN KEY ("securityUserEventTypeId") REFERENCES "Security"."SecurityUserEventType"("id")		-- Foreign key to the SecurityUserEventType table.
);
-- Index on the SecurityUserEvent table's securityUserId field.
CREATE INDEX "I_SecurityUserEvent_securityUserId" ON "Security"."SecurityUserEvent" ("securityUserId")
;

-- Index on the SecurityUserEvent table's securityUserEventTypeId field.
CREATE INDEX "I_SecurityUserEvent_securityUserEventTypeId" ON "Security"."SecurityUserEvent" ("securityUserEventTypeId")
;

-- Index on the SecurityUserEvent table's timeStamp field.
CREATE INDEX "I_SecurityUserEvent_timeStamp" ON "Security"."SecurityUserEvent" ("timeStamp")
;

-- Index on the SecurityUserEvent table's active field.
CREATE INDEX "I_SecurityUserEvent_active" ON "Security"."SecurityUserEvent" ("active")
;

-- Index on the SecurityUserEvent table's deleted field.
CREATE INDEX "I_SecurityUserEvent_deleted" ON "Security"."SecurityUserEvent" ("deleted")
;

-- Index on the SecurityUserEvent table's id,active,deleted fields.
CREATE INDEX "I_SecurityUserEvent_id_active_deleted" ON "Security"."SecurityUserEvent" ("id", "active", "deleted")
;


CREATE TABLE "Security"."SecurityUserPasswordResetToken"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityUserId" INT NOT NULL,		-- Link to the SecurityUser table.
	"token" VARCHAR(250) NOT NULL,		-- The token to use for this password reset request
	"timeStamp" TIMESTAMP NOT NULL,		-- The point in time when this request was created.
	"expiry" TIMESTAMP NOT NULL,		-- The expiry time for this password reset request
	"systemInitiated" BOOLEAN NOT NULL DEFAULT false,		-- Whether or not this token reset process was system initiated or not
	"completed" BOOLEAN NOT NULL DEFAULT false,		-- Whether or not this token reset process is completed
	"comments" VARCHAR(1000) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityUserId" FOREIGN KEY ("securityUserId") REFERENCES "Security"."SecurityUser"("id")		-- Foreign key to the SecurityUser table.
);
-- Index on the SecurityUserPasswordResetToken table's securityUserId field.
CREATE INDEX "I_SecurityUserPasswordResetToken_securityUserId" ON "Security"."SecurityUserPasswordResetToken" ("securityUserId")
;

-- Index on the SecurityUserPasswordResetToken table's token field.
CREATE INDEX "I_SecurityUserPasswordResetToken_token" ON "Security"."SecurityUserPasswordResetToken" ("token")
;

-- Index on the SecurityUserPasswordResetToken table's timeStamp field.
CREATE INDEX "I_SecurityUserPasswordResetToken_timeStamp" ON "Security"."SecurityUserPasswordResetToken" ("timeStamp")
;

-- Index on the SecurityUserPasswordResetToken table's expiry field.
CREATE INDEX "I_SecurityUserPasswordResetToken_expiry" ON "Security"."SecurityUserPasswordResetToken" ("expiry")
;

-- Index on the SecurityUserPasswordResetToken table's systemInitiated field.
CREATE INDEX "I_SecurityUserPasswordResetToken_systemInitiated" ON "Security"."SecurityUserPasswordResetToken" ("systemInitiated")
;

-- Index on the SecurityUserPasswordResetToken table's completed field.
CREATE INDEX "I_SecurityUserPasswordResetToken_completed" ON "Security"."SecurityUserPasswordResetToken" ("completed")
;

-- Index on the SecurityUserPasswordResetToken table's active field.
CREATE INDEX "I_SecurityUserPasswordResetToken_active" ON "Security"."SecurityUserPasswordResetToken" ("active")
;

-- Index on the SecurityUserPasswordResetToken table's deleted field.
CREATE INDEX "I_SecurityUserPasswordResetToken_deleted" ON "Security"."SecurityUserPasswordResetToken" ("deleted")
;

-- Index on the SecurityUserPasswordResetToken table's id,active,deleted fields.
CREATE INDEX "I_SecurityUserPasswordResetToken_id_active_deleted" ON "Security"."SecurityUserPasswordResetToken" ("id", "active", "deleted")
;


CREATE TABLE "Security"."SecurityGroup"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the SecurityGroup table's name field.
CREATE INDEX "I_SecurityGroup_name" ON "Security"."SecurityGroup" ("name")
;

-- Index on the SecurityGroup table's active field.
CREATE INDEX "I_SecurityGroup_active" ON "Security"."SecurityGroup" ("active")
;

-- Index on the SecurityGroup table's deleted field.
CREATE INDEX "I_SecurityGroup_deleted" ON "Security"."SecurityGroup" ("deleted")
;

-- Index on the SecurityGroup table's id,active,deleted fields.
CREATE INDEX "I_SecurityGroup_id_active_deleted" ON "Security"."SecurityGroup" ("id", "active", "deleted")
;


CREATE TABLE "Security"."SecurityUserSecurityGroup"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityUserId" INT NOT NULL,		-- Link to the SecurityUser table.
	"securityGroupId" INT NOT NULL,		-- Link to the SecurityGroup table.
	"comments" VARCHAR(1000) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityUserId" FOREIGN KEY ("securityUserId") REFERENCES "Security"."SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	CONSTRAINT "securityGroupId" FOREIGN KEY ("securityGroupId") REFERENCES "Security"."SecurityGroup"("id"),		-- Foreign key to the SecurityGroup table.
	CONSTRAINT "UC_SecurityUserSecurityGroup_securityUserId_securityGroupId" UNIQUE ( "securityUserId", "securityGroupId") 		-- Uniqueness enforced on the SecurityUserSecurityGroup table's securityUserId and securityGroupId fields.
);
-- Index on the SecurityUserSecurityGroup table's securityUserId field.
CREATE INDEX "I_SecurityUserSecurityGroup_securityUserId" ON "Security"."SecurityUserSecurityGroup" ("securityUserId")
;

-- Index on the SecurityUserSecurityGroup table's securityGroupId field.
CREATE INDEX "I_SecurityUserSecurityGroup_securityGroupId" ON "Security"."SecurityUserSecurityGroup" ("securityGroupId")
;

-- Index on the SecurityUserSecurityGroup table's active field.
CREATE INDEX "I_SecurityUserSecurityGroup_active" ON "Security"."SecurityUserSecurityGroup" ("active")
;

-- Index on the SecurityUserSecurityGroup table's deleted field.
CREATE INDEX "I_SecurityUserSecurityGroup_deleted" ON "Security"."SecurityUserSecurityGroup" ("deleted")
;

-- Index on the SecurityUserSecurityGroup table's id,active,deleted fields.
CREATE INDEX "I_SecurityUserSecurityGroup_id_active_deleted" ON "Security"."SecurityUserSecurityGroup" ("id", "active", "deleted")
;


CREATE TABLE "Security"."Privilege"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL
);
-- Index on the Privilege table's name field.
CREATE INDEX "I_Privilege_name" ON "Security"."Privilege" ("name")
;

INSERT INTO "Security"."Privilege" ( "name", "description" ) VALUES  ( 'No Access', 'No Access' );

INSERT INTO "Security"."Privilege" ( "name", "description" ) VALUES  ( 'Anonymous Read Only', 'Read Only Access, With All Sensitive Data Redacted' );

INSERT INTO "Security"."Privilege" ( "name", "description" ) VALUES  ( 'Read Only', 'Read Only Access For General Use' );

INSERT INTO "Security"."Privilege" ( "name", "description" ) VALUES  ( 'Read and Write', 'Read and Write Access' );

INSERT INTO "Security"."Privilege" ( "name", "description" ) VALUES  ( 'Administrative', 'Complete Administrative Access' );

INSERT INTO "Security"."Privilege" ( "name", "description" ) VALUES  ( 'Custom', 'Custom Access Level' );


CREATE TABLE "Security"."SecurityRole"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"privilegeId" INT NOT NULL,		-- Link to the Privilege table.
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL,
	"comments" VARCHAR(1000) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "privilegeId" FOREIGN KEY ("privilegeId") REFERENCES "Security"."Privilege"("id")		-- Foreign key to the Privilege table.
);
-- Index on the SecurityRole table's privilegeId field.
CREATE INDEX "I_SecurityRole_privilegeId" ON "Security"."SecurityRole" ("privilegeId")
;

-- Index on the SecurityRole table's name field.
CREATE INDEX "I_SecurityRole_name" ON "Security"."SecurityRole" ("name")
;

-- Index on the SecurityRole table's active field.
CREATE INDEX "I_SecurityRole_active" ON "Security"."SecurityRole" ("active")
;

-- Index on the SecurityRole table's deleted field.
CREATE INDEX "I_SecurityRole_deleted" ON "Security"."SecurityRole" ("deleted")
;

-- Index on the SecurityRole table's id,active,deleted fields.
CREATE INDEX "I_SecurityRole_id_active_deleted" ON "Security"."SecurityRole" ("id", "active", "deleted")
;


CREATE TABLE "Security"."SecurityUserSecurityRole"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityUserId" INT NOT NULL,		-- Link to the SecurityUser table.
	"securityRoleId" INT NOT NULL,		-- Link to the SecurityRole table.
	"comments" VARCHAR(1000) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityUserId" FOREIGN KEY ("securityUserId") REFERENCES "Security"."SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	CONSTRAINT "securityRoleId" FOREIGN KEY ("securityRoleId") REFERENCES "Security"."SecurityRole"("id"),		-- Foreign key to the SecurityRole table.
	CONSTRAINT "UC_SecurityUserSecurityRole_securityUserId_securityRoleId" UNIQUE ( "securityUserId", "securityRoleId") 		-- Uniqueness enforced on the SecurityUserSecurityRole table's securityUserId and securityRoleId fields.
);
-- Index on the SecurityUserSecurityRole table's securityUserId field.
CREATE INDEX "I_SecurityUserSecurityRole_securityUserId" ON "Security"."SecurityUserSecurityRole" ("securityUserId")
;

-- Index on the SecurityUserSecurityRole table's securityRoleId field.
CREATE INDEX "I_SecurityUserSecurityRole_securityRoleId" ON "Security"."SecurityUserSecurityRole" ("securityRoleId")
;

-- Index on the SecurityUserSecurityRole table's active field.
CREATE INDEX "I_SecurityUserSecurityRole_active" ON "Security"."SecurityUserSecurityRole" ("active")
;

-- Index on the SecurityUserSecurityRole table's deleted field.
CREATE INDEX "I_SecurityUserSecurityRole_deleted" ON "Security"."SecurityUserSecurityRole" ("deleted")
;

-- Index on the SecurityUserSecurityRole table's id,active,deleted fields.
CREATE INDEX "I_SecurityUserSecurityRole_id_active_deleted" ON "Security"."SecurityUserSecurityRole" ("id", "active", "deleted")
;

-- Index on the SecurityUserSecurityRole table's securityUserId,active,deleted fields.
CREATE INDEX "I_SecurityUserSecurityRole_securityUserId_active_deleted" ON "Security"."SecurityUserSecurityRole" ("securityUserId", "active", "deleted")
;

-- Index on the SecurityUserSecurityRole table's securityRoleId,active,deleted fields.
CREATE INDEX "I_SecurityUserSecurityRole_securityRoleId_active_deleted" ON "Security"."SecurityUserSecurityRole" ("securityRoleId", "active", "deleted")
;


CREATE TABLE "Security"."SecurityGroupSecurityRole"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityGroupId" INT NOT NULL,		-- Link to the SecurityGroup table.
	"securityRoleId" INT NOT NULL,		-- Link to the SecurityRole table.
	"comments" VARCHAR(1000) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityGroupId" FOREIGN KEY ("securityGroupId") REFERENCES "Security"."SecurityGroup"("id"),		-- Foreign key to the SecurityGroup table.
	CONSTRAINT "securityRoleId" FOREIGN KEY ("securityRoleId") REFERENCES "Security"."SecurityRole"("id"),		-- Foreign key to the SecurityRole table.
	CONSTRAINT "UC_SecurityGroupSecurityRole_securityGroupId_securityRoleId" UNIQUE ( "securityGroupId", "securityRoleId") 		-- Uniqueness enforced on the SecurityGroupSecurityRole table's securityGroupId and securityRoleId fields.
);
-- Index on the SecurityGroupSecurityRole table's securityGroupId field.
CREATE INDEX "I_SecurityGroupSecurityRole_securityGroupId" ON "Security"."SecurityGroupSecurityRole" ("securityGroupId")
;

-- Index on the SecurityGroupSecurityRole table's securityRoleId field.
CREATE INDEX "I_SecurityGroupSecurityRole_securityRoleId" ON "Security"."SecurityGroupSecurityRole" ("securityRoleId")
;

-- Index on the SecurityGroupSecurityRole table's active field.
CREATE INDEX "I_SecurityGroupSecurityRole_active" ON "Security"."SecurityGroupSecurityRole" ("active")
;

-- Index on the SecurityGroupSecurityRole table's deleted field.
CREATE INDEX "I_SecurityGroupSecurityRole_deleted" ON "Security"."SecurityGroupSecurityRole" ("deleted")
;

-- Index on the SecurityGroupSecurityRole table's id,active,deleted fields.
CREATE INDEX "I_SecurityGroupSecurityRole_id_active_deleted" ON "Security"."SecurityGroupSecurityRole" ("id", "active", "deleted")
;

-- Index on the SecurityGroupSecurityRole table's securityGroupId,active,deleted fields.
CREATE INDEX "I_SecurityGroupSecurityRole_securityGroupId_active_deleted" ON "Security"."SecurityGroupSecurityRole" ("securityGroupId", "active", "deleted")
;

-- Index on the SecurityGroupSecurityRole table's securityRoleId,active,deleted fields.
CREATE INDEX "I_SecurityGroupSecurityRole_securityRoleId_active_deleted" ON "Security"."SecurityGroupSecurityRole" ("securityRoleId", "active", "deleted")
;


CREATE TABLE "Security"."Module"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the Module table's name field.
CREATE INDEX "I_Module_name" ON "Security"."Module" ("name")
;

-- Index on the Module table's active field.
CREATE INDEX "I_Module_active" ON "Security"."Module" ("active")
;

-- Index on the Module table's deleted field.
CREATE INDEX "I_Module_deleted" ON "Security"."Module" ("deleted")
;

-- Index on the Module table's id,active,deleted fields.
CREATE INDEX "I_Module_id_active_deleted" ON "Security"."Module" ("id", "active", "deleted")
;


CREATE TABLE "Security"."ModuleSecurityRole"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"moduleId" INT NOT NULL,		-- Link to the Module table.
	"securityRoleId" INT NOT NULL,		-- Link to the SecurityRole table.
	"comments" VARCHAR(1000) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "moduleId" FOREIGN KEY ("moduleId") REFERENCES "Security"."Module"("id"),		-- Foreign key to the Module table.
	CONSTRAINT "securityRoleId" FOREIGN KEY ("securityRoleId") REFERENCES "Security"."SecurityRole"("id"),		-- Foreign key to the SecurityRole table.
	CONSTRAINT "UC_ModuleSecurityRole_moduleId_securityRoleId" UNIQUE ( "moduleId", "securityRoleId") 		-- Uniqueness enforced on the ModuleSecurityRole table's moduleId and securityRoleId fields.
);
-- Index on the ModuleSecurityRole table's moduleId field.
CREATE INDEX "I_ModuleSecurityRole_moduleId" ON "Security"."ModuleSecurityRole" ("moduleId")
;

-- Index on the ModuleSecurityRole table's securityRoleId field.
CREATE INDEX "I_ModuleSecurityRole_securityRoleId" ON "Security"."ModuleSecurityRole" ("securityRoleId")
;

-- Index on the ModuleSecurityRole table's active field.
CREATE INDEX "I_ModuleSecurityRole_active" ON "Security"."ModuleSecurityRole" ("active")
;

-- Index on the ModuleSecurityRole table's deleted field.
CREATE INDEX "I_ModuleSecurityRole_deleted" ON "Security"."ModuleSecurityRole" ("deleted")
;

-- Index on the ModuleSecurityRole table's id,active,deleted fields.
CREATE INDEX "I_ModuleSecurityRole_id_active_deleted" ON "Security"."ModuleSecurityRole" ("id", "active", "deleted")
;

-- Index on the ModuleSecurityRole table's securityRoleId,active,deleted fields.
CREATE INDEX "I_ModuleSecurityRole_securityRoleId_active_deleted" ON "Security"."ModuleSecurityRole" ("securityRoleId", "active", "deleted")
;


CREATE TABLE "Security"."SystemSetting"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL,
	"value" TEXT NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the SystemSetting table's name field.
CREATE INDEX "I_SystemSetting_name" ON "Security"."SystemSetting" ("name")
;

-- Index on the SystemSetting table's active field.
CREATE INDEX "I_SystemSetting_active" ON "Security"."SystemSetting" ("active")
;

-- Index on the SystemSetting table's deleted field.
CREATE INDEX "I_SystemSetting_deleted" ON "Security"."SystemSetting" ("deleted")
;

-- Index on the SystemSetting table's id,active,deleted fields.
CREATE INDEX "I_SystemSetting_id_active_deleted" ON "Security"."SystemSetting" ("id", "active", "deleted")
;


CREATE TABLE "Security"."LoginAttempt"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"timeStamp" TIMESTAMP NOT NULL,
	"userName" VARCHAR(250) NULL,
	"passwordHash" INT NULL,
	"resource" VARCHAR(500) NULL,
	"sessionId" VARCHAR(50) NULL,
	"ipAddress" VARCHAR(50) NULL,
	"userAgent" VARCHAR(250) NULL,
	"value" TEXT NULL,
	"success" BOOLEAN NULL,		-- null = unknown/pending, true = success, false = failure
	"securityUserId" INT NULL,		-- Link to user if identified during login attempt
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityUserId" FOREIGN KEY ("securityUserId") REFERENCES "Security"."SecurityUser"("id")		-- Foreign key to the SecurityUser table.
);
-- Index on the LoginAttempt table's securityUserId field.
CREATE INDEX "I_LoginAttempt_securityUserId" ON "Security"."LoginAttempt" ("securityUserId")
;

-- Index on the LoginAttempt table's active field.
CREATE INDEX "I_LoginAttempt_active" ON "Security"."LoginAttempt" ("active")
;

-- Index on the LoginAttempt table's deleted field.
CREATE INDEX "I_LoginAttempt_deleted" ON "Security"."LoginAttempt" ("deleted")
;

-- Index on the LoginAttempt table's id,active,deleted fields.
CREATE INDEX "I_loginAttempt_id_active_deleted" ON "Security"."LoginAttempt" ("id", "active", "deleted")
;


CREATE TABLE "Security"."EntityDataToken"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityUserId" INT NOT NULL,		-- Link to the SecurityUser table.
	"moduleId" INT NOT NULL,		-- Link to the Module table.
	"entity" VARCHAR(250) NOT NULL,
	"sessionId" VARCHAR(50) NOT NULL,
	"authenticationToken" VARCHAR(50) NOT NULL,		-- This is the authentication token that gets set into the user data of the forms authentication ticket
	"token" VARCHAR(50) NOT NULL UNIQUE,
	"timeStamp" TIMESTAMP NOT NULL,
	"comments" VARCHAR(1000) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityUserId" FOREIGN KEY ("securityUserId") REFERENCES "Security"."SecurityUser"("id"),		-- Foreign key to the SecurityUser table.
	CONSTRAINT "moduleId" FOREIGN KEY ("moduleId") REFERENCES "Security"."Module"("id")		-- Foreign key to the Module table.
);
-- Index on the EntityDataToken table's securityUserId field.
CREATE INDEX "I_EntityDataToken_securityUserId" ON "Security"."EntityDataToken" ("securityUserId")
;

-- Index on the EntityDataToken table's moduleId field.
CREATE INDEX "I_EntityDataToken_moduleId" ON "Security"."EntityDataToken" ("moduleId")
;

-- Index on the EntityDataToken table's active field.
CREATE INDEX "I_EntityDataToken_active" ON "Security"."EntityDataToken" ("active")
;

-- Index on the EntityDataToken table's deleted field.
CREATE INDEX "I_EntityDataToken_deleted" ON "Security"."EntityDataToken" ("deleted")
;

-- Index on the EntityDataToken table's token field.
CREATE INDEX "I_EntityDataToken_token" ON "Security"."EntityDataToken" ("token")
;

-- Index on the EntityDataToken table's securityUserId,moduleId,sessionId fields.
CREATE INDEX "I_EntityDataToken_securityUserId_moduleId_sessionId" ON "Security"."EntityDataToken" ("securityUserId", "moduleId", "sessionId")
;

-- Index on the EntityDataToken table's securityUserId,moduleId,token,sessionId fields.
CREATE INDEX "I_EntityDataToken_securityUserId_moduleId_token_sessionId" ON "Security"."EntityDataToken" ("securityUserId", "moduleId", "token", "sessionId")
;

-- Index on the EntityDataToken table's id,active,deleted fields.
CREATE INDEX "I_EntityDataToken_id_active_deleted" ON "Security"."EntityDataToken" ("id", "active", "deleted")
;


CREATE TABLE "Security"."EntityDataTokenEventType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL
);
-- Index on the EntityDataTokenEventType table's name field.
CREATE INDEX "I_EntityDataTokenEventType_name" ON "Security"."EntityDataTokenEventType" ("name")
;

INSERT INTO "Security"."EntityDataTokenEventType" ( "name", "description" ) VALUES  ( 'ReadFromEntity', 'Read From Entity' );

INSERT INTO "Security"."EntityDataTokenEventType" ( "name", "description" ) VALUES  ( 'CascadeValidatedReadFromEntity', 'Cascade Validated Read From Entity' );

INSERT INTO "Security"."EntityDataTokenEventType" ( "name", "description" ) VALUES  ( 'WriteToEntity', 'Write To Entity' );

INSERT INTO "Security"."EntityDataTokenEventType" ( "name", "description" ) VALUES  ( 'CascadeValidatedWriteToEntity', 'Cascade Validated Write To Entity' );

INSERT INTO "Security"."EntityDataTokenEventType" ( "name", "description" ) VALUES  ( 'ReuseExistingToken', 'Reuse Existing Token' );


CREATE TABLE "Security"."EntityDataTokenEvent"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"entityDataTokenId" INT NOT NULL,		-- Link to the EntityDataToken table.
	"entityDataTokenEventTypeId" INT NOT NULL,		-- Link to the EntityDataTokenEventType table.
	"timeStamp" TIMESTAMP NOT NULL,
	"comments" VARCHAR(1000) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "entityDataTokenId" FOREIGN KEY ("entityDataTokenId") REFERENCES "Security"."EntityDataToken"("id"),		-- Foreign key to the EntityDataToken table.
	CONSTRAINT "entityDataTokenEventTypeId" FOREIGN KEY ("entityDataTokenEventTypeId") REFERENCES "Security"."EntityDataTokenEventType"("id")		-- Foreign key to the EntityDataTokenEventType table.
);
-- Index on the EntityDataTokenEvent table's entityDataTokenId field.
CREATE INDEX "I_EntityDataTokenEvent_entityDataTokenId" ON "Security"."EntityDataTokenEvent" ("entityDataTokenId")
;

-- Index on the EntityDataTokenEvent table's entityDataTokenEventTypeId field.
CREATE INDEX "I_EntityDataTokenEvent_entityDataTokenEventTypeId" ON "Security"."EntityDataTokenEvent" ("entityDataTokenEventTypeId")
;

-- Index on the EntityDataTokenEvent table's active field.
CREATE INDEX "I_EntityDataTokenEvent_active" ON "Security"."EntityDataTokenEvent" ("active")
;

-- Index on the EntityDataTokenEvent table's deleted field.
CREATE INDEX "I_EntityDataTokenEvent_deleted" ON "Security"."EntityDataTokenEvent" ("deleted")
;

-- Index on the EntityDataTokenEvent table's id,active,deleted fields.
CREATE INDEX "I_EntityDataTokenEvent_id_active_deleted" ON "Security"."EntityDataTokenEvent" ("id", "active", "deleted")
;


CREATE TABLE "Security"."OAUTHToken"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"token" VARCHAR(250) NOT NULL,
	"expiryDateTime" TIMESTAMP NOT NULL,
	"userData" VARCHAR(1000) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the OAUTHToken table's token field.
CREATE INDEX "I_OAUTHToken_token" ON "Security"."OAUTHToken" ("token")
;

-- Index on the OAUTHToken table's expiryDateTime field.
CREATE INDEX "I_OAUTHToken_expiryDateTime" ON "Security"."OAUTHToken" ("expiryDateTime")
;

-- Index on the OAUTHToken table's active field.
CREATE INDEX "I_OAUTHToken_active" ON "Security"."OAUTHToken" ("active")
;

-- Index on the OAUTHToken table's deleted field.
CREATE INDEX "I_OAUTHToken_deleted" ON "Security"."OAUTHToken" ("deleted")
;

-- Index on the OAUTHToken table's id,active,deleted fields.
CREATE INDEX "I_OauthToken_id_active_deleted" ON "Security"."OAUTHToken" ("id", "active", "deleted")
;


CREATE TABLE "Security"."UserSession"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"securityUserId" INT NOT NULL,		-- Link to the SecurityUser table.
	"objectGuid" VARCHAR(50) NOT NULL,		-- User's objectGuid for reliable identity resolution
	"tokenId" VARCHAR(250) NULL,		-- OpenIddict token ID for correlation
	"sessionStart" TIMESTAMP NOT NULL,		-- When the token was issued
	"expiresAt" TIMESTAMP NOT NULL,		-- When the token expires
	"ipAddress" VARCHAR(50) NULL,		-- Client IP address at login
	"userAgent" VARCHAR(500) NULL,		-- Browser/client user agent
	"loginMethod" VARCHAR(50) NULL,		-- Login method: Password, Microsoft, Google, RefreshToken
	"clientApplication" VARCHAR(100) NULL,		-- Client application name
	"isRevoked" BOOLEAN NOT NULL DEFAULT false,		-- Whether session has been administratively revoked
	"revokedAt" TIMESTAMP NULL,		-- When session was revoked
	"revokedBy" VARCHAR(100) NULL,		-- Who revoked the session (admin username)
	"revokedReason" VARCHAR(500) NULL,		-- Reason for revocation
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "securityUserId" FOREIGN KEY ("securityUserId") REFERENCES "Security"."SecurityUser"("id")		-- Foreign key to the SecurityUser table.
);
-- Index on the UserSession table's securityUserId field.
CREATE INDEX "I_UserSession_securityUserId" ON "Security"."UserSession" ("securityUserId")
;

-- Index on the UserSession table's objectGuid field.
CREATE INDEX "I_UserSession_objectGuid" ON "Security"."UserSession" ("objectGuid")
;

-- Index on the UserSession table's tokenId field.
CREATE INDEX "I_UserSession_tokenId" ON "Security"."UserSession" ("tokenId")
;

-- Index on the UserSession table's sessionStart field.
CREATE INDEX "I_UserSession_sessionStart" ON "Security"."UserSession" ("sessionStart")
;

-- Index on the UserSession table's expiresAt field.
CREATE INDEX "I_UserSession_expiresAt" ON "Security"."UserSession" ("expiresAt")
;

-- Index on the UserSession table's loginMethod field.
CREATE INDEX "I_UserSession_loginMethod" ON "Security"."UserSession" ("loginMethod")
;

-- Index on the UserSession table's isRevoked field.
CREATE INDEX "I_UserSession_isRevoked" ON "Security"."UserSession" ("isRevoked")
;

-- Index on the UserSession table's active field.
CREATE INDEX "I_UserSession_active" ON "Security"."UserSession" ("active")
;

-- Index on the UserSession table's deleted field.
CREATE INDEX "I_UserSession_deleted" ON "Security"."UserSession" ("deleted")
;

-- Index on the UserSession table's id,active,deleted fields.
CREATE INDEX "I_UserSession_id_active_deleted" ON "Security"."UserSession" ("id", "active", "deleted")
;

-- Index on the UserSession table's securityUserId,isRevoked,active,deleted fields.
CREATE INDEX "I_UserSession_securityUserId_isRevoked_active_deleted" ON "Security"."UserSession" ("securityUserId", "isRevoked", "active", "deleted")
;


