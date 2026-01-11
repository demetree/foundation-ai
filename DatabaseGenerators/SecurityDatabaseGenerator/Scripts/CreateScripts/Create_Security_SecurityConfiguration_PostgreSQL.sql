-- 
-- Define the Security Module
-- 
INSERT INTO "Security"."Module" ( "name", "description" ) VALUES  ( 'Security', 'The Security Module' );


-- 
-- Define the Security Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Security Administrator', 'Security Administrator Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Administrative' LIMIT 1) );

INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Security Reader', 'Security Reader Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Read Only' LIMIT 1) );

INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Security Reader and Writer', 'Security Reader and Writer Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Read and Write' LIMIT 1) );

INSERT INTO "Security"."SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Security No Access', 'Security No Access Role', ( SELECT id FROM "Privilege" WHERE "name" = 'No Access' LIMIT 1) );


-- 
-- Link the Security module to the roles
-- 
INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Security' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Security Administrator' LIMIT 1) );

INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Security' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Security Reader' LIMIT 1) );

INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Security' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Security Reader and Writer' LIMIT 1) );

INSERT INTO "Security"."ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Security' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Security No Access' LIMIT 1) );


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO "Security"."SecurityUserSecurityRole" ( "securityUserId", "securityRoleId", "active", "deleted" ) VALUES  ( ( SELECT id FROM "SecurityUser" WHERE "accountName" = 'Admin' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Security Administrator' LIMIT 1), '1', '0' );


