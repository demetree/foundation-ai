/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE "OpenIddictTokens"
-- DROP TABLE "OpenIddictAuthorizations"
-- DROP TABLE "OpenIddictScopes"
-- DROP TABLE "OpenIddictApplications"

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

CREATE TABLE "OpenIddictApplications"
(
	"Id" VARCHAR(500) PRIMARY KEY ASC NOT NULL,
	"ApplicationType" VARCHAR(50) NULL COLLATE NOCASE,
	"ClientId" VARCHAR(100) NULL COLLATE NOCASE,
	"ClientSecret" TEXT NULL COLLATE NOCASE,
	"ClientType" VARCHAR(50) NULL COLLATE NOCASE,
	"ConcurrencyToken" VARCHAR(500) NULL COLLATE NOCASE,
	"ConsentType" VARCHAR(50) NULL COLLATE NOCASE,
	"DisplayName" TEXT NULL COLLATE NOCASE,
	"DisplayNames" TEXT NULL COLLATE NOCASE,
	"JsonWebKeySet" TEXT NULL COLLATE NOCASE,
	"Permissions" TEXT NULL COLLATE NOCASE,
	"PostLogoutRedirectUris" TEXT NULL COLLATE NOCASE,
	"Properties" TEXT NULL COLLATE NOCASE,
	"RedirectUris" TEXT NULL COLLATE NOCASE,
	"Requirements" TEXT NULL COLLATE NOCASE,
	"Settings" TEXT NULL COLLATE NOCASE
);
-- Index on the OpenIddictApplications table's ClientId field.
CREATE INDEX "I_OpenIddictApplications_ClientId" ON "OpenIddictApplications" ("ClientId")
;


CREATE TABLE "OpenIddictScopes"
(
	"Id" VARCHAR(500) PRIMARY KEY ASC NOT NULL,
	"ConcurrencyToken" VARCHAR(50) NULL COLLATE NOCASE,
	"Description" TEXT NULL COLLATE NOCASE,
	"Descriptions" TEXT NULL COLLATE NOCASE,
	"DisplayName" TEXT NULL COLLATE NOCASE,
	"DisplayNames" TEXT NULL COLLATE NOCASE,
	"Name" VARCHAR(250) NULL COLLATE NOCASE,
	"Properties" TEXT NULL COLLATE NOCASE,
	"Resources" TEXT NULL COLLATE NOCASE
);
-- Index on the OpenIddictScopes table's Name field.
CREATE INDEX "I_OpenIddictScopes_Name" ON "OpenIddictScopes" ("Name")
;


CREATE TABLE "OpenIddictAuthorizations"
(
	"Id" VARCHAR(500) PRIMARY KEY ASC NOT NULL,
	"ApplicationId" VARCHAR(500) NOT NULL,		-- Link to the OpenIddictApplications table.
	"ConcurrencyToken" VARCHAR(50) NULL COLLATE NOCASE,
	"CreationDate" DATETIME NULL,
	"Properties" TEXT NULL COLLATE NOCASE,
	"Scopes" TEXT NULL COLLATE NOCASE,
	"Status" VARCHAR(50) NULL COLLATE NOCASE,
	"Subject" VARCHAR(500) NULL COLLATE NOCASE,
	"Type" VARCHAR(50) NULL COLLATE NOCASE,
	FOREIGN KEY ("ApplicationId") REFERENCES "OpenIddictApplications"("Id")		-- Foreign key to the OpenIddictApplications table.
);
-- Index on the OpenIddictAuthorizations table's ApplicationId,Status,Subject,Type fields.
CREATE INDEX "I_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type" ON "OpenIddictAuthorizations" ("ApplicationId", "Status", "Subject", "Type")
;


CREATE TABLE "OpenIddictTokens"
(
	"Id" VARCHAR(500) PRIMARY KEY ASC NOT NULL,
	"ApplicationId" VARCHAR(500) NOT NULL,		-- Link to the OpenIddictApplications table.
	"AuthorizationId" VARCHAR(500) NULL,		-- Link to the OpenIddictAuthorizations table.
	"ConcurrencyToken" VARCHAR(50) NULL COLLATE NOCASE,
	"CreationDate" DATETIME NULL,
	"ExpirationDate" DATETIME NULL,
	"Payload" TEXT NULL COLLATE NOCASE,
	"Properties" TEXT NULL COLLATE NOCASE,
	"RedemptionDate" DATETIME NULL,
	"ReferenceId" VARCHAR(100) NULL COLLATE NOCASE,
	"Status" VARCHAR(50) NULL COLLATE NOCASE,
	"Subject" VARCHAR(500) NULL COLLATE NOCASE,
	"Type" VARCHAR(50) NULL COLLATE NOCASE,
	FOREIGN KEY ("ApplicationId") REFERENCES "OpenIddictApplications"("Id"),		-- Foreign key to the OpenIddictApplications table.
	FOREIGN KEY ("AuthorizationId") REFERENCES "OpenIddictAuthorizations"("Id")		-- Foreign key to the OpenIddictAuthorizations table.
);
-- Index on the OpenIddictTokens table's AuthorizationId field.
CREATE INDEX "I_OpenIddictTokens_AuthorizationId" ON "OpenIddictTokens" ("AuthorizationId")
;

-- Index on the OpenIddictTokens table's ReferenceId field.
CREATE INDEX "I_OpenIddictTokens_ReferenceId" ON "OpenIddictTokens" ("ReferenceId")
;

-- Index on the OpenIddictTokens table's ApplicationId,Status,Subject,Type fields.
CREATE INDEX "I_OpenIddictTokens_ApplicationId_Status_Subject_Type" ON "OpenIddictTokens" ("ApplicationId", "Status", "Subject", "Type")
;


