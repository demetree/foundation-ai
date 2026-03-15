-- 
-- Define the Community Module
-- 
INSERT INTO "Module" ( "name", "description" ) VALUES  ( 'Community', 'The Community Module' );


-- 
-- Define the Community Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Community Administrator', 'Community Administrator Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Administrative' LIMIT 1) );

INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Community Reader', 'Community Reader Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Read Only' LIMIT 1) );

INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Community Reader and Writer', 'Community Reader and Writer Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Read and Write' LIMIT 1) );

INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Community No Access', 'Community No Access Role', ( SELECT id FROM "Privilege" WHERE "name" = 'No Access' LIMIT 1) );


-- 
-- Link the Community module to the roles
-- 
INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Community' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Community Administrator' LIMIT 1) );

INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Community' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Community Reader' LIMIT 1) );

INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Community' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Community Reader and Writer' LIMIT 1) );

INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Community' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Community No Access' LIMIT 1) );


-- 
-- Define the custom roles for the Community module
-- 
INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Community Content Writer', 'Community Content Writer Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Custom' LIMIT 1) );

INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Community Navigation Writer', 'Community Navigation Writer Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Custom' LIMIT 1) );

INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Community Admin', 'Community Admin Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Custom' LIMIT 1) );


INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Community' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Community Content Writer' LIMIT 1) );

INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Community' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Community Navigation Writer' LIMIT 1) );

INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Community' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Community Admin' LIMIT 1) );


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO "SecurityUserSecurityRole" ( "securityUserId", "securityRoleId", "active", "deleted" ) VALUES  ( ( SELECT id FROM "SecurityUser" WHERE "accountName" = 'Admin' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Community Administrator' LIMIT 1), '1', '0' );


