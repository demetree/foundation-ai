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



/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE "Security"."OpenIddictTokens"
-- DROP TABLE "Security"."OpenIddictAuthorizations"
-- DROP TABLE "Security"."OpenIddictScopes"
-- DROP TABLE "Security"."OpenIddictApplications"

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON "OpenIddictTokens" DISABLE
-- ALTER INDEX ALL ON "OpenIddictAuthorizations" DISABLE
-- ALTER INDEX ALL ON "OpenIddictScopes" DISABLE
-- ALTER INDEX ALL ON "OpenIddictApplications" DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON "OpenIddictTokens" REBUILD
-- ALTER INDEX ALL ON "OpenIddictAuthorizations" REBUILD
-- ALTER INDEX ALL ON "OpenIddictScopes" REBUILD
-- ALTER INDEX ALL ON "OpenIddictApplications" REBUILD

CREATE TABLE "Security"."OpenIddictApplications"
(
	"Id" VARCHAR(500) PRIMARY KEY NOT NULL,
	"ApplicationType" VARCHAR(50) NULL,
	"ClientId" VARCHAR(100) NULL,
	"ClientSecret" TEXT NULL,
	"ClientType" VARCHAR(50) NULL,
	"ConcurrencyToken" VARCHAR(500) NULL,
	"ConsentType" VARCHAR(50) NULL,
	"DisplayName" TEXT NULL,
	"DisplayNames" TEXT NULL,
	"JsonWebKeySet" TEXT NULL,
	"Permissions" TEXT NULL,
	"PostLogoutRedirectUris" TEXT NULL,
	"Properties" TEXT NULL,
	"RedirectUris" TEXT NULL,
	"Requirements" TEXT NULL,
	"Settings" TEXT NULL
);
-- Index on the OpenIddictApplications table's ClientId field.
CREATE INDEX "I_OpenIddictApplications_ClientId" ON "Security"."OpenIddictApplications" ("ClientId");


CREATE TABLE "Security"."OpenIddictScopes"
(
	"Id" VARCHAR(500) PRIMARY KEY NOT NULL,
	"ConcurrencyToken" VARCHAR(50) NULL,
	"Description" TEXT NULL,
	"Descriptions" TEXT NULL,
	"DisplayName" TEXT NULL,
	"DisplayNames" TEXT NULL,
	"Name" VARCHAR(250) NULL,
	"Properties" TEXT NULL,
	"Resources" TEXT NULL
);
-- Index on the OpenIddictScopes table's Name field.
CREATE INDEX "I_OpenIddictScopes_Name" ON "Security"."OpenIddictScopes" ("Name");


CREATE TABLE "Security"."OpenIddictAuthorizations"
(
	"Id" VARCHAR(500) PRIMARY KEY NOT NULL,
	"ApplicationId" VARCHAR(500) NOT NULL,		-- Link to the OpenIddictApplications table.
	"ConcurrencyToken" VARCHAR(50) NULL,
	"CreationDate" TIMESTAMP NULL,
	"Properties" TEXT NULL,
	"Scopes" TEXT NULL,
	"Status" VARCHAR(50) NULL,
	"Subject" VARCHAR(500) NULL,
	"Type" VARCHAR(50) NULL,
	CONSTRAINT "FK_OpenIddictAuthorizations_OpenIddictApplications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "dbo"."OpenIddictApplications"("Id")		-- Foreign key to the OpenIddictApplications table.
);
-- Index on the OpenIddictAuthorizations table's ApplicationId,Status,Subject,Type fields.
CREATE INDEX "I_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type" ON "Security"."OpenIddictAuthorizations" ("ApplicationId", "Status", "Subject", "Type");


CREATE TABLE "Security"."OpenIddictTokens"
(
	"Id" VARCHAR(500) PRIMARY KEY NOT NULL,
	"ApplicationId" VARCHAR(500) NOT NULL,		-- Link to the OpenIddictApplications table.
	"AuthorizationId" VARCHAR(500) NULL,		-- Link to the OpenIddictAuthorizations table.
	"ConcurrencyToken" VARCHAR(50) NULL,
	"CreationDate" TIMESTAMP NULL,
	"ExpirationDate" TIMESTAMP NULL,
	"Payload" TEXT NULL,
	"Properties" TEXT NULL,
	"RedemptionDate" TIMESTAMP NULL,
	"ReferenceId" VARCHAR(100) NULL,
	"Status" VARCHAR(50) NULL,
	"Subject" VARCHAR(500) NULL,
	"Type" VARCHAR(50) NULL,
	CONSTRAINT "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "dbo"."OpenIddictApplications"("Id"),		-- Foreign key to the OpenIddictApplications table.
	CONSTRAINT "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId" FOREIGN KEY ("AuthorizationId") REFERENCES "dbo"."OpenIddictAuthorizations"("Id")		-- Foreign key to the OpenIddictAuthorizations table.
);
-- Index on the OpenIddictTokens table's AuthorizationId field.
CREATE INDEX "I_OpenIddictTokens_AuthorizationId" ON "Security"."OpenIddictTokens" ("AuthorizationId");

-- Index on the OpenIddictTokens table's ReferenceId field.
CREATE INDEX "I_OpenIddictTokens_ReferenceId" ON "Security"."OpenIddictTokens" ("ReferenceId");

-- Index on the OpenIddictTokens table's ApplicationId,Status,Subject,Type fields.
CREATE INDEX "I_OpenIddictTokens_ApplicationId_Status_Subject_Type" ON "Security"."OpenIddictTokens" ("ApplicationId", "Status", "Subject", "Type");


