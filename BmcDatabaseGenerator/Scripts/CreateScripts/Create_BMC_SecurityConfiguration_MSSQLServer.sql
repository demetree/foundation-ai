-- 
-- Define the BMC Module
-- 
INSERT INTO [Security].[Module] ( [name], [description] ) VALUES  ( 'BMC', 'The BMC Module' )
GO


-- 
-- Define the BMC Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'BMC Administrator', 'BMC Administrator Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Administrative' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'BMC Reader', 'BMC Reader Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Read Only' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'BMC Reader and Writer', 'BMC Reader and Writer Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Read and Write' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'BMC No Access', 'BMC No Access Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'No Access' ) )
GO


-- 
-- Link the BMC module to the roles
-- 
INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'BMC' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'BMC Administrator' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'BMC' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'BMC Reader' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'BMC' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'BMC Reader and Writer' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'BMC' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'BMC No Access' ) )
GO


-- 
-- Define the custom roles for the BMC module
-- 
INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'BMC Catalog Writer', 'BMC Catalog Writer Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Custom' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'BMC Collection Writer', 'BMC Collection Writer Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Custom' ) )
GO

INSERT INTO [Security].[SecurityRole] ( [name], [description], [privilegeId] ) VALUES  ( 'BMC Instruction Writer', 'BMC Instruction Writer Role', ( SELECT TOP 1 id FROM [Security].[Privilege] WHERE [name] = 'Custom' ) )
GO


INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'BMC' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'BMC Catalog Writer' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'BMC' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'BMC Collection Writer' ) )
GO

INSERT INTO [Security].[ModuleSecurityRole] ( [moduleId], [securityRoleId] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[Module] WHERE [name] = 'BMC' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'BMC Instruction Writer' ) )
GO


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO [Security].[SecurityUserSecurityRole] ( [securityUserId], [securityRoleId], [active], [deleted] ) VALUES  ( ( SELECT TOP 1 id FROM [Security].[SecurityUser] WHERE [accountName] = 'Admin' ), ( SELECT TOP 1 id FROM [Security].[SecurityRole] WHERE [name] = 'BMC Administrator' ), '1', '0' )
GO


