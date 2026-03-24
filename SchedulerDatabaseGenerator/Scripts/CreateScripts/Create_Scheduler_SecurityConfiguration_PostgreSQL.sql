-- 
-- Define the Scheduler Module
-- 
INSERT INTO "Security"."Module" ( "name", "description" ) VALUES  ( 'Scheduler', 'The Scheduler Module' );


-- 
-- Define the Scheduler Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Scheduler Administrator', 'Scheduler Administrator Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Administrative' LIMIT 1) );

INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Scheduler Reader', 'Scheduler Reader Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Read Only' LIMIT 1) );

INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Scheduler Reader and Writer', 'Scheduler Reader and Writer Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Read and Write' LIMIT 1) );

INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Scheduler No Access', 'Scheduler No Access Role', ( SELECT id FROM "Privilege" WHERE "name" = 'No Access' LIMIT 1) );


-- 
-- Link the Scheduler module to the roles
-- 
INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Scheduler' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Scheduler Administrator' LIMIT 1) );

INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Scheduler' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Scheduler Reader' LIMIT 1) );

INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Scheduler' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Scheduler Reader and Writer' LIMIT 1) );

INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Scheduler' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Scheduler No Access' LIMIT 1) );


-- 
-- Define the custom roles for the Scheduler module
-- 
INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Scheduler Config Writer', 'Scheduler Config Writer Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Custom' LIMIT 1) );

INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Scheduler Contact Writer', 'Scheduler Contact Writer Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Custom' LIMIT 1) );

INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Scheduler Resource Writer', 'Scheduler Resource Writer Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Custom' LIMIT 1) );

INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Fundraising Manager', 'Fundraising Manager Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Custom' LIMIT 1) );

INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Volunteer Manager', 'Volunteer Manager Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Custom' LIMIT 1) );


INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Scheduler' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Scheduler Config Writer' LIMIT 1) );

INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Scheduler' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Scheduler Contact Writer' LIMIT 1) );

INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Scheduler' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Scheduler Resource Writer' LIMIT 1) );

INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Scheduler' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Fundraising Manager' LIMIT 1) );

INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Scheduler' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Volunteer Manager' LIMIT 1) );


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO "Security"."SecurityUserSecurityRole" ( "securityUserId", "securityRoleId", "active", "deleted" ) VALUES  ( ( SELECT id FROM "SecurityUser" WHERE "accountName" = 'Admin' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Scheduler Administrator' LIMIT 1), '1', '0' );


