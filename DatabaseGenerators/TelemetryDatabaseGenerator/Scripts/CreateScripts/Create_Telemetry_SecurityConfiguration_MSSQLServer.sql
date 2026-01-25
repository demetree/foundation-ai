-- 
-- Define the Telemetry Module
-- 
INSERT INTO [Security].[Module] ( [name], [description] ) VALUES  ( 'Telemetry', 'The Telemetry Module' )
GO


-- 
-- Define the Telemetry Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Telemetry Administrator', 'Telemetry Administrator Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Administrative' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Telemetry Reader', 'Telemetry Reader Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Read Only' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Telemetry Reader and Writer', 'Telemetry Reader and Writer Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Read and Write' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'Telemetry No Access', 'Telemetry No Access Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'No Access' ) )
GO


-- 
-- Link the Telemetry module to the roles
-- 
INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Telemetry' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Telemetry Administrator' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Telemetry' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Telemetry Reader' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Telemetry' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Telemetry Reader and Writer' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'Telemetry' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Telemetry No Access' ) )
GO


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO [Security].[SecurityUserSecurityRole] ( [securityUserId], [securityRoleId], [active], [deleted] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[SecurityUser] WHERE [accountName] = 'Admin' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'Telemetry Administrator' ), '1', '0' )
GO


