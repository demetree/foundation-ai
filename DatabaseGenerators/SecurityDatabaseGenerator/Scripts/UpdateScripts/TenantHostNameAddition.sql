ALTER TABLE [Security].[SecurityTenant] ADD [hostname] NVARCHAR(250) NULL		-- The hostname used for HTTP Host header tenant resolution. E.g. 'pettyharbour.example.com'. Used by multi-tenant public-facing apps like Community CMS to determine which tenant's data to serve.
GO

-- Index on the SecurityTenant table's hostName field.
CREATE UNIQUE INDEX [I_SecurityTenant_hostName] ON [Security].[SecurityTenant] ([hostName])
 WHERE [hostName] IS NOT NULL
GO
