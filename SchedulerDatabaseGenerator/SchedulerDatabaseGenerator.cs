using Foundation.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Foundation.CodeGeneration.DatabaseGenerator;


namespace Foundation.Scheduler.Database
{
    /// <summary>
    /// Complete database schema generator for the Scheduler scheduling system.
    /// 
    /// Scheduler is a multi-tenant, general-purpose resource scheduling system with a strong focus
    /// on construction activities while remaining flexible for other domains.
    /// 
    /// Key capabilities:
    /// - Events with arbitrary time ranges
    /// - Flexible resource assignments (full duration or partial overlapping intervals)
    /// - Role-based assignments
    /// - Resource availability / blackout periods
    /// - Persistent crew definitions with members and roles
    /// - Crew-level scheduling (assign entire crew as a unit)
    /// - Calendar grouping for visibility
    /// 
    /// The schema is deliberately kept separate from other systems to allow
    /// independent evolution and integration via background processes.
    /// </summary>
    public class SchedulerDatabaseGenerator : DatabaseGenerator
    {
        private const int CLIENT_REGULAR_USER_SECURITY_LEVEL = 1;
        private const int CLIENT_ADMIN_USER_SECURITY_LEVEL = 50;
        private const int SYSTEM_ADMIN_SECURITY_LEVEL = 150;
        private const int FOUNDATION_ADMIN_SECURITY_LEVEL = 255;

        public SchedulerDatabaseGenerator() : base("Scheduler", "Scheduler")
        {
            database.comment = @"Scheduler scheduling system database schema.
This is a multi-tenant resource scheduling system designed primarily for construction resource planning
but flexible enough for other use cases. It supports events, individual and crew-based resource assignments,
partial time assignments, role designation, availability blackouts, and calendar grouping.
All operational tables include multi-tenant support, versioning where appropriate, auditing, and security controls.";

            this.database.SetSchemaName("Scheduler");


            #region Setup Master Data - Resource Types, Countries, states, time zones etc..

            //
            // Attribute Definition Types (Text, Number, Date, etc.)
            //
            Database.Table attributeDefinitionTypeTable = database.AddTable("AttributeDefinitionType");
            attributeDefinitionTypeTable.comment = "Master list of available attribute data types.";
            attributeDefinitionTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            attributeDefinitionTypeTable.AddIdField();
            attributeDefinitionTypeTable.AddNameAndDescriptionFields(true, true, false);
            attributeDefinitionTypeTable.AddSequenceField();
            attributeDefinitionTypeTable.AddControlFields();

            attributeDefinitionTypeTable.AddData(new Dictionary<string, string> { { "name", "Text" }, { "description", "Single line text" }, { "sequence", "1" }, { "objectGuid", "d1a1b2c3-1111-2222-3333-444455556661" } });
            attributeDefinitionTypeTable.AddData(new Dictionary<string, string> { { "name", "Number" }, { "description", "Numeric value" }, { "sequence", "2" }, { "objectGuid", "d1a1b2c3-1111-2222-3333-444455556662" } });
            attributeDefinitionTypeTable.AddData(new Dictionary<string, string> { { "name", "Date" }, { "description", "Date value (no time)" }, { "sequence", "3" }, { "objectGuid", "d1a1b2c3-1111-2222-3333-444455556663" } });
            attributeDefinitionTypeTable.AddData(new Dictionary<string, string> { { "name", "Boolean" }, { "description", "True/False checkbox" }, { "sequence", "4" }, { "objectGuid", "d1a1b2c3-1111-2222-3333-444455556664" } });
            attributeDefinitionTypeTable.AddData(new Dictionary<string, string> { { "name", "Select" }, { "description", "Dropdown selection" }, { "sequence", "5" }, { "objectGuid", "d1a1b2c3-1111-2222-3333-444455556665" } });
            // attributeDefinitionTypeTable.AddData(new Dictionary<string, string> { { "name", "MultiSelect" }, { "description", "Multiple selection" }, { "sequence", "6" }, { "objectGuid", "d1a1b2c3-1111-2222-3333-444455556666" } });


            //
            // Attribute Definition Entities (Contact, Constituent, etc.)
            //
            Database.Table attributeDefinitionEntityTable = database.AddTable("AttributeDefinitionEntity");
            attributeDefinitionEntityTable.comment = "Master list of entities that support custom attributes.";
            attributeDefinitionEntityTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            attributeDefinitionEntityTable.AddIdField();
            attributeDefinitionEntityTable.AddNameAndDescriptionFields(true, true, false);
            attributeDefinitionEntityTable.AddControlFields();

            attributeDefinitionEntityTable.AddData(new Dictionary<string, string> { { "name", "Contact" }, { "description", "Contact Records" }, { "objectGuid", "e2a1b2c3-1111-2222-3333-444455556661" } });
            attributeDefinitionEntityTable.AddData(new Dictionary<string, string> { { "name", "Constituent" }, { "description", "Constituent Records" }, { "objectGuid", "e2a1b2c3-1111-2222-3333-444455556662" } });


            Database.Table attributeDefinitionTable = database.AddTable("AttributeDefinition");
            attributeDefinitionTable.comment = "Definitions for custom attributes on various entities (Contact, Constituent, etc.)";
            attributeDefinitionTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            attributeDefinitionTable.AddIdField();
            attributeDefinitionTable.AddMultiTenantSupport();
            
            // Replaced string EntityName with FK
            attributeDefinitionTable.AddForeignKeyField(attributeDefinitionEntityTable, true).AddScriptComments("The entity this attribute applies to (e.g., Contact)");
            
            attributeDefinitionTable.AddString100Field("key").AddScriptComments("The JSON key for the attribute");
            attributeDefinitionTable.AddString250Field("label").AddScriptComments("The human-readable label for the attribute");

            // Replaced string Type with FK
            attributeDefinitionTable.AddForeignKeyField(attributeDefinitionTypeTable, true).AddScriptComments("Data type: Text, Number, Date, etc.");

            attributeDefinitionTable.AddTextField("options").AddScriptComments("JSON options for Select/MultiSelect types"); 
            attributeDefinitionTable.AddBoolField("isRequired", false, false);
            attributeDefinitionTable.AddSequenceField(); // For sort order
            attributeDefinitionTable.AddVersionControl();
            attributeDefinitionTable.AddControlFields();
            
            // Updated Unique Constraint to use FK IDs
            attributeDefinitionTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "attributeDefinitionEntityId", "key" }, true);

            Database.Table iconTable = database.AddTable("Icon");
            iconTable.comment = "List of icons to use on user interfaces.  Not tenant editable.";
            iconTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            iconTable.AddIdField();
            iconTable.AddNameField(true, true);
            iconTable.AddString50Field("fontAwesomeCode");
            iconTable.AddSequenceField();
            iconTable.AddControlFields();

            // -------------------------------------------------
            // Icon – master list of Font Awesome 6 Free icons for UI rendering
            // -------------------------------------------------

            // Person / User icons
            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Person" },
    { "fontAwesomeCode","fa-solid fa-user" },
    { "sequence",       "1" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0001" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "People" },
    { "fontAwesomeCode","fa-solid fa-users" },
    { "sequence",       "2" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0002" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Supervisor" },
    { "fontAwesomeCode","fa-solid fa-user-tie" },
    { "sequence",       "3" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0003" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Operator" },
    { "fontAwesomeCode","fa-solid fa-hard-hat" },
    { "sequence",       "4" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0004" }
});

            // Equipment & Assets
            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Equipment" },
    { "fontAwesomeCode","fa-solid fa-truck" },
    { "sequence",       "10" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0010" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Roller" },
    { "fontAwesomeCode","fa-solid fa-road" },
    { "sequence",       "11" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0011" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Crane" },
    { "fontAwesomeCode","fa-solid fa-tower-broadcast" },
    { "sequence",       "12" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0012" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Vehicle" },
    { "fontAwesomeCode","fa-solid fa-truck-pickup" },
    { "sequence",       "13" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0013" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Tool" },
    { "fontAwesomeCode","fa-solid fa-toolbox" },
    { "sequence",       "14" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0014" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Room" },
    { "fontAwesomeCode","fa-solid fa-door-open" },
    { "sequence",       "15" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0015" }
});

            // Project / Target Types
            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Project" },
    { "fontAwesomeCode","fa-solid fa-briefcase" },
    { "sequence",       "20" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0020" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Construction Site" },
    { "fontAwesomeCode","fa-solid fa-helmet-safety" },
    { "sequence",       "21" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0021" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Patient" },
    { "fontAwesomeCode","fa-solid fa-bed-pulse" },
    { "sequence",       "22" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0022" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Home" },
    { "fontAwesomeCode","fa-solid fa-house-medical" },
    { "sequence",       "23" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0023" }
});

            // Calendar & Scheduling
            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Calendar" },
    { "fontAwesomeCode","fa-solid fa-calendar-days" },
    { "sequence",       "30" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0030" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Maintenance" },
    { "fontAwesomeCode","fa-solid fa-wrench" },
    { "sequence",       "31" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0031" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Event" },
    { "fontAwesomeCode","fa-solid fa-calendar-check" },
    { "sequence",       "32" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0032" }
});

            // Priority
            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "High Priority" },
    { "fontAwesomeCode","fa-solid fa-triangle-exclamation" },
    { "sequence",       "40" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0040" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Medium Priority" },
    { "fontAwesomeCode","fa-solid fa-circle-exclamation" },
    { "sequence",       "41" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0041" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Low Priority" },
    { "fontAwesomeCode","fa-solid fa-circle-info" },
    { "sequence",       "42" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0042" }
});

            // Actions & Misc
            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Assignment" },
    { "fontAwesomeCode","fa-solid fa-user-check" },
    { "sequence",       "50" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0050" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Crew" },
    { "fontAwesomeCode","fa-solid fa-users-gear" },
    { "sequence",       "51" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0051" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Qualification" },
    { "fontAwesomeCode","fa-solid fa-certificate" },
    { "sequence",       "52" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0052" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Travel" },
    { "fontAwesomeCode","fa-solid fa-car" },
    { "sequence",       "53" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0053" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Location" },
    { "fontAwesomeCode","fa-solid fa-location-dot" },
    { "sequence",       "54" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0054" }
});

            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Notification" },
    { "fontAwesomeCode","fa-solid fa-bell" },
    { "sequence",       "55" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0055" }
});


            // -------------------------------------------------
            // NEW ADDITIONS - Construction & Trades
            // -------------------------------------------------
            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Hammer" },
                { "fontAwesomeCode","fa-solid fa-hammer" },
                { "sequence",       "100" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0100" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Wrench" },
                { "fontAwesomeCode","fa-solid fa-wrench" },
                { "sequence",       "101" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0101" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Screwdriver" },
                { "fontAwesomeCode","fa-solid fa-screwdriver-wrench" },
                { "sequence",       "102" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0102" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Paint Roller" },
                { "fontAwesomeCode","fa-solid fa-paint-roller" },
                { "sequence",       "103" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0103" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Brush" },
                { "fontAwesomeCode","fa-solid fa-brush" },
                { "sequence",       "104" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0104" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Ruler / Measurements" },
                { "fontAwesomeCode","fa-solid fa-ruler-combined" },
                { "sequence",       "105" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0105" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Drafting / Architecture" },
                { "fontAwesomeCode","fa-solid fa-compass-drafting" },
                { "sequence",       "106" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0106" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Electricity / Power" },
                { "fontAwesomeCode","fa-solid fa-bolt" },
                { "sequence",       "107" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0107" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Water / Plumbing" },
                { "fontAwesomeCode","fa-solid fa-faucet-drip" },
                { "sequence",       "108" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0108" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Trowel / Masonry" },
                { "fontAwesomeCode","fa-solid fa-trowel" },
                { "sequence",       "109" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0109" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Bucket" },
                { "fontAwesomeCode","fa-solid fa-bucket" },
                { "sequence",       "110" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0110" }
            });

            // -------------------------------------------------
            // NEW ADDITIONS - Healthcare & Medical
            // -------------------------------------------------
            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Doctor" },
                { "fontAwesomeCode","fa-solid fa-user-doctor" },
                { "sequence",       "200" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0200" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Nurse" },
                { "fontAwesomeCode","fa-solid fa-user-nurse" },
                { "sequence",       "201" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0201" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Stethoscope" },
                { "fontAwesomeCode","fa-solid fa-stethoscope" },
                { "sequence",       "202" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0202" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Syringe / Vaccine" },
                { "fontAwesomeCode","fa-solid fa-syringe" },
                { "sequence",       "203" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0203" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "First Aid" },
                { "fontAwesomeCode","fa-solid fa-kit-medical" },
                { "sequence",       "204" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0204" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Pills / Medication" },
                { "fontAwesomeCode","fa-solid fa-pills" },
                { "sequence",       "205" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0205" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Hospital" },
                { "fontAwesomeCode","fa-solid fa-hospital" },
                { "sequence",       "206" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0206" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Wheelchair / Accessibility" },
                { "fontAwesomeCode","fa-solid fa-wheelchair" },
                { "sequence",       "207" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0207" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Heart / Vitals" },
                { "fontAwesomeCode","fa-solid fa-heart-pulse" },
                { "sequence",       "208" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0208" }
            });

            // -------------------------------------------------
            // NEW ADDITIONS - Logistics & Transportation
            // -------------------------------------------------
            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Box / Package" },
                { "fontAwesomeCode","fa-solid fa-box" },
                { "sequence",       "300" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0300" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Pallet" },
                { "fontAwesomeCode","fa-solid fa-pallet" },
                { "sequence",       "301" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0301" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Warehouse" },
                { "fontAwesomeCode","fa-solid fa-warehouse" },
                { "sequence",       "302" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0302" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Map Pin" },
                { "fontAwesomeCode","fa-solid fa-map-pin" },
                { "sequence",       "303" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0303" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Route" },
                { "fontAwesomeCode","fa-solid fa-route" },
                { "sequence",       "304" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0304" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Ship / Marine" },
                { "fontAwesomeCode","fa-solid fa-ship" },
                { "sequence",       "305" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0305" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Plane / Air" },
                { "fontAwesomeCode","fa-solid fa-plane" },
                { "sequence",       "306" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0306" }
            });

            // -------------------------------------------------
            // NEW ADDITIONS - Office, Finance & Admin
            // -------------------------------------------------
            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Building / Office" },
                { "fontAwesomeCode","fa-solid fa-building" },
                { "sequence",       "400" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0400" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Money / Finance" },
                { "fontAwesomeCode","fa-solid fa-money-bill-wave" },
                { "sequence",       "401" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0401" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Credit Card" },
                { "fontAwesomeCode","fa-solid fa-credit-card" },
                { "sequence",       "402" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0402" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Contract / Document" },
                { "fontAwesomeCode","fa-solid fa-file-contract" },
                { "sequence",       "403" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0403" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Signature" },
                { "fontAwesomeCode","fa-solid fa-file-signature" },
                { "sequence",       "404" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0404" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Clipboard / Checklist" },
                { "fontAwesomeCode","fa-solid fa-clipboard-list" },
                { "sequence",       "405" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0405" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Chart / Analytics" },
                { "fontAwesomeCode","fa-solid fa-chart-line" },
                { "sequence",       "406" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0406" }
            });

            // -------------------------------------------------
            // NEW ADDITIONS - IT & Communication
            // -------------------------------------------------
            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Phone" },
                { "fontAwesomeCode","fa-solid fa-phone" },
                { "sequence",       "500" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0500" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Laptop" },
                { "fontAwesomeCode","fa-solid fa-laptop" },
                { "sequence",       "501" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0501" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Server / Database" },
                { "fontAwesomeCode","fa-solid fa-server" },
                { "sequence",       "502" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0502" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Wifi" },
                { "fontAwesomeCode","fa-solid fa-wifi" },
                { "sequence",       "503" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0503" }
            });

            // -------------------------------------------------
            // NEW ADDITIONS - General Status & UI
            // -------------------------------------------------
            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Check / Success" },
                { "fontAwesomeCode","fa-solid fa-check" },
                { "sequence",       "600" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0600" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "X / Cancel" },
                { "fontAwesomeCode","fa-solid fa-xmark" },
                { "sequence",       "601" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0601" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Ban / Blocked" },
                { "fontAwesomeCode","fa-solid fa-ban" },
                { "sequence",       "602" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0602" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Clock / Time" },
                { "fontAwesomeCode","fa-solid fa-clock" },
                { "sequence",       "603" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0603" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Hourglass / Waiting" },
                { "fontAwesomeCode","fa-solid fa-hourglass-half" },
                { "sequence",       "604" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0604" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Lock / Security" },
                { "fontAwesomeCode","fa-solid fa-lock" },
                { "sequence",       "605" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0605" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Trash / Delete" },
                { "fontAwesomeCode","fa-solid fa-trash" },
                { "sequence",       "606" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0606" }
            });

            // -------------------------------------------------
            // NEW ADDITIONS - Nature & Weather
            // -------------------------------------------------
            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Sun / Day" },
                { "fontAwesomeCode","fa-solid fa-sun" },
                { "sequence",       "700" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0700" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Cloud" },
                { "fontAwesomeCode","fa-solid fa-cloud" },
                { "sequence",       "701" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0701" }
            });

            iconTable.AddData(new Dictionary<string, string>
            {
                { "name",           "Tree / Landscape" },
                { "fontAwesomeCode","fa-solid fa-tree" },
                { "sequence",       "702" },
                { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0702" }
            });


            // Fallback
            iconTable.AddData(new Dictionary<string, string>
{
    { "name",           "Default" },
    { "fontAwesomeCode","fa-solid fa-circle" },
    { "sequence",       "999" },
    { "objectGuid",     "a1b2c3d4-5678-9012-3456-789abcde0999" }
});



            Database.Table salutationTable = database.AddTable("Salutation");
            salutationTable.comment = "The master list of salutations";
            salutationTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            salutationTable.AddIdField();
            salutationTable.AddNameAndDescriptionFields(true, true, false);
            salutationTable.AddSequenceField();
            salutationTable.AddControlFields();

            salutationTable.AddData(new Dictionary<string, string> { { "name", "Mr." },
                                                        { "description", "Mister" },
                                                        { "sequence", "1" },
                                                        { "objectGuid", "0e2c9a70-3a90-49f7-9f0a-539fb232a667" } });

            salutationTable.AddData(new Dictionary<string, string> { { "name", "Mrs." },
                                                        { "description", "Mrs." },
                                                        { "sequence", "2" },
                                                        { "objectGuid", "738abc0a-c637-4d45-89a1-4efb5da4e1d6" } });

            salutationTable.AddData(new Dictionary<string, string> { { "name", "Ms." },
                                                        { "description", "Ms." },
                                                        { "sequence", "3" },
                                                        { "objectGuid", "e4f9cfe6-c9dc-44a4-8977-67a8e90f94f8" } });

            salutationTable.AddData(new Dictionary<string, string> { { "name", "Dr." },
                                                        { "description", "Doctor" },
                                                        { "sequence", "4" },
                                                        { "objectGuid", "67be6b22-591f-4b7c-8366-bc3e7304ec90" } });

            salutationTable.AddData(new Dictionary<string, string> { { "name", "Prof." },
                                                        { "description", "Professor" },
                                                        { "sequence", "5" },
                                                        { "objectGuid", "8334e778-b326-4313-8891-c84cf9067d4f" } });

            salutationTable.AddData(new Dictionary<string, string> { { "name", "Rev." },
                                                        { "description", "Reverend" },
                                                        { "sequence", "6" },
                                                        { "objectGuid", "f27ca1ef-1d00-4d03-9ccd-79a2f97cb2e6" } });

            salutationTable.AddData(new Dictionary<string, string> { { "name", "" },
                                                        { "description", "No Salutation" },
                                                        { "sequence", "7" },
                                                        { "objectGuid", "df674e7a-16d8-4e75-bb2b-2a965e1725f1" } });



            Database.Table resourceTypeTable = database.AddTable("ResourceType");
            resourceTypeTable.comment = "Tenant specific master list of resource categories.";
            resourceTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            resourceTypeTable.AddIdField();
            resourceTypeTable.AddMultiTenantSupport();
            resourceTypeTable.AddNameAndDescriptionFields(true, true, false);
            resourceTypeTable.AddBoolField("isBillable", true, false);
            resourceTypeTable.AddSequenceField();
            resourceTypeTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            resourceTypeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            resourceTypeTable.AddControlFields();

            resourceTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Person" },
                { "description", "Human resource (operator, supervisor, engineer, etc.)" },
                { "sequence", "1" },
                { "objectGuid", "a1b2c3d4-5678-9012-3456-789abcde0001" } });

            resourceTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Equipment" },
                { "description", "Heavy machinery (rollers, excavators, loaders, etc.)" },
                { "sequence", "2" },
                { "objectGuid", "a1b2c3d4-5678-9012-3456-789abcde0002" } });

            resourceTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Vehicle" },
                { "description", "Trucks, service vehicles, etc." },
                { "sequence", "3" },
                { "objectGuid", "a1b2c3d4-5678-9012-3456-789abcde0003" } });

            resourceTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Tool" },
                { "description", "Smaller tools or shared items" },
                { "sequence", "4" },
                { "objectGuid", "a1b2c3d4-5678-9012-3456-789abcde0004" } });

            resourceTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Room" },
                { "description", "Meeting rooms, office spaces, etc." },
                { "sequence", "5" },
                { "objectGuid", "a1b2c3d4-5678-9012-3456-789abcde0005" } });


            Database.Table priorityTable = database.AddTable("Priority");
            priorityTable.comment = "List of priority values - Tenant configurable for flexibilty";
            priorityTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            priorityTable.AddIdField();
            priorityTable.AddMultiTenantSupport();
            priorityTable.AddNameAndDescriptionFields(true, true, false);
            priorityTable.AddSequenceField();
            priorityTable.AddForeignKeyField(iconTable, true);
            priorityTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            priorityTable.AddControlFields();

            priorityTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "High" },
                { "description", "High Priority" },
                { "color", "#FF0F0F"},
                { "sequence", "1" },
                { "objectGuid", "bcde74de-3f66-4c62-ad38-a5941871cea2" } });


            priorityTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Medium" },
                { "description", "Medium Priority" },
                { "color", "#E8E8E8"},
                { "sequence", "2" },
                { "objectGuid", "f2058cd4-aecf-4e28-b40c-6c181e67c0f4" } });


            priorityTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Low" },
                { "description", "Low Priority" },
                { "color", "#E8E8E8"},
                { "sequence", "3" },
                { "objectGuid", "25e075c3-a513-4a45-9fbc-106afc890821" } });


            Database.Table contactMethodTable = database.AddTable("ContactMethod");
            contactMethodTable.comment = "List of standard contact methods";
            contactMethodTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            contactMethodTable.AddIdField();
            contactMethodTable.AddNameAndDescriptionFields(true, true, false);
            contactMethodTable.AddSequenceField();
            contactMethodTable.AddForeignKeyField(iconTable, true);
            contactMethodTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            contactMethodTable.AddControlFields();

            contactMethodTable.AddData(new Dictionary<string, string> {
                { "name", "Mobile Phone" },
                { "description", "Mobile Phone" },
                { "sequence", "1" },
                { "objectGuid", "c8e56688-e480-426d-b49d-f7f7e7c1802c" } });

            contactMethodTable.AddData(new Dictionary<string, string> {
                { "name", "Phone" },
                { "description", "Phone" },
                { "sequence", "2" },
                { "objectGuid", "df379702-6082-4084-bf4e-f722893f33a2" } });


            contactMethodTable.AddData(new Dictionary<string, string> {
                { "name", "Email" },
                { "description", "Email" },
                { "sequence", "3" },
                { "objectGuid", "1fbea244-8312-4d8c-8218-b4b5d0788510" } });

            contactMethodTable.AddData(new Dictionary<string, string> {
                { "name", "Text" },
                { "description", "Text" },
                { "sequence", "4" },
                { "objectGuid", "9ad23e9b-76fe-4e35-9c9b-8a53b9037cce" } });

            contactMethodTable.AddData(new Dictionary<string, string> {
                { "name", "Video Call" },
                { "description", "Video Call" },
                { "sequence", "5" },
                { "objectGuid", "f89b6825-fd15-419f-baef-ec6c9ae61127" } });

            contactMethodTable.AddData(new Dictionary<string, string> {
                { "name", "In Person" },
                { "description", "In Person" },
                { "sequence", "6" },
                { "objectGuid", "91c03a84-0772-443b-8eba-e6810ec4912a" } });



            // Master list of rate types
            Database.Table rateTypeTable = database.AddTable("RateType");
            rateTypeTable.comment = "The rate types";
            rateTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            rateTypeTable.AddIdField();
            rateTypeTable.AddMultiTenantSupport();
            rateTypeTable.AddNameAndDescriptionFields(true, true, false);
            rateTypeTable.AddSequenceField();
            rateTypeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            rateTypeTable.AddControlFields();

            rateTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Standard" },
                { "description", "Standard Billing Rate" },
                { "sequence", "1" },
                { "objectGuid", "e0d3b9b8-2b93-45e1-8de2-dba9603c38b9" } });

            rateTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Overtime" },
                { "description", "Overtime Billing Rate" },
                { "sequence", "2" },
                { "objectGuid", "84897121-1587-4930-9d8c-4389ac0d222f" } });

            rateTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "DoubleTime" },
                { "description", "DoubleTime Billing Rate" },
                { "sequence", "3" },
                { "objectGuid", "fad24a49-924d-403f-a013-114ceb13ae27" } });

            rateTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Travel" },
                { "description", "Travel Billing Rate" },
                { "sequence", "4" },
                { "objectGuid", "fa0f7edd-8443-419d-9aea-229a2e61730f" } });




            Database.Table interactionTypeTable = database.AddTable("InteractionType");
            interactionTypeTable.comment = "Master list of interaction types.";
            interactionTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            interactionTypeTable.AddIdField();
            interactionTypeTable.AddNameAndDescriptionFields(true, true, false);
            interactionTypeTable.AddSequenceField();
            interactionTypeTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            interactionTypeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            interactionTypeTable.AddControlFields();

            interactionTypeTable.AddData(new Dictionary<string, string> {
                { "name", "In Person" },
                { "description", "In Person meeting" },
                { "sequence", "1" },
                { "objectGuid", "4a503ab2-a58e-403a-a400-027985773cb6" } });


            interactionTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Phone Call" },
                { "description", "Phone Call" },
                { "sequence", "2" },
                { "objectGuid", "16988bb1-54d3-4bb9-b6a7-bfadface573d" } });

            interactionTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Video Call" },
                { "description", "Video Call" },
                { "sequence", "3" },
                { "objectGuid", "337a67d5-53b8-4a67-ac4b-97818d0b0fa4" } });

            interactionTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Text Message" },
                { "description", "Text Message" },
                { "sequence", "4" },
                { "objectGuid", "10ea655e-07ae-46cf-bbf3-076c3643e16b" } });

            interactionTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Email Message" },
                { "description", "Email Message" },
                { "sequence", "5" },
                { "objectGuid", "eeb14f23-857e-416e-80a0-9a2f82b57bf7" } });


            // Master list of currencies
            Database.Table currencyTable = database.AddTable("Currency");
            currencyTable.comment = "The currencies";
            currencyTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            currencyTable.AddIdField();
            currencyTable.AddMultiTenantSupport();
            currencyTable.AddNameAndDescriptionFields(true, true, false);
            currencyTable.AddString10Field("code", false);
            currencyTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            currencyTable.AddBoolField("isDefault", false, false).AddScriptComments("Default currency for tenant.");
            currencyTable.AddSequenceField();
            currencyTable.AddControlFields();

            currencyTable.AddUniqueConstraint("tenantGuid", "code");

            currencyTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "US Dollas" },
                { "description", "United States Dollars" },
                { "code", "USD"},
                { "sequence", "1" },
                { "objectGuid", "5d460ce9-4cf5-41c3-ab9d-9ef104b0a276" } });

            currencyTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Canadian" },
                { "description", "Canadian Dollars" },
                { "code", "CAD"},
                { "sequence", "2" },
                { "objectGuid", "c6673662-f1c9-4aee-b5df-867500cb8545" } });



            // Master list of charge types
            Database.Table chargeTypeTable = database.AddTable("ChargeType");
            chargeTypeTable.comment = @"====================================================================================================
 CHARGE MASTER (Like Epic CDM)
 Master list of chargeable items (revenue or expenses). e.g., ""Site Visit Fee"" (revenue), ""Travel Expense"" (expense).
 Tied to RateType for billing context.
 ====================================================================================================";

            chargeTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            chargeTypeTable.AddIdField();
            chargeTypeTable.AddMultiTenantSupport();
            chargeTypeTable.AddNameAndDescriptionFields(true, true, false);
            chargeTypeTable.AddString100Field("externalId", true).CreateIndex();
            chargeTypeTable.AddBoolField("isRevenue", false, true).AddScriptComments("True = Revenue (billable), False = Expense (cost)");
            chargeTypeTable.AddBoolField("isTaxable", true, false);
            chargeTypeTable.AddMoneyField("defaultAmount", true, true).AddScriptComments("Optional default value for auto-drops");
            chargeTypeTable.AddString500Field("defaultDescription", true).AddScriptComments("sometimes auto-dropped charges need a note (e.g., \"Travel to site – 45 km\").");
            chargeTypeTable.AddForeignKeyField(rateTypeTable, true, true).AddScriptComments("Link to RateType (e.g., 'Standard', 'Overtime')");
            chargeTypeTable.AddForeignKeyField(currencyTable, false, true);
            chargeTypeTable.AddSequenceField();
            chargeTypeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            chargeTypeTable.AddVersionControl();
            chargeTypeTable.AddControlFields();






            Database.Table tagTable = database.AddTable("Tag");
            tagTable.comment = "Tenant specific master list of tags.";
            tagTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            tagTable.AddIdField();
            tagTable.AddMultiTenantSupport();
            tagTable.AddNameAndDescriptionFields(true, true, false);
            tagTable.AddSequenceField();
            tagTable.AddBoolField("isSystem").AddScriptComments("To mark as system tag for protected / special handling.  For things like 'deceased'.");
            tagTable.AddForeignKeyField(priorityTable, true, true);
            tagTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            tagTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            tagTable.AddControlFields();

            tagTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Person" },
                { "description", "Human resource (operator, supervisor, engineer, etc.)" },
                { "sequence", "1" },
                { "objectGuid", "a1b2c3d4-5678-9012-3456-789abcde0001" } });


            //
            // Add the standard time zone table
            //
            Database.Table timeZoneTable = database.AddStandardTimeZoneTable(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);


            Database.Table countryTable = database.AddStandardCountryTable(0, FOUNDATION_ADMIN_SECURITY_LEVEL);


            Database.Table stateProvinceTable = database.AddStandardStateProvinceTable(countryTable, 0, FOUNDATION_ADMIN_SECURITY_LEVEL, "StateProvince");


            #endregion


            #region Contact 


            //
            // The contact types - Not tenant specific
            //
            Database.Table contactTypeTable = database.AddTable("ContactType");
            contactTypeTable.comment = "Master list of office types.  Used for categorizing offices.  Not tenant specific";
            contactTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            contactTypeTable.AddIdField();
            contactTypeTable.AddNameAndDescriptionFields(true, true, false);
            contactTypeTable.AddSequenceField();
            contactTypeTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            contactTypeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            contactTypeTable.AddControlFields();

            contactTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Project Manager" },
                { "description", "Primary contact for project coordination" },
                { "sequence", "1" },
                { "objectGuid", "16df32e3-67e4-4012-b2e5-8810b8ab46b9" } });


            contactTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Billing Contact" },
                { "description", "Handles invoices and payments" },
                { "sequence", "2" },
                { "objectGuid", "1e92d7e0-599c-4c72-9e52-731c1129dd88" } });

            contactTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Site Superintendent" },
                { "description", "Site Superintendent" },
                { "sequence", "3" },
                { "objectGuid", "f3397214-a488-4522-9968-69f6e9985942" } });

            contactTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Safety Officer" },
                { "description", "Health & safety representative" },
                { "sequence", "4" },
                { "objectGuid", "cfdc40e3-36cb-4cee-863b-184a494f89bb" } });

            contactTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Technical Contact" },
                { "description", "Engineering or specs questions" },
                { "sequence", "5" },
                { "objectGuid", "9586c951-4a27-4975-94c0-70252c86880b" } });

            contactTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Emergency Contact" },
                { "description", "For urgent notifications" },
                { "sequence", "6" },
                { "objectGuid", "7ff865f4-977a-4e94-974b-e86d942a8405" } });

            contactTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Accounts Payable" },
                { "description", "Payment processing" },
                { "sequence", "7" },
                { "objectGuid", "f42ce916-a408-44d7-bbd4-9f6fc00243e4" } });

            contactTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Volunteer" },
                { "description", "Volunteer" },
                { "sequence", "8" },
                { "objectGuid", "776395dd-6187-44aa-910e-1bf0135cc88a" } });

            contactTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Staff" },
                { "description", "Staff" },
                { "sequence", "9" },
                { "objectGuid", "5cd5bdee-ba1b-43de-8249-8909546b7d28" } });


            contactTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Resident" },
                { "description", "Resident" },
                { "sequence", "10" },
                { "objectGuid", "688ae8cf-ae9d-44f2-a3a4-a900fff70fd9" } });

            contactTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Other" },
                { "description", "Other" },
                { "sequence", "99" },
                { "objectGuid", "95b327b8-9bfc-4338-a04c-e3f61c56f397" } });


            //
            // A contact represents a person with name and contact details.  It is linked to other tables.
            //
            Database.Table contactTable = database.AddTable("Contact");
            contactTable.maxPostBytes = 5_000_000;          // Cap posts at 5 mb
            contactTable.comment = "The contact data";
            contactTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            contactTable.AddIdField();
            contactTable.AddMultiTenantSupport();
            contactTable.AddForeignKeyField(contactTypeTable, false, true);
            contactTable.AddString250Field("firstName", false);
            contactTable.AddString250Field("middleName", true);
            contactTable.AddString250Field("lastName", false);
            contactTable.AddForeignKeyField(salutationTable, true, false);
            contactTable.AddString250Field("title", true);
            contactTable.AddDateField("birthDate", true, true);
            contactTable.AddString250Field("company", true).CreateIndex();
            contactTable.AddString250Field("email", true).CreateIndex(true).comment = "emails must be unique to one contact.";
            contactTable.AddString50Field("phone", true).CreateIndex();
            contactTable.AddString50Field("mobile", true).CreateIndex();
            contactTable.AddString250Field("position", true).CreateIndex();
            contactTable.AddWebAddressField("webSite", true);
            contactTable.AddForeignKeyField(contactMethodTable, true);
            contactTable.AddTextField("notes", true);
            contactTable.AddForeignKeyField(timeZoneTable, true).AddScriptComments("The contact's time zone");

            Database.Table.Field contactAttributeField = contactTable.AddTextField("attributes", true).AddScriptComments("to store arbitrary JSON");
            contactAttributeField.hideOnDefaultLists = true;


            contactTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            contactTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            contactTable.AddBinaryDataFields("avatar");            // avatar details

            Database.Table.Field cteiField = contactTable.AddString100Field("externalId", true);
            cteiField.readPermissionLevelNeeded = CLIENT_ADMIN_USER_SECURITY_LEVEL;                     // arbitrary key for an external system's key
            cteiField.hideOnDefaultLists = true;

            contactTable.AddVersionControl();
            contactTable.AddControlFields();


            contactTable.canBeFavourited = true;

            contactTable.CreateIndexForFields(new List<string>() { "tenantGuid", "externalId" });
            contactTable.CreateIndexForFields(new List<string>() { "tenantGuid", "lastName", "firstName" });




            Database.Table contactTagTable = database.AddTable("ContactTag");
            contactTagTable.comment = "The contact Tag data";
            contactTagTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            contactTagTable.AddIdField();
            contactTagTable.AddMultiTenantSupport();
            contactTagTable.AddForeignKeyField(contactTable, false, true);
            contactTagTable.AddForeignKeyField(tagTable, false, true);

            contactTagTable.AddVersionControl();        // this for meta data moreso than expected changes
            contactTagTable.AddControlFields();


            //
            // The relationship types - Not tenant specific
            //
            Database.Table relationshipTypeTable = database.AddTable("RelationshipType");
            relationshipTypeTable.comment = "Master list of office types.  Used for categorizing offices.  Not tenant specific";
            relationshipTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            relationshipTypeTable.AddIdField();
            relationshipTypeTable.AddNameAndDescriptionFields(true, true, false);
            relationshipTypeTable.AddBoolField("isEmergencyEligible", false, false);
            relationshipTypeTable.AddSequenceField();
            relationshipTypeTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            relationshipTypeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            relationshipTypeTable.AddControlFields();

            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Self" },
                { "description", "Self" },
                { "isEmergencyEligible", "0" },
                { "sequence", "1" },
                { "objectGuid", "3d4ec50a-552b-4826-9f7c-a27915134a21" } });

            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Colleague" },
                { "description", "Colleague" },
                { "isEmergencyEligible", "0" },
                { "sequence", "2" },
                { "objectGuid", "968a530e-2ec8-449a-b2fa-e853bb82b2c2" } });

            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Spouse" },
                { "description", "Husband/Wife/Partner" },
                { "isEmergencyEligible", "1" },
                { "sequence", "3" },
                { "objectGuid", "e0020ae1-4b49-4d3e-a5a1-67f96ca239c8" } });

            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Parent" },
                { "description", "Mother/Father" },
                { "isEmergencyEligible", "1" },
                { "sequence", "4" },
                { "objectGuid", "8622604b-c5d5-4363-9d63-b0c34f3facb2" } });

            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Child" },
                { "description", "Son/Daughter" },
                { "isEmergencyEligible", "1" },
                { "sequence", "5" },
                { "objectGuid", "d35f8329-f18b-445d-8404-0c8fafd9c43b" } });

            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Sibling" },
                { "description", "Brother/Sister" },
                { "isEmergencyEligible", "1" },
                { "sequence", "6" },
                { "objectGuid", "07ed8aa5-9034-4cad-b8cc-c5564c5945d9" } });

            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Friend" },
                { "description", "Close friend" },
                { "isEmergencyEligible", "1" },
                { "sequence", "7" },
                { "objectGuid", "57a2e1c3-d06e-48cf-aca5-fe5f396e968f" } });

            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Supervisor" },
                { "description", "Direct manager" },
                { "isEmergencyEligible", "0" },
                { "sequence", "8" },
                { "objectGuid", "4f51e255-4c2c-41c5-92d9-b051d7d1b15a" } });

            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Mentor" },
                { "description", "Mentor" },
                { "isEmergencyEligible", "0" },
                { "sequence", "9" },
                { "objectGuid", "acfdbb6a-bc68-4753-990c-001c9800c155" } });


            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Mechanic" },
                { "description", "Equipment Maintenance" },
                { "isEmergencyEligible", "0" },
                { "sequence", "10" },
                { "objectGuid", "3108554f-3943-4b8c-a196-ee8154cf9918" } });


            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Resident" },
                { "description", "Resident" },
                { "isEmergencyEligible", "1" },
                { "sequence", "11" },
                { "objectGuid", "1b92d6de-a154-419e-a3dc-2f0186f029de" } });


            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Owner" },
                { "description", "Owner" },
                { "isEmergencyEligible", "1" },
                { "sequence", "12" },
                { "objectGuid", "e603de2c-8f55-44bb-9198-eaa1c1808498" } });


            relationshipTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Other" },
                { "description", "Other relationship" },
                { "isEmergencyEligible", "0" },
                { "sequence", "99" },
                { "objectGuid", "b0fc78e9-ca52-4fdc-823f-0339e11dc069" } });


            Database.Table contactContactTable = database.AddTable("ContactContact");
            contactContactTable.comment = "The link between a contact and other contacts.";
            contactContactTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            contactContactTable.AddIdField();
            contactContactTable.AddMultiTenantSupport();
            contactContactTable.AddForeignKeyField(contactTable, false, true);
            contactContactTable.AddForeignKeyField("relatedContactId", contactTable, false, true);
            contactContactTable.AddBoolField("isPrimary", false, false).AddScriptComments("Indicates whether or not this contact should be considered a primary contact of the contact.");
            contactContactTable.AddForeignKeyField(relationshipTypeTable, false).AddScriptComments("A description of the relationship between the contact and the contact.");
            contactContactTable.AddVersionControl();
            contactContactTable.AddControlFields();

            contactContactTable.AddUniqueConstraint("tenantGuid", "contactId", "relatedContactId", false);


            #endregion


            #region Office Related

            //
            // The office types - Not tenant specific
            //
            Database.Table officeTypeTable = database.AddTable("OfficeType");
            officeTypeTable.comment = "Master list of office types.  Used for categorizing offices.  Not tenant specific";
            officeTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            officeTypeTable.AddIdField();
            officeTypeTable.AddNameAndDescriptionFields(true, true, false);
            officeTypeTable.AddSequenceField();
            officeTypeTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            officeTypeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            officeTypeTable.AddControlFields();

            //  - Central administration, executive team
            officeTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Headquarters " },
                { "description", "Headquarters" },
                { "sequence", "1" },
                { "objectGuid", "3dc56597-1ab7-403e-bad9-8bd52c674f9d" } });

            // e.g., "Eastern Region", "Western Canada".
            officeTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Regional Office" },
                { "description", "Regional Office" },
                { "sequence", "2" },
                { "objectGuid", "f28b5678-de69-43a3-9a9e-7194df40ea32" } });

            // standard local office
            officeTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Branch Office" },
                { "description", "Branch Office" },
                { "sequence", "3" },
                { "objectGuid", "d504aef3-b582-4f6d-91c8-b76142f5462a" } });

            // Equipment and vehicle storage.
            officeTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Depot / Yard" },
                { "description", "Depot / Yard" },
                { "sequence", "4" },
                { "objectGuid", "98b72f2e-de47-4268-885e-3ab7a63e9e8c" } });

            // Back-office functions (billing, HR).
            officeTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Administrative Office" },
                { "description", "Administrative Office" },
                { "sequence", "5" },
                { "objectGuid", "edc174d4-66f3-410f-a173-b15007c1ff48" } });

            officeTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Warehouse" },
                { "description", "Warehouse" },
                { "sequence", "6" },
                { "objectGuid", "c595846a-c3f3-4e07-9df0-af117fa5a400" } });

            officeTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Hospital" },
                { "description", "Hospital" },
                { "sequence", "7" },
                { "objectGuid", "52a134df-ff0c-4391-ac85-93be54e9541b" } });

            officeTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Clinic" },
                { "description", "Clinic" },
                { "sequence", "8" },
                { "objectGuid", "9bd149c1-ca03-49c1-a71f-7d8479697205" } });

            //
            // The the office for business organization purposes
            //
            Database.Table officeTable = database.AddTable("Office");
            officeTable.maxPostBytes = 5_000_000;          // Cap posts at 5 mb
            officeTable.comment = @"The main list of offices operated by an organization using the Scheduler.  Allows schedule and resource grouping.";
            officeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            officeTable.AddIdField();
            officeTable.AddMultiTenantSupport();
            officeTable.AddNameAndDescriptionFields(true, true, true);
            officeTable.AddForeignKeyField(officeTypeTable, false, true);
            officeTable.AddForeignKeyField(timeZoneTable, false, true).AddScriptComments("Time zone of the office.");
            officeTable.AddForeignKeyField(currencyTable, false, true).AddScriptComments("Default billing currency of the office.");
            officeTable.AddString250Field("addressLine1", false);
            officeTable.AddString250Field("addressLine2", true);
            officeTable.AddString100Field("city", false);
            officeTable.AddString100Field("postalCode", true);
            officeTable.AddForeignKeyField(stateProvinceTable, false, true);
            officeTable.AddForeignKeyField(countryTable, false, true);
            officeTable.AddString100Field("phone", true);
            officeTable.AddString250Field("email", true).CreateIndex(true);     // Email must be unique to the office

            officeTable.AddDoubleField("latitude", true, null).AddScriptComments("Optional latitude position");
            officeTable.AddDoubleField("longitude", true, null).AddScriptComments("Optional longitude position");

            officeTable.AddTextField("notes", true);
            Database.Table.Field oeiField = officeTable.AddString100Field("externalId", true).AddScriptComments("Optional reference to an ID in an external system ");
            oeiField.readPermissionLevelNeeded = CLIENT_ADMIN_USER_SECURITY_LEVEL;
            oeiField.hideOnDefaultLists = true;

            officeTable.AddHTMLColorField("color", true).AddScriptComments("Override of Target Type Hex color for UI display");

            Database.Table.Field oaField = officeTable.AddTextField("attributes", true).AddScriptComments("to store arbitrary JSON");
            oaField.hideOnDefaultLists = true;

            officeTable.AddBinaryDataFields("avatar");            // avatar details

            officeTable.AddVersionControl();
            officeTable.AddControlFields();
            officeTable.canBeFavourited = true;


            Database.Table officeContactTable = database.AddTable("OfficeContact");
            officeContactTable.comment = "The link between contacts and offices.";
            officeContactTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            officeContactTable.AddIdField();
            officeContactTable.AddMultiTenantSupport();
            officeContactTable.AddForeignKeyField(officeTable, false, true);
            officeContactTable.AddForeignKeyField(contactTable, false, true);
            officeContactTable.AddBoolField("isPrimary", false, false).AddScriptComments("Indicates whether or not this contact should be considered a primary contact of the office.");
            officeContactTable.AddForeignKeyField(relationshipTypeTable, false).AddScriptComments("A description of the relationship between the office and the contact.");
            officeContactTable.AddVersionControl();
            officeContactTable.AddControlFields();


            officeContactTable.AddUniqueConstraint("tenantGuid", "officeId", "contactId", false);


            #endregion



            #region Calendars (optional grouping)



            Database.Table calendarTable = database.AddTable("Calendar");
            calendarTable.comment = "Optional logical grouping of events for visibility and filtering (e.g., '2026 Road Projects', 'Maintenance Calendar').";
            calendarTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            calendarTable.AddIdField();
            calendarTable.AddMultiTenantSupport();
            calendarTable.AddNameAndDescriptionFields(true, true, true);
            calendarTable.AddForeignKeyField(officeTable, true, true).AddScriptComments("Optional office binding for the calendar");
            calendarTable.AddBoolField("isDefault", true);
            calendarTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            calendarTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            calendarTable.AddControlFields();
            calendarTable.AddVersionControl();
            calendarTable.canBeFavourited = true;


            #endregion



            #region Client Related


            Database.Table clientTypeTable = database.AddTable("ClientType");
            clientTypeTable.comment = "Master list of client types.  Used for categorizing clients.";
            clientTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            clientTypeTable.AddIdField();
            clientTypeTable.AddMultiTenantSupport();
            clientTypeTable.AddNameAndDescriptionFields(true, true, false);
            clientTypeTable.AddSequenceField();
            clientTypeTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            clientTypeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            clientTypeTable.AddControlFields();

            clientTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Construction " },
                { "description", "A construction client" },
                { "sequence", "1" },
                { "objectGuid", "331c07c6-bcd1-4d8d-b796-d81216bba704" } });

            clientTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Healthcare" },
                { "description", "A healthcare client" },
                { "sequence", "2" },
                { "objectGuid", "701001e4-4034-4b18-ab29-b514b08bc541" } });



            // The the client for billing purposes
            Database.Table clientTable = database.AddTable("Client");
            clientTable.maxPostBytes = 5_000_000;          // Cap posts at 5 mb
            clientTable.comment = @"The main client list.  Is not directly schedulable, but provides billing details.  Contains scheduling targets which are schedulable.";
            clientTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            clientTable.AddIdField();
            clientTable.AddMultiTenantSupport();
            clientTable.AddNameAndDescriptionFields(true, true, true);
            clientTable.AddForeignKeyField(clientTypeTable, false, true);
            clientTable.AddForeignKeyField(currencyTable, false, true);
            clientTable.AddForeignKeyField(timeZoneTable, false, true);
            clientTable.AddForeignKeyField(calendarTable, true, false).AddScriptComments("An optional default calendar for the scheduling target's belonging to the client.");
            clientTable.AddString250Field("addressLine1", false);
            clientTable.AddString250Field("addressLine2", true);
            clientTable.AddString100Field("city", false);
            clientTable.AddString100Field("postalCode", true);
            clientTable.AddForeignKeyField(stateProvinceTable, false, true);
            clientTable.AddForeignKeyField(countryTable, false, true);
            clientTable.AddString100Field("phone", true);
            clientTable.AddString250Field("email", true).CreateIndex(true).comment = "emails must be unique to one Client.";

            clientTable.AddDoubleField("latitude", true, null).AddScriptComments("Optional latitude position");
            clientTable.AddDoubleField("longitude", true, null).AddScriptComments("Optional longitude position");

            clientTable.AddTextField("notes", true);
            Database.Table.Field ceiField = clientTable.AddString100Field("externalId", true).AddScriptComments("Optional reference to an ID in an external system");
            ceiField.readPermissionLevelNeeded = CLIENT_ADMIN_USER_SECURITY_LEVEL;
            ceiField.hideOnDefaultLists = true;


            clientTable.AddHTMLColorField("color", true).AddScriptComments("Override of Target Type Hex color for UI display");
            
            Database.Table.Field caField = clientTable.AddTextField("attributes", true).AddScriptComments("to store arbitrary JSON");
            caField.hideOnDefaultLists = true;

            clientTable.AddBinaryDataFields("avatar");            // avatar details

            clientTable.AddVersionControl();
            clientTable.AddControlFields();
            clientTable.canBeFavourited = true;


            Database.Table clientContactTable = database.AddTable("ClientContact");
            clientContactTable.comment = "The link between contacts and clients.";
            clientContactTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            clientContactTable.AddIdField();
            clientContactTable.AddMultiTenantSupport();
            clientContactTable.AddForeignKeyField(clientTable, false, true);
            clientContactTable.AddForeignKeyField(contactTable, false, true);
            clientContactTable.AddBoolField("isPrimary", false, false).AddScriptComments("Indicates whether or not this contact should be considered a primary contact of the client.");
            clientContactTable.AddForeignKeyField(relationshipTypeTable, false).AddScriptComments("A description of the relationship between the client and the contact.");
            clientContactTable.AddVersionControl();
            clientContactTable.AddControlFields();


            clientContactTable.AddUniqueConstraint("tenantGuid", "clientId", "contactId", false);



            #endregion

            #region Tenant Profile 
            Database.Table tenantProfile = database.AddTable("TenantProfile");
            tenantProfile.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            tenantProfile.comment = "Tenant-level information. Client admins manage this data.";
            tenantProfile.AddIdField();
            tenantProfile.AddMultiTenantSupport();
            tenantProfile.AddNameAndDescriptionFields(true, true, true);
            tenantProfile.AddBinaryDataFields("companyLogo"); // Company Logo
            tenantProfile.AddString250Field("addressLine1");
            tenantProfile.AddString250Field("addressLine2");
            tenantProfile.AddString250Field("addressLine3");
            tenantProfile.AddString100Field("city");
            tenantProfile.AddString100Field("postalCode");
            tenantProfile.AddForeignKeyField(stateProvinceTable, true, false);
            tenantProfile.AddForeignKeyField(countryTable, true, false);
            tenantProfile.AddForeignKeyField(timeZoneTable, true, true);
            tenantProfile.AddString100Field("phoneNumber"); // Main contact number
            tenantProfile.AddString250Field("email");       // General contact email
            tenantProfile.AddWebAddressField("website");     // Website link
            tenantProfile.AddDoubleField("latitude", true, null).AddScriptComments("Optional latitude position");
            tenantProfile.AddDoubleField("longitude", true, null).AddScriptComments("Optional longitude position");

            tenantProfile.AddHTMLColorField("primaryColor");
            tenantProfile.AddHTMLColorField("secondaryColor");
            tenantProfile.AddBoolField("displaysMetric", false, false).AddScriptComments("True if the tenant defaults to using metric units when creating projects.    Note that this does not affect the storage units, which are always metric.");
            tenantProfile.AddBoolField("displaysUSTerms", false, false).AddScriptComments("True if the tenant defaults to using terms for the US market, such as Zip code,.");
            tenantProfile.AddVersionControl(); // Includes version field
            tenantProfile.AddControlFields();   // Includes Active and Deleted flags

            tenantProfile.canBeFavourited = true;

            #endregion


            #region Qualifications / Certifications

            // Tenant specific master list of qualifications/certifications
            Database.Table qualificationTable = database.AddTable("Qualification");
            qualificationTable.comment = @"Master list of qualifications, certifications, or competencies required for certain work.  Examples: RN License, Crane Operator Certification, OSHA 30, Pediatric Specialty, Confined Space Entry.";
            qualificationTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            qualificationTable.AddIdField();
            qualificationTable.AddMultiTenantSupport();
            qualificationTable.AddNameAndDescriptionFields(true, true, false);
            qualificationTable.AddBoolField("isLicense", true, null).AddScriptComments("for special handling (e.g., expiry warnings)");
            qualificationTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            qualificationTable.AddSequenceField();
            qualificationTable.AddControlFields();
            qualificationTable.canBeFavourited = true;

            //
            // Table is now multi-tenanted.  Keeping these here only for reference examples.
            //
            //// Seed common examples
            //qualificationTable.AddData(new Dictionary<string, string> {
            //    { "name", "Registered Nurse (RN)" }, { "description", "State nursing license" }, { "sequence", "1" },
            //    { "objectGuid", "d4e5f6a7-b8c9-0123-4567-89abcdef0001" } });
            //qualificationTable.AddData(new Dictionary<string, string> {
            //    { "name", "Crane Operator NCCCO" }, { "description", "National Commission for the Certification of Crane Operators" }, { "sequence", "2" },
            //    { "objectGuid", "d4e5f6a7-b8c9-0123-4567-89abcdef0002" } });
            //qualificationTable.AddData(new Dictionary<string, string> {
            //    { "name", "OSHA 30-Hour" }, { "description", "OSHA 30-hour construction safety training" }, { "sequence", "3" },
            //    { "objectGuid", "d4e5f6a7-b8c9-0123-4567-89abcdef0003" } });



            #endregion


            #region Assignment Roles (tenant-configurable)

            Database.Table assignmentRoleTable = database.AddTable("AssignmentRole");
            assignmentRoleTable.comment = @"Tenant-configurable roles that a resource can fulfil during an event.  Examples: Operator, Supervisor, Driver, Spotter, Safety Officer.  Used for business rule enforcement and richer reporting.";
            assignmentRoleTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            assignmentRoleTable.AddIdField();
            assignmentRoleTable.AddMultiTenantSupport();
            assignmentRoleTable.AddNameAndDescriptionFields(true, true, true);
            assignmentRoleTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            assignmentRoleTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            assignmentRoleTable.AddSequenceField();
            assignmentRoleTable.AddControlFields();

            //
            // Seed with common construction roles as examples on 0 tenant guid – tenant setup should add more
            //
            assignmentRoleTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", Guid.Empty.ToString() },
                { "name", "Operator" },
                { "description", "Primary equipment operator" },
                { "sequence", "1" },
                { "objectGuid", "b2c3d4e5-6789-0123-4567-89abcdef0001" } });

            assignmentRoleTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", Guid.Empty.ToString() },
                { "name", "Supervisor" },
                { "description", "Site supervisor" },
                { "sequence", "2" },
                { "objectGuid", "b2c3d4e5-6789-0123-4567-89abcdef0002" } });

            assignmentRoleTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", Guid.Empty.ToString() },
                { "name", "Driver" },
                { "description", "Haul truck or service vehicle driver" },
                { "sequence", "3" },
                { "objectGuid", "b2c3d4e5-6789-0123-4567-89abcdef0003" } });

            assignmentRoleTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", Guid.Empty.ToString() },
                { "name", "Spotter" },
                { "description", "Safety spotter / banksman" },
                { "sequence", "4" },
                { "objectGuid", "b2c3d4e5-6789-0123-4567-89abcdef0004" } });


            //
            // AssignmentRole Qualifications to indicate what qualifications are needed by a resource to fill the role.
            //
            Database.Table assignmentRoleQualificationRequirementTable = database.AddTable("AssignmentRoleQualificationRequirement");
            assignmentRoleQualificationRequirementTable.comment = @"Defines which qualifications are required to fulfill a specific AssignmentRole.  This is the most common way to enforce certification requirements.";
            assignmentRoleQualificationRequirementTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            assignmentRoleQualificationRequirementTable.AddIdField();
            assignmentRoleQualificationRequirementTable.AddMultiTenantSupport();
            assignmentRoleQualificationRequirementTable.AddForeignKeyField(assignmentRoleTable, false, true);
            assignmentRoleQualificationRequirementTable.AddForeignKeyField(qualificationTable, false, true);
            assignmentRoleQualificationRequirementTable.AddBoolField("isRequired", false, true).AddScriptComments("true = mandatory to fulfill role, false = preferred/recommended");
            assignmentRoleQualificationRequirementTable.AddVersionControl();
            assignmentRoleQualificationRequirementTable.AddControlFields();

            assignmentRoleQualificationRequirementTable.AddUniqueConstraint("tenantGuid", "assignmentRoleId", "qualificationId", false);

            #endregion

            #region Statuses


            Database.Table eventStatusTable = database.AddTable("EventStatus");
            eventStatusTable.comment = "Master list of event statuses (Planned, In Progress, Completed, No-Show, Canceled, etc.)";
            eventStatusTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            eventStatusTable.AddIdField();
            eventStatusTable.AddNameAndDescriptionFields(true, true, false);
            eventStatusTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            eventStatusTable.AddSequenceField();
            eventStatusTable.AddControlFields();

            // Seed common statuses
            eventStatusTable.AddData(new Dictionary<string, string> {
    { "name", "Planned" }, { "description", "Scheduled but not started" }, { "sequence", "1" },
    { "objectGuid", "005bdc39-da8e-465a-a17e-78aafffb390a" } });
            eventStatusTable.AddData(new Dictionary<string, string> {
    { "name", "In Progress" }, { "description", "Started" }, { "sequence", "2" },
    { "objectGuid", "513bd381-6ab9-407c-ac4d-9187f6f92e16" } });
            eventStatusTable.AddData(new Dictionary<string, string> {
    { "name", "Completed" }, { "description", "Finished successfully" }, { "sequence", "3" },
    { "objectGuid", "6af9e244-2eff-463b-a40c-821fe00fa644" } });
            eventStatusTable.AddData(new Dictionary<string, string> {
    { "name", "No-Show" }, { "description", "No Show" }, { "sequence", "4" },
    { "objectGuid", "d7e81b73-bbe3-42dd-bcf6-856a82b9fce1" } });
            eventStatusTable.AddData(new Dictionary<string, string> {
    { "name", "Canceled" }, { "description", "Explicitly canceled" }, { "sequence", "5" },
    { "objectGuid", "01148ccb-e746-4218-88c5-8f0a5ee36adc" } });


            Database.Table paymentTypeTable = database.AddTable("PaymentType");
            paymentTypeTable.comment = "Master list of payment types ( credit card, check, cash, etc..)";
            paymentTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            paymentTypeTable.AddIdField();
            paymentTypeTable.AddNameAndDescriptionFields(true, true, false);
            paymentTypeTable.AddSequenceField();
            paymentTypeTable.AddControlFields();

            paymentTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Credit Card" },
                { "description", "Credit Card" },
                { "sequence", "1" },
                { "objectGuid", "3353a9f0-1b8e-4170-a20a-d35eab81fab8" } });

            paymentTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Check" },
                { "description", "Check" },
                { "sequence", "2" },
                { "objectGuid", "19376f2d-87c0-4eb5-a11c-f02cb4f9b412" } });

            paymentTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Cash" },
                { "description", "Cash" },
                { "sequence", "3" },
                { "objectGuid", "dca9c876-bb7d-4c33-8ef4-96a955dacbb0" } });

            paymentTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Crypto" },
                { "description", "Crypto" },
                { "sequence", "4" },
                { "objectGuid", "8be012ff-f305-45cd-bedb-cc5b9f11f3ef" } });

            paymentTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Stock" },
                { "description", "Stock" },
                { "sequence", "5" },
                { "objectGuid", "427451dc-b522-4613-aa3a-57593b6d4d03" } });


            Database.Table receiptTypeTable = database.AddTable("ReceiptType");
            receiptTypeTable.comment = "Master list of receipt types";
            receiptTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            receiptTypeTable.AddIdField();
            receiptTypeTable.AddNameAndDescriptionFields(true, true, false);
            receiptTypeTable.AddSequenceField();
            receiptTypeTable.AddControlFields();

            receiptTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Receipted" },
                { "description", "Receipted" },
                { "sequence", "1" },
                { "objectGuid", "b0a794eb-afa9-4791-b164-e28e5ed21a35" } });

            receiptTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Do Not Receipt" },
                { "description", "Do Not Receipt" },
                { "sequence", "2" },
                { "objectGuid", "d6ceb144-aced-4e2a-9407-a2b0c995c795" } });




            Database.Table bookingSourceTypeTable = database.AddTable("BookingSourceType");
            bookingSourceTypeTable.comment = "Master list of booking sources ( walk-in, phone, online)";
            bookingSourceTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            bookingSourceTypeTable.AddIdField();
            bookingSourceTypeTable.AddNameAndDescriptionFields(true, true, false);
            bookingSourceTypeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            bookingSourceTypeTable.AddSequenceField();
            bookingSourceTypeTable.AddControlFields();

            // Seed common booking sources
            bookingSourceTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Administrative" },
                { "description", "Administrative" },
                { "sequence", "1" },
                { "objectGuid", "3ec3e46a-ece8-4364-8396-beaf23aa0a2a" } });


            bookingSourceTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Phone" }, 
                { "description", "Phone" }, 
                { "sequence", "2" },
                { "objectGuid", "cb9c2d46-29d5-4caa-9d5c-9e84356edf86" } });

            bookingSourceTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Walk-in" },
                { "description", "Walk-in" },
                { "sequence", "3" },
                { "objectGuid", "fc0a5ebf-794d-4e61-9dce-f308da9d9ba4" } });


            bookingSourceTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Online" },
                { "description", "Online" },
                { "sequence", "4" },
                { "objectGuid", "1955a3f1-adce-4bc4-99d1-86362ff98a57" } });



            // Master table for assignment statuses
            Database.Table assignmentStatusTable = database.AddTable("AssignmentStatus");
            assignmentStatusTable.comment = "Master list of assignment statuses (Planned, In Progress, Completed, No-Show, Canceled, etc.)";
            assignmentStatusTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            assignmentStatusTable.AddIdField();
            assignmentStatusTable.AddNameAndDescriptionFields(true, true, false);
            assignmentStatusTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            assignmentStatusTable.AddSequenceField();
            assignmentStatusTable.AddControlFields();


            // Seed common statuses
            assignmentStatusTable.AddData(new Dictionary<string, string> {
                { "name", "Planned" }, 
                { "description", "Scheduled but not started" }, 
                { "sequence", "1" },
                { "objectGuid", "82fff66d-f6b4-44fe-9892-c7415cd0d401" } });
            
            assignmentStatusTable.AddData(new Dictionary<string, string> {
                { "name", "In Progress" }, 
                { "description", "Started" }, 
                { "sequence", "2" },
                { "objectGuid", "34183a16-1a64-4106-b28e-db454b06b5a6" } });
            
            assignmentStatusTable.AddData(new Dictionary<string, string> {
                { "name", "Completed" }, 
                { "description", "Finished successfully" }, 
                { "sequence", "3" },
                { "objectGuid", "765c3c6d-782b-4393-bdab-cbf2a4a34eb6" } });
            
            assignmentStatusTable.AddData(new Dictionary<string, string> {
                { "name", "No-Show" }, 
                { "description", "Patient/resource didn't appear" }, 
                { "sequence", "4" },
                { "objectGuid", "121271a6-7d93-4460-909f-2dc6e618538f" } });

            assignmentStatusTable.AddData(new Dictionary<string, string> {
                { "name", "Canceled" }, 
                { "description", "Explicitly canceled" }, 
                { "sequence", "5" },
                { "objectGuid", "cb14a7ad-fe10-4b2b-996c-7b5598810608" } });


            #endregion

            #region Scheduling Targets - The "what" we are scheduling into

            // Master list of target types
            Database.Table schedulingTargetTypeTable = database.AddTable("SchedulingTargetType");
            schedulingTargetTypeTable.comment = "Master list of scheduling target categories (e.g., Project, Patient, Customer). Used for UI grouping and filtering.";
            schedulingTargetTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            schedulingTargetTypeTable.AddIdField();
            schedulingTargetTypeTable.AddMultiTenantSupport();
            schedulingTargetTypeTable.AddNameAndDescriptionFields(true, true, false);
            schedulingTargetTypeTable.AddSequenceField();
            schedulingTargetTypeTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            schedulingTargetTypeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            schedulingTargetTypeTable.AddControlFields();

            schedulingTargetTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Construction Project" },
                { "description", "A construction job with one or more sites" },
                { "sequence", "1" },
                { "objectGuid", "0ceaf00d-c58f-48a6-a18e-9a3e07452a23" } });

            schedulingTargetTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Patient" },
                { "description", "Healthcare patient with multiple care locations" },
                { "sequence", "2" },
                { "objectGuid", "7e14d7a8-f13d-4524-a679-6cbae24d9d97" } });
            
            schedulingTargetTypeTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Service Customer" },
                { "description", "Field service customer with multiple service addresses" },
                { "sequence", "3" },
                { "objectGuid", "6b3aa295-a54b-45dd-bda5-d75d157f376c" } });

            //
            // The actual container that events are scheduled into
            //
            Database.Table schedulingTargetTable = database.AddTable("SchedulingTarget");
            schedulingTargetTable.maxPostBytes = 5_000_000;          // Cap posts at 5 mb
            schedulingTargetTable.comment = @"The core container that ScheduledEvents are scheduled into.   Examples: a construction project, a healthcare patient, a service customer.  Supports multiple addresses and recurring scheduling patterns.";
            schedulingTargetTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            schedulingTargetTable.AddIdField();
            schedulingTargetTable.AddMultiTenantSupport();
            schedulingTargetTable.AddNameAndDescriptionFields(true, true, true);
            schedulingTargetTable.AddForeignKeyField(officeTable, true, true).AddScriptComments("Optional office binding for a scheduling target.");
            schedulingTargetTable.AddForeignKeyField(clientTable, false, true).AddScriptComments("The client that this scheduling target belongs to.");
            schedulingTargetTable.AddForeignKeyField(schedulingTargetTypeTable, false, true);
            schedulingTargetTable.AddForeignKeyField(timeZoneTable, false, true);
            schedulingTargetTable.AddForeignKeyField(calendarTable, true, false).AddScriptComments("An optional default calendar for this scheduling target.");


            schedulingTargetTable.AddTextField("notes", true);
            Database.Table.Field steiField = schedulingTargetTable.AddString100Field("externalId", true).AddScriptComments("Optional reference to an ID in an external system");
            steiField.readPermissionLevelNeeded = CLIENT_ADMIN_USER_SECURITY_LEVEL;
            steiField.hideOnDefaultLists = true;


            schedulingTargetTable.AddHTMLColorField("color", true).AddScriptComments("Override of Target Type Hex color for UI display");
            
            Database.Table.Field staField = schedulingTargetTable.AddTextField("attributes", true).AddScriptComments("to store arbitrary JSON");
            staField.hideOnDefaultLists = true;

            schedulingTargetTable.AddBinaryDataFields("avatar");            // avatar details
            schedulingTargetTable.AddVersionControl();
            schedulingTargetTable.AddControlFields();
            schedulingTargetTable.SetDisplayNameField("name");
            schedulingTargetTable.canBeFavourited = true;



            Database.Table schedulingTargetContactTable = database.AddTable("SchedulingTargetContact");
            schedulingTargetContactTable.comment = "The link between scheduling targets and contacts.";
            schedulingTargetContactTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            schedulingTargetContactTable.AddIdField();
            schedulingTargetContactTable.AddMultiTenantSupport();
            schedulingTargetContactTable.AddForeignKeyField(schedulingTargetTable, false, true);
            schedulingTargetContactTable.AddForeignKeyField(contactTable, false, true);
            schedulingTargetContactTable.AddBoolField("isPrimary", false, false).AddScriptComments("Indicates whether or not this contact should be considered a primary contact of the scheduling target.");
            schedulingTargetContactTable.AddForeignKeyField(relationshipTypeTable, false).AddScriptComments("A description of the relationship between the scheduling target and the contact.");
            schedulingTargetContactTable.AddVersionControl();
            schedulingTargetContactTable.AddControlFields();


            schedulingTargetContactTable.AddUniqueConstraint("tenantGuid", "schedulingTargetId", "contactId", false);



            //
            // Junction table for multiple addresses per target
            //
            Database.Table schedulingTargetAddressTable = database.AddTable("SchedulingTargetAddress");
            schedulingTargetAddressTable.comment = "Links SchedulingTargets to multiple addresses (e.g., multiple job sites, patient home + hospital).";
            schedulingTargetAddressTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            schedulingTargetAddressTable.AddIdField();
            schedulingTargetAddressTable.AddMultiTenantSupport();
            schedulingTargetAddressTable.AddForeignKeyField(schedulingTargetTable, false, true).AddScriptComments("Primary  schuduling target for this address - could be null if there is a client linked to this, so the address would be for all targets in the client."); ;
            schedulingTargetAddressTable.AddForeignKeyField(clientTable, true, true).AddScriptComments("Optional client level link.  The presence of a value here indicates that the address is to be shared across all scheduling targets for the client.");

            // Simple address fields with links to state/province and country
            schedulingTargetAddressTable.AddString250Field("addressLine1", false);
            schedulingTargetAddressTable.AddString250Field("addressLine2", true);
            schedulingTargetAddressTable.AddString100Field("city", false);
            schedulingTargetAddressTable.AddString100Field("postalCode", true);
            schedulingTargetAddressTable.AddForeignKeyField(stateProvinceTable, false, true);
            schedulingTargetAddressTable.AddForeignKeyField(countryTable, false, true);

            schedulingTargetAddressTable.AddDoubleField("latitude", true, null).AddScriptComments("Optional latitude position");
            schedulingTargetAddressTable.AddDoubleField("longitude", true, null).AddScriptComments("Optional longitude position");

            schedulingTargetAddressTable.AddString250Field("label", true).AddScriptComments("e.g., 'Main Site', 'Patient Home', 'Hospital Ward'");
            schedulingTargetAddressTable.AddBoolField("isPrimary", false, false).AddScriptComments("Whether or not this is the scheduling target's main address.");
            schedulingTargetAddressTable.AddVersionControl();
            schedulingTargetAddressTable.AddControlFields();
            schedulingTargetAddressTable.AddUniqueConstraint(new List<String>() { "tenantGuid", "schedulingTargetId", "addressLine1", "city", "postalCode" }, true);


            // Requirements for SchedulingTargets (projects/patients)
            Database.Table schedulingTargetQualificationRequirementTable = database.AddTable("SchedulingTargetQualificationRequirement");
            schedulingTargetQualificationRequirementTable.comment = @"Defines which qualifications are required (or preferred) for working on a specific SchedulingTarget.  - isRequired = true then resource MUST have qualification  - isRequired = false then nice-to-have (warning only)";
            schedulingTargetQualificationRequirementTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            schedulingTargetQualificationRequirementTable.AddIdField();
            schedulingTargetQualificationRequirementTable.AddMultiTenantSupport();
            schedulingTargetQualificationRequirementTable.AddForeignKeyField(schedulingTargetTable, false, true);
            schedulingTargetQualificationRequirementTable.AddForeignKeyField(qualificationTable, false, true);
            schedulingTargetQualificationRequirementTable.AddBoolField("isRequired", false, true).AddScriptComments("true = mandatory, false = preferred");
            schedulingTargetQualificationRequirementTable.AddVersionControl();
            schedulingTargetQualificationRequirementTable.AddControlFields();

            schedulingTargetQualificationRequirementTable.AddUniqueConstraint("tenantGuid", "schedulingTargetId", "qualificationId", false);


            #endregion



            #region Recurrence Support

            // Master list of recurrence frequencies 
            Database.Table recurrenceFrequencyTable = database.AddTable("RecurrenceFrequency");
            recurrenceFrequencyTable.comment = "Master list of recurrence frequencies. Mirrors common iCalendar frequencies.";
            recurrenceFrequencyTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            recurrenceFrequencyTable.AddIdField();
            recurrenceFrequencyTable.AddNameAndDescriptionFields(true, true, false);
            recurrenceFrequencyTable.AddSequenceField();
            recurrenceFrequencyTable.AddControlFields();

            recurrenceFrequencyTable.AddData(new Dictionary<string, string> {
                { "name", "Once" }, 
                { "description", "Does not repeat" }, 
                { "sequence", "1" },
                { "objectGuid", "a2e0f727-8e79-4add-af0a-495e89a4c6b7" } });


            recurrenceFrequencyTable.AddData(new Dictionary<string, string> {
                { "name", "Daily" }, 
                { "description", "Repeats every day or every N days" }, 
                { "sequence", "2" },
                { "objectGuid", "bd28a0b1-26cf-4973-9129-bcd1cc5c9f67" } });

            recurrenceFrequencyTable.AddData(new Dictionary<string, string> {
                { "name", "Weekly" }, 
                { "description", "Repeats every week on selected days" }, 
                { "sequence", "3" },
                { "objectGuid", "044f3c91-7745-467a-955a-809acdc0dba7" } });

            recurrenceFrequencyTable.AddData(new Dictionary<string, string> {
                { "name", "Monthly" }, 
                { "description", "Repeats monthly (by day of month or day of week)" }, 
                { "sequence", "4" },
                { "objectGuid", "fa0a9d3f-86e2-46c1-9a14-ea3858facf09" } });

            recurrenceFrequencyTable.AddData(new Dictionary<string, string> {
                { "name", "Yearly" }, 
                { "description", "Repeats annually" }, 
                { "sequence", "5" },
                { "objectGuid", "3ffeb2e0-0ced-4fc2-a268-bb31a3f5a861" } });

            // The recurrence rule attached to a ScheduledEvent (one-to-one for simplicity)
            Database.Table recurrenceRuleTable = database.AddTable("RecurrenceRule");
            recurrenceRuleTable.comment = @"Defines a recurrence pattern for a ScheduledEvent.  One ScheduledEvent can have zero or one RecurrenceRule (for recurring series).  Instances are generated on-the-fly or materialized as needed.";
            recurrenceRuleTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            recurrenceRuleTable.AddIdField();
            recurrenceRuleTable.AddMultiTenantSupport();
            recurrenceRuleTable.AddForeignKeyField(recurrenceFrequencyTable, false, true);

            // Basic interval (every N units)
            recurrenceRuleTable.AddIntField("interval", false, 1).AddScriptComments("How often the pattern repeats (e.g., every 2 weeks)");

            // End conditions
            recurrenceRuleTable.AddDateTimeField("untilDateTime", true).AddScriptComments("Recurrence ends on this date (inclusive). NULL = no end date");

            recurrenceRuleTable.AddIntField("count", true).AddScriptComments("Maximum number of occurrences. NULL = unlimited");

            // Weekly: days of week (bitmask or JSON array — we'll use simple bitmask for queryability)
            recurrenceRuleTable.AddIntField("dayOfWeekMask", true, 0).AddScriptComments(@"Bitmask for weekly recurrence:  1 = Sunday, 2 = Monday, 4 = Tuesday, 8 = Wednesday, 16 = Thursday, 32 = Friday, 64 = Saturday Example: Monday + Wednesday + Friday = 2 + 8 + 32 = 42");

            // Monthly: day of month or Nth day of week
            recurrenceRuleTable.AddIntField("dayOfMonth", true).AddScriptComments("For monthly: specific day (1-31). NULL if using dayOfWeekInMonth");
            recurrenceRuleTable.AddIntField("dayOfWeekInMonth", true).AddScriptComments("Values: 1 = first, 2 = second, 3 = third, 4 = fourth, 5 = last, -1 = second-to-last, etc. Combine with dayOfWeekMask.  ");

            recurrenceRuleTable.AddVersionControl();
            recurrenceRuleTable.AddControlFields();


            #endregion


            #region Shift Patterns
            Database.Table shiftPatternTable = database.AddTable("ShiftPattern");
            shiftPatternTable.comment = @"Reusable standard shift patterns (e.g., 'Day Shift', 'Night Shift', 'Weekend Crew').  Resources can be assigned to a pattern, or have custom overrides.";
            shiftPatternTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            shiftPatternTable.AddIdField();
            shiftPatternTable.AddMultiTenantSupport();
            shiftPatternTable.AddNameAndDescriptionFields(true, true, true);
            shiftPatternTable.AddForeignKeyField(timeZoneTable, true, true);
            shiftPatternTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            shiftPatternTable.AddVersionControl();
            shiftPatternTable.AddControlFields();
            shiftPatternTable.canBeFavourited = true;


            Database.Table shiftPatternDayTable = database.AddTable("ShiftPatternDay");
            shiftPatternDayTable.comment = "Defines the days and availability windows for a ShiftPattern.";
            shiftPatternDayTable.AddIdField();
            shiftPatternDayTable.AddMultiTenantSupport();
            shiftPatternDayTable.AddForeignKeyField(shiftPatternTable, false, true);
            shiftPatternDayTable.AddIntField("dayOfWeek", false, 1).AddScriptComments("Day this rule applies to   1=Sunday..7=Saturday");
            shiftPatternDayTable.AddTimeField("startTime", false, true).AddScriptComments("Start of available window (local to pattern time zone) e.g., 07:00:00");
            shiftPatternDayTable.AddSingleField("hours", false, 8).AddScriptComments("Hours available from start time (handles overnight shifts cleanly)  Defaults to 8");
            shiftPatternDayTable.AddString250Field("label", true).AddScriptComments("e.g., Main Shift");
            shiftPatternDayTable.AddVersionControl();
            shiftPatternDayTable.AddControlFields();

            shiftPatternDayTable.AddUniqueConstraint("tenantGuid", "shiftPatternId", "dayOfWeek", false);

            #endregion

            #region Resources

            Database.Table resourceTable = database.AddTable("Resource");
            resourceTable.maxPostBytes = 5_000_000;          // Cap posts at 5 mb
            resourceTable.comment = @"The schedulable entities – people and assets.  Examples: 'John Doe (Operator)', 'CAT CP56B Roller #12', 'Conference Room A'.";
            resourceTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            resourceTable.AddIdField();
            resourceTable.AddMultiTenantSupport();
            resourceTable.AddNameAndDescriptionFields(true, true, true);
            resourceTable.AddForeignKeyField(officeTable, true, true).AddScriptComments("Optional office binding for a resource.");
            resourceTable.AddForeignKeyField(resourceTypeTable, false, true);
            resourceTable.AddForeignKeyField(shiftPatternTable, true, true).AddScriptComments("Standard shift pattern this resource follows (NULL = custom shifts via ResourceShift)");
            resourceTable.AddForeignKeyField(timeZoneTable, false, true);
            resourceTable.AddSingleField("targetWeeklyWorkHours", true, null);
            resourceTable.AddTextField("notes", true);

            Database.Table.Field rteiField = resourceTable.AddString100Field("externalId", true).AddScriptComments("Optional reference to an ID in an external system");
            rteiField.readPermissionLevelNeeded = CLIENT_ADMIN_USER_SECURITY_LEVEL;
            rteiField.hideOnDefaultLists = true;

            resourceTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            
            Database.Table.Field rtaField = resourceTable.AddTextField("attributes", true).AddScriptComments("to store arbitrary JSON");
            rtaField.hideOnDefaultLists = true;

            resourceTable.AddBinaryDataFields("avatar");            // avatar details
            resourceTable.AddVersionControl();
            resourceTable.AddControlFields();
            resourceTable.SetDisplayNameField("name");
            resourceTable.canBeFavourited = true;

            resourceTable.CreateIndexForFields(new List<string> { "tenantGuid", "externalId" });



            Database.Table resourceContactTable = database.AddTable("ResourceContact");
            resourceContactTable.comment = "The link between scheduling targets and contacts.";
            resourceContactTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            resourceContactTable.AddIdField();
            resourceContactTable.AddMultiTenantSupport();
            resourceContactTable.AddForeignKeyField(resourceTable, false, true);
            resourceContactTable.AddForeignKeyField(contactTable, false, true);
            resourceContactTable.AddBoolField("isPrimary", false, false).AddScriptComments("Indicates whether or not this contact should be considered a primary contact of the resource.");
            resourceContactTable.AddForeignKeyField(relationshipTypeTable, false).AddScriptComments("A description of the relationship between the resource and the contact.");
            resourceContactTable.AddVersionControl();
            resourceContactTable.AddControlFields();

            resourceContactTable.AddUniqueConstraint("tenantGuid", "resourceId", "contactId", false);



            /*
             * 

             -- "Find me the rate for John Doe (ResourceID 10) working as an Operator (RoleID 5) on Project A (TargetID 99)"
SELECT TOP 1 *
FROM [Scheduler].[RateSheet]
WHERE tenantGuid = @MyTenant
  AND rateType = 'Standard'
  AND effectiveDate <= @WorkDate
  AND (
       (schedulingTargetId = 99 AND resourceId = 10)       -- 1. Project + Person (Most Specific)
    OR (schedulingTargetId = 99 AND assignmentRoleId = 5)  -- 2. Project + Role (Prevailing Wage)
    OR (resourceId = 10 AND schedulingTargetId IS NULL)    -- 3. Person Global (John's special rate)
    OR (assignmentRoleId = 5 AND schedulingTargetId IS NULL) -- 4. Role Global (Standard Operator Rate)
  )
ORDER BY 
   -- Sort by "Specificity Score" to get the best match first
   (CASE WHEN schedulingTargetId IS NOT NULL THEN 2 ELSE 0 END + 
    CASE WHEN resourceId IS NOT NULL THEN 1 ELSE 0 END) DESC,
   effectiveDate DESC
             * 
             * 
             * */
            Database.Table rateSheetTable = database.AddTable("RateSheet");
            rateSheetTable.comment = @"Master Rate Sheet. 
Replaces simple Resource-based rating with a hierarchical lookup system.
Hierarchy Logic (System should look for the first match in this order):
1. Specific Resource on Specific Project (schedulingTargetId + resourceId)
2. Specific Role on Specific Project (schedulingTargetId + assignmentRoleId)
3. Specific Resource Global Rate (resourceId)
4. Specific Role Global Rate (assignmentRoleId)";

            rateSheetTable.SetTableToBeReadonlyForControllerCreationPurposes();     // make this read only so we can override the put/post to add some business rules to validate the hierarchical specificity rules

            rateSheetTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            rateSheetTable.AddIdField();
            rateSheetTable.AddMultiTenantSupport();

            //
            // The office to which the rate applies - optional
            //
            rateSheetTable.AddForeignKeyField(officeTable, true, true).AddScriptComments("Optional office binding for a rate sheet.");

            // The "Who" (Nullable columns allow for the hierarchy)
            rateSheetTable.AddForeignKeyField(assignmentRoleTable, true, true).AddScriptComments("Link to AssignmentRole. If populated, applies to anyone in this role.");
            rateSheetTable.AddForeignKeyField(resourceTable, true, true).AddScriptComments("Link to Resource. If populated, overrides the Role rate.");

            // -- The "Where" (Optional - for Project/Customer specific pricing)
            rateSheetTable.AddForeignKeyField(schedulingTargetTable, true, true).AddScriptComments("Link to SchedulingTarget. If populated, applies only to this project.");

            // The "What"
            rateSheetTable.AddForeignKeyField(rateTypeTable, false, true).AddScriptComments("e.g., 'Standard', 'Overtime', 'DoubleTime', 'Travel', 'Standby'");

            // The "When"
            rateSheetTable.AddDateTimeField("effectiveDate", false).AddScriptComments("The date this rate becomes active. Allows for historical reporting and future price increases.");

            // The "How Much"
            rateSheetTable.AddForeignKeyField(currencyTable, false, true);

            rateSheetTable.AddMoneyField("costRate", false, true).AddScriptComments("Internal Cost (payroll)");
            rateSheetTable.AddMoneyField("billingRate", false, true).AddScriptComments("Invoicing Cost (customre)");
            rateSheetTable.AddTextField("notes", true).AddScriptComments("For ad-hoc notes about the entry");
            rateSheetTable.AddVersionControl();
            rateSheetTable.AddControlFields();

            rateSheetTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "assignmentRoleId", "resourceId", "schedulingTargetId", "rateTypeId", "effectiveDate" }, true);

            // Index to optimize the hierarchical lookup logic
            rateSheetTable.CreateIndexForFields(new List<string>() { "tenantGuid", "schedulingTargetId", "resourceId", "assignmentRoleId", "rateTypeId", "effectiveDate" });

            // Which resources hold which qualifications
            Database.Table resourceQualificationTable = database.AddTable("ResourceQualification");
            resourceQualificationTable.comment = @"Links resources to qualifications they possess.  Includes expiry date, issuing authority, and notes.";
            resourceQualificationTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            resourceQualificationTable.AddIdField();
            resourceQualificationTable.AddMultiTenantSupport();
            resourceQualificationTable.AddForeignKeyField(resourceTable, false, true);
            resourceQualificationTable.AddForeignKeyField(qualificationTable, false, true);

            resourceQualificationTable.AddDateTimeField("issueDate", true).AddScriptComments("Date qualification was granted");
            resourceQualificationTable.AddDateTimeField("expiryDate", true).AddScriptComments("NULL = no expiry (e.g., permanent license)").CreateIndex();
            resourceQualificationTable.AddString250Field("issuer", true).AddScriptComments("e.g., State Board of Nursing, NCCCO");
            resourceQualificationTable.AddTextField("notes", true);

            resourceQualificationTable.AddVersionControl();
            resourceQualificationTable.AddControlFields();

            resourceQualificationTable.AddUniqueConstraint("tenantGuid", "resourceId", "qualificationId", false);



            #endregion



            #region Resource Availability / Blackout Periods And Resource Shfits

            Database.Table resourceAvailabilityTable = database.AddTable("ResourceAvailability");
            resourceAvailabilityTable.comment = @"Defines periods when a resource is unavailable (blackouts).  Used for vacations, maintenance, training, etc.  If endDateTime is NULL the blackout is ongoing until cleared.";
            resourceAvailabilityTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            resourceAvailabilityTable.AddIdField();
            resourceAvailabilityTable.AddMultiTenantSupport();
            resourceAvailabilityTable.AddForeignKeyField(resourceTable, false, true);
            resourceAvailabilityTable.AddForeignKeyField(timeZoneTable, true, true);
            resourceAvailabilityTable.AddDateTimeField("startDateTime", false).AddScriptComments("Inclusive start of the blackout period");
            resourceAvailabilityTable.AddDateTimeField("endDateTime", true).AddScriptComments("NULL = ongoing blackout");
            resourceAvailabilityTable.AddString250Field("reason", true);
            resourceAvailabilityTable.AddTextField("notes", true);
            resourceAvailabilityTable.AddVersionControl();
            resourceAvailabilityTable.AddControlFields();

            resourceAvailabilityTable.CreateIndexForFields(new List<string> { "tenantGuid", "resourceId", "startDateTime", "endDateTime" });



            // shift for a resource - for override purposes
            Database.Table resourceShiftTable = database.AddTable("ResourceShift").AddScriptComments("Ovrerride shift for a resource");
            resourceShiftTable.comment = @"Defines regular working shifts for a resource (e.g., clinician hours).  Used to determine baseline availability. Blackouts (ResourceAvailability) override these for exceptions.";
            resourceShiftTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            resourceShiftTable.AddIdField();
            resourceShiftTable.AddMultiTenantSupport();
            resourceShiftTable.AddForeignKeyField(resourceTable, false, true);

            // Day of week (1=Sunday, 7=Saturday) — matches .NET DayOfWeek
            resourceShiftTable.AddIntField("dayOfWeek", false, 1).AddScriptComments("1=Sunday through 7=Saturday");

            // Shift times (required)
            resourceShiftTable.AddForeignKeyField(timeZoneTable, true, true);
            resourceShiftTable.AddTimeField("startTime", false, true).AddScriptComments("Shift start time (e.g., 09:00:00)");
            resourceShiftTable.AddSingleField("hours", false, 8).AddScriptComments("Hours available from start time (handles overnight shifts cleanly)  Defaults to 8");

            //resourceShiftTable.AddTimeField("endTime", false, true).AddScriptComments("Shift end time (e.g., 17:00:00)");

            // Optional label for complex shifts
            resourceShiftTable.AddString250Field("label", true).AddScriptComments("e.g., 'Morning Clinic', 'On-Call'");

            resourceShiftTable.AddVersionControl();
            resourceShiftTable.AddControlFields();

            // Unique: one shift definition per resource per day
            resourceShiftTable.AddUniqueConstraint("tenantGuid", "resourceId", "dayOfWeek", false);

            #endregion


            #region Crews – Persistent groups of resources

            Database.Table crewTable = database.AddTable("Crew");
            crewTable.maxPostBytes = 5_000_000;          // Cap posts at 5 mb
            crewTable.comment = @"Named, reusable group of resources that are typically scheduled together.  Common in construction (e.g., a roller + operator + spotter).  Crews can be assigned to events as a single unit.";

            crewTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            crewTable.AddIdField();
            crewTable.AddMultiTenantSupport();
            crewTable.AddNameAndDescriptionFields(true, true, true);
            crewTable.AddTextField("notes", true);
            crewTable.AddForeignKeyField(officeTable, true, true).AddScriptComments("Optional office binding for a crew.");
            crewTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            crewTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            crewTable.AddBinaryDataFields("avatar");            // avatar details
            crewTable.AddVersionControl();
            crewTable.AddControlFields();
            crewTable.SetDisplayNameField("name");
            crewTable.canBeFavourited = true;

            Database.Table crewMemberTable = database.AddTable("CrewMember");
            crewMemberTable.comment = @"Membership definition for a crew.  Specifies which resource belongs to which crew, the role they play within the crew, and a display sequence.";
            crewMemberTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            crewMemberTable.AddIdField();
            crewMemberTable.AddMultiTenantSupport();
            crewMemberTable.AddForeignKeyField(crewTable, false, true);
            crewMemberTable.AddForeignKeyField(resourceTable, false, true);
            crewMemberTable.AddForeignKeyField(assignmentRoleTable, true, true).AddScriptComments("Optional default role this member fulfils when the crew is assigned");
            crewMemberTable.AddIntField("sequence", false, 1).AddScriptComments("Display/order position within the crew (lower numbers appear first)");
            crewMemberTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            crewMemberTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            crewMemberTable.AddVersionControl();
            crewMemberTable.AddControlFields();

            crewMemberTable.AddUniqueConstraint("tenantGuid", "crewId", "resourceId", false);

            #endregion


            #region Scheduled Events


            Database.Table scheduledEventTemplateTable = database.AddTable("ScheduledEventTemplate");
            scheduledEventTemplateTable.comment = @"Pre-defined event templates for common appointment/activity types.   Includes default duration, required roles, default assignments, etc.";
            scheduledEventTemplateTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            scheduledEventTemplateTable.AddIdField();
            scheduledEventTemplateTable.AddMultiTenantSupport();
            scheduledEventTemplateTable.AddNameAndDescriptionFields(true, true, true);

            scheduledEventTemplateTable.AddBoolField("defaultAllDay", false).AddScriptComments("Default all day flag.");

            // Default duration in minutes
            scheduledEventTemplateTable.AddIntField("defaultDurationMinutes", false, 60);

            // Optional link to SchedulingTargetType (e.g., only for Patients)
            scheduledEventTemplateTable.AddForeignKeyField(schedulingTargetTypeTable, true, true).AddScriptComments("Optional target type");

            // Optional link to PriorityType table
            scheduledEventTemplateTable.AddForeignKeyField(priorityTable, true, false).AddScriptComments("Optional priority");

            // Default location pattern
            scheduledEventTemplateTable.AddString250Field("defaultLocationPattern", true).AddScriptComments("e.g., 'Patient Home', 'Main Site'");

            scheduledEventTemplateTable.AddVersionControl();
            scheduledEventTemplateTable.AddControlFields();
            scheduledEventTemplateTable.canBeFavourited = true;


            Database.Table scheduledEventTemplateChargeTable = database.AddTable("ScheduledEventTemplateCharge");
            scheduledEventTemplateChargeTable.comment = @"====================================================================================================
 SCHEDULED EVENT TEMPLATE CHARGES (For Auto-Dropping)
 Defines default charges for ScheduledEventTemplate).
 When an event is created from a template, these charges are auto-dropped onto the event.
 ====================================================================================================";
            scheduledEventTemplateChargeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            scheduledEventTemplateChargeTable.AddIdField();
            scheduledEventTemplateChargeTable.AddMultiTenantSupport();
            scheduledEventTemplateChargeTable.AddForeignKeyField(scheduledEventTemplateTable, false, true).AddScriptComments("Link to ScheduledEventTemplate");
            scheduledEventTemplateChargeTable.AddForeignKeyField(chargeTypeTable, false, true).AddScriptComments("Link to ChargeType (the charge to drop).");
            scheduledEventTemplateChargeTable.AddMoneyField("defaultAmount", false, true).AddScriptComments("The amount to auto-drop (can be overridden on event).");
            scheduledEventTemplateChargeTable.AddBoolField("isRequired", false, true).AddScriptComments("some default charges might be optional (e.g., optional add-on fee).");
            scheduledEventTemplateChargeTable.AddVersionControl();
            scheduledEventTemplateChargeTable.AddControlFields();



            // Template-level qualification defaults
            Database.Table scheduledEventTemplateQualificationRequirementTable = database.AddTable("ScheduledEventTemplateQualificationRequirement");
            scheduledEventTemplateQualificationRequirementTable.comment = "Default qualification requirements for events created from a template.";
            scheduledEventTemplateQualificationRequirementTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            scheduledEventTemplateQualificationRequirementTable.AddIdField();
            scheduledEventTemplateQualificationRequirementTable.AddMultiTenantSupport();
            scheduledEventTemplateQualificationRequirementTable.AddForeignKeyField(scheduledEventTemplateTable, false, true);
            scheduledEventTemplateQualificationRequirementTable.AddForeignKeyField(qualificationTable, false, true);
            scheduledEventTemplateQualificationRequirementTable.AddBoolField("isRequired", false, true);
            scheduledEventTemplateQualificationRequirementTable.AddVersionControl();
            scheduledEventTemplateQualificationRequirementTable.AddControlFields();

            scheduledEventTemplateQualificationRequirementTable.AddUniqueConstraint("tenantGuid", "scheduledEventTemplateId", "qualificationId", false);



            Database.Table scheduledEventTable = database.AddTable("ScheduledEvent");


            //
            // Custom overrides for the getters.  Primarily so that we can make custom versions that support ranges for the date filters for start and end date.
            //
            // Default implemnentation is equality for those fields...
            //
            scheduledEventTable.SetCodeGeneratorOverideExpectedRules(mvcDefineFieldsToBeOverridden: false,
                                                                     mvcDefineChildrenEntitiesToBeOverridden: false,
                                                                     webAPIListGetterToBeOverridden: true,          // To allow for custom getter that makes data params ranges
                                                                     webAPIIdGetterToBeOverridden: false,
                                                                     webAPIPostToBeOverridden: false,
                                                                     webAPIPutToBeOverridden: false,
                                                                     webAPIDeleteToBeOverridden: false,
                                                                     webAPIGetListDataToBeOverridden: true,
                                                                     webAPIGetRowCountToBeOverridden: true);        // To allow for custom getter that makes data params ranges



            scheduledEventTable.comment = @"Core scheduling entity – any planned activity with a defined time range.  This managest recurrences with the 'Detachment Model'

How it works:
The Master: You create the Series (Event A). It has the RecurrenceRule.
The Virtuals: The UI calculates the ""Ghost"" instances for display.
The Exception: If you assign a specific crew to next Tuesday's instance (or move it), the system ""Detaches"" that instance.
It creates a new row in ScheduledEvent (Event B).
Event B is linked to Event A via a parentScheduledEventId.
You add a record to RecurrenceException for Event A saying ""Skip the normal generation for Date X.""
You attach the specific Crew/Resource to Event B.";
            scheduledEventTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            scheduledEventTable.AddIdField();
            scheduledEventTable.AddMultiTenantSupport();

            scheduledEventTable.AddForeignKeyField(officeTable, true, true).AddScriptComments("Snapshot of office that the first resource assigned to this event belongs to.  This should NOT be updated if a resource moves to a different office post event assignment.  It should only change if there was an original entry error that needs to be corrected.");
            scheduledEventTable.AddForeignKeyField(clientTable, true, true).AddScriptComments("Snapshot of client that this event belongs to.  It should be that of the scheduling target.  It should only change if there was an original entry error that needs to be corrected.");

            // Retain the template that was used to create this scheduled event, if one was used.
            scheduledEventTable.AddForeignKeyField(scheduledEventTemplateTable, true, true).AddScriptComments("Optional template/type of this scheduled event.");

            // Link ScheduledEvent to RecurrenceRule (one-to-one)
            scheduledEventTable.AddForeignKeyField(recurrenceRuleTable, true, true).AddScriptComments("Optional recurrence pattern for this event series");


            // ScheduledEvent to link to SchedulingTarget
            scheduledEventTable.AddForeignKeyField(schedulingTargetTable, true, true).AddScriptComments("The SchedulingTarget (project, patient, etc.) this event is scheduled into");
            scheduledEventTable.AddForeignKeyField(timeZoneTable, true, true);
            scheduledEventTable.AddForeignKeyField("parentScheduledEventId", scheduledEventTable, true, true).AddScriptComments("If populated, this Event is a specific \"Detached\" instance of a Series");
            scheduledEventTable.AddDateTimeField("recurrenceInstanceDate", true).AddScriptComments("The original date this instance represented (crucial for matching with RecurrenceException)");
            scheduledEventTable.AddNameAndDescriptionFields(true, true, true);
            scheduledEventTable.AddBoolField("isAllDay", true, false).AddScriptComments("Whether this is an all day event or not");
            scheduledEventTable.AddDateTimeField("startDateTime", false).AddScriptComments("Inclusive start of the event in UTC").CreateIndex();
            scheduledEventTable.AddDateTimeField("endDateTime", false).AddScriptComments("Exclusive end of the event in UTC").CreateIndex();
            scheduledEventTable.AddString250Field("location", true).CreateIndex();
            scheduledEventTable.AddForeignKeyField(eventStatusTable, false, true).AddScriptComments("Status for the event");
            scheduledEventTable.AddForeignKeyField(resourceTable, true, true).AddScriptComments("Optional primary/lead resource for the event");
            scheduledEventTable.AddForeignKeyField(crewTable, true, true).AddScriptComments("Optional primary/lead crew for the event");
            scheduledEventTable.AddForeignKeyField(priorityTable, true, true).AddScriptComments("Optional priority");
            scheduledEventTable.AddForeignKeyField(bookingSourceTypeTable, true, true).AddScriptComments("Optional booking source for reservation type workflows.");
            scheduledEventTable.AddIntField("partySize", true, null).AddScriptComments("Optional for use when running as a reservation system");
            scheduledEventTable.AddTextField("notes", true);
            scheduledEventTable.AddHTMLColorField("color", true).AddScriptComments("Override Hex color for UI display");


            Database.Table.Field seeiField = scheduledEventTable.AddString100Field("externalId", true).AddScriptComments("Optional link to an entity in another system");
            seeiField.readPermissionLevelNeeded = CLIENT_ADMIN_USER_SECURITY_LEVEL;
            seeiField.hideOnDefaultLists = true;

            Database.Table.Field seaField = scheduledEventTable.AddTextField("attributes", true).AddScriptComments("to store arbitrary JSON");
            seaField.hideOnDefaultLists = true;

            scheduledEventTable.AddVersionControl();
            scheduledEventTable.AddControlFields();
            scheduledEventTable.SetDisplayNameField("name");
            scheduledEventTable.canBeFavourited = true;

            scheduledEventTable.CreateIndexForFields(new List<string> { "tenantGuid", "startDateTime", "endDateTime" });


            Database.Table chargeStatusTable = database.AddTable("ChargeStatus");
            chargeStatusTable.comment = "Master list of charge statuses (Pending, Approved, Invoiced, Void)";
            chargeStatusTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            chargeStatusTable.AddIdField();
            chargeStatusTable.AddNameAndDescriptionFields(true, true, false);
            chargeStatusTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            chargeStatusTable.AddSequenceField();
            chargeStatusTable.AddControlFields();


            // Seed common statuses
            chargeStatusTable.AddData(new Dictionary<string, string> {
                { "name", "Pending" },
                { "description", "Pending Approval" },
                { "sequence", "1" },
                { "color", "#B8FFC3" },
                { "objectGuid", "1379f1da-c3cc-4149-998a-95ffa1728db6" } });

            chargeStatusTable.AddData(new Dictionary<string, string> {
                { "name", "Approved" },
                { "description", "Approved " },
                { "sequence", "2" },
                { "color", "#59FF6F" },
                { "objectGuid", "ea16c955-9ccf-4489-acc0-0757c39ac3b6" } });


            chargeStatusTable.AddData(new Dictionary<string, string> {
                { "name", "Invoiced" },
                { "description", "Invoiced" },
                { "sequence", "3" },
                { "color", "#35A145" },
                { "objectGuid", "d250cc5c-51e9-49bb-91ce-4be47fc30dc0" } });


            chargeStatusTable.AddData(new Dictionary<string, string> {
                { "name", "Void" },
                { "description", "Void - Charge Disregarded" },
                { "color", "#C62828" },
                { "sequence", "4" },
                { "objectGuid", "19d6560f-ed85-4d1e-905f-9a6e3dfb3026" } });


            Database.Table eventChargeTable = database.AddTable("EventCharge");
            eventChargeTable.comment = @"====================================================================================================
 EVENT CHARGES
 Stores charges dropped on ScheduledEvents (automatic or manual).
 Linked to ChargeType for categorization.
 Exportable to QuickBooks as JournalEntries or Invoices.

DESIGN NOTE: EventCharge supports both flat fees and quantity-based charges.
- Flat fee: quantity = 1, unitPrice = NULL or = extendedAmount
- Variable: quantity > 0, unitPrice set → extendedAmount = quantity × unitPrice
- The system should always store the final extendedAmount (allows manual overrides)
- Use externalId + exportedDate for idempotent GL sync

====================================================================================================";

            eventChargeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            eventChargeTable.AddIdField();
            eventChargeTable.AddMultiTenantSupport();
            eventChargeTable.AddForeignKeyField(scheduledEventTable, false, true).AddScriptComments("Link to the ScheduledEvent table.");
            eventChargeTable.AddForeignKeyField(resourceTable, true, true).AddScriptComments("Optional link to resource to bind charge to specific resources (e.g., labor cost per operator");
            eventChargeTable.AddForeignKeyField(chargeTypeTable, false, true).AddScriptComments("Link to the ChargeType table (defines revenue/expense category).");
            eventChargeTable.AddForeignKeyField(chargeStatusTable, false, true).AddScriptComments("Link to the ChargeStatus table.  Tracks the status of the charge from creation through invoicing or cancelling.");
            eventChargeTable.AddDecimalField("quantity", true, 1, true).AddScriptComments("Quantity (hours, units, km, etc.)");
            eventChargeTable.AddMoneyField("unitPrice", true, true).AddScriptComments("Price per unit (can be NULL for flat fees)");
            eventChargeTable.AddMoneyField("extendedAmount", false, 0, true).AddScriptComments("Always the final calculated/total amount (quantity × unitPrice, or just amount) Does not include taxes.");
            eventChargeTable.AddMoneyField("taxAmount", false, 0, true).AddScriptComments("The calculated tax based on isTaxable");


            eventChargeTable.AddForeignKeyField(currencyTable, false, true).AddScriptComments("Link to Currency table.");
            eventChargeTable.AddForeignKeyField(rateTypeTable, true, true).AddScriptComments("Optional link to RateType (e.g., 'Overtime').");
            eventChargeTable.AddTextField("notes", true).AddScriptComments("Optional notes about the charge");
            eventChargeTable.AddBoolField("isAutomatic", false, true).AddScriptComments("1 = auto-dropped from event type, 0 = manual add/edit.");
            eventChargeTable.AddDateTimeField("exportedDate", true).AddScriptComments("When this charge was last exported (null = not exported yet).");
            eventChargeTable.AddString100Field("externalId", true).AddScriptComments("Identifier from extenral system - possibly invoice number or some other billing grouper").CreateIndex();
            eventChargeTable.AddVersionControl();
            eventChargeTable.AddControlFields();



            //
            // Add contact interaction after scheduled event so we can use scheduled interaction as an FK cleanly and not be the last field if we were to add it at the end if we had defined this table before.
            //
            Database.Table contactInteractionTable = database.AddTable("ContactInteraction");
            contactInteractionTable.comment = "The contact interaction data";
            contactInteractionTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            contactInteractionTable.AddIdField();
            contactInteractionTable.AddMultiTenantSupport();
            contactInteractionTable.AddForeignKeyField(contactTable, false, true).AddScriptComments("The contact that is the target of the interaction.");
            contactInteractionTable.AddForeignKeyField("initiatingContactId", contactTable, true, true).AddScriptComments("Optional contact that initiated the interaction.  This would be staff of the company using the scheduler");
            contactInteractionTable.AddForeignKeyField(interactionTypeTable, false, true);
            contactInteractionTable.AddForeignKeyField(scheduledEventTable, true, true).AddScriptComments("The optional event that the interaction is regarding.");

            contactInteractionTable.AddDateTimeField("startTime", false);
            contactInteractionTable.AddDateTimeField("endTime", true);
            contactInteractionTable.AddTextField("notes", true).AddScriptComments("Optional notes about the interaction");
            contactInteractionTable.AddTextField("location", true).AddScriptComments("Optional location details about the interaction");

            contactInteractionTable.AddForeignKeyField(priorityTable, true, true).AddScriptComments("Optional priority for the interaction.");

            Database.Table.Field cieiField = contactInteractionTable.AddString100Field("externalId", true);
            cieiField.readPermissionLevelNeeded = CLIENT_ADMIN_USER_SECURITY_LEVEL;                     // arbitrary key for an external system's key
            cieiField.hideOnDefaultLists = true;
            cieiField.CreateIndex();

            contactInteractionTable.AddVersionControl();
            contactInteractionTable.AddControlFields();

            contactInteractionTable.CreateIndexForFields(new List<string>() { "tenantGuid", "contactId", "startTime" });




            Database.Table eventCalendarTable = database.AddTable("EventCalendar");
            eventCalendarTable.comment = "Many-to-many relationship between events and calendars.";
            eventCalendarTable.AddIdField();
            eventCalendarTable.AddMultiTenantSupport();
            eventCalendarTable.AddForeignKeyField(scheduledEventTable, false, true);
            eventCalendarTable.AddForeignKeyField(calendarTable, false, true);
            eventCalendarTable.AddControlFields();
            eventCalendarTable.AddUniqueConstraint("tenantGuid", "scheduledEventId", "calendarId", false);





            Database.Table dependencyTypeTable = database.AddTable("DependencyType");
            dependencyTypeTable.comment = "Master list of depedency types";
            dependencyTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            dependencyTypeTable.AddIdField();
            dependencyTypeTable.AddNameAndDescriptionFields(true, true, false);
            dependencyTypeTable.AddSequenceField();
            dependencyTypeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            dependencyTypeTable.AddControlFields();

            dependencyTypeTable.AddData(new Dictionary<string, string> {
                { "name", "FS" },
                { "description", "Finish to Start" },
                { "sequence", "1" },
                { "objectGuid", "f08977bf-af84-4d89-9821-f8a2404028fa" } });

            dependencyTypeTable.AddData(new Dictionary<string, string> {
                { "name", "SS" },
                { "description", "Start to Start" },
                { "sequence", "2" },
                { "objectGuid", "51398efa-2489-41ba-a1b6-77d11ce6253b" } });

            dependencyTypeTable.AddData(new Dictionary<string, string> {
                { "name", "SF" },
                { "description", "Start to Finish" },
                { "sequence", "3" },
                { "objectGuid", "637dc30a-adc3-47ad-87fa-3c826b7d808f" } });

            dependencyTypeTable.AddData(new Dictionary<string, string> {
                { "name", "FF" },
                { "description", "Finish to Finish" },
                { "sequence", "4" },
                { "objectGuid", "fc7b4932-e79a-4085-9c87-404d29331f85" } });


            Database.Table scheduledEventDependencyTable = database.AddTable("ScheduledEventDependency");


            scheduledEventDependencyTable.comment = @"Dependencies that a scheduled event has that could affect it.";
            scheduledEventDependencyTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            scheduledEventDependencyTable.AddIdField();
            scheduledEventDependencyTable.AddMultiTenantSupport();
            scheduledEventDependencyTable.AddForeignKeyField("predecessorEventId", scheduledEventTable, false, true).AddScriptComments("The task that must happen first");
            scheduledEventDependencyTable.AddForeignKeyField("successorEventId", scheduledEventTable, false, true).AddScriptComments("The task that waits");
            scheduledEventDependencyTable.AddForeignKeyField(dependencyTypeTable, false, false);
            scheduledEventDependencyTable.AddIntField("lagMinutes", false, 0);
            scheduledEventDependencyTable.AddVersionControl();
            scheduledEventDependencyTable.AddControlFields();

            scheduledEventDependencyTable.AddUniqueConstraint("tenantGuid", "predecessorEventId", "successorEventId");



            Database.Table scheduledEventQualificationRequirementTable = database.AddTable("ScheduledEventQualificationRequirement");

            scheduledEventQualificationRequirementTable.comment = @"Specific qualifications required for a single event instance, overriding or adding to role/site reqs..";
            scheduledEventQualificationRequirementTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            scheduledEventQualificationRequirementTable.AddIdField();
            scheduledEventQualificationRequirementTable.AddMultiTenantSupport();
            scheduledEventQualificationRequirementTable.AddForeignKeyField(scheduledEventTable, false);
            scheduledEventQualificationRequirementTable.AddForeignKeyField(qualificationTable, false);
            scheduledEventQualificationRequirementTable.AddVersionControl();
            scheduledEventQualificationRequirementTable.AddControlFields();



            // Exception dates (canceled or moved instances)
            Database.Table recurrenceExceptionTable = database.AddTable("RecurrenceException");
            recurrenceExceptionTable.comment = @"Exceptions to a recurring series.  Used for canceled dates or moved instances (original date + new date).";
            recurrenceExceptionTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            recurrenceExceptionTable.AddIdField();
            recurrenceExceptionTable.AddMultiTenantSupport();
            recurrenceExceptionTable.AddForeignKeyField(scheduledEventTable, false, true);
            recurrenceExceptionTable.AddDateTimeField("exceptionDateTime", false).AddScriptComments("The original occurrence date/time that is excepted");
            recurrenceExceptionTable.AddDateTimeField("movedToDateTime", true).AddScriptComments("NULL = canceled, non-NULL = moved to this new date/time");
            recurrenceExceptionTable.AddString250Field("reason", true);
            recurrenceExceptionTable.AddVersionControl();
            recurrenceExceptionTable.AddControlFields();

            recurrenceExceptionTable.AddUniqueConstraint("tenantGuid", "scheduledEventId", "exceptionDateTime", false);


            #endregion



            #region Event Resource Assignments (individual + crew)

            Database.Table eventResourceAssignmentTable = database.AddTable("EventResourceAssignment");
            eventResourceAssignmentTable.comment = @"Links resources (or entire crews) to events.  Supports partial assignments and role designation.  - If crewId is non-NULL → this row represents assignment of the whole crew - If resourceId is non-NULL and crewId is NULL → individual resource assignment - assignmentStart/End NULL → uses full event duration";
            eventResourceAssignmentTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            eventResourceAssignmentTable.AddIdField();
            eventResourceAssignmentTable.AddMultiTenantSupport();
            eventResourceAssignmentTable.AddForeignKeyField(scheduledEventTable, false, true);
            eventResourceAssignmentTable.AddForeignKeyField(officeTable, true, true).AddScriptComments("Snapshot of office resource assigned to this event belongs to at the time of assignment.  This should never change, and should NOT be updated if a resource moves to a different office post event assignment.");

            eventResourceAssignmentTable.AddForeignKeyField(resourceTable, true, true).AddScriptComments("Required for individual assignments; should be NULL when crewId is used");
            eventResourceAssignmentTable.AddForeignKeyField(crewTable, true, true).AddScriptComments("Optional – when set, assigns the entire crew as a unit");
            eventResourceAssignmentTable.AddForeignKeyField(assignmentRoleTable, true, true).AddScriptComments("Optional role for this assignment (individual or crew member default)");


            Database.Table.Field eventResourceAssignemtAssignmentStatusField = eventResourceAssignmentTable.AddForeignKeyField(assignmentStatusTable, false, true).AddScriptComments("NULL = Planned, non-NULL links to AssignmentStatus master table");
            eventResourceAssignemtAssignmentStatusField.defaultValue = "1"; // This here to make the 'Planned' status the default value.

            eventResourceAssignmentTable.AddDateTimeField("assignmentStartDateTime", true).AddScriptComments("NULL = starts at event start");
            eventResourceAssignmentTable.AddDateTimeField("assignmentEndDateTime", true).AddScriptComments("NULL = ends at event end");
            eventResourceAssignmentTable.AddTextField("notes", true);

            eventResourceAssignmentTable.AddBoolField("isTravelRequired", true).AddScriptComments("Whether or not travel is required for the assignment");

            eventResourceAssignmentTable.AddIntField("travelDurationMinutes", true, 0).AddScriptComments("Time required to get to the site");
            eventResourceAssignmentTable.AddSingleField("distanceKilometers", true, 0).AddScriptComments("Useful for expense calculation");
            eventResourceAssignmentTable.AddString100Field("startLocation", true);


            eventResourceAssignmentTable.AddDateTimeField("actualStartDateTime", true);
            eventResourceAssignmentTable.AddDateTimeField("actualEndDateTime", true);
            eventResourceAssignmentTable.AddTextField("actualNotes", true);

            eventResourceAssignmentTable.AddVersionControl();
            eventResourceAssignmentTable.AddControlFields();

            // Performance index for conflict and blackout detection queries
            eventResourceAssignmentTable.CreateIndexForFields(new List<string> { "tenantGuid", "resourceId", "assignmentStartDateTime", "assignmentEndDateTime" });
            eventResourceAssignmentTable.CreateIndexForFields(new List<string> { "tenantGuid", "crewId", "assignmentStartDateTime", "assignmentEndDateTime" });





            #endregion

            #region Notification subscripion rules

            Database.Table notificationTypeTable = database.AddTable("NotificationType");
            notificationTypeTable.comment = "Master list of notification types";
            notificationTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            notificationTypeTable.AddIdField();
            notificationTypeTable.AddNameAndDescriptionFields(true, true, false);
            notificationTypeTable.AddSequenceField();
            notificationTypeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            notificationTypeTable.AddControlFields();

            notificationTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Email" },
                { "description", "Send to email address" },
                { "sequence", "1" },
                { "objectGuid", "73ff7b17-3fd7-40ce-91bf-c91daca7b4ce" } });

            notificationTypeTable.AddData(new Dictionary<string, string> {
                { "name", "SMS" },
                { "description", "Sent to cell phone via SMS message" },
                { "sequence", "2" },
                { "objectGuid", "89391299-4427-43f6-bcf2-0266e47e83a7" } });

            notificationTypeTable.AddData(new Dictionary<string, string> {
                { "name", "Push" },
                { "description", "Sent to cell phone via Push notification" },
                { "sequence", "3" },
                { "objectGuid", "0395ddde-58dc-4577-9dae-07614680c386" } });



            Database.Table notificationSubscriptionTable = database.AddTable("NotificationSubscription");
            notificationSubscriptionTable.comment = @"Links resources (or entire crews) to events.  Supports partial assignments and role designation.  - If crewId is non-NULL → this row represents assignment of the whole crew - If resourceId is non-NULL and crewId is NULL → individual resource assignment - assignmentStart/End NULL → uses full event duration";
            notificationSubscriptionTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            notificationSubscriptionTable.AddIdField();
            notificationSubscriptionTable.AddMultiTenantSupport();
            notificationSubscriptionTable.AddForeignKeyField(resourceTable, true, true).AddScriptComments("Optional resource for this notification subscription.  Needs either this or contact to be valid.");
            notificationSubscriptionTable.AddForeignKeyField(contactTable, true, true).AddScriptComments("Optional contact for this notification subscription.  Needs either this or resource to be valid.");
            notificationSubscriptionTable.AddForeignKeyField(notificationTypeTable, false, true);
            notificationSubscriptionTable.AddIntField("triggerEvents", false, 1).AddScriptComments("Bitmask: 1=Assigned, 2=Canceled, 4=Modified, 8=Reminder");
            notificationSubscriptionTable.AddString250Field("recipientAddress", false).AddScriptComments("Email address or Phone #");

            notificationSubscriptionTable.AddVersionControl();
            notificationSubscriptionTable.AddControlFields();

            #endregion


            #region Fundraising Module


            Database.Table fundTable = database.AddTable("Fund");

            fundTable.comment = @"====================================================================================================
   FUNDRAISING MASTER DATA (The ""Codes"" in DonorPerfect)
   DP relies on three tiers of coding:
   1. Fund (GL Code) - Where the money goes in the bank.
   2. Campaign - The broad initiative (e.g., ""Capital Campaign"").
   3. Appeal - The specific ask (e.g., ""November Mailer"").
   ====================================================================================================

-- FUNDS (General Ledger Codes)";

            fundTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            fundTable.AddIdField();
            fundTable.AddMultiTenantSupport();
            fundTable.AddNameAndDescriptionFields(true, true, true);
            fundTable.AddString100Field("glCode").AddScriptComments("The accounting code");
            fundTable.AddBoolField("isRestricted", false, false).AddScriptComments("Legal restriction on funds");
            fundTable.AddMoneyField("goalAmount", true, true);
            fundTable.AddTextField("notes", true);
            fundTable.AddSequenceField();
            fundTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            fundTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            fundTable.AddVersionControl();
            fundTable.AddControlFields();



            Database.Table campaignTable = database.AddTable("Campaign");

            campaignTable.comment = @" 2. CAMPAIGNS (Broad Initiatives)";

            campaignTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            campaignTable.AddIdField();
            campaignTable.AddMultiTenantSupport();
            campaignTable.AddNameAndDescriptionFields(true, true, true);
            campaignTable.AddDateField("startDate", true, true);
            campaignTable.AddDateField("endDate", true, true);
            campaignTable.AddMoneyField("fundRaisingGoal", true, true);
            campaignTable.AddTextField("notes", true);
            campaignTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            campaignTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            campaignTable.AddVersionControl();
            campaignTable.AddControlFields();
            campaignTable.canBeFavourited = true;


            Database.Table appealTable = database.AddTable("Appeal");

            appealTable.comment = @" 3. APPEALS (Specific Solicitations)";

            appealTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            appealTable.AddIdField();
            appealTable.AddMultiTenantSupport();
            appealTable.AddForeignKeyField(campaignTable, true).AddScriptComments("Optional link to parent campaign");
            appealTable.AddNameAndDescriptionFields(true, true, true);
            appealTable.AddMoneyField("costPerUnit", true, true).AddScriptComments("For ROI calculation (Cost vs. Raised)");
            appealTable.AddTextField("notes", true);
            appealTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            appealTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            appealTable.AddVersionControl();
            appealTable.AddControlFields();
            appealTable.canBeFavourited = true;



            Database.Table householdTable = database.AddTable("Household");
            householdTable.maxPostBytes = 5_000_000;          // Cap posts at 5 mb

            householdTable.comment = @"====================================================================================================
   HOUSEHOLD MANAGEMENT
   Standardizes how multiple constituents are grouped for mailing, receipting, and recognition.
   This allows for ""The Smith Family"" recognition even if John and Jane have separate records.
   ====================================================================================================";

            householdTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            householdTable.AddIdField();
            householdTable.AddMultiTenantSupport();
            householdTable.AddNameAndDescriptionFields(true, true, true);

            //
            // Scheduling target link for scheduling integration and address information.  One household has one scheduling target
            //
            householdTable.AddForeignKeyField(schedulingTargetTable, true, true);


            //
            // Mailing overrides
            //
            householdTable.AddString250Field("formalSalutation", true).AddScriptComments("ex. \"Mr. and Mrs. John Smith\"");
            householdTable.AddString250Field("informalSalutation", true).AddScriptComments("ex. \"John and Jane\"");
            householdTable.AddString250Field("addressee", true).AddScriptComments("The label for the envelope");


            //
            // Calculated/Cached Metrics for the WHOLE family (Critical for "Major Donor" identification)
            //
            householdTable.AddMoneyField("totalHouseholdGiving", false, 0, true);
            householdTable.AddDateField("lastHouseholdGiftDate", true, true);


            householdTable.AddTextField("notes", true);
            householdTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            householdTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            householdTable.AddBinaryDataFields("avatar");            // avatar details
            householdTable.AddVersionControl();
            householdTable.AddControlFields();
            householdTable.canBeFavourited = true;


            Database.Table constituentJourneyStageTable = database.AddTable("ConstituentJourneyStage");
            constituentJourneyStageTable.comment = "Defines stages in a donor's journey (e.g., Target, Qualified, Cultivated, Solicited, Stewardship).";
            constituentJourneyStageTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            constituentJourneyStageTable.AddIdField();
            constituentJourneyStageTable.AddMultiTenantSupport();
            constituentJourneyStageTable.AddNameAndDescriptionFields(true, true, true);
            constituentJourneyStageTable.AddSequenceField();
            constituentJourneyStageTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            constituentJourneyStageTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");

            // Criteria fields for auto-calculation/suggestion
            constituentJourneyStageTable.AddMoneyField("minLifetimeGiving", true, true).AddScriptComments("Optional criteria: Minimum total giving to qualify for this stage.");
            constituentJourneyStageTable.AddMoneyField("maxLifetimeGiving", true, true).AddScriptComments("Optional criteria: Maximum total giving");
            constituentJourneyStageTable.AddMoneyField("minSingleGiftAmount", true, true).AddScriptComments("Optional criteria: Min single gift size");

            // Smart Criteria
            constituentJourneyStageTable.AddBoolField("isDefault", false).AddScriptComments("If true, this is the default stage for new constituents.");
            constituentJourneyStageTable.AddMoneyField("minAnnualGiving", true, true).AddScriptComments("Optional: Minimum giving in the past 365 days.");
            constituentJourneyStageTable.AddIntField("maxDaysSinceLastGift", true, 0).AddScriptComments("Optional: Maximum days elapsed since the last gift (recency limit).");
            constituentJourneyStageTable.AddIntField("minGiftCount", true, 0).AddScriptComments("Optional: Minimum number of gifts required.");
            
            constituentJourneyStageTable.AddVersionControl();
            constituentJourneyStageTable.AddControlFields();
            constituentJourneyStageTable.canBeFavourited = true;

            // Seed common stages
            constituentJourneyStageTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Unqualified" },
                { "description", "New potential donor." },
                { "sequence", "1" },
                { "isDefault", "1" },
                { "color", "#9E9E9E" },
                { "objectGuid", "d8663e5e-749c-4638-b69d-21d96078659d" } });

            constituentJourneyStageTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Qualified" },
                { "description", "Donor has been qualified." },
                { "sequence", "2" },
                { "isDefault", "0" },
                { "color", "#2196F3" },
                { "objectGuid", "ad06353d-2476-4322-836f-5374825968f9" } });

             constituentJourneyStageTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Cultivated" },
                { "description", "Relationship is being built." },
                { "sequence", "3" },
                { "isDefault", "0" },
                { "color", "#4CAF50" },
                { "objectGuid", "e8b60384-9336-4022-8b4b-970752538965" } });

            constituentJourneyStageTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Solicited" },
                { "description", "Ask has been made." },
                { "sequence", "4" },
                { "isDefault", "0" },
                { "color", "#FF9800" },
                { "objectGuid", "64319688-fd06-4074-8902-628670bf7471" } });

            constituentJourneyStageTable.AddData(new Dictionary<string, string> {
                { "tenantGuid", "00000000-0000-0000-0000-000000000000"},
                { "name", "Stewardship" },
                { "description", "Ongoing maintenance." },
                { "sequence", "5" },
                { "isDefault", "0" },
                { "color", "#9C27B0" },
                { "objectGuid", "1d971578-8319-482a-9e8c-529141873837" } });


            Database.Table constituentTable = database.AddTable("Constituent");
            constituentTable.maxPostBytes = 5_000_000;          // Cap posts at 5 mb

            constituentTable.comment = @" ====================================================================================================
   CONSTITUENT MANAGEMENT
   In DP, a Constituent is the heart of the system. 
   Here, we link to your existing Contact (Person) or Client (Organization) tables.
   This table stores the ""Fundraising Intelligence"" (RFM metrics).
   ====================================================================================================";

            constituentTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            constituentTable.AddIdField();
            constituentTable.AddMultiTenantSupport();



            constituentTable.AddForeignKeyField(contactTable, true).AddScriptComments("Polymorphic Link: A constituent is EITHER a generic Contact OR a Client (Company)");
            constituentTable.AddForeignKeyField(clientTable, true).AddScriptComments("Polymorphic Link: A constituent is EITHER a generic Contact OR a Client (Company)");


            //
            // Links a consituent to a household
            //
            constituentTable.AddForeignKeyField(householdTable, true, true).AddScriptComments("Links a constituent to a household");

            constituentTable.AddString50Field("constituentNumber", false).AddScriptComments("The distinct 'Donor ID'");

            constituentTable.AddBoolField("doNotSolicit", false, false);
            constituentTable.AddBoolField("doNotEmail", false, false);
            constituentTable.AddBoolField("doNotMail", false, false);


            //
            //Caching Calculated Metrics(DP Style for fast reporting)
            //
            constituentTable.AddMoneyField("totalLifetimeGiving", false, 0, true).AddScriptComments("Caching Calculated Metrics(DP Style for fast reporting)");
            constituentTable.AddMoneyField("totalYTDGiving", false, 0, true).AddScriptComments("Caching Calculated Metrics(DP Style for fast reporting)");
            constituentTable.AddDateField("lastGiftDate", true, true).AddScriptComments("Caching Calculated Metrics(DP Style for fast reporting)");
            constituentTable.AddMoneyField("lastGiftAmount", true, true).AddScriptComments("Caching Calculated Metrics(DP Style for fast reporting)");
            constituentTable.AddMoneyField("largestGiftAmount", true, true).AddScriptComments("Caching Calculated Metrics(DP Style for fast reporting)");
            constituentTable.AddIntField("totalGiftCount", true).AddScriptComments("Caching Calculated Metrics(DP Style for fast reporting)");

            constituentTable.AddString100Field("externalId", true).AddScriptComments("For things like QBO Customer ID");

            //
            // Standard attributes
            //
            constituentTable.AddTextField("notes", true);
            
            // Donor Journey Link
            constituentTable.AddForeignKeyField(constituentJourneyStageTable, true, true).AddScriptComments("Current stage in the donor journey.");
            constituentTable.AddDateTimeField("dateEnteredCurrentStage", true).AddScriptComments("Date when the constituent moved to the current stage.");
            Database.Table.Field constituentAttributesField = constituentTable.AddTextField("attributes", true).AddScriptComments("to store arbitrary JSON");
            constituentAttributesField.hideOnDefaultLists = true;

            constituentTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            constituentTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            constituentTable.AddBinaryDataFields("avatar");            // avatar details
            constituentTable.AddVersionControl();
            constituentTable.AddControlFields();
            constituentTable.canBeFavourited = true;



            Database.Table pledgeTable = database.AddTable("Pledge");

            pledgeTable.comment = @" ====================================================================================================
   PLEDGES
   A promise to pay. Gifts will link to this to ""pay it down"".
   ====================================================================================================";

            pledgeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_ADMIN_USER_SECURITY_LEVEL);
            pledgeTable.AddIdField();
            pledgeTable.AddMultiTenantSupport();
            pledgeTable.AddForeignKeyField(constituentTable, false);

            pledgeTable.AddMoneyField("totalAmount", false, true);
            pledgeTable.AddMoneyField("balanceAmount", false, true).AddScriptComments("Calculated: Total - Sum(LinkedGifts)");

            pledgeTable.AddDateField("pledgeDate", false, true);

            pledgeTable.AddDateField("startDate", true, true);
            pledgeTable.AddDateField("endDate", true, true);

            pledgeTable.AddForeignKeyField(recurrenceFrequencyTable, true, true);
            pledgeTable.AddForeignKeyField(fundTable, false, true);
            pledgeTable.AddForeignKeyField(campaignTable, true, true);
            pledgeTable.AddForeignKeyField(appealTable, true, true);

            pledgeTable.AddMoneyField("writeOffAmount", false, true).AddScriptComments("If they default on the pledge");
            pledgeTable.AddBoolField("isWrittenOff", false, false);

            pledgeTable.AddTextField("notes", true);
            pledgeTable.AddVersionControl();
            pledgeTable.AddControlFields();
            pledgeTable.canBeFavourited = true;


            Database.Table tributeTypeTable = database.AddTable("TributeType");
            tributeTypeTable.comment = "Master list of tribute types ( memory, honor, etc..)";
            tributeTypeTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            tributeTypeTable.AddIdField();
            tributeTypeTable.AddNameAndDescriptionFields(true, true, false);
            tributeTypeTable.AddSequenceField();
            tributeTypeTable.AddControlFields();

            tributeTypeTable.AddData(new Dictionary<string, string> {
                { "name", "In Memory Of" },
                { "description", "In Memory Of" },
                { "sequence", "1" },
                { "objectGuid", "27781845-ed5e-4bba-9216-751d5a8d778a" } });

            tributeTypeTable.AddData(new Dictionary<string, string> {
                { "name", "In Honor Of" },
                { "description", "In Honor Of" },
                { "sequence", "2" },
                { "objectGuid", "31af7566-28d1-460f-9cd9-9d70711b5983" } });



            Database.Table batchStatusTable = database.AddTable("BatchStatus");

            batchStatusTable.comment = @"====================================================================================================
   BATCH CONTROL
   This prevents data entry errors by forcing the user to balance ""Control Totals"" vs ""Actual Totals"".
   ====================================================================================================";
            batchStatusTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, FOUNDATION_ADMIN_SECURITY_LEVEL);
            batchStatusTable.AddIdField();
            batchStatusTable.AddNameAndDescriptionFields(true, true, false);
            batchStatusTable.AddSequenceField();
            batchStatusTable.AddControlFields();

            batchStatusTable.AddData(new Dictionary<string, string> {
                { "name", "Open" },
                { "description", "Data entry in progress" },
                { "sequence", "1" },
                { "objectGuid", "d87c06b0-9b5e-4597-8968-ad5f987e2afd" } });

            batchStatusTable.AddData(new Dictionary<string, string> {
                { "name", "Balanced" },
                { "description", "Control totals match entry totals" },
                { "sequence", "2" },
                { "objectGuid", "b5942c13-47d1-4753-a655-140454e1d0a4" } });

            batchStatusTable.AddData(new Dictionary<string, string> {
                { "name", "Posted" },
                { "description", "Transactions committed to GL / Donor History" },
                { "sequence", "3" },
                { "objectGuid", "640a7bb7-59da-423b-b2e5-a10124594331" } });


            batchStatusTable.AddData(new Dictionary<string, string> {
                { "name", "Closed" },
                { "description", "Closed" },
                { "sequence", "4" },
                { "objectGuid", "5c60e28a-ba9f-4098-9a04-50fcb139bd8c" } });



            Database.Table batchTable = database.AddTable("Batch");

            batchTable.comment = "The Batch Header for processing gifts.";

            batchTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            batchTable.AddIdField();
            batchTable.AddMultiTenantSupport();
            batchTable.AddString100Field("batchNumber", false).AddScriptComments("User-facing ID (e.g., \"2026-01-15-MAIL\"");
            batchTable.AddString500Field("description", true);
            batchTable.AddDateTimeField("dateOpened", false);
            batchTable.AddDateTimeField("datePosted", true);

            batchTable.AddForeignKeyField(batchStatusTable, false, true);

            //
            // Control Totals (What the user *expects* to enter from their stack of checks)
            //
            batchTable.AddMoneyField("controlAmount", false, 0, true);
            batchTable.AddIntField("controlCount", false, 0);

            //
            // Defaults (To speed up data entry for items in this batch)
            //
            batchTable.AddForeignKeyField("defaultFundId", fundTable, true, true).AddScriptComments("Optional default fund");
            batchTable.AddForeignKeyField("defaultCampaignId", campaignTable, true, true).AddScriptComments("Optional default campaign");
            batchTable.AddForeignKeyField("defaultAppealId", appealTable, true, true).AddScriptComments("Optional default appeal");
            batchTable.AddDateField("defaultDate", true, true).AddScriptComments("Optional default date");

            batchTable.AddVersionControl();
            batchTable.AddControlFields();




            Database.Table tributeTable = database.AddTable("Tribute");
            tributeTable.maxPostBytes = 5_000_000;          // Cap posts at 5 mb

            tributeTable.comment = "The Tribute Definition (e.g., \"The John Doe Memorial Fund\")";
            tributeTable.AddIdField();
            tributeTable.AddMultiTenantSupport();

            tributeTable.AddNameAndDescriptionFields(true, true, true);
            tributeTable.AddForeignKeyField(tributeTypeTable, true, true);

            tributeTable.AddForeignKeyField("defaultAcknowledgeeId", constituentTable, true, false).AddScriptComments("Constituent to notify (e.g., the widow)");

            tributeTable.AddDateField("startDate", true, true);
            tributeTable.AddDateField("endDate", true, true);

            tributeTable.AddForeignKeyField(iconTable, true).AddScriptComments("Icon to use for UI display");
            tributeTable.AddHTMLColorField("color", true).AddScriptComments("Hex color for UI display");
            tributeTable.AddBinaryDataFields("avatar");            // avatar details



            tributeTable.AddVersionControl();
            tributeTable.AddControlFields();

            Database.Table giftTable = database.AddTable("Gift");

            giftTable.SetTableToBeReadonlyForControllerCreationPurposes();

            giftTable.comment = @"  ====================================================================================================
   GIFTS (Transactions)
   The money coming in.
   ====================================================================================================";

            giftTable.SetMinimumPermissionLevels(CLIENT_REGULAR_USER_SECURITY_LEVEL, CLIENT_REGULAR_USER_SECURITY_LEVEL);
            giftTable.AddIdField();
            giftTable.AddMultiTenantSupport();
            giftTable.AddForeignKeyField(officeTable, true).AddScriptComments("Which office received/owns this gift");
            giftTable.AddForeignKeyField(constituentTable, false);
            giftTable.AddForeignKeyField(pledgeTable, true);

            giftTable.AddMoneyField("amount", false, true);
            giftTable.AddDateTimeField("receivedDate", false).AddScriptComments("When it was recieved");
            giftTable.AddDateTimeField("postedDate", true).AddScriptComments("When it hit the GL");


            //
            // Coding
            //
            giftTable.AddForeignKeyField(fundTable, false);
            giftTable.AddForeignKeyField(campaignTable, true);
            giftTable.AddForeignKeyField(appealTable, true);

            //
            // payment details
            //
            giftTable.AddForeignKeyField(paymentTypeTable, false, true);
            giftTable.AddString100Field("referenceNumber", true).AddScriptComments("Check # or Transaction ID");
            giftTable.AddForeignKeyField(batchTable, true, true).AddScriptComments("Link to processing batch");


            //
            // Receipting
            //
            giftTable.AddForeignKeyField(receiptTypeTable, true, true);
            giftTable.AddDateTimeField("receiptDate", true);


            //
            // Tribute(In Memory Of / In Honor Of)
            //
            giftTable.AddForeignKeyField(tributeTable, true, true);

            giftTable.AddTextField("notes", true);
            giftTable.AddVersionControl();
            giftTable.AddControlFields();


            Database.Table softCreditTable = database.AddTable("SoftCredit");

            softCreditTable.comment = @"====================================================================================================
   SOFT CREDITS
   Critical for DP functionality. Allows a gift from ""Husband"" to also show up on ""Wife's"" record 
   without doubling the financial totals.
   ====================================================================================================";
            softCreditTable.AddIdField();
            softCreditTable.AddMultiTenantSupport();
            softCreditTable.AddForeignKeyField(giftTable, false, true);
            softCreditTable.AddForeignKeyField(constituentTable, false, true).AddScriptComments("The person getting the soft credit");
            softCreditTable.AddMoneyField("amount", false, true).AddScriptComments("Might be full amount or partial");
            softCreditTable.AddTextField("notes", true);
            softCreditTable.AddVersionControl();
            softCreditTable.AddControlFields();


            #endregion
        }
    }
}