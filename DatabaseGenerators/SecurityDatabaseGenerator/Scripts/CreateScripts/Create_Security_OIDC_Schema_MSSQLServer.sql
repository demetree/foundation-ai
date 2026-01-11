CREATE DATABASE [Security]
GO

ALTER DATABASE [Security] SET RECOVERY SIMPLE
GO

USE [Security]
GO

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE [dbo].[OpenIddictTokens]
-- DROP TABLE [dbo].[OpenIddictAuthorizations]
-- DROP TABLE [dbo].[OpenIddictScopes]
-- DROP TABLE [dbo].[OpenIddictApplications]

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON [dbo].[OpenIddictTokens] DISABLE
-- ALTER INDEX ALL ON [dbo].[OpenIddictAuthorizations] DISABLE
-- ALTER INDEX ALL ON [dbo].[OpenIddictScopes] DISABLE
-- ALTER INDEX ALL ON [dbo].[OpenIddictApplications] DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON [dbo].[OpenIddictTokens] REBUILD
-- ALTER INDEX ALL ON [dbo].[OpenIddictAuthorizations] REBUILD
-- ALTER INDEX ALL ON [dbo].[OpenIddictScopes] REBUILD
-- ALTER INDEX ALL ON [dbo].[OpenIddictApplications] REBUILD

CREATE TABLE [dbo].[OpenIddictApplications]
(
	[Id] NVARCHAR(500) PRIMARY KEY NOT NULL,
	[ApplicationType] NVARCHAR(50) NULL,
	[ClientId] NVARCHAR(100) NULL,
	[ClientSecret] NVARCHAR(MAX) NULL,
	[ClientType] NVARCHAR(50) NULL,
	[ConcurrencyToken] NVARCHAR(500) NULL,
	[ConsentType] NVARCHAR(50) NULL,
	[DisplayName] NVARCHAR(MAX) NULL,
	[DisplayNames] NVARCHAR(MAX) NULL,
	[JsonWebKeySet] NVARCHAR(MAX) NULL,
	[Permissions] NVARCHAR(MAX) NULL,
	[PostLogoutRedirectUris] NVARCHAR(MAX) NULL,
	[Properties] NVARCHAR(MAX) NULL,
	[RedirectUris] NVARCHAR(MAX) NULL,
	[Requirements] NVARCHAR(MAX) NULL,
	[Settings] NVARCHAR(MAX) NULL
)
GO

-- Index on the OpenIddictApplications table's ClientId field.
CREATE INDEX [I_OpenIddictApplications_ClientId] ON [dbo].[OpenIddictApplications] ([ClientId])
GO


CREATE TABLE [dbo].[OpenIddictScopes]
(
	[Id] NVARCHAR(500) PRIMARY KEY NOT NULL,
	[ConcurrencyToken] NVARCHAR(50) NULL,
	[Description] NVARCHAR(MAX) NULL,
	[Descriptions] NVARCHAR(MAX) NULL,
	[DisplayName] NVARCHAR(MAX) NULL,
	[DisplayNames] NVARCHAR(MAX) NULL,
	[Name] NVARCHAR(250) NULL,
	[Properties] NVARCHAR(MAX) NULL,
	[Resources] NVARCHAR(MAX) NULL
)
GO

-- Index on the OpenIddictScopes table's Name field.
CREATE INDEX [I_OpenIddictScopes_Name] ON [dbo].[OpenIddictScopes] ([Name])
GO


CREATE TABLE [dbo].[OpenIddictAuthorizations]
(
	[Id] NVARCHAR(500) PRIMARY KEY NOT NULL,
	[ApplicationId] NVARCHAR(500) NOT NULL,		-- Link to the OpenIddictApplications table.
	[ConcurrencyToken] NVARCHAR(50) NULL,
	[CreationDate] DATETIME2(7) NULL,
	[Properties] NVARCHAR(MAX) NULL,
	[Scopes] NVARCHAR(MAX) NULL,
	[Status] NVARCHAR(50) NULL,
	[Subject] NVARCHAR(500) NULL,
	[Type] NVARCHAR(50) NULL
	CONSTRAINT [FK_OpenIddictAuthorizations_OpenIddictApplications_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[OpenIddictApplications] ([Id])		-- Foreign key to the OpenIddictApplications table.
)
GO

-- Index on the OpenIddictAuthorizations table's ApplicationId,Status,Subject,Type fields.
CREATE INDEX [I_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type] ON [dbo].[OpenIddictAuthorizations] ([ApplicationId], [Status], [Subject], [Type])
GO


CREATE TABLE [dbo].[OpenIddictTokens]
(
	[Id] NVARCHAR(500) PRIMARY KEY NOT NULL,
	[ApplicationId] NVARCHAR(500) NOT NULL,		-- Link to the OpenIddictApplications table.
	[AuthorizationId] NVARCHAR(500) NULL,		-- Link to the OpenIddictAuthorizations table.
	[ConcurrencyToken] NVARCHAR(50) NULL,
	[CreationDate] DATETIME2(7) NULL,
	[ExpirationDate] DATETIME2(7) NULL,
	[Payload] NVARCHAR(MAX) NULL,
	[Properties] NVARCHAR(MAX) NULL,
	[RedemptionDate] DATETIME2(7) NULL,
	[ReferenceId] NVARCHAR(100) NULL,
	[Status] NVARCHAR(50) NULL,
	[Subject] NVARCHAR(500) NULL,
	[Type] NVARCHAR(50) NULL
	CONSTRAINT [FK_OpenIddictTokens_OpenIddictApplications_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[OpenIddictApplications] ([Id]),		-- Foreign key to the OpenIddictApplications table.
	CONSTRAINT [FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId] FOREIGN KEY ([AuthorizationId]) REFERENCES [dbo].[OpenIddictAuthorizations] ([Id])		-- Foreign key to the OpenIddictAuthorizations table.
)
GO

-- Index on the OpenIddictTokens table's AuthorizationId field.
CREATE INDEX [I_OpenIddictTokens_AuthorizationId] ON [dbo].[OpenIddictTokens] ([AuthorizationId])
GO

-- Index on the OpenIddictTokens table's ReferenceId field.
CREATE INDEX [I_OpenIddictTokens_ReferenceId] ON [dbo].[OpenIddictTokens] ([ReferenceId])
GO

-- Index on the OpenIddictTokens table's ApplicationId,Status,Subject,Type fields.
CREATE INDEX [I_OpenIddictTokens_ApplicationId_Status_Subject_Type] ON [dbo].[OpenIddictTokens] ([ApplicationId], [Status], [Subject], [Type])
GO


