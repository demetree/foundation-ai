-- 
-- Define the Scheduler Module
-- 
INSERT INTO [Security].[Module] ( [name], [description] ) VALUES  ( 'Scheduler', 'The Scheduler Module' )
GO


-- 
-- Define the Scheduler Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Scheduler Administrator', 'Scheduler Administrator Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Administrative' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Scheduler Reader', 'Scheduler Reader Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Read Only' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Scheduler Reader and Writer', 'Scheduler Reader and Writer Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Read and Write' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Scheduler No Access', 'Scheduler No Access Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'No Access' ) )
GO


-- 
-- Link the Scheduler module to the roles
-- 
INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Scheduler' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Scheduler Administrator' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Scheduler' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Scheduler Reader' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Scheduler' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Scheduler Reader and Writer' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Scheduler' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Scheduler No Access' ) )
GO


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO [Security].[SecurityUserSecurityRole] ( [securityUserId], [securityRoleId], [active], [deleted] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[SecurityUser] WHERE [accountName] = 'Admin' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Scheduler Administrator' ), '1', '0' )
GO


