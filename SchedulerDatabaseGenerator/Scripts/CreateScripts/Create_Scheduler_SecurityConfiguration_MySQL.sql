-- 
-- Define the Scheduler Module
-- 
INSERT INTO `Module` ( `name`, `description` ) VALUES  ( 'Scheduler', 'The Scheduler Module' );


-- 
-- Define the Scheduler Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'Scheduler Administrator', 'Scheduler Administrator Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Administrative' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'Scheduler Reader', 'Scheduler Reader Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Read Only' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'Scheduler Reader and Writer', 'Scheduler Reader and Writer Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Read and Write' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'Scheduler No Access', 'Scheduler No Access Role', ( SELECT id FROM `Privilege` WHERE `name` = 'No Access' LIMIT 1 ) );


-- 
-- Link the Scheduler module to the roles
-- 
INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'Scheduler' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Scheduler Administrator' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'Scheduler' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Scheduler Reader' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'Scheduler' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Scheduler Reader and Writer' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'Scheduler' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Scheduler No Access' LIMIT 1 ) );


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO `SecurityUserSecurityRole` ( `securityUserId`, `securityRoleId`, `active`, `deleted` ) VALUES  ( ( SELECT id FROM `SecurityUser` WHERE `accountName` = 'Admin' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Scheduler Administrator' LIMIT 1 ), '1', '0' );


