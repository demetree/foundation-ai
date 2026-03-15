-- 
-- Define the Community Module
-- 
INSERT INTO [Security].[Module] ( [name], [description] ) VALUES  ( 'Community', 'The Community Module' )
GO


-- 
-- Define the Community Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Community Administrator', 'Community Administrator Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Administrative' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Community Reader', 'Community Reader Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Read Only' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Community Reader and Writer', 'Community Reader and Writer Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Read and Write' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Community No Access', 'Community No Access Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'No Access' ) )
GO


-- 
-- Link the Community module to the roles
-- 
INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Community' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Community Administrator' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Community' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Community Reader' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Community' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Community Reader and Writer' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Community' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Community No Access' ) )
GO


-- 
-- Define the custom roles for the Community module
-- 
INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Community Content Writer', 'Community Content Writer Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Custom' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Community Navigation Writer', 'Community Navigation Writer Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Custom' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Community Admin', 'Community Admin Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Custom' ) )
GO


INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Community' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Community Content Writer' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Community' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Community Navigation Writer' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Community' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Community Admin' ) )
GO


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO [Security].[SecurityUserSecurityRole] ( [securityUserId], [securityRoleId], [active], [deleted] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[SecurityUser] WHERE [accountName] = 'Admin' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Community Administrator' ), '1', '0' )
GO


