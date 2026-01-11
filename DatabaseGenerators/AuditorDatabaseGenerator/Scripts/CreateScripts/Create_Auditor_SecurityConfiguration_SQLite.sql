-- 
-- Define the Auditor Module
-- 
INSERT INTO "Module" ( "name", "description" ) VALUES  ( 'Auditor', 'The Auditor Module' );


-- 
-- Define the Auditor Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Auditor Administrator', 'Auditor Administrator Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Administrative' LIMIT 1) );

INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Auditor Reader', 'Auditor Reader Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Read Only' LIMIT 1) );

INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Auditor Reader and Writer', 'Auditor Reader and Writer Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Read and Write' LIMIT 1) );

INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Auditor No Access', 'Auditor No Access Role', ( SELECT id FROM "Privilege" WHERE "name" = 'No Access' LIMIT 1) );


-- 
-- Link the Auditor module to the roles
-- 
INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Auditor' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Auditor Administrator' LIMIT 1) );

INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Auditor' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Auditor Reader' LIMIT 1) );

INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Auditor' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Auditor Reader and Writer' LIMIT 1) );

INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Auditor' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Auditor No Access' LIMIT 1) );


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO "SecurityUserSecurityRole" ( "securityUserId", "securityRoleId", "active", "deleted" ) VALUES  ( ( SELECT id FROM "SecurityUser" WHERE "accountName" = 'Admin' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Auditor Administrator' LIMIT 1), '1', '0' );


