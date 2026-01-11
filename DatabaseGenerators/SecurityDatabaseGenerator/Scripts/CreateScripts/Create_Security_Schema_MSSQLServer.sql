CREATE DATABASE [Security]
GO

ALTER DATABASE [Security] SET RECOVERY SIMPLE
GO

USE [Security]
GO

CREATE SCHEMA [Security]
GO

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE [Security].[OAUTHToken]
-- DROP TABLE [Security].[EntityDataTokenEvent]
-- DROP TABLE [Security].[EntityDataTokenEventType]
-- DROP TABLE [Security].[EntityDataToken]
-- DROP TABLE [Security].[LoginAttempt]
-- DROP TABLE [Security].[SystemSetting]
-- DROP TABLE [Security].[ModuleSecurityRole]
-- DROP TABLE [Security].[Module]
-- DROP TABLE [Security].[SecurityGroupSecurityRole]
-- DROP TABLE [Security].[SecurityUserSecurityRole]
-- DROP TABLE [Security].[SecurityRole]
-- DROP TABLE [Security].[Privilege]
-- DROP TABLE [Security].[SecurityUserSecurityGroup]
-- DROP TABLE [Security].[SecurityGroup]
-- DROP TABLE [Security].[SecurityUserPasswordResetToken]
-- DROP TABLE [Security].[SecurityUserEvent]
-- DROP TABLE [Security].[SecurityUserEventType]
-- DROP TABLE [Security].[SecurityTeamUser]
-- DROP TABLE [Security].[SecurityDepartmentUser]
-- DROP TABLE [Security].[SecurityOrganizationUser]
-- DROP TABLE [Security].[SecurityTenantUser]
-- DROP TABLE [Security].[SecurityUser]
-- DROP TABLE [Security].[SecurityUserTitle]
-- DROP TABLE [Security].[SecurityTeam]
-- DROP TABLE [Security].[SecurityDepartment]
-- DROP TABLE [Security].[SecurityOrganization]
-- DROP TABLE [Security].[SecurityTenant]

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON [Security].[OAUTHToken] DISABLE
-- ALTER INDEX ALL ON [Security].[EntityDataTokenEvent] DISABLE
-- ALTER INDEX ALL ON [Security].[EntityDataTokenEventType] DISABLE
-- ALTER INDEX ALL ON [Security].[EntityDataToken] DISABLE
-- ALTER INDEX ALL ON [Security].[LoginAttempt] DISABLE
-- ALTER INDEX ALL ON [Security].[SystemSetting] DISABLE
-- ALTER INDEX ALL ON [Security].[ModuleSecurityRole] DISABLE
-- ALTER INDEX ALL ON [Security].[Module] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityGroupSecurityRole] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityUserSecurityRole] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityRole] DISABLE
-- ALTER INDEX ALL ON [Security].[Privilege] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityUserSecurityGroup] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityGroup] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityUserPasswordResetToken] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityUserEvent] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityUserEventType] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityTeamUser] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityDepartmentUser] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityOrganizationUser] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityTenantUser] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityUser] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityUserTitle] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityTeam] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityDepartment] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityOrganization] DISABLE
-- ALTER INDEX ALL ON [Security].[SecurityTenant] DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON [Security].[OAUTHToken] REBUILD
-- ALTER INDEX ALL ON [Security].[EntityDataTokenEvent] REBUILD
-- ALTER INDEX ALL ON [Security].[EntityDataTokenEventType] REBUILD
-- ALTER INDEX ALL ON [Security].[EntityDataToken] REBUILD
-- ALTER INDEX ALL ON [Security].[LoginAttempt] REBUILD
-- ALTER INDEX ALL ON [Security].[SystemSetting] REBUILD
-- ALTER INDEX ALL ON [Security].[ModuleSecurityRole] REBUILD
-- ALTER INDEX ALL ON [Security].[Module] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityGroupSecurityRole] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityUserSecurityRole] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityRole] REBUILD
-- ALTER INDEX ALL ON [Security].[Privilege] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityUserSecurityGroup] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityGroup] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityUserPasswordResetToken] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityUserEvent] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityUserEventType] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityTeamUser] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityDepartmentUser] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityOrganizationUser] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityTenantUser] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityUser] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityUserTitle] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityTeam] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityDepartment] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityOrganization] REBUILD
-- ALTER INDEX ALL ON [Security].[SecurityTenant] REBUILD

CREATE TABLE [Security].[SecurityTenant]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the SecurityTenant table's name field.
CREATE INDEX [I_SecurityTenant_name] ON [Security].[SecurityTenant] ([name])
GO

-- Index on the SecurityTenant table's active field.
CREATE INDEX [I_SecurityTenant_active] ON [Security].[SecurityTenant] ([active])
GO

-- Index on the SecurityTenant table's deleted field.
CREATE INDEX [I_SecurityTenant_deleted] ON [Security].[SecurityTenant] ([deleted])
GO

-- Index on the SecurityTenant table's id,active,deleted fields.
CREATE INDEX [I_SecurityTenant_id_active_deleted] ON [Security].[SecurityTenant] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityOrganization]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityTenantId] INT NOT NULL,		-- Link to the SecurityTenant table.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityOrganization_SecurityTenant_securityTenantId] FOREIGN KEY ([securityTenantId]) REFERENCES [Security].[SecurityTenant] ([id]),		-- Foreign key to the SecurityTenant table.
	CONSTRAINT [UC_SecurityOrganization_securityTenantId_name] UNIQUE ( [securityTenantId], [name]) 		-- Uniqueness enforced on the SecurityOrganization table's securityTenantId and name fields.
)
GO

-- Index on the SecurityOrganization table's securityTenantId field.
CREATE INDEX [I_SecurityOrganization_securityTenantId] ON [Security].[SecurityOrganization] ([securityTenantId])
GO

-- Index on the SecurityOrganization table's name field.
CREATE INDEX [I_SecurityOrganization_name] ON [Security].[SecurityOrganization] ([name])
GO

-- Index on the SecurityOrganization table's active field.
CREATE INDEX [I_SecurityOrganization_active] ON [Security].[SecurityOrganization] ([active])
GO

-- Index on the SecurityOrganization table's deleted field.
CREATE INDEX [I_SecurityOrganization_deleted] ON [Security].[SecurityOrganization] ([deleted])
GO

-- Index on the SecurityOrganization table's id,active,deleted fields.
CREATE INDEX [I_SecurityOrganization_id_active_deleted] ON [Security].[SecurityOrganization] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityDepartment]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityOrganizationId] INT NOT NULL,		-- Link to the SecurityOrganization table.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityDepartment_SecurityOrganization_securityOrganizationId] FOREIGN KEY ([securityOrganizationId]) REFERENCES [Security].[SecurityOrganization] ([id]),		-- Foreign key to the SecurityOrganization table.
	CONSTRAINT [UC_SecurityDepartment_securityOrganizationId_name] UNIQUE ( [securityOrganizationId], [name]) 		-- Uniqueness enforced on the SecurityDepartment table's securityOrganizationId and name fields.
)
GO

-- Index on the SecurityDepartment table's securityOrganizationId field.
CREATE INDEX [I_SecurityDepartment_securityOrganizationId] ON [Security].[SecurityDepartment] ([securityOrganizationId])
GO

-- Index on the SecurityDepartment table's name field.
CREATE INDEX [I_SecurityDepartment_name] ON [Security].[SecurityDepartment] ([name])
GO

-- Index on the SecurityDepartment table's active field.
CREATE INDEX [I_SecurityDepartment_active] ON [Security].[SecurityDepartment] ([active])
GO

-- Index on the SecurityDepartment table's deleted field.
CREATE INDEX [I_SecurityDepartment_deleted] ON [Security].[SecurityDepartment] ([deleted])
GO

-- Index on the SecurityDepartment table's id,active,deleted fields.
CREATE INDEX [I_SecurityDepartment_id_active_deleted] ON [Security].[SecurityDepartment] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityTeam]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityDepartmentId] INT NOT NULL,		-- Link to the SecurityDepartment table.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityTeam_SecurityDepartment_securityDepartmentId] FOREIGN KEY ([securityDepartmentId]) REFERENCES [Security].[SecurityDepartment] ([id]),		-- Foreign key to the SecurityDepartment table.
	CONSTRAINT [UC_SecurityTeam_securityDepartmentId_name] UNIQUE ( [securityDepartmentId], [name]) 		-- Uniqueness enforced on the SecurityTeam table's securityDepartmentId and name fields.
)
GO

-- Index on the SecurityTeam table's securityDepartmentId field.
CREATE INDEX [I_SecurityTeam_securityDepartmentId] ON [Security].[SecurityTeam] ([securityDepartmentId])
GO

-- Index on the SecurityTeam table's name field.
CREATE INDEX [I_SecurityTeam_name] ON [Security].[SecurityTeam] ([name])
GO

-- Index on the SecurityTeam table's active field.
CREATE INDEX [I_SecurityTeam_active] ON [Security].[SecurityTeam] ([active])
GO

-- Index on the SecurityTeam table's deleted field.
CREATE INDEX [I_SecurityTeam_deleted] ON [Security].[SecurityTeam] ([deleted])
GO

-- Index on the SecurityTeam table's id,active,deleted fields.
CREATE INDEX [I_SecurityTeam_id_active_deleted] ON [Security].[SecurityTeam] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityUserTitle]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the SecurityUserTitle table's name field.
CREATE INDEX [I_SecurityUserTitle_name] ON [Security].[SecurityUserTitle] ([name])
GO

-- Index on the SecurityUserTitle table's active field.
CREATE INDEX [I_SecurityUserTitle_active] ON [Security].[SecurityUserTitle] ([active])
GO

-- Index on the SecurityUserTitle table's deleted field.
CREATE INDEX [I_SecurityUserTitle_deleted] ON [Security].[SecurityUserTitle] ([deleted])
GO

-- Index on the SecurityUserTitle table's id,active,deleted fields.
CREATE INDEX [I_SecurityUserTitle_id_active_deleted] ON [Security].[SecurityUserTitle] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityUser]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[accountName] NVARCHAR(250) NOT NULL UNIQUE,
	[activeDirectoryAccount] BIT NOT NULL DEFAULT 0,
	[password] NVARCHAR(250) NULL,
	[canLogin] BIT NOT NULL DEFAULT 1,		-- Whether or not the user can login.  Should be true for people, or API access accounts, and false for internal use service accounts that should never be allowed to login.
	[mustChangePassword] BIT NOT NULL DEFAULT 0,		-- True if the user is required to change their password
	[firstName] NVARCHAR(100) NULL,
	[middleName] NVARCHAR(100) NULL,
	[lastName] NVARCHAR(100) NULL,
	[dateOfBirth] DATETIME2(7) NULL,
	[emailAddress] NVARCHAR(100) NULL,
	[cellPhoneNumber] NVARCHAR(100) NULL,
	[phoneNumber] NVARCHAR(50) NULL,
	[phoneExtension] NVARCHAR(50) NULL,
	[description] NVARCHAR(500) NULL,
	[securityUserTitleId] INT NULL,		-- Link to the SecurityUserTitle table.
	[reportsToSecurityUserId] INT NULL,		-- Link to the SecurityUser table.
	[authenticationDomain] NVARCHAR(100) NULL,
	[failedLoginCount] INT NULL,
	[lastLoginAttempt] DATETIME2(7) NULL,
	[mostRecentActivity] DATETIME2(7) NULL,
	[alternateIdentifier] NVARCHAR(100) NULL,
	[image] VARBINARY(MAX) NULL,
	[settings] NVARCHAR(MAX) NULL,
	[securityTenantId] INT NULL,		-- The tenant that this user is linked to
	[readPermissionLevel] INT NOT NULL DEFAULT 0,
	[writePermissionLevel] INT NOT NULL DEFAULT 0,
	[securityOrganizationId] INT NULL,		-- The default organization to use when creating data, and null is provided as an organization on a data visibility enabled table
	[securityDepartmentId] INT NULL,		-- The default department to use when creating data, and null is provided as a department on a data visibility enabled table
	[securityTeamId] INT NULL,		-- The default team to use when creating data, and null is provided as a team on a data visibility enabled table
	[authenticationToken] NVARCHAR(100) NULL,
	[authenticationTokenExpiry] DATETIME2(7) NULL,
	[twoFactorToken] NVARCHAR(10) NULL,
	[twoFactorTokenExpiry] DATETIME2(7) NULL,
	[twoFactorSendByEmail] BIT NULL,
	[twoFactorSendBySMS] BIT NULL,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityUser_SecurityUserTitle_securityUserTitleId] FOREIGN KEY ([securityUserTitleId]) REFERENCES [Security].[SecurityUserTitle] ([id]),		-- Foreign key to the SecurityUserTitle table.
	CONSTRAINT [FK_SecurityUser_SecurityUser_reportsToSecurityUserId] FOREIGN KEY ([reportsToSecurityUserId]) REFERENCES [Security].[SecurityUser] ([id]),		-- Foreign key to the SecurityUser table.
	CONSTRAINT [FK_SecurityUser_SecurityTenant_securityTenantId] FOREIGN KEY ([securityTenantId]) REFERENCES [Security].[SecurityTenant] ([id]),		-- Foreign key to the SecurityTenant table.
	CONSTRAINT [FK_SecurityUser_SecurityOrganization_securityOrganizationId] FOREIGN KEY ([securityOrganizationId]) REFERENCES [Security].[SecurityOrganization] ([id]),		-- Foreign key to the SecurityOrganization table.
	CONSTRAINT [FK_SecurityUser_SecurityDepartment_securityDepartmentId] FOREIGN KEY ([securityDepartmentId]) REFERENCES [Security].[SecurityDepartment] ([id]),		-- Foreign key to the SecurityDepartment table.
	CONSTRAINT [FK_SecurityUser_SecurityTeam_securityTeamId] FOREIGN KEY ([securityTeamId]) REFERENCES [Security].[SecurityTeam] ([id])		-- Foreign key to the SecurityTeam table.
)
GO

-- Index on the SecurityUser table's accountName field.
CREATE INDEX [I_SecurityUser_accountName] ON [Security].[SecurityUser] ([accountName])
GO

-- Index on the SecurityUser table's securityUserTitleId field.
CREATE INDEX [I_SecurityUser_securityUserTitleId] ON [Security].[SecurityUser] ([securityUserTitleId])
GO

-- Index on the SecurityUser table's reportsToSecurityUserId field.
CREATE INDEX [I_SecurityUser_reportsToSecurityUserId] ON [Security].[SecurityUser] ([reportsToSecurityUserId])
GO

-- Index on the SecurityUser table's alternateIdentifier field.
CREATE INDEX [I_SecurityUser_alternateIdentifier] ON [Security].[SecurityUser] ([alternateIdentifier])
GO

-- Index on the SecurityUser table's securityTenantId field.
CREATE INDEX [I_SecurityUser_securityTenantId] ON [Security].[SecurityUser] ([securityTenantId])
GO

-- Index on the SecurityUser table's securityOrganizationId field.
CREATE INDEX [I_SecurityUser_securityOrganizationId] ON [Security].[SecurityUser] ([securityOrganizationId])
GO

-- Index on the SecurityUser table's securityDepartmentId field.
CREATE INDEX [I_SecurityUser_securityDepartmentId] ON [Security].[SecurityUser] ([securityDepartmentId])
GO

-- Index on the SecurityUser table's securityTeamId field.
CREATE INDEX [I_SecurityUser_securityTeamId] ON [Security].[SecurityUser] ([securityTeamId])
GO

-- Index on the SecurityUser table's authenticationToken field.
CREATE INDEX [I_SecurityUser_authenticationToken] ON [Security].[SecurityUser] ([authenticationToken])
GO

-- Index on the SecurityUser table's objectGuid field.
CREATE INDEX [I_SecurityUser_objectGuid] ON [Security].[SecurityUser] ([objectGuid])
GO

-- Index on the SecurityUser table's active field.
CREATE INDEX [I_SecurityUser_active] ON [Security].[SecurityUser] ([active])
GO

-- Index on the SecurityUser table's deleted field.
CREATE INDEX [I_SecurityUser_deleted] ON [Security].[SecurityUser] ([deleted])
GO

-- Index on the SecurityUser table's accountName,activeDirectoryAccount,active,deleted fields.
CREATE INDEX [I_SecurityUser_accountName_activeDirectoryAccount_active_deleted] ON [Security].[SecurityUser] ([accountName], [activeDirectoryAccount], [active], [deleted])
GO

-- Index on the SecurityUser table's id,active,deleted fields.
CREATE INDEX [I_SecurityUser_id_active_deleted] ON [Security].[SecurityUser] ([id], [active], [deleted])
GO

INSERT INTO [Security].[SecurityUser] ( [accountName], [activeDirectoryAccount], [canLogin], [mustChangePassword], [firstName], [lastName], [password], [description], [readPermissionLevel], [writePermissionLevel], [objectGuid] ) VALUES  ( 'Admin', 0, 1, 1, 'Admin', 'User', '$HASH$V1000$10000$7lx52j0Z5CjBUyu8L84pOmsOo+jNH/pVZ1VlI4EBjAftRag+', 'Refer to generator for default password.', 255, 255, '3b1cabc6-472c-4cef-b831-b4b2052e4c10' )
GO


CREATE TABLE [Security].[SecurityTenantUser]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityTenantId] INT NOT NULL,		-- Link to the SecurityTenant table.
	[securityUserId] INT NOT NULL,		-- Link to the SecurityUser table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityTenantUser_SecurityTenant_securityTenantId] FOREIGN KEY ([securityTenantId]) REFERENCES [Security].[SecurityTenant] ([id]),		-- Foreign key to the SecurityTenant table.
	CONSTRAINT [FK_SecurityTenantUser_SecurityUser_securityUserId] FOREIGN KEY ([securityUserId]) REFERENCES [Security].[SecurityUser] ([id]),		-- Foreign key to the SecurityUser table.
	CONSTRAINT [UC_SecurityTenantUser_securityTenantId_securityUserId] UNIQUE ( [securityTenantId], [securityUserId]) 		-- Uniqueness enforced on the SecurityTenantUser table's securityTenantId and securityUserId fields.
)
GO

-- Index on the SecurityTenantUser table's securityTenantId field.
CREATE INDEX [I_SecurityTenantUser_securityTenantId] ON [Security].[SecurityTenantUser] ([securityTenantId])
GO

-- Index on the SecurityTenantUser table's securityUserId field.
CREATE INDEX [I_SecurityTenantUser_securityUserId] ON [Security].[SecurityTenantUser] ([securityUserId])
GO

-- Index on the SecurityTenantUser table's active field.
CREATE INDEX [I_SecurityTenantUser_active] ON [Security].[SecurityTenantUser] ([active])
GO

-- Index on the SecurityTenantUser table's deleted field.
CREATE INDEX [I_SecurityTenantUser_deleted] ON [Security].[SecurityTenantUser] ([deleted])
GO

-- Index on the SecurityTenantUser table's id,active,deleted fields.
CREATE INDEX [I_SecurityTenantUser_id_active_deleted] ON [Security].[SecurityTenantUser] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityOrganizationUser]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityOrganizationId] INT NOT NULL,		-- Link to the SecurityOrganization table.
	[securityUserId] INT NOT NULL,		-- Link to the SecurityUser table.
	[canRead] BIT NOT NULL DEFAULT 0,
	[canWrite] BIT NOT NULL DEFAULT 0,
	[canChangeHierarchy] BIT NOT NULL DEFAULT 0,
	[canChangeOwner] BIT NOT NULL DEFAULT 0,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityOrganizationUser_SecurityOrganization_securityOrganizationId] FOREIGN KEY ([securityOrganizationId]) REFERENCES [Security].[SecurityOrganization] ([id]),		-- Foreign key to the SecurityOrganization table.
	CONSTRAINT [FK_SecurityOrganizationUser_SecurityUser_securityUserId] FOREIGN KEY ([securityUserId]) REFERENCES [Security].[SecurityUser] ([id]),		-- Foreign key to the SecurityUser table.
	CONSTRAINT [UC_SecurityOrganizationUser_securityOrganizationId_securityUserId] UNIQUE ( [securityOrganizationId], [securityUserId]) 		-- Uniqueness enforced on the SecurityOrganizationUser table's securityOrganizationId and securityUserId fields.
)
GO

-- Index on the SecurityOrganizationUser table's securityOrganizationId field.
CREATE INDEX [I_SecurityOrganizationUser_securityOrganizationId] ON [Security].[SecurityOrganizationUser] ([securityOrganizationId])
GO

-- Index on the SecurityOrganizationUser table's securityUserId field.
CREATE INDEX [I_SecurityOrganizationUser_securityUserId] ON [Security].[SecurityOrganizationUser] ([securityUserId])
GO

-- Index on the SecurityOrganizationUser table's canRead field.
CREATE INDEX [I_SecurityOrganizationUser_canRead] ON [Security].[SecurityOrganizationUser] ([canRead])
GO

-- Index on the SecurityOrganizationUser table's canWrite field.
CREATE INDEX [I_SecurityOrganizationUser_canWrite] ON [Security].[SecurityOrganizationUser] ([canWrite])
GO

-- Index on the SecurityOrganizationUser table's canChangeHierarchy field.
CREATE INDEX [I_SecurityOrganizationUser_canChangeHierarchy] ON [Security].[SecurityOrganizationUser] ([canChangeHierarchy])
GO

-- Index on the SecurityOrganizationUser table's canChangeOwner field.
CREATE INDEX [I_SecurityOrganizationUser_canChangeOwner] ON [Security].[SecurityOrganizationUser] ([canChangeOwner])
GO

-- Index on the SecurityOrganizationUser table's active field.
CREATE INDEX [I_SecurityOrganizationUser_active] ON [Security].[SecurityOrganizationUser] ([active])
GO

-- Index on the SecurityOrganizationUser table's deleted field.
CREATE INDEX [I_SecurityOrganizationUser_deleted] ON [Security].[SecurityOrganizationUser] ([deleted])
GO

-- Index on the SecurityOrganizationUser table's id,active,deleted fields.
CREATE INDEX [I_SecurityOrganizationUser_id_active_deleted] ON [Security].[SecurityOrganizationUser] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityDepartmentUser]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityDepartmentId] INT NOT NULL,		-- Link to the SecurityDepartment table.
	[securityUserId] INT NOT NULL,		-- Link to the SecurityUser table.
	[canRead] BIT NOT NULL DEFAULT 0,
	[canWrite] BIT NOT NULL DEFAULT 0,
	[canChangeHierarchy] BIT NOT NULL DEFAULT 0,
	[canChangeOwner] BIT NOT NULL DEFAULT 0,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityDepartmentUser_SecurityDepartment_securityDepartmentId] FOREIGN KEY ([securityDepartmentId]) REFERENCES [Security].[SecurityDepartment] ([id]),		-- Foreign key to the SecurityDepartment table.
	CONSTRAINT [FK_SecurityDepartmentUser_SecurityUser_securityUserId] FOREIGN KEY ([securityUserId]) REFERENCES [Security].[SecurityUser] ([id]),		-- Foreign key to the SecurityUser table.
	CONSTRAINT [UC_SecurityDepartmentUser_securityDepartmentId_securityUserId] UNIQUE ( [securityDepartmentId], [securityUserId]) 		-- Uniqueness enforced on the SecurityDepartmentUser table's securityDepartmentId and securityUserId fields.
)
GO

-- Index on the SecurityDepartmentUser table's securityDepartmentId field.
CREATE INDEX [I_SecurityDepartmentUser_securityDepartmentId] ON [Security].[SecurityDepartmentUser] ([securityDepartmentId])
GO

-- Index on the SecurityDepartmentUser table's securityUserId field.
CREATE INDEX [I_SecurityDepartmentUser_securityUserId] ON [Security].[SecurityDepartmentUser] ([securityUserId])
GO

-- Index on the SecurityDepartmentUser table's canRead field.
CREATE INDEX [I_SecurityDepartmentUser_canRead] ON [Security].[SecurityDepartmentUser] ([canRead])
GO

-- Index on the SecurityDepartmentUser table's canWrite field.
CREATE INDEX [I_SecurityDepartmentUser_canWrite] ON [Security].[SecurityDepartmentUser] ([canWrite])
GO

-- Index on the SecurityDepartmentUser table's canChangeHierarchy field.
CREATE INDEX [I_SecurityDepartmentUser_canChangeHierarchy] ON [Security].[SecurityDepartmentUser] ([canChangeHierarchy])
GO

-- Index on the SecurityDepartmentUser table's canChangeOwner field.
CREATE INDEX [I_SecurityDepartmentUser_canChangeOwner] ON [Security].[SecurityDepartmentUser] ([canChangeOwner])
GO

-- Index on the SecurityDepartmentUser table's active field.
CREATE INDEX [I_SecurityDepartmentUser_active] ON [Security].[SecurityDepartmentUser] ([active])
GO

-- Index on the SecurityDepartmentUser table's deleted field.
CREATE INDEX [I_SecurityDepartmentUser_deleted] ON [Security].[SecurityDepartmentUser] ([deleted])
GO

-- Index on the SecurityDepartmentUser table's id,active,deleted fields.
CREATE INDEX [I_SecurityDepartmentUser_id_active_deleted] ON [Security].[SecurityDepartmentUser] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityTeamUser]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityTeamId] INT NOT NULL,		-- Link to the SecurityTeam table.
	[securityUserId] INT NOT NULL,		-- Link to the SecurityUser table.
	[canRead] BIT NOT NULL DEFAULT 0,
	[canWrite] BIT NOT NULL DEFAULT 0,
	[canChangeHierarchy] BIT NOT NULL DEFAULT 0,
	[canChangeOwner] BIT NOT NULL DEFAULT 0,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityTeamUser_SecurityTeam_securityTeamId] FOREIGN KEY ([securityTeamId]) REFERENCES [Security].[SecurityTeam] ([id]),		-- Foreign key to the SecurityTeam table.
	CONSTRAINT [FK_SecurityTeamUser_SecurityUser_securityUserId] FOREIGN KEY ([securityUserId]) REFERENCES [Security].[SecurityUser] ([id]),		-- Foreign key to the SecurityUser table.
	CONSTRAINT [UC_SecurityTeamUser_securityTeamId_securityUserId] UNIQUE ( [securityTeamId], [securityUserId]) 		-- Uniqueness enforced on the SecurityTeamUser table's securityTeamId and securityUserId fields.
)
GO

-- Index on the SecurityTeamUser table's securityTeamId field.
CREATE INDEX [I_SecurityTeamUser_securityTeamId] ON [Security].[SecurityTeamUser] ([securityTeamId])
GO

-- Index on the SecurityTeamUser table's securityUserId field.
CREATE INDEX [I_SecurityTeamUser_securityUserId] ON [Security].[SecurityTeamUser] ([securityUserId])
GO

-- Index on the SecurityTeamUser table's canRead field.
CREATE INDEX [I_SecurityTeamUser_canRead] ON [Security].[SecurityTeamUser] ([canRead])
GO

-- Index on the SecurityTeamUser table's canWrite field.
CREATE INDEX [I_SecurityTeamUser_canWrite] ON [Security].[SecurityTeamUser] ([canWrite])
GO

-- Index on the SecurityTeamUser table's canChangeHierarchy field.
CREATE INDEX [I_SecurityTeamUser_canChangeHierarchy] ON [Security].[SecurityTeamUser] ([canChangeHierarchy])
GO

-- Index on the SecurityTeamUser table's canChangeOwner field.
CREATE INDEX [I_SecurityTeamUser_canChangeOwner] ON [Security].[SecurityTeamUser] ([canChangeOwner])
GO

-- Index on the SecurityTeamUser table's active field.
CREATE INDEX [I_SecurityTeamUser_active] ON [Security].[SecurityTeamUser] ([active])
GO

-- Index on the SecurityTeamUser table's deleted field.
CREATE INDEX [I_SecurityTeamUser_deleted] ON [Security].[SecurityTeamUser] ([deleted])
GO

-- Index on the SecurityTeamUser table's id,active,deleted fields.
CREATE INDEX [I_SecurityTeamUser_id_active_deleted] ON [Security].[SecurityTeamUser] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityUserEventType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL
)
GO

-- Index on the SecurityUserEventType table's name field.
CREATE INDEX [I_SecurityUserEventType_name] ON [Security].[SecurityUserEventType] ([name])
GO

INSERT INTO [Security].[SecurityUserEventType] ( [name], [description] ) VALUES  ( 'LoginSuccess', 'Login Success' )
GO

INSERT INTO [Security].[SecurityUserEventType] ( [name], [description] ) VALUES  ( 'LoginFailure', 'Login Failure' )
GO

INSERT INTO [Security].[SecurityUserEventType] ( [name], [description] ) VALUES  ( 'LoginAttemptDuringCooldown', 'Login Attempt During Cooldown' )
GO

INSERT INTO [Security].[SecurityUserEventType] ( [name], [description] ) VALUES  ( 'Logout', 'Logout' )
GO

INSERT INTO [Security].[SecurityUserEventType] ( [name], [description] ) VALUES  ( 'TwoFactorSend', 'TwoFactorSend' )
GO

INSERT INTO [Security].[SecurityUserEventType] ( [name], [description] ) VALUES  ( 'Miscellaneous', 'Miscellaneous' )
GO

INSERT INTO [Security].[SecurityUserEventType] ( [name], [description] ) VALUES  ( 'AccountInactivated', 'AccountInactivated' )
GO

INSERT INTO [Security].[SecurityUserEventType] ( [name], [description] ) VALUES  ( 'UserInitiatedPasswordResetRequest', 'UserInitiatedPasswordResetRequest' )
GO

INSERT INTO [Security].[SecurityUserEventType] ( [name], [description] ) VALUES  ( 'UserInitiatedPasswordResetCompleted', 'UserInitiatedPasswordResetCompleted' )
GO

INSERT INTO [Security].[SecurityUserEventType] ( [name], [description] ) VALUES  ( 'SystemInitiatedPasswordResetRequest', 'SystemInitiatedPasswordResetRequest' )
GO

INSERT INTO [Security].[SecurityUserEventType] ( [name], [description] ) VALUES  ( 'SystemInitiatedPasswordResetCompleted', 'SystemInitiatedPasswordResetCompleted' )
GO


CREATE TABLE [Security].[SecurityUserEvent]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityUserId] INT NOT NULL,		-- Link to the SecurityUser table.
	[securityUserEventTypeId] INT NOT NULL,		-- Link to the SecurityUserEventType table.
	[timeStamp] DATETIME2(7) NOT NULL,
	[comments] NVARCHAR(1000) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityUserEvent_SecurityUser_securityUserId] FOREIGN KEY ([securityUserId]) REFERENCES [Security].[SecurityUser] ([id]),		-- Foreign key to the SecurityUser table.
	CONSTRAINT [FK_SecurityUserEvent_SecurityUserEventType_securityUserEventTypeId] FOREIGN KEY ([securityUserEventTypeId]) REFERENCES [Security].[SecurityUserEventType] ([id])		-- Foreign key to the SecurityUserEventType table.
)
GO

-- Index on the SecurityUserEvent table's securityUserId field.
CREATE INDEX [I_SecurityUserEvent_securityUserId] ON [Security].[SecurityUserEvent] ([securityUserId])
GO

-- Index on the SecurityUserEvent table's securityUserEventTypeId field.
CREATE INDEX [I_SecurityUserEvent_securityUserEventTypeId] ON [Security].[SecurityUserEvent] ([securityUserEventTypeId])
GO

-- Index on the SecurityUserEvent table's timeStamp field.
CREATE INDEX [I_SecurityUserEvent_timeStamp] ON [Security].[SecurityUserEvent] ([timeStamp])
GO

-- Index on the SecurityUserEvent table's active field.
CREATE INDEX [I_SecurityUserEvent_active] ON [Security].[SecurityUserEvent] ([active])
GO

-- Index on the SecurityUserEvent table's deleted field.
CREATE INDEX [I_SecurityUserEvent_deleted] ON [Security].[SecurityUserEvent] ([deleted])
GO

-- Index on the SecurityUserEvent table's id,active,deleted fields.
CREATE INDEX [I_SecurityUserEvent_id_active_deleted] ON [Security].[SecurityUserEvent] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityUserPasswordResetToken]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityUserId] INT NOT NULL,		-- Link to the SecurityUser table.
	[token] NVARCHAR(250) NOT NULL,		-- The token to use for this password reset request
	[timeStamp] DATETIME2(7) NOT NULL,		-- The point in time when this request was created.
	[expiry] DATETIME2(7) NOT NULL,		-- The expiry time for this password reset request
	[systemInitiated] BIT NOT NULL DEFAULT 0,		-- Whether or not this token reset process was system initiated or not
	[completed] BIT NOT NULL DEFAULT 0,		-- Whether or not this token reset process is completed
	[comments] NVARCHAR(1000) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityUserPasswordResetToken_SecurityUser_securityUserId] FOREIGN KEY ([securityUserId]) REFERENCES [Security].[SecurityUser] ([id])		-- Foreign key to the SecurityUser table.
)
GO

-- Index on the SecurityUserPasswordResetToken table's securityUserId field.
CREATE INDEX [I_SecurityUserPasswordResetToken_securityUserId] ON [Security].[SecurityUserPasswordResetToken] ([securityUserId])
GO

-- Index on the SecurityUserPasswordResetToken table's token field.
CREATE INDEX [I_SecurityUserPasswordResetToken_token] ON [Security].[SecurityUserPasswordResetToken] ([token])
GO

-- Index on the SecurityUserPasswordResetToken table's timeStamp field.
CREATE INDEX [I_SecurityUserPasswordResetToken_timeStamp] ON [Security].[SecurityUserPasswordResetToken] ([timeStamp])
GO

-- Index on the SecurityUserPasswordResetToken table's expiry field.
CREATE INDEX [I_SecurityUserPasswordResetToken_expiry] ON [Security].[SecurityUserPasswordResetToken] ([expiry])
GO

-- Index on the SecurityUserPasswordResetToken table's systemInitiated field.
CREATE INDEX [I_SecurityUserPasswordResetToken_systemInitiated] ON [Security].[SecurityUserPasswordResetToken] ([systemInitiated])
GO

-- Index on the SecurityUserPasswordResetToken table's completed field.
CREATE INDEX [I_SecurityUserPasswordResetToken_completed] ON [Security].[SecurityUserPasswordResetToken] ([completed])
GO

-- Index on the SecurityUserPasswordResetToken table's active field.
CREATE INDEX [I_SecurityUserPasswordResetToken_active] ON [Security].[SecurityUserPasswordResetToken] ([active])
GO

-- Index on the SecurityUserPasswordResetToken table's deleted field.
CREATE INDEX [I_SecurityUserPasswordResetToken_deleted] ON [Security].[SecurityUserPasswordResetToken] ([deleted])
GO

-- Index on the SecurityUserPasswordResetToken table's id,active,deleted fields.
CREATE INDEX [I_SecurityUserPasswordResetToken_id_active_deleted] ON [Security].[SecurityUserPasswordResetToken] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityGroup]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the SecurityGroup table's name field.
CREATE INDEX [I_SecurityGroup_name] ON [Security].[SecurityGroup] ([name])
GO

-- Index on the SecurityGroup table's active field.
CREATE INDEX [I_SecurityGroup_active] ON [Security].[SecurityGroup] ([active])
GO

-- Index on the SecurityGroup table's deleted field.
CREATE INDEX [I_SecurityGroup_deleted] ON [Security].[SecurityGroup] ([deleted])
GO

-- Index on the SecurityGroup table's id,active,deleted fields.
CREATE INDEX [I_SecurityGroup_id_active_deleted] ON [Security].[SecurityGroup] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityUserSecurityGroup]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityUserId] INT NOT NULL,		-- Link to the SecurityUser table.
	[securityGroupId] INT NOT NULL,		-- Link to the SecurityGroup table.
	[comments] NVARCHAR(1000) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityUserSecurityGroup_SecurityUser_securityUserId] FOREIGN KEY ([securityUserId]) REFERENCES [Security].[SecurityUser] ([id]),		-- Foreign key to the SecurityUser table.
	CONSTRAINT [FK_SecurityUserSecurityGroup_SecurityGroup_securityGroupId] FOREIGN KEY ([securityGroupId]) REFERENCES [Security].[SecurityGroup] ([id]),		-- Foreign key to the SecurityGroup table.
	CONSTRAINT [UC_SecurityUserSecurityGroup_securityUserId_securityGroupId] UNIQUE ( [securityUserId], [securityGroupId]) 		-- Uniqueness enforced on the SecurityUserSecurityGroup table's securityUserId and securityGroupId fields.
)
GO

-- Index on the SecurityUserSecurityGroup table's securityUserId field.
CREATE INDEX [I_SecurityUserSecurityGroup_securityUserId] ON [Security].[SecurityUserSecurityGroup] ([securityUserId])
GO

-- Index on the SecurityUserSecurityGroup table's securityGroupId field.
CREATE INDEX [I_SecurityUserSecurityGroup_securityGroupId] ON [Security].[SecurityUserSecurityGroup] ([securityGroupId])
GO

-- Index on the SecurityUserSecurityGroup table's active field.
CREATE INDEX [I_SecurityUserSecurityGroup_active] ON [Security].[SecurityUserSecurityGroup] ([active])
GO

-- Index on the SecurityUserSecurityGroup table's deleted field.
CREATE INDEX [I_SecurityUserSecurityGroup_deleted] ON [Security].[SecurityUserSecurityGroup] ([deleted])
GO

-- Index on the SecurityUserSecurityGroup table's id,active,deleted fields.
CREATE INDEX [I_SecurityUserSecurityGroup_id_active_deleted] ON [Security].[SecurityUserSecurityGroup] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[Privilege]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL
)
GO

-- Index on the Privilege table's name field.
CREATE INDEX [I_Privilege_name] ON [Security].[Privilege] ([name])
GO

INSERT INTO [Security].[Privilege] ( [name], [description] ) VALUES  ( 'No Access', 'No Access' )
GO

INSERT INTO [Security].[Privilege] ( [name], [description] ) VALUES  ( 'Anonymous Read Only', 'Read Only Access, With All Sensitive Data Redacted' )
GO

INSERT INTO [Security].[Privilege] ( [name], [description] ) VALUES  ( 'Read Only', 'Read Only Access For General Use' )
GO

INSERT INTO [Security].[Privilege] ( [name], [description] ) VALUES  ( 'Read and Write', 'Read and Write Access' )
GO

INSERT INTO [Security].[Privilege] ( [name], [description] ) VALUES  ( 'Administrative', 'Complete Administrative Access' )
GO

INSERT INTO [Security].[Privilege] ( [name], [description] ) VALUES  ( 'Custom', 'Custom Access Level' )
GO


CREATE TABLE [Security].[SecurityRole]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[privilegeId] INT NOT NULL,		-- Link to the Privilege table.
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL,
	[comments] NVARCHAR(1000) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityRole_Privilege_privilegeId] FOREIGN KEY ([privilegeId]) REFERENCES [Security].[Privilege] ([id])		-- Foreign key to the Privilege table.
)
GO

-- Index on the SecurityRole table's privilegeId field.
CREATE INDEX [I_SecurityRole_privilegeId] ON [Security].[SecurityRole] ([privilegeId])
GO

-- Index on the SecurityRole table's name field.
CREATE INDEX [I_SecurityRole_name] ON [Security].[SecurityRole] ([name])
GO

-- Index on the SecurityRole table's active field.
CREATE INDEX [I_SecurityRole_active] ON [Security].[SecurityRole] ([active])
GO

-- Index on the SecurityRole table's deleted field.
CREATE INDEX [I_SecurityRole_deleted] ON [Security].[SecurityRole] ([deleted])
GO

-- Index on the SecurityRole table's id,active,deleted fields.
CREATE INDEX [I_SecurityRole_id_active_deleted] ON [Security].[SecurityRole] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityUserSecurityRole]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityUserId] INT NOT NULL,		-- Link to the SecurityUser table.
	[securityRoleId] INT NOT NULL,		-- Link to the SecurityRole table.
	[comments] NVARCHAR(1000) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityUserSecurityRole_SecurityUser_securityUserId] FOREIGN KEY ([securityUserId]) REFERENCES [Security].[SecurityUser] ([id]),		-- Foreign key to the SecurityUser table.
	CONSTRAINT [FK_SecurityUserSecurityRole_SecurityRole_securityRoleId] FOREIGN KEY ([securityRoleId]) REFERENCES [Security].[SecurityRole] ([id]),		-- Foreign key to the SecurityRole table.
	CONSTRAINT [UC_SecurityUserSecurityRole_securityUserId_securityRoleId] UNIQUE ( [securityUserId], [securityRoleId]) 		-- Uniqueness enforced on the SecurityUserSecurityRole table's securityUserId and securityRoleId fields.
)
GO

-- Index on the SecurityUserSecurityRole table's securityUserId field.
CREATE INDEX [I_SecurityUserSecurityRole_securityUserId] ON [Security].[SecurityUserSecurityRole] ([securityUserId])
GO

-- Index on the SecurityUserSecurityRole table's securityRoleId field.
CREATE INDEX [I_SecurityUserSecurityRole_securityRoleId] ON [Security].[SecurityUserSecurityRole] ([securityRoleId])
GO

-- Index on the SecurityUserSecurityRole table's active field.
CREATE INDEX [I_SecurityUserSecurityRole_active] ON [Security].[SecurityUserSecurityRole] ([active])
GO

-- Index on the SecurityUserSecurityRole table's deleted field.
CREATE INDEX [I_SecurityUserSecurityRole_deleted] ON [Security].[SecurityUserSecurityRole] ([deleted])
GO

-- Index on the SecurityUserSecurityRole table's id,active,deleted fields.
CREATE INDEX [I_SecurityUserSecurityRole_id_active_deleted] ON [Security].[SecurityUserSecurityRole] ([id], [active], [deleted])
GO

-- Index on the SecurityUserSecurityRole table's securityUserId,active,deleted fields.
CREATE INDEX [I_SecurityUserSecurityRole_securityUserId_active_deleted] ON [Security].[SecurityUserSecurityRole] ([securityUserId], [active], [deleted])
GO

-- Index on the SecurityUserSecurityRole table's securityRoleId,active,deleted fields.
CREATE INDEX [I_SecurityUserSecurityRole_securityRoleId_active_deleted] ON [Security].[SecurityUserSecurityRole] ([securityRoleId], [active], [deleted])
GO


CREATE TABLE [Security].[SecurityGroupSecurityRole]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityGroupId] INT NOT NULL,		-- Link to the SecurityGroup table.
	[securityRoleId] INT NOT NULL,		-- Link to the SecurityRole table.
	[comments] NVARCHAR(1000) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SecurityGroupSecurityRole_SecurityGroup_securityGroupId] FOREIGN KEY ([securityGroupId]) REFERENCES [Security].[SecurityGroup] ([id]),		-- Foreign key to the SecurityGroup table.
	CONSTRAINT [FK_SecurityGroupSecurityRole_SecurityRole_securityRoleId] FOREIGN KEY ([securityRoleId]) REFERENCES [Security].[SecurityRole] ([id]),		-- Foreign key to the SecurityRole table.
	CONSTRAINT [UC_SecurityGroupSecurityRole_securityGroupId_securityRoleId] UNIQUE ( [securityGroupId], [securityRoleId]) 		-- Uniqueness enforced on the SecurityGroupSecurityRole table's securityGroupId and securityRoleId fields.
)
GO

-- Index on the SecurityGroupSecurityRole table's securityGroupId field.
CREATE INDEX [I_SecurityGroupSecurityRole_securityGroupId] ON [Security].[SecurityGroupSecurityRole] ([securityGroupId])
GO

-- Index on the SecurityGroupSecurityRole table's securityRoleId field.
CREATE INDEX [I_SecurityGroupSecurityRole_securityRoleId] ON [Security].[SecurityGroupSecurityRole] ([securityRoleId])
GO

-- Index on the SecurityGroupSecurityRole table's active field.
CREATE INDEX [I_SecurityGroupSecurityRole_active] ON [Security].[SecurityGroupSecurityRole] ([active])
GO

-- Index on the SecurityGroupSecurityRole table's deleted field.
CREATE INDEX [I_SecurityGroupSecurityRole_deleted] ON [Security].[SecurityGroupSecurityRole] ([deleted])
GO

-- Index on the SecurityGroupSecurityRole table's id,active,deleted fields.
CREATE INDEX [I_SecurityGroupSecurityRole_id_active_deleted] ON [Security].[SecurityGroupSecurityRole] ([id], [active], [deleted])
GO

-- Index on the SecurityGroupSecurityRole table's securityGroupId,active,deleted fields.
CREATE INDEX [I_SecurityGroupSecurityRole_securityGroupId_active_deleted] ON [Security].[SecurityGroupSecurityRole] ([securityGroupId], [active], [deleted])
GO

-- Index on the SecurityGroupSecurityRole table's securityRoleId,active,deleted fields.
CREATE INDEX [I_SecurityGroupSecurityRole_securityRoleId_active_deleted] ON [Security].[SecurityGroupSecurityRole] ([securityRoleId], [active], [deleted])
GO


CREATE TABLE [Security].[Module]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the Module table's name field.
CREATE INDEX [I_Module_name] ON [Security].[Module] ([name])
GO

-- Index on the Module table's active field.
CREATE INDEX [I_Module_active] ON [Security].[Module] ([active])
GO

-- Index on the Module table's deleted field.
CREATE INDEX [I_Module_deleted] ON [Security].[Module] ([deleted])
GO

-- Index on the Module table's id,active,deleted fields.
CREATE INDEX [I_Module_id_active_deleted] ON [Security].[Module] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[ModuleSecurityRole]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[moduleId] INT NOT NULL,		-- Link to the Module table.
	[securityRoleId] INT NOT NULL,		-- Link to the SecurityRole table.
	[comments] NVARCHAR(1000) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ModuleSecurityRole_Module_moduleId] FOREIGN KEY ([moduleId]) REFERENCES [Security].[Module] ([id]),		-- Foreign key to the Module table.
	CONSTRAINT [FK_ModuleSecurityRole_SecurityRole_securityRoleId] FOREIGN KEY ([securityRoleId]) REFERENCES [Security].[SecurityRole] ([id]),		-- Foreign key to the SecurityRole table.
	CONSTRAINT [UC_ModuleSecurityRole_moduleId_securityRoleId] UNIQUE ( [moduleId], [securityRoleId]) 		-- Uniqueness enforced on the ModuleSecurityRole table's moduleId and securityRoleId fields.
)
GO

-- Index on the ModuleSecurityRole table's moduleId field.
CREATE INDEX [I_ModuleSecurityRole_moduleId] ON [Security].[ModuleSecurityRole] ([moduleId])
GO

-- Index on the ModuleSecurityRole table's securityRoleId field.
CREATE INDEX [I_ModuleSecurityRole_securityRoleId] ON [Security].[ModuleSecurityRole] ([securityRoleId])
GO

-- Index on the ModuleSecurityRole table's active field.
CREATE INDEX [I_ModuleSecurityRole_active] ON [Security].[ModuleSecurityRole] ([active])
GO

-- Index on the ModuleSecurityRole table's deleted field.
CREATE INDEX [I_ModuleSecurityRole_deleted] ON [Security].[ModuleSecurityRole] ([deleted])
GO

-- Index on the ModuleSecurityRole table's id,active,deleted fields.
CREATE INDEX [I_ModuleSecurityRole_id_active_deleted] ON [Security].[ModuleSecurityRole] ([id], [active], [deleted])
GO

-- Index on the ModuleSecurityRole table's securityRoleId,active,deleted fields.
CREATE INDEX [I_ModuleSecurityRole_securityRoleId_active_deleted] ON [Security].[ModuleSecurityRole] ([securityRoleId], [active], [deleted])
GO


CREATE TABLE [Security].[SystemSetting]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL,
	[value] NVARCHAR(MAX) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the SystemSetting table's name field.
CREATE INDEX [I_SystemSetting_name] ON [Security].[SystemSetting] ([name])
GO

-- Index on the SystemSetting table's active field.
CREATE INDEX [I_SystemSetting_active] ON [Security].[SystemSetting] ([active])
GO

-- Index on the SystemSetting table's deleted field.
CREATE INDEX [I_SystemSetting_deleted] ON [Security].[SystemSetting] ([deleted])
GO

-- Index on the SystemSetting table's id,active,deleted fields.
CREATE INDEX [I_SystemSetting_id_active_deleted] ON [Security].[SystemSetting] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[LoginAttempt]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[timeStamp] DATETIME2(7) NOT NULL,
	[userName] NVARCHAR(250) NULL,
	[passwordHash] INT NULL,
	[resource] NVARCHAR(500) NULL,
	[sessionId] NVARCHAR(50) NULL,
	[ipAddress] NVARCHAR(50) NULL,
	[userAgent] NVARCHAR(250) NULL,
	[value] NVARCHAR(MAX) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the LoginAttempt table's active field.
CREATE INDEX [I_LoginAttempt_active] ON [Security].[LoginAttempt] ([active])
GO

-- Index on the LoginAttempt table's deleted field.
CREATE INDEX [I_LoginAttempt_deleted] ON [Security].[LoginAttempt] ([deleted])
GO

-- Index on the LoginAttempt table's id,active,deleted fields.
CREATE INDEX [I_loginAttempt_id_active_deleted] ON [Security].[LoginAttempt] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[EntityDataToken]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityUserId] INT NOT NULL,		-- Link to the SecurityUser table.
	[moduleId] INT NOT NULL,		-- Link to the Module table.
	[entity] NVARCHAR(250) NOT NULL,
	[sessionId] NVARCHAR(50) NOT NULL,
	[authenticationToken] NVARCHAR(50) NOT NULL,		-- This is the authentication token that gets set into the user data of the forms authentication ticket
	[token] NVARCHAR(50) NOT NULL UNIQUE,
	[timeStamp] DATETIME2(7) NOT NULL,
	[comments] NVARCHAR(1000) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_EntityDataToken_SecurityUser_securityUserId] FOREIGN KEY ([securityUserId]) REFERENCES [Security].[SecurityUser] ([id]),		-- Foreign key to the SecurityUser table.
	CONSTRAINT [FK_EntityDataToken_Module_moduleId] FOREIGN KEY ([moduleId]) REFERENCES [Security].[Module] ([id])		-- Foreign key to the Module table.
)
GO

-- Index on the EntityDataToken table's securityUserId field.
CREATE INDEX [I_EntityDataToken_securityUserId] ON [Security].[EntityDataToken] ([securityUserId])
GO

-- Index on the EntityDataToken table's moduleId field.
CREATE INDEX [I_EntityDataToken_moduleId] ON [Security].[EntityDataToken] ([moduleId])
GO

-- Index on the EntityDataToken table's active field.
CREATE INDEX [I_EntityDataToken_active] ON [Security].[EntityDataToken] ([active])
GO

-- Index on the EntityDataToken table's deleted field.
CREATE INDEX [I_EntityDataToken_deleted] ON [Security].[EntityDataToken] ([deleted])
GO

-- Index on the EntityDataToken table's token field.
CREATE INDEX [I_EntityDataToken_token] ON [Security].[EntityDataToken] ([token])
GO

-- Index on the EntityDataToken table's securityUserId,moduleId,sessionId fields.
CREATE INDEX [I_EntityDataToken_securityUserId_moduleId_sessionId] ON [Security].[EntityDataToken] ([securityUserId], [moduleId], [sessionId])
GO

-- Index on the EntityDataToken table's securityUserId,moduleId,token,sessionId fields.
CREATE INDEX [I_EntityDataToken_securityUserId_moduleId_token_sessionId] ON [Security].[EntityDataToken] ([securityUserId], [moduleId], [token], [sessionId])
GO

-- Index on the EntityDataToken table's id,active,deleted fields.
CREATE INDEX [I_EntityDataToken_id_active_deleted] ON [Security].[EntityDataToken] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[EntityDataTokenEventType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL
)
GO

-- Index on the EntityDataTokenEventType table's name field.
CREATE INDEX [I_EntityDataTokenEventType_name] ON [Security].[EntityDataTokenEventType] ([name])
GO

INSERT INTO [Security].[EntityDataTokenEventType] ( [name], [description] ) VALUES  ( 'ReadFromEntity', 'Read From Entity' )
GO

INSERT INTO [Security].[EntityDataTokenEventType] ( [name], [description] ) VALUES  ( 'CascadeValidatedReadFromEntity', 'Cascade Validated Read From Entity' )
GO

INSERT INTO [Security].[EntityDataTokenEventType] ( [name], [description] ) VALUES  ( 'WriteToEntity', 'Write To Entity' )
GO

INSERT INTO [Security].[EntityDataTokenEventType] ( [name], [description] ) VALUES  ( 'CascadeValidatedWriteToEntity', 'Cascade Validated Write To Entity' )
GO

INSERT INTO [Security].[EntityDataTokenEventType] ( [name], [description] ) VALUES  ( 'ReuseExistingToken', 'Reuse Existing Token' )
GO


CREATE TABLE [Security].[EntityDataTokenEvent]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[entityDataTokenId] INT NOT NULL,		-- Link to the EntityDataToken table.
	[entityDataTokenEventTypeId] INT NOT NULL,		-- Link to the EntityDataTokenEventType table.
	[timeStamp] DATETIME2(7) NOT NULL,
	[comments] NVARCHAR(1000) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_EntityDataTokenEvent_EntityDataToken_entityDataTokenId] FOREIGN KEY ([entityDataTokenId]) REFERENCES [Security].[EntityDataToken] ([id]),		-- Foreign key to the EntityDataToken table.
	CONSTRAINT [FK_EntityDataTokenEvent_EntityDataTokenEventType_entityDataTokenEventTypeId] FOREIGN KEY ([entityDataTokenEventTypeId]) REFERENCES [Security].[EntityDataTokenEventType] ([id])		-- Foreign key to the EntityDataTokenEventType table.
)
GO

-- Index on the EntityDataTokenEvent table's entityDataTokenId field.
CREATE INDEX [I_EntityDataTokenEvent_entityDataTokenId] ON [Security].[EntityDataTokenEvent] ([entityDataTokenId])
GO

-- Index on the EntityDataTokenEvent table's entityDataTokenEventTypeId field.
CREATE INDEX [I_EntityDataTokenEvent_entityDataTokenEventTypeId] ON [Security].[EntityDataTokenEvent] ([entityDataTokenEventTypeId])
GO

-- Index on the EntityDataTokenEvent table's active field.
CREATE INDEX [I_EntityDataTokenEvent_active] ON [Security].[EntityDataTokenEvent] ([active])
GO

-- Index on the EntityDataTokenEvent table's deleted field.
CREATE INDEX [I_EntityDataTokenEvent_deleted] ON [Security].[EntityDataTokenEvent] ([deleted])
GO

-- Index on the EntityDataTokenEvent table's id,active,deleted fields.
CREATE INDEX [I_EntityDataTokenEvent_id_active_deleted] ON [Security].[EntityDataTokenEvent] ([id], [active], [deleted])
GO


CREATE TABLE [Security].[OAUTHToken]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[token] NVARCHAR(250) NOT NULL,
	[expiryDateTime] DATETIME2(7) NOT NULL,
	[userData] NVARCHAR(1000) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the OAUTHToken table's token field.
CREATE INDEX [I_OAUTHToken_token] ON [Security].[OAUTHToken] ([token])
GO

-- Index on the OAUTHToken table's expiryDateTime field.
CREATE INDEX [I_OAUTHToken_expiryDateTime] ON [Security].[OAUTHToken] ([expiryDateTime])
GO

-- Index on the OAUTHToken table's active field.
CREATE INDEX [I_OAUTHToken_active] ON [Security].[OAUTHToken] ([active])
GO

-- Index on the OAUTHToken table's deleted field.
CREATE INDEX [I_OAUTHToken_deleted] ON [Security].[OAUTHToken] ([deleted])
GO

-- Index on the OAUTHToken table's id,active,deleted fields.
CREATE INDEX [I_OauthToken_id_active_deleted] ON [Security].[OAUTHToken] ([id], [active], [deleted])
GO


