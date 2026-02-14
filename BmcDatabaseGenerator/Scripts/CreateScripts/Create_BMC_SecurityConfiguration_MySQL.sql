-- 
-- Define the BMC Module
-- 
INSERT INTO `Module` ( `name`, `description` ) VALUES  ( 'BMC', 'The BMC Module' );


-- 
-- Define the BMC Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'BMC Administrator', 'BMC Administrator Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Administrative' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'BMC Reader', 'BMC Reader Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Read Only' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'BMC Reader and Writer', 'BMC Reader and Writer Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Read and Write' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'BMC No Access', 'BMC No Access Role', ( SELECT id FROM `Privilege` WHERE `name` = 'No Access' LIMIT 1 ) );


-- 
-- Link the BMC module to the roles
-- 
INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'BMC' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'BMC Administrator' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'BMC' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'BMC Reader' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'BMC' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'BMC Reader and Writer' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'BMC' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'BMC No Access' LIMIT 1 ) );


-- 
-- Define the custom roles for the BMC module
-- 
INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'BMC Catalog Writer', 'BMC Catalog Writer Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Custom' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'BMC Collection Writer', 'BMC Collection Writer Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Custom' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'BMC Instruction Writer', 'BMC Instruction Writer Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Custom' LIMIT 1 ) );


INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'BMC' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'BMC Catalog Writer' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'BMC' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'BMC Collection Writer' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'BMC' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'BMC Instruction Writer' LIMIT 1 ) );


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO `SecurityUserSecurityRole` ( `securityUserId`, `securityRoleId`, `active`, `deleted` ) VALUES  ( ( SELECT id FROM `SecurityUser` WHERE `accountName` = 'Admin' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'BMC Administrator' LIMIT 1 ), '1', '0' );


