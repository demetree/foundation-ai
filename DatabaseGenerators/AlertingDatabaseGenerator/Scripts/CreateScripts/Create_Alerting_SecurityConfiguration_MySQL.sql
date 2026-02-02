-- 
-- Define the Alerting Module
-- 
INSERT INTO `Module` ( `name`, `description` ) VALUES  ( 'Alerting', 'The Alerting Module' );


-- 
-- Define the Alerting Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'Alerting Administrator', 'Alerting Administrator Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Administrative' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'Alerting Reader', 'Alerting Reader Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Read Only' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'Alerting Reader and Writer', 'Alerting Reader and Writer Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Read and Write' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'Alerting No Access', 'Alerting No Access Role', ( SELECT id FROM `Privilege` WHERE `name` = 'No Access' LIMIT 1 ) );


-- 
-- Link the Alerting module to the roles
-- 
INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'Alerting' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Alerting Administrator' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'Alerting' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Alerting Reader' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'Alerting' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Alerting Reader and Writer' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'Alerting' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Alerting No Access' LIMIT 1 ) );


-- 
-- Define the custom roles for the Alerting module
-- 
INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'Alerting User Writer', 'Alerting User Writer Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Custom' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'Alerting Schedule Writer', 'Alerting Schedule Writer Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Custom' LIMIT 1 ) );

INSERT INTO `SecurityRole` ( `name`, `description`, `privilegeId` ) VALUES  ( 'Alerting Master Config Writer', 'Alerting Master Config Writer Role', ( SELECT id FROM `Privilege` WHERE `name` = 'Custom' LIMIT 1 ) );


INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'Alerting' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Alerting User Writer' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'Alerting' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Alerting Schedule Writer' LIMIT 1 ) );

INSERT INTO `ModuleSecurityRole` ( `moduleId`, `securityRoleId` ) VALUES  ( ( SELECT id FROM `Module` WHERE `name` = 'Alerting' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Alerting Master Config Writer' LIMIT 1 ) );


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO `SecurityUserSecurityRole` ( `securityUserId`, `securityRoleId`, `active`, `deleted` ) VALUES  ( ( SELECT id FROM `SecurityUser` WHERE `accountName` = 'Admin' LIMIT 1 ), ( SELECT id FROM `SecurityRole` WHERE `name` = 'Alerting Administrator' LIMIT 1 ), '1', '0' );


