CREATE TABLE [Security].[UserSession]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[securityUserId] INT NOT NULL,		-- Link to the SecurityUser table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL,		-- User's objectGuid for reliable identity resolution
	[tokenId] NVARCHAR(250) NULL,		-- OpenIddict token ID for correlation
	[sessionStart] DATETIME2(7) NOT NULL,		-- When the token was issued
	[expiresAt] DATETIME2(7) NOT NULL,		-- When the token expires
	[ipAddress] NVARCHAR(50) NULL,		-- Client IP address at login
	[userAgent] NVARCHAR(500) NULL,		-- Browser/client user agent
	[loginMethod] NVARCHAR(50) NULL,		-- Login method: Password, Microsoft, Google, RefreshToken
	[clientApplication] NVARCHAR(100) NULL,		-- Client application name
	[isRevoked] BIT NOT NULL DEFAULT 0,		-- Whether session has been administratively revoked
	[revokedAt] DATETIME2(7) NULL,		-- When session was revoked
	[revokedBy] NVARCHAR(100) NULL,		-- Who revoked the session (admin username)
	[revokedReason] NVARCHAR(500) NULL,		-- Reason for revocation
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserSession_SecurityUser_securityUserId] FOREIGN KEY ([securityUserId]) REFERENCES [Security].[SecurityUser] ([id])		-- Foreign key to the SecurityUser table.
)
GO

-- Index on the UserSession table's securityUserId field.
CREATE INDEX [I_UserSession_securityUserId] ON [Security].[UserSession] ([securityUserId])
GO

-- Index on the UserSession table's objectGuid field.
CREATE INDEX [I_UserSession_objectGuid] ON [Security].[UserSession] ([objectGuid])
GO

-- Index on the UserSession table's tokenId field.
CREATE INDEX [I_UserSession_tokenId] ON [Security].[UserSession] ([tokenId])
GO

-- Index on the UserSession table's sessionStart field.
CREATE INDEX [I_UserSession_sessionStart] ON [Security].[UserSession] ([sessionStart])
GO

-- Index on the UserSession table's expiresAt field.
CREATE INDEX [I_UserSession_expiresAt] ON [Security].[UserSession] ([expiresAt])
GO

-- Index on the UserSession table's loginMethod field.
CREATE INDEX [I_UserSession_loginMethod] ON [Security].[UserSession] ([loginMethod])
GO

-- Index on the UserSession table's isRevoked field.
CREATE INDEX [I_UserSession_isRevoked] ON [Security].[UserSession] ([isRevoked])
GO

-- Index on the UserSession table's active field.
CREATE INDEX [I_UserSession_active] ON [Security].[UserSession] ([active])
GO

-- Index on the UserSession table's deleted field.
CREATE INDEX [I_UserSession_deleted] ON [Security].[UserSession] ([deleted])
GO

-- Index on the UserSession table's id,active,deleted fields.
CREATE INDEX [I_UserSession_id_active_deleted] ON [Security].[UserSession] ([id], [active], [deleted])
GO

-- Index on the UserSession table's securityUserId,isRevoked,active,deleted fields.
CREATE INDEX [I_UserSession_securityUserId_isRevoked_active_deleted] ON [Security].[UserSession] ([securityUserId], [isRevoked], [active], [deleted])
GO

--
-- Couple of new fields on Login Attempt
--
ALTER TABLE [Security].[LoginAttempt] ADD [success] BIT NULL			-- null = unknown/pending, true = success, false = failure
GO

ALTER TABLE [Security].[LoginAttempt] ADD [securityUserId] INT NULL		-- Link to user if identified during login attempt
GO

ALTER TABLE [Security].[LoginAttempt] ADD CONSTRAINT [FK_LoginAttempt_SecurityUser_securityUserId] FOREIGN KEY ([securityUserId]) REFERENCES [Security].[SecurityUser] ([id])		-- Foreign key to the SecurityUser table.
GO

-- Index on the LoginAttempt table's securityUserId field.
CREATE INDEX [I_LoginAttempt_securityUserId] ON [Security].[LoginAttempt] ([securityUserId])
GO


