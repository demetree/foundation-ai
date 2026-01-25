-- 
-- Define the Telemetry Module
-- 
INSERT INTO "Module" ( "name", "description" ) VALUES  ( 'Telemetry', 'The Telemetry Module' );


-- 
-- Define the Telemetry Module 'Administrator', 'Reader', 'Reader and Writer', and 'No Access' roles
-- 
INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Telemetry Administrator', 'Telemetry Administrator Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Administrative' LIMIT 1) );

INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Telemetry Reader', 'Telemetry Reader Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Read Only' LIMIT 1) );

INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Telemetry Reader and Writer', 'Telemetry Reader and Writer Role', ( SELECT id FROM "Privilege" WHERE "name" = 'Read and Write' LIMIT 1) );

INSERT INTO "SecurityRole" ( "name", "description", "privilegeId" ) VALUES  ( 'Telemetry No Access', 'Telemetry No Access Role', ( SELECT id FROM "Privilege" WHERE "name" = 'No Access' LIMIT 1) );


-- 
-- Link the Telemetry module to the roles
-- 
INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Telemetry' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Telemetry Administrator' LIMIT 1) );

INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Telemetry' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Telemetry Reader' LIMIT 1) );

INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Telemetry' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Telemetry Reader and Writer' LIMIT 1) );

INSERT INTO "ModuleSecurityRole" ( "moduleId", "securityRoleId" ) VALUES  ( ( SELECT id FROM "Module" WHERE "name" = 'Telemetry' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Telemetry No Access' LIMIT 1) );


-- 
-- Give the admin user administrative rights to the module
-- 
INSERT INTO "SecurityUserSecurityRole" ( "securityUserId", "securityRoleId", "active", "deleted" ) VALUES  ( ( SELECT id FROM "SecurityUser" WHERE "accountName" = 'Admin' LIMIT 1), ( SELECT id FROM "SecurityRole" WHERE "name" = 'Telemetry Administrator' LIMIT 1), '1', '0' );


