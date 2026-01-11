-- 
-- Define the Auditor Module
-- 
INSERT INTO [Security].[Module] ( [name], [description] ) VALUES  ( 'Auditor', 'The Auditor Module' )
GO


-- 
-- Define the Auditor Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Auditor Administrator', 'Auditor Administrator Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Administrative' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Auditor Reader', 'Auditor Reader Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Read Only' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Auditor Reader and Writer', 'Auditor Reader and Writer Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Read and Write' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Auditor No Access', 'Auditor No Access Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'No Access' ) )
GO


-- 
-- Link the Auditor module to the roles
-- 
INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Auditor' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Auditor Administrator' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Auditor' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Auditor Reader' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Auditor' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Auditor Reader and Writer' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Auditor' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Auditor No Access' ) )
GO


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO [Security].[SecurityUserSecurityRole] ( [securityUserId], [securityRoleId], [active], [deleted] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[SecurityUser] WHERE [accountName] = 'Admin' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Auditor Administrator' ), '1', '0' )
GO


