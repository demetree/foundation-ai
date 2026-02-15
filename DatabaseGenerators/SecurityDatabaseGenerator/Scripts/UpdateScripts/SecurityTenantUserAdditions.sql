
ALTER TABLE Security.SecurityTenantUser ADD [isOwner] BIT NOT NULL DEFAULT 0		-- Whether this user is the owner/creator of the tenant. Only owners can invite/remove members and manage tenant settings.
ALTER TABLE Security.SecurityTenantUser ADD [canRead] BIT NOT NULL DEFAULT 0		-- Whether this user has read access to the tenant's data.
ALTER TABLE Security.SecurityTenantUser ADD [canWrite] BIT NOT NULL DEFAULT 0		-- Whether this user has write access to the tenant's data.
GO
