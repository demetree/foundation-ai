using Foundation.CodeGeneration;
using System;
using System.Collections.Generic;
using static Foundation.CodeGeneration.DatabaseGenerator;


namespace Foundation.BMC.Database
{
    /// <summary>
    /// Complete database schema generator for the BMC (Brick Machine Construction) system.
    /// 
    /// BMC is a multi-tenant, interactive LEGO and LEGO Technic building and simulation platform.
    /// 
    /// Key capabilities:
    /// - Database of all available brick parts with connection point metadata
    /// - 3D model building with connector-based snapping
    /// - Physics simulation of mechanical systems (gears, axles, motors)
    /// - Colour management with LDraw colour standard compatibility
    /// - Kit and instruction authoring
    /// 
    /// The schema is deliberately kept separate from other systems to allow
    /// independent evolution and integration via background processes.
    /// </summary>
    public class BmcDatabaseGenerator : DatabaseGenerator
    {
        // ── Permission levels ──

        // Read level for all tenant-visible tables
        private const int BMC_READER_PERMISSION_LEVEL = 1;

        // Write levels – tiered by functional area
        private const int BMC_BUILDER_WRITER_PERMISSION_LEVEL = 1;            // Day-to-day building (projects, placed bricks)
        private const int BMC_COLLECTION_WRITER_PERMISSION_LEVEL = 10;        // User's personal collection / inventory
        private const int BMC_INSTRUCTION_WRITER_PERMISSION_LEVEL = 20;       // Instructions, manuals, pages
        private const int BMC_CATALOG_WRITER_PERMISSION_LEVEL = 50;           // Admin: parts catalog, colours, connectors
        private const int BMC_COMMUNITY_WRITER_PERMISSION_LEVEL = 1;          // Social interactions (follows, likes, comments, MOC publishing)
        private const int BMC_MODERATOR_PERMISSION_LEVEL = 100;               // Content moderation and community management
        private const int BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL = 255;      // Foundation / system admin only

        // ── Custom role names ──
        private const string BMC_CATALOG_WRITER_CUSTOM_ROLE_NAME = "BMC Catalog Writer";
        private const string BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME = "BMC Collection Writer";
        private const string BMC_INSTRUCTION_WRITER_CUSTOM_ROLE_NAME = "BMC Instruction Writer";
        private const string BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME = "BMC Community Writer";
        private const string BMC_MODERATOR_CUSTOM_ROLE_NAME = "BMC Moderator";


        public BmcDatabaseGenerator() : base("BMC", "BMC")
        {
            database.comment = @"BMC (Brick Machine Construction) database schema.
This is a multi-tenant interactive building and physics simulation platform for LEGO and LEGO Technic models.
It supports a parts catalog with connector metadata, 3D model building with placement and connection tracking,
and physics simulation of mechanical assemblies.
All operational tables include multi-tenant support, versioning where appropriate, auditing, and security controls.";

            this.database.SetSchemaName("BMC");

            //
            // Register custom roles for granular write access control
            //
            this.database.AddCustomRole(BMC_CATALOG_WRITER_CUSTOM_ROLE_NAME, $"{BMC_CATALOG_WRITER_CUSTOM_ROLE_NAME} Role");
            this.database.AddCustomRole(BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME, $"{BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME} Role");
            this.database.AddCustomRole(BMC_INSTRUCTION_WRITER_CUSTOM_ROLE_NAME, $"{BMC_INSTRUCTION_WRITER_CUSTOM_ROLE_NAME} Role");
            this.database.AddCustomRole(BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME, $"{BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME} Role");
            this.database.AddCustomRole(BMC_MODERATOR_CUSTOM_ROLE_NAME, $"{BMC_MODERATOR_CUSTOM_ROLE_NAME} Role");


            #region Master Data - Part Categories, Connector Types, Colours

            // -------------------------------------------------
            // BrickCategory — Master list of part families
            // -------------------------------------------------
            Database.Table brickCategoryTable = database.AddTable("BrickCategory");
            brickCategoryTable.comment = "Master list of part categories (Beam, Plate, Gear, Axle, Motor, Pneumatic, etc.)";
            brickCategoryTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            brickCategoryTable.AddIdField();
            brickCategoryTable.AddNameAndDescriptionFields(true, true, false);

            brickCategoryTable.AddIntField("rebrickablePartCategoryId", true).AddScriptComments("Rebrickable part_cat_id for cross-referencing during bulk import");

            brickCategoryTable.AddSequenceField();
            brickCategoryTable.AddControlFields();

            // Seed data - core part categories (aligned with LDraw AVATAR categories + Technic specializations)
            // Standard System categories
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Plate" }, { "description", "Standard plates of various sizes" }, { "sequence", "1" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000001" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Brick" }, { "description", "Standard bricks of various sizes" }, { "sequence", "2" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000002" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Tile" }, { "description", "Smooth-top tiles without studs" }, { "sequence", "3" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000003" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Slope" }, { "description", "Angled slope bricks and roof pieces" }, { "sequence", "4" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000004" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Wedge" }, { "description", "Wedge-shaped plates and bricks" }, { "sequence", "5" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000005" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Arch" }, { "description", "Arched bricks and curved elements" }, { "sequence", "6" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000006" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Cylinder" }, { "description", "Round bricks, cylinders, and cones" }, { "sequence", "7" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000007" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Cone" }, { "description", "Cone-shaped parts" }, { "sequence", "8" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000008" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Bracket" }, { "description", "Angle brackets for sideways building" }, { "sequence", "9" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000009" } });

            // Technic categories
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Beam" }, { "description", "Technic beams and liftarms" }, { "sequence", "10" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000010" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Pin" }, { "description", "Technic pins and connectors" }, { "sequence", "11" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000011" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Axle" }, { "description", "Technic axles of various lengths" }, { "sequence", "12" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000012" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Gear" }, { "description", "Technic gears of various tooth counts" }, { "sequence", "13" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000013" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Motor" }, { "description", "Powered motors (Power Functions, Powered Up)" }, { "sequence", "14" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000014" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Pneumatic" }, { "description", "Pneumatic cylinders, pumps, and tubing" }, { "sequence", "15" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000015" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Differential" }, { "description", "Differential gear assemblies" }, { "sequence", "16" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000016" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Hinge" }, { "description", "Hinge bricks, plates, and click hinges" }, { "sequence", "17" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000017" } });

            // Structural and building categories
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Panel" }, { "description", "Panels, fairings, and body pieces" }, { "sequence", "20" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000020" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Wheel" }, { "description", "Wheels, tyres, and rims" }, { "sequence", "21" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000021" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Window" }, { "description", "Windows, glass, and frames" }, { "sequence", "22" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000022" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Door" }, { "description", "Doors and door frames" }, { "sequence", "23" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000023" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Fence" }, { "description", "Fences, railings, and barriers" }, { "sequence", "24" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000024" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Baseplate" }, { "description", "Baseplates and road plates" }, { "sequence", "25" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000025" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Bar" }, { "description", "Bars, antennas, and clips" }, { "sequence", "26" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000026" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Support" }, { "description", "Support structures, columns, and pillars" }, { "sequence", "27" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000027" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Container" }, { "description", "Boxes, crates, and storage containers" }, { "sequence", "28" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000028" } });

            // Specialty categories
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Decorative" }, { "description", "Decorative, printed, and sticker parts" }, { "sequence", "30" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000030" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Electric" }, { "description", "Electrical components, lights, and sensors" }, { "sequence", "31" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000031" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Propeller" }, { "description", "Propellers, rotors, and blades" }, { "sequence", "32" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000032" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Wing" }, { "description", "Wings and aircraft body parts" }, { "sequence", "33" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000033" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Train" }, { "description", "Train track, wheels, and specialized train parts" }, { "sequence", "34" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000034" } });


            // -------------------------------------------------
            // ConnectorType — Types of connection points on parts
            // -------------------------------------------------
            Database.Table connectorTypeTable = database.AddTable("ConnectorType");
            connectorTypeTable.comment = "Master list of physical connection types that define how parts can join together.";
            connectorTypeTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            connectorTypeTable.AddIdField();
            connectorTypeTable.AddNameAndDescriptionFields(true, true, false);

            // Connector physics properties
            connectorTypeTable.AddIntField("degreesOfFreedom").AddScriptComments("Number of degrees of freedom when connected (0=fixed, 1=rotation, 2=rotation+slide)");
            connectorTypeTable.AddBoolField("allowsRotation", false, false).AddScriptComments("Whether this connection allows rotation around its axis");
            connectorTypeTable.AddBoolField("allowsSlide", false, false).AddScriptComments("Whether this connection allows sliding along its axis");

            // Constraint parameters for physics simulation
            connectorTypeTable.AddSingleField("minAngleDegrees", true).AddScriptComments("Minimum angle of rotation for hinge-type connectors (null = no minimum)");
            connectorTypeTable.AddSingleField("maxAngleDegrees", true).AddScriptComments("Maximum angle of rotation for hinge-type connectors (null = no maximum)");
            connectorTypeTable.AddSingleField("snapIncrementDegrees", true).AddScriptComments("Snap increment for click hinges (e.g. 22.5 degrees, null = continuous)");
            connectorTypeTable.AddSingleField("clutchForceNewtons", true).AddScriptComments("Force needed to disconnect this connector type (null = unknown)");
            connectorTypeTable.AddString10Field("maleOrFemale", true).AddScriptComments("Pairing role: Male, Female, or Both (for compatibility matching logic)");

            connectorTypeTable.AddSequenceField();
            connectorTypeTable.AddControlFields();

            // Connector type compatibility — which types can connect to each other
            connectorTypeTable.AddData(new Dictionary<string, string> { { "name", "Stud" }, { "description", "Standard LEGO stud (male connector)" }, { "degreesOfFreedom", "0" }, { "allowsRotation", "false" }, { "allowsSlide", "false" }, { "sequence", "1" }, { "objectGuid", "c0110001-0001-4000-8000-000000000001" } });
            connectorTypeTable.AddData(new Dictionary<string, string> { { "name", "AntiStud" }, { "description", "Standard LEGO anti-stud receptacle (female connector)" }, { "degreesOfFreedom", "0" }, { "allowsRotation", "false" }, { "allowsSlide", "false" }, { "sequence", "2" }, { "objectGuid", "c0110001-0001-4000-8000-000000000002" } });
            connectorTypeTable.AddData(new Dictionary<string, string> { { "name", "PinHole" }, { "description", "Technic pin hole — accepts a pin for rotational connection" }, { "degreesOfFreedom", "1" }, { "allowsRotation", "true" }, { "allowsSlide", "false" }, { "sequence", "10" }, { "objectGuid", "c0110001-0001-4000-8000-000000000010" } });
            connectorTypeTable.AddData(new Dictionary<string, string> { { "name", "Pin" }, { "description", "Technic pin — inserts into a pin hole" }, { "degreesOfFreedom", "1" }, { "allowsRotation", "true" }, { "allowsSlide", "false" }, { "sequence", "11" }, { "objectGuid", "c0110001-0001-4000-8000-000000000011" } });
            connectorTypeTable.AddData(new Dictionary<string, string> { { "name", "AxleHole" }, { "description", "Technic axle hole — accepts an axle for locked rotational transfer" }, { "degreesOfFreedom", "1" }, { "allowsRotation", "true" }, { "allowsSlide", "false" }, { "sequence", "12" }, { "objectGuid", "c0110001-0001-4000-8000-000000000012" } });
            connectorTypeTable.AddData(new Dictionary<string, string> { { "name", "AxleEnd" }, { "description", "End of a Technic axle — inserts into an axle hole" }, { "degreesOfFreedom", "1" }, { "allowsRotation", "true" }, { "allowsSlide", "false" }, { "sequence", "13" }, { "objectGuid", "c0110001-0001-4000-8000-000000000013" } });
            connectorTypeTable.AddData(new Dictionary<string, string> { { "name", "BallJointSocket" }, { "description", "Ball joint socket — accepts a ball joint for multi-axis rotation" }, { "degreesOfFreedom", "2" }, { "allowsRotation", "true" }, { "allowsSlide", "false" }, { "sequence", "20" }, { "objectGuid", "c0110001-0001-4000-8000-000000000020" } });
            connectorTypeTable.AddData(new Dictionary<string, string> { { "name", "BallJoint" }, { "description", "Ball joint — inserts into a ball joint socket" }, { "degreesOfFreedom", "2" }, { "allowsRotation", "true" }, { "allowsSlide", "false" }, { "sequence", "21" }, { "objectGuid", "c0110001-0001-4000-8000-000000000021" } });


            // -------------------------------------------------
            // ConnectorTypeCompatibility — Which connector types can mate
            // -------------------------------------------------
            Database.Table connectorTypeCompatibilityTable = database.AddTable("ConnectorTypeCompatibility");
            connectorTypeCompatibilityTable.comment = "Defines which connector types can physically connect to each other (e.g. Stud↔AntiStud, Pin↔PinHole).";
            connectorTypeCompatibilityTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            connectorTypeCompatibilityTable.AddIdField();

            connectorTypeCompatibilityTable.AddForeignKeyField("maleConnectorTypeId", connectorTypeTable, false).AddScriptComments("The male/inserting connector type");
            connectorTypeCompatibilityTable.AddForeignKeyField("femaleConnectorTypeId", connectorTypeTable, false).AddScriptComments("The female/receiving connector type");
            connectorTypeCompatibilityTable.AddString50Field("connectionStrength", false).AddScriptComments("Connection strength: Tight, Friction, Free");

            connectorTypeCompatibilityTable.AddControlFields();

            connectorTypeCompatibilityTable.AddUniqueConstraint(new List<string>() { "maleConnectorTypeId", "femaleConnectorTypeId" }, false);

            // Seed data — core compatibility pairs
            connectorTypeCompatibilityTable.AddData(new Dictionary<string, string> { { "connectionStrength", "Tight" }, { "link:ConnectorType:name:maleConnectorTypeId", "Stud" }, { "link:ConnectorType:name:femaleConnectorTypeId", "AntiStud" }, { "objectGuid", "cc100001-0001-4000-8000-000000000001" } });
            connectorTypeCompatibilityTable.AddData(new Dictionary<string, string> { { "connectionStrength", "Friction" }, { "link:ConnectorType:name:maleConnectorTypeId", "Pin" }, { "link:ConnectorType:name:femaleConnectorTypeId", "PinHole" }, { "objectGuid", "cc100001-0001-4000-8000-000000000002" } });
            connectorTypeCompatibilityTable.AddData(new Dictionary<string, string> { { "connectionStrength", "Tight" }, { "link:ConnectorType:name:maleConnectorTypeId", "AxleEnd" }, { "link:ConnectorType:name:femaleConnectorTypeId", "AxleHole" }, { "objectGuid", "cc100001-0001-4000-8000-000000000003" } });
            connectorTypeCompatibilityTable.AddData(new Dictionary<string, string> { { "connectionStrength", "Friction" }, { "link:ConnectorType:name:maleConnectorTypeId", "BallJoint" }, { "link:ConnectorType:name:femaleConnectorTypeId", "BallJointSocket" }, { "objectGuid", "cc100001-0001-4000-8000-000000000004" } });


            // -------------------------------------------------
            // ColourFinish — Material finish types for colour rendering
            // -------------------------------------------------
            Database.Table colourFinishTable = database.AddTable("ColourFinish");
            colourFinishTable.comment = "Lookup table of material finish types that define how a colour is rendered (e.g. Solid, Chrome, Rubber).";
            colourFinishTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            colourFinishTable.AddIdField();
            colourFinishTable.AddNameAndDescriptionFields(true, true, false);

            colourFinishTable.AddBoolField("requiresEnvironmentMap", false, false).AddScriptComments("Whether this finish needs environment mapping for reflections (Chrome, Metal)");
            colourFinishTable.AddBoolField("isMatte", false, false).AddScriptComments("Whether this finish has a matte/non-glossy appearance (Rubber)");
            colourFinishTable.AddIntField("defaultAlpha", true).AddScriptComments("Default alpha for this finish type, null = use colour-specific alpha");

            colourFinishTable.AddSequenceField();
            colourFinishTable.AddControlFields();

            // Seed data — the 9 LDraw finish categories
            colourFinishTable.AddData(new Dictionary<string, string> { { "name", "Solid" }, { "description", "Standard opaque plastic finish" }, { "requiresEnvironmentMap", "false" }, { "isMatte", "false" }, { "sequence", "1" }, { "objectGuid", "cf100001-0001-4000-8000-000000000001" } });
            colourFinishTable.AddData(new Dictionary<string, string> { { "name", "Transparent" }, { "description", "See-through plastic finish" }, { "requiresEnvironmentMap", "false" }, { "isMatte", "false" }, { "defaultAlpha", "128" }, { "sequence", "2" }, { "objectGuid", "cf100001-0001-4000-8000-000000000002" } });
            colourFinishTable.AddData(new Dictionary<string, string> { { "name", "Chrome" }, { "description", "Highly reflective chrome-plated metal finish" }, { "requiresEnvironmentMap", "true" }, { "isMatte", "false" }, { "sequence", "3" }, { "objectGuid", "cf100001-0001-4000-8000-000000000003" } });
            colourFinishTable.AddData(new Dictionary<string, string> { { "name", "Pearlescent" }, { "description", "Iridescent pearl-like plastic finish" }, { "requiresEnvironmentMap", "true" }, { "isMatte", "false" }, { "sequence", "4" }, { "objectGuid", "cf100001-0001-4000-8000-000000000004" } });
            colourFinishTable.AddData(new Dictionary<string, string> { { "name", "Metal" }, { "description", "Metallic paint or lacquer finish" }, { "requiresEnvironmentMap", "true" }, { "isMatte", "false" }, { "sequence", "5" }, { "objectGuid", "cf100001-0001-4000-8000-000000000005" } });
            colourFinishTable.AddData(new Dictionary<string, string> { { "name", "Rubber" }, { "description", "Matte rubber or soft-touch finish" }, { "requiresEnvironmentMap", "false" }, { "isMatte", "true" }, { "sequence", "6" }, { "objectGuid", "cf100001-0001-4000-8000-000000000006" } });
            colourFinishTable.AddData(new Dictionary<string, string> { { "name", "Glitter" }, { "description", "Transparent plastic with embedded glitter particles" }, { "requiresEnvironmentMap", "false" }, { "isMatte", "false" }, { "defaultAlpha", "128" }, { "sequence", "7" }, { "objectGuid", "cf100001-0001-4000-8000-000000000007" } });
            colourFinishTable.AddData(new Dictionary<string, string> { { "name", "Speckle" }, { "description", "Solid plastic with embedded speckle particles" }, { "requiresEnvironmentMap", "false" }, { "isMatte", "false" }, { "sequence", "8" }, { "objectGuid", "cf100001-0001-4000-8000-000000000008" } });
            colourFinishTable.AddData(new Dictionary<string, string> { { "name", "Milky" }, { "description", "Semi-translucent milky or glow-in-the-dark finish" }, { "requiresEnvironmentMap", "false" }, { "isMatte", "false" }, { "defaultAlpha", "240" }, { "sequence", "9" }, { "objectGuid", "cf100001-0001-4000-8000-000000000009" } });
            colourFinishTable.AddData(new Dictionary<string, string> { { "name", "Fabric" }, { "description", "Fabric or cloth material finish for flags, capes, and similar elements" }, { "requiresEnvironmentMap", "false" }, { "isMatte", "true" }, { "sequence", "10" }, { "objectGuid", "cf100001-0001-4000-8000-000000000010" } });


            // -------------------------------------------------
            // BrickColour — Colour definitions compatible with LDraw standard
            // -------------------------------------------------
            Database.Table brickColourTable = database.AddTable("BrickColour");
            brickColourTable.comment = "Colour definitions for brick parts. Compatible with the LDraw colour standard.";
            brickColourTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            brickColourTable.AddIdField();
            brickColourTable.AddNameField(true, true);

            // Rebrickable-primary identity — rebrickableColorId is the primary lookup key
            brickColourTable.AddIntField("rebrickableColorId", false).AddScriptComments("Rebrickable color ID — primary lookup key for API sync");
            brickColourTable.AddIntField("ldrawColourCode", true).AddScriptComments("LDraw standard colour code number (nullable — some Rebrickable colors may not map to LDraw)");
            brickColourTable.AddIntField("bricklinkColorId", true).AddScriptComments("BrickLink color ID for cross-referencing");
            brickColourTable.AddIntField("brickowlColorId", true).AddScriptComments("BrickOwl color ID for cross-referencing");

            brickColourTable.AddHTMLColorField("hexRgb").AddScriptComments("Hex RGB colour value (e.g. #FF0000)");
            brickColourTable.AddHTMLColorField("hexEdgeColour").AddScriptComments("LDraw edge/contrast colour hex value for wireframe and outline rendering");
            brickColourTable.AddIntField("alpha").AddScriptComments("Alpha transparency value (0-255, 255 = fully opaque)");
            brickColourTable.AddBoolField("isTransparent", false, false).AddScriptComments("Whether this colour is transparent (convenience flag derived from alpha)");
            brickColourTable.AddBoolField("isMetallic", false, false).AddScriptComments("Whether this colour has a metallic finish (convenience flag)");
            brickColourTable.AddForeignKeyField(colourFinishTable, false).AddScriptComments("Material finish type — FK to ColourFinish lookup table");
            brickColourTable.AddIntField("luminance", true).AddScriptComments("Glow brightness (0-255) for glow-in-the-dark colours. Null for non-glowing.");
            brickColourTable.AddIntField("legoColourId", true).AddScriptComments("Official LEGO colour number for cross-referencing with LEGO catalogues");

            brickColourTable.AddSequenceField();
            brickColourTable.AddControlFields();

            brickColourTable.AddUniqueConstraint(new List<string>() { "rebrickableColorId" }, false);

            //
            // No need to do this.  Data sync will take care of it.
            //
            //// Seed common solid colours (edge colours, finish FK, and LEGO IDs from LDConfig.ldr)
            //brickColourTable.AddData(new Dictionary<string, string> { { "name", "Black" }, { "ldrawColourCode", "0" }, { "hexRgb", "#1B2A34" }, { "hexEdgeColour", "#808080" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "26" }, { "sequence", "1" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000001" } });
            //brickColourTable.AddData(new Dictionary<string, string> { { "name", "Blue" }, { "ldrawColourCode", "1" }, { "hexRgb", "#1E5AA8" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "23" }, { "sequence", "2" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000002" } });
            //brickColourTable.AddData(new Dictionary<string, string> { { "name", "Green" }, { "ldrawColourCode", "2" }, { "hexRgb", "#00852B" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "28" }, { "sequence", "3" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000003" } });
            //brickColourTable.AddData(new Dictionary<string, string> { { "name", "Red" }, { "ldrawColourCode", "4" }, { "hexRgb", "#B40000" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "21" }, { "sequence", "4" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000004" } });
            //brickColourTable.AddData(new Dictionary<string, string> { { "name", "Yellow" }, { "ldrawColourCode", "14" }, { "hexRgb", "#FAC80A" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "24" }, { "sequence", "5" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000005" } });
            //brickColourTable.AddData(new Dictionary<string, string> { { "name", "White" }, { "ldrawColourCode", "15" }, { "hexRgb", "#F4F4F4" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "1" }, { "sequence", "6" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000006" } });
            //brickColourTable.AddData(new Dictionary<string, string> { { "name", "Light Bluish Grey" }, { "ldrawColourCode", "71" }, { "hexRgb", "#969696" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "194" }, { "sequence", "7" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000007" } });
            //brickColourTable.AddData(new Dictionary<string, string> { { "name", "Dark Bluish Grey" }, { "ldrawColourCode", "72" }, { "hexRgb", "#646464" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "199" }, { "sequence", "8" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000008" } });

            #endregion


            #region Parts Catalog

            // -------------------------------------------------
            // PartType — LDraw part classification types
            // -------------------------------------------------
            Database.Table partTypeTable = database.AddTable("PartType");
            partTypeTable.comment = "Lookup table of LDraw part classification types (Part, Subpart, Primitive, etc.).";
            partTypeTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            partTypeTable.AddIdField();
            partTypeTable.AddNameAndDescriptionFields(true, true, false);

            partTypeTable.AddBoolField("isUserVisible", false, true).AddScriptComments("Whether parts of this type should appear in the user-facing part picker");

            partTypeTable.AddSequenceField();
            partTypeTable.AddControlFields();

            // Seed data — LDraw part classifications
            partTypeTable.AddData(new Dictionary<string, string> { { "name", "Part" }, { "description", "A complete, standalone part (e.g. Brick 2x4)" }, { "isUserVisible", "true" }, { "sequence", "1" }, { "objectGuid", "df6fb298-9f61-41ce-aad2-37c00bc14efd" } });
            partTypeTable.AddData(new Dictionary<string, string> { { "name", "Subpart" }, { "description", "A reusable component used internally by other parts" }, { "isUserVisible", "false" }, { "sequence", "2" }, { "objectGuid", "71ed658f-8695-44df-9448-669348bcfab4" } });
            partTypeTable.AddData(new Dictionary<string, string> { { "name", "Primitive" }, { "description", "A low-level geometric primitive (cylinder, stud shape)" }, { "isUserVisible", "false" }, { "sequence", "3" }, { "objectGuid", "cae03dfa-930b-47e3-acd0-83241eaae69d" } });
            partTypeTable.AddData(new Dictionary<string, string> { { "name", "Shortcut" }, { "description", "A convenience combination of multiple parts (e.g. hinge assembly)" }, { "isUserVisible", "true" }, { "sequence", "4" }, { "objectGuid", "a800b3c0-e7d1-46f3-830d-f2c93f7f8e4d" } });
            partTypeTable.AddData(new Dictionary<string, string> { { "name", "Alias" }, { "description", "An alternate ID that maps to another part" }, { "isUserVisible", "false" }, { "sequence", "5" }, { "objectGuid", "9c5c8f5c-6397-4233-b360-0292adc30304" } });


            // -------------------------------------------------
            // BrickPart — Individual part definitions
            // -------------------------------------------------
            Database.Table brickPartTable = database.AddTable("BrickPart");
            brickPartTable.comment = "Individual brick part definitions. Each row represents a unique part shape (independent of colour).";
            brickPartTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_CATALOG_WRITER_PERMISSION_LEVEL);
            brickPartTable.customWriteAccessRole = BMC_CATALOG_WRITER_CUSTOM_ROLE_NAME;
            brickPartTable.AddIdField();
            brickPartTable.AddNameField(true, true);

            // Rebrickable-primary identity — rebrickablePartNum is the primary lookup key
            brickPartTable.AddString100Field("rebrickablePartNum", false).AddScriptComments("Rebrickable part_num — primary lookup key for the parts catalog");
            brickPartTable.AddString250Field("rebrickablePartUrl", true).AddScriptComments("URL to part page on Rebrickable");
            brickPartTable.AddString250Field("rebrickableImgUrl", true).AddScriptComments("URL to part image on Rebrickable");

            // Cross-reference IDs to other ecosystems
            brickPartTable.AddString100Field("ldrawPartId", true).AddScriptComments("LDraw part ID (e.g. 3001, 32523) — nullable, some Rebrickable parts have no LDraw file");
            brickPartTable.AddString100Field("bricklinkId", true).AddScriptComments("BrickLink part ID for cross-referencing");
            brickPartTable.AddString100Field("brickowlId", true).AddScriptComments("BrickOwl part ID for cross-referencing");
            brickPartTable.AddString100Field("legoDesignId", true).AddScriptComments("Official LEGO design number for cross-referencing");

            // LDraw metadata (populated when LDraw geometry is available)
            brickPartTable.AddString250Field("ldrawTitle").AddScriptComments("Raw title from the LDraw .dat file (e.g. 'Brick  2 x  4', 'Technic Beam  3')");
            brickPartTable.AddString100Field("ldrawCategory").AddScriptComments("Part category from LDraw !CATEGORY meta or inferred from title first word");
            brickPartTable.AddForeignKeyField(partTypeTable, false).AddScriptComments("LDraw part classification — FK to PartType lookup table");
            brickPartTable.AddTextField("keywords").AddScriptComments("Comma-separated keywords from LDraw !KEYWORDS meta lines for search");
            brickPartTable.AddString100Field("author").AddScriptComments("Part author from the LDraw Author: header line");

            brickPartTable.AddForeignKeyField(brickCategoryTable, false).AddScriptComments("The category this part belongs to");

            // Physical dimensions in LDraw units (1 LDU = 0.4mm)
            brickPartTable.AddSingleField("widthLdu", true).AddScriptComments("Part width in LDraw units (null if not yet computed)");
            brickPartTable.AddSingleField("heightLdu", true).AddScriptComments("Part height in LDraw units (null if not yet computed)");
            brickPartTable.AddSingleField("depthLdu", true).AddScriptComments("Part depth in LDraw units (null if not yet computed)");

            // Physics properties
            brickPartTable.AddSingleField("massGrams", true).AddScriptComments("Part mass in grams (for physics simulation, null if unknown)");
            brickPartTable.AddSingleField("momentOfInertiaX", true).AddScriptComments("Rotational inertia about X axis (kg·m², null if unknown)");
            brickPartTable.AddSingleField("momentOfInertiaY", true).AddScriptComments("Rotational inertia about Y axis (kg·m², null if unknown)");
            brickPartTable.AddSingleField("momentOfInertiaZ", true).AddScriptComments("Rotational inertia about Z axis (kg·m², null if unknown)");
            brickPartTable.AddSingleField("frictionCoefficient", true).AddScriptComments("Surface friction coefficient for physics (null if unknown)");
            brickPartTable.AddString50Field("materialType", true).AddScriptComments("Material type: ABS, Rubber, Metal, Fabric, etc. (null if unknown)");
            brickPartTable.AddSingleField("centerOfMassX", true).AddScriptComments("Center of mass X offset from part origin in LDU (null if unknown)");
            brickPartTable.AddSingleField("centerOfMassY", true).AddScriptComments("Center of mass Y offset from part origin in LDU (null if unknown)");
            brickPartTable.AddSingleField("centerOfMassZ", true).AddScriptComments("Center of mass Z offset from part origin in LDU (null if unknown)");

            // Geometry — stored as binary blob for deployment simplicity
            brickPartTable.AddBinaryDataFields("geometry");  // geometryFileName, geometrySize, geometryData, geometryMimeType
            brickPartTable.AddString50Field("geometryFileFormat", true).AddScriptComments("Format of stored geometry: LDraw, ProcessedBinary, etc.");
            brickPartTable.AddString250Field("geometryOriginalFileName", true).AddScriptComments("Original LDraw filename for reference (e.g. '3001.dat')");

            // Bounding volume (pre-computed from geometry, for fast spatial queries)
            brickPartTable.AddSingleField("boundingBoxMinX", true).AddScriptComments("Axis-aligned bounding box minimum X in LDU");
            brickPartTable.AddSingleField("boundingBoxMinY", true).AddScriptComments("Axis-aligned bounding box minimum Y in LDU");
            brickPartTable.AddSingleField("boundingBoxMinZ", true).AddScriptComments("Axis-aligned bounding box minimum Z in LDU");
            brickPartTable.AddSingleField("boundingBoxMaxX", true).AddScriptComments("Axis-aligned bounding box maximum X in LDU");
            brickPartTable.AddSingleField("boundingBoxMaxY", true).AddScriptComments("Axis-aligned bounding box maximum Y in LDU");
            brickPartTable.AddSingleField("boundingBoxMaxZ", true).AddScriptComments("Axis-aligned bounding box maximum Z in LDU");

            // Geometry metadata
            brickPartTable.AddIntField("subFileCount", true).AddScriptComments("Number of LDraw sub-file references in this part (for instancing)");
            brickPartTable.AddIntField("polygonCount", true).AddScriptComments("Total polygon count for LOD decisions and performance budgets");

            // Technic-specific properties
            brickPartTable.AddIntField("toothCount", true).AddScriptComments("For gears: number of teeth. Null for non-gear parts.");
            brickPartTable.AddSingleField("gearRatio", true).AddScriptComments("For gears: effective gear ratio relative to a base gear. Null for non-gear parts.");

            // Sync metadata
            brickPartTable.AddDateTimeField("lastModifiedDate", true).AddScriptComments("Last modification date for incremental sync with Rebrickable");

            brickPartTable.AddVersionControl();
            brickPartTable.AddControlFields();

            brickPartTable.AddUniqueConstraint(new List<string>() { "rebrickablePartNum" }, false);


            // -------------------------------------------------
            // BrickPartConnector — Connection points on individual parts
            // -------------------------------------------------
            Database.Table brickPartConnectorTable = database.AddTable("BrickPartConnector");
            brickPartConnectorTable.comment = "Defines the physical connection points on each brick part, including position and connector type.";
            brickPartConnectorTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_CATALOG_WRITER_PERMISSION_LEVEL);
            brickPartConnectorTable.customWriteAccessRole = BMC_CATALOG_WRITER_CUSTOM_ROLE_NAME;
            brickPartConnectorTable.AddIdField();

            brickPartConnectorTable.AddForeignKeyField(brickPartTable, false).AddScriptComments("The part this connector belongs to");
            brickPartConnectorTable.AddForeignKeyField(connectorTypeTable, false).AddScriptComments("The type of connector (Stud, PinHole, AxleHole, etc.)");

            // Position relative to the part origin, in LDraw units
            brickPartConnectorTable.AddSingleField("positionX").AddScriptComments("X position of connector relative to part origin (LDU)");
            brickPartConnectorTable.AddSingleField("positionY").AddScriptComments("Y position of connector relative to part origin (LDU)");
            brickPartConnectorTable.AddSingleField("positionZ").AddScriptComments("Z position of connector relative to part origin (LDU)");

            // Orientation of the connector (unit vector indicating the connection axis direction)
            brickPartConnectorTable.AddSingleField("orientationX").AddScriptComments("X component of connector direction unit vector");
            brickPartConnectorTable.AddSingleField("orientationY").AddScriptComments("Y component of connector direction unit vector");
            brickPartConnectorTable.AddSingleField("orientationZ").AddScriptComments("Z component of connector direction unit vector");

            // Grouping and extraction metadata
            brickPartConnectorTable.AddIntField("connectorGroupId", true).AddScriptComments("Groups connectors that act together (e.g. all studs on top of a 2x4 share a group ID)");
            brickPartConnectorTable.AddBoolField("isAutoExtracted", false, false).AddScriptComments("Whether this connector position was auto-extracted from LDraw geometry analysis");

            brickPartConnectorTable.AddSequenceField();
            brickPartConnectorTable.AddControlFields();


            // -------------------------------------------------
            // BrickPartColour — Part / Colour availability mapping
            // -------------------------------------------------
            Database.Table brickPartColourTable = database.AddTable("BrickPartColour");
            brickPartColourTable.comment = "Maps which colours each brick part is available in. A part can exist in multiple colours.";
            brickPartColourTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_CATALOG_WRITER_PERMISSION_LEVEL);
            brickPartColourTable.customWriteAccessRole = BMC_CATALOG_WRITER_CUSTOM_ROLE_NAME;
            brickPartColourTable.AddIdField();

            brickPartColourTable.AddForeignKeyField(brickPartTable, false).AddScriptComments("The brick part");
            brickPartColourTable.AddForeignKeyField(brickColourTable, false).AddScriptComments("The colour this part is available in");

            brickPartColourTable.AddControlFields();

            brickPartColourTable.AddUniqueConstraint(new List<string>() { "brickPartId", "brickColourId" }, false);


            // -------------------------------------------------
            // PartSubFileReference — Hierarchical LDraw sub-file references
            // -------------------------------------------------
            Database.Table partSubFileReferenceTable = database.AddTable("PartSubFileReference");
            partSubFileReferenceTable.comment = "Models how LDraw parts reference sub-files hierarchically. Crucial for instanced rendering and understanding part decomposition.";
            partSubFileReferenceTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_CATALOG_WRITER_PERMISSION_LEVEL);
            partSubFileReferenceTable.customWriteAccessRole = BMC_CATALOG_WRITER_CUSTOM_ROLE_NAME;
            partSubFileReferenceTable.AddIdField();

            partSubFileReferenceTable.AddForeignKeyField("parentBrickPartId", brickPartTable, false).AddScriptComments("The part containing the sub-file reference");
            partSubFileReferenceTable.AddForeignKeyField("referencedBrickPartId", brickPartTable, true).AddScriptComments("The referenced sub-file as a BrickPart (null if sub-file is not cataloged)");
            partSubFileReferenceTable.AddString250Field("referencedFileName", false).AddScriptComments("Original LDraw sub-file filename (e.g. 'stud.dat', 's/3001s01.dat')");
            partSubFileReferenceTable.AddString500Field("transformMatrix", false).AddScriptComments("4x3 transform matrix as space-delimited floats (x y z a b c d e f g h i)");
            partSubFileReferenceTable.AddIntField("colorCode", true).AddScriptComments("LDraw color code override (16=inherit parent, null=inherit parent)");

            partSubFileReferenceTable.AddSequenceField();
            partSubFileReferenceTable.AddControlFields();

            #endregion


            #region Model Building

            // -------------------------------------------------
            // Project — A user's building project
            // -------------------------------------------------
            Database.Table projectTable = database.AddTable("Project");
            projectTable.comment = "A user's building project. Contains placed bricks and their connections to form a model.";
            projectTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            projectTable.AddIdField();
            projectTable.AddMultiTenantSupport();
            projectTable.AddNameAndDescriptionFields(true, true, false);

            projectTable.AddTextField("notes").AddScriptComments("Free-form notes about the project");
            projectTable.AddString250Field("thumbnailImagePath").AddScriptComments("Relative path to project thumbnail image for listings");
            projectTable.AddIntField("partCount", true).AddScriptComments("Cached total part count for quick display without querying PlacedBrick");
            projectTable.AddDateTimeField("lastBuildDate", true).AddScriptComments("When the user last modified the build (placed or moved a brick)");

            projectTable.AddVersionControl();
            projectTable.AddControlFields();


            // -------------------------------------------------
            // PlacedBrick — An instance of a part placed in a project
            // -------------------------------------------------
            Database.Table placedBrickTable = database.AddTable("PlacedBrick");
            placedBrickTable.comment = "An instance of a brick part placed within a project. Tracks position, rotation, and colour.";
            placedBrickTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            placedBrickTable.AddIdField();
            placedBrickTable.AddMultiTenantSupport();

            placedBrickTable.AddForeignKeyField(projectTable, false).AddScriptComments("The project this brick is placed in");
            placedBrickTable.AddForeignKeyField(brickPartTable, false).AddScriptComments("The part definition being placed");
            placedBrickTable.AddForeignKeyField(brickColourTable, false).AddScriptComments("The colour of this placed brick instance");

            // Position in world coordinates (LDraw units)
            placedBrickTable.AddSingleField("positionX").AddScriptComments("X position in world coordinates (LDU)");
            placedBrickTable.AddSingleField("positionY").AddScriptComments("Y position in world coordinates (LDU)");
            placedBrickTable.AddSingleField("positionZ").AddScriptComments("Z position in world coordinates (LDU)");

            // Rotation as quaternion (more stable than Euler angles for 3D)
            placedBrickTable.AddSingleField("rotationX").AddScriptComments("Quaternion X component");
            placedBrickTable.AddSingleField("rotationY").AddScriptComments("Quaternion Y component");
            placedBrickTable.AddSingleField("rotationZ").AddScriptComments("Quaternion Z component");
            placedBrickTable.AddSingleField("rotationW").AddScriptComments("Quaternion W component");

            // Build step for instruction ordering
            placedBrickTable.AddIntField("buildStepNumber", true).AddScriptComments("Optional build step number for instruction ordering");
            placedBrickTable.AddBoolField("isHidden", false, false).AddScriptComments("Whether this brick is temporarily hidden in the editor");

            placedBrickTable.AddVersionControl();
            placedBrickTable.AddControlFields();


            // -------------------------------------------------
            // BrickConnection — Connector-to-connector joins between placed bricks
            // -------------------------------------------------
            Database.Table brickConnectionTable = database.AddTable("BrickConnection");
            brickConnectionTable.comment = "Records which connector on one placed brick is joined to which connector on another placed brick.";
            brickConnectionTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            brickConnectionTable.AddIdField();
            brickConnectionTable.AddMultiTenantSupport();

            brickConnectionTable.AddForeignKeyField(projectTable, false).AddScriptComments("The project this connection belongs to");

            // Source side of the connection — proper FK relationships
            brickConnectionTable.AddForeignKeyField("sourcePlacedBrickId", placedBrickTable, false).AddScriptComments("FK to the source PlacedBrick");
            brickConnectionTable.AddForeignKeyField("sourceConnectorId", brickPartConnectorTable, false).AddScriptComments("FK to the BrickPartConnector on the source brick");

            // Target side of the connection — proper FK relationships
            brickConnectionTable.AddForeignKeyField("targetPlacedBrickId", placedBrickTable, false).AddScriptComments("FK to the target PlacedBrick");
            brickConnectionTable.AddForeignKeyField("targetConnectorId", brickPartConnectorTable, false).AddScriptComments("FK to the BrickPartConnector on the target brick");

            // Connection metadata for physics simulation
            brickConnectionTable.AddString50Field("connectionStrength", true).AddScriptComments("Connection strength: Snapped, Friction, Free (null = unknown)");
            brickConnectionTable.AddBoolField("isLocked", false, false).AddScriptComments("Whether the user has locked this connection from being broken in the editor");
            brickConnectionTable.AddSingleField("angleDegrees", true).AddScriptComments("Current angle for rotational connections (null = default/fixed)");

            brickConnectionTable.AddControlFields();


            // -------------------------------------------------
            // Submodel — A named sub-assembly within a project
            // -------------------------------------------------
            Database.Table submodelTable = database.AddTable("Submodel");
            submodelTable.comment = "A named sub-assembly within a project, similar to LDraw subfiles. Allows hierarchical builds.";
            submodelTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            submodelTable.AddIdField();
            submodelTable.AddMultiTenantSupport();

            submodelTable.AddForeignKeyField(projectTable, false).AddScriptComments("The project this submodel belongs to");
            submodelTable.AddNameAndDescriptionFields(true, true, false);
            submodelTable.AddForeignKeyField(submodelTable, true).AddScriptComments("Optional parent submodel for nested sub-assemblies (self-referencing FK, null = top-level)");

            submodelTable.AddSequenceField();
            submodelTable.AddVersionControl();
            submodelTable.AddControlFields();


            // -------------------------------------------------
            // SubmodelPlacedBrick — Links placed bricks to a submodel
            // -------------------------------------------------
            Database.Table submodelPlacedBrickTable = database.AddTable("SubmodelPlacedBrick");
            submodelPlacedBrickTable.comment = "Maps placed bricks to the submodel they belong to. A placed brick can only belong to one submodel.";
            submodelPlacedBrickTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            submodelPlacedBrickTable.AddIdField();
            submodelPlacedBrickTable.AddMultiTenantSupport();

            submodelPlacedBrickTable.AddForeignKeyField(submodelTable, false).AddScriptComments("The submodel this brick belongs to");
            submodelPlacedBrickTable.AddForeignKeyField(placedBrickTable, false).AddScriptComments("The placed brick assigned to this submodel");

            submodelPlacedBrickTable.AddControlFields();

            submodelPlacedBrickTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "placedBrickId" }, false);


            // -------------------------------------------------
            // ProjectTag — Tag lookup for project categorization
            // -------------------------------------------------
            Database.Table projectTagTable = database.AddTable("ProjectTag");
            projectTagTable.comment = "User-defined tags for categorizing and filtering projects (e.g. Technic, MOC, WIP).";
            projectTagTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            projectTagTable.AddIdField();
            projectTagTable.AddMultiTenantSupport();
            projectTagTable.AddNameAndDescriptionFields(true, true, false);

            projectTagTable.AddControlFields();


            // -------------------------------------------------
            // ProjectTagAssignment — Many-to-many: projects ↔ tags
            // -------------------------------------------------
            Database.Table projectTagAssignmentTable = database.AddTable("ProjectTagAssignment");
            projectTagAssignmentTable.comment = "Many-to-many mapping between projects and tags.";
            projectTagAssignmentTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            projectTagAssignmentTable.AddIdField();
            projectTagAssignmentTable.AddMultiTenantSupport();

            projectTagAssignmentTable.AddForeignKeyField(projectTable, false).AddScriptComments("The project being tagged");
            projectTagAssignmentTable.AddForeignKeyField(projectTagTable, false).AddScriptComments("The tag applied to the project");

            projectTagAssignmentTable.AddControlFields();

            projectTagAssignmentTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "projectId", "projectTagId" }, false);


            // -------------------------------------------------
            // ProjectCameraPreset — Saved camera positions for a project
            // -------------------------------------------------
            Database.Table projectCameraPresetTable = database.AddTable("ProjectCameraPreset");
            projectCameraPresetTable.comment = "Saved camera positions and orientations for quick viewport recall in the 3D editor.";
            projectCameraPresetTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            projectCameraPresetTable.AddIdField();
            projectCameraPresetTable.AddMultiTenantSupport();

            projectCameraPresetTable.AddForeignKeyField(projectTable, false).AddScriptComments("The project this camera preset belongs to");
            projectCameraPresetTable.AddNameField(true, true);

            // Camera position in world coordinates
            projectCameraPresetTable.AddSingleField("positionX").AddScriptComments("Camera X position in world coordinates (LDU)");
            projectCameraPresetTable.AddSingleField("positionY").AddScriptComments("Camera Y position in world coordinates (LDU)");
            projectCameraPresetTable.AddSingleField("positionZ").AddScriptComments("Camera Z position in world coordinates (LDU)");

            // Camera look-at target
            projectCameraPresetTable.AddSingleField("targetX").AddScriptComments("Camera target X position (look-at point)");
            projectCameraPresetTable.AddSingleField("targetY").AddScriptComments("Camera target Y position (look-at point)");
            projectCameraPresetTable.AddSingleField("targetZ").AddScriptComments("Camera target Z position (look-at point)");

            projectCameraPresetTable.AddSingleField("zoom").AddScriptComments("Camera zoom level / field of view");
            projectCameraPresetTable.AddBoolField("isPerspective", false, true).AddScriptComments("True for perspective projection, false for orthographic");

            projectCameraPresetTable.AddSequenceField();
            projectCameraPresetTable.AddControlFields();


            // -------------------------------------------------
            // ProjectReferenceImage — Overlay reference images for proportioning
            // -------------------------------------------------
            Database.Table projectReferenceImageTable = database.AddTable("ProjectReferenceImage");
            projectReferenceImageTable.comment = "Reference images overlaid in the 3D editor for proportioning and tracing.";
            projectReferenceImageTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            projectReferenceImageTable.AddIdField();
            projectReferenceImageTable.AddMultiTenantSupport();

            projectReferenceImageTable.AddForeignKeyField(projectTable, false).AddScriptComments("The project this reference image belongs to");
            projectReferenceImageTable.AddNameField(true, true);
            projectReferenceImageTable.AddString250Field("imageFilePath").AddScriptComments("Relative path to the uploaded reference image file");

            projectReferenceImageTable.AddSingleField("opacity").AddScriptComments("Opacity of the overlay (0.0 = invisible, 1.0 = fully opaque)");
            projectReferenceImageTable.AddSingleField("positionX").AddScriptComments("X position of the image plane in world coordinates (LDU)");
            projectReferenceImageTable.AddSingleField("positionY").AddScriptComments("Y position of the image plane in world coordinates (LDU)");
            projectReferenceImageTable.AddSingleField("positionZ").AddScriptComments("Z position of the image plane in world coordinates (LDU)");
            projectReferenceImageTable.AddSingleField("scaleX").AddScriptComments("Horizontal scale factor for the reference image");
            projectReferenceImageTable.AddSingleField("scaleY").AddScriptComments("Vertical scale factor for the reference image");
            projectReferenceImageTable.AddBoolField("isVisible", false, true).AddScriptComments("Whether the reference image is currently visible in the editor");

            projectReferenceImageTable.AddControlFields();

            #endregion


            #region Model Documents and Build Steps (MPD Support)

            // -------------------------------------------------
            // ModelDocument — Top-level container for multi-part model documents
            // -------------------------------------------------
            Database.Table modelDocumentTable = database.AddTable("ModelDocument");
            modelDocumentTable.comment = "Persistent representation of an imported or authored multi-part model document (MPD/LDR). Top-level container for complex models with build steps.";
            modelDocumentTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            modelDocumentTable.AddIdField();
            modelDocumentTable.AddMultiTenantSupport();

            modelDocumentTable.AddForeignKeyField(projectTable, true).AddScriptComments("Optional link to a BMC project if authored in BMC (null for imported documents)");
            modelDocumentTable.AddString250Field("name", false).AddScriptComments("Document name or title").CreateIndex();
            modelDocumentTable.AddTextField("description").AddScriptComments("Description of the model document");
            modelDocumentTable.AddString50Field("sourceFormat", false).AddScriptComments("Source file format: MPD, LDR, StudioIO, BMCNative");
            modelDocumentTable.AddString250Field("sourceFileName").AddScriptComments("Original filename if imported (e.g. 'crane_42131.mpd')");
            modelDocumentTable.AddBinaryDataFields("sourceFile");  // sourceFileFileName, sourceFileSize, sourceFileData, sourceFileMimeType
            modelDocumentTable.AddString100Field("author").AddScriptComments("Model author from the file header");
            modelDocumentTable.AddIntField("totalPartCount", true).AddScriptComments("Cached total part count across all sub-files");
            modelDocumentTable.AddIntField("totalStepCount", true).AddScriptComments("Cached total build step count across all sub-files");

            modelDocumentTable.AddVersionControl();
            modelDocumentTable.AddControlFields();


            // -------------------------------------------------
            // ModelSubFile — Individual sub-files within an MPD document
            // -------------------------------------------------
            Database.Table modelSubFileTable = database.AddTable("ModelSubFile");
            modelSubFileTable.comment = "Individual sub-files within a model document (MPD). Each represents a sub-assembly or the main model. Maps to LDraw '0 FILE' blocks.";
            modelSubFileTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            modelSubFileTable.AddIdField();
            modelSubFileTable.AddMultiTenantSupport();

            modelSubFileTable.AddForeignKeyField(modelDocumentTable, false).AddScriptComments("The document this sub-file belongs to");
            modelSubFileTable.AddString250Field("fileName", false).AddScriptComments("Sub-file name as declared in '0 FILE' (e.g. 'main.ldr', 'wheel_assembly.ldr')");
            modelSubFileTable.AddBoolField("isMainModel", false, false).AddScriptComments("Whether this is the main (first) model in the MPD — only rendered sub-files are those referenced by this");
            modelSubFileTable.AddForeignKeyField("parentModelSubFileId", modelSubFileTable, true).AddScriptComments("Optional parent sub-file for nested sub-assemblies (null = top-level)");

            modelSubFileTable.AddSequenceField();
            modelSubFileTable.AddControlFields();

            modelSubFileTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "modelDocumentId", "fileName" }, false);


            // -------------------------------------------------
            // ModelBuildStep — Individual build steps within a sub-file
            // -------------------------------------------------
            Database.Table modelBuildStepTable = database.AddTable("ModelBuildStep");
            modelBuildStepTable.comment = "Individual build steps within a model sub-file. Modeled from LDraw '0 STEP' and '0 ROTSTEP' meta-commands.";
            modelBuildStepTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            modelBuildStepTable.AddIdField();
            modelBuildStepTable.AddMultiTenantSupport();

            modelBuildStepTable.AddForeignKeyField(modelSubFileTable, false).AddScriptComments("The sub-file this step belongs to");
            modelBuildStepTable.AddIntField("stepNumber", false).AddScriptComments("Sequential step number within this sub-file");

            // ROTSTEP view rotation for instruction rendering
            modelBuildStepTable.AddString10Field("rotationType", true).AddScriptComments("ROTSTEP rotation type: REL (relative), ABS (absolute), ADD (additive). Null = no rotation.");
            modelBuildStepTable.AddSingleField("rotationX", true).AddScriptComments("ROTSTEP X rotation angle in degrees");
            modelBuildStepTable.AddSingleField("rotationY", true).AddScriptComments("ROTSTEP Y rotation angle in degrees");
            modelBuildStepTable.AddSingleField("rotationZ", true).AddScriptComments("ROTSTEP Z rotation angle in degrees");
            modelBuildStepTable.AddTextField("description").AddScriptComments("Optional step description or annotation");

            modelBuildStepTable.AddControlFields();


            // -------------------------------------------------
            // ModelStepPart — Parts placed during a specific build step
            // -------------------------------------------------
            Database.Table modelStepPartTable = database.AddTable("ModelStepPart");
            modelStepPartTable.comment = "Parts placed during a specific build step. Represents LDraw type 1 sub-file reference lines between STEP commands.";
            modelStepPartTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            modelStepPartTable.AddIdField();
            modelStepPartTable.AddMultiTenantSupport();

            modelStepPartTable.AddForeignKeyField(modelBuildStepTable, false).AddScriptComments("The build step this part placement belongs to");
            modelStepPartTable.AddForeignKeyField(brickPartTable, true).AddScriptComments("FK to BrickPart if this part is in the catalog (null if not yet cataloged)");
            modelStepPartTable.AddForeignKeyField(brickColourTable, true).AddScriptComments("FK to BrickColour if the color is mapped (null if unmapped)");
            modelStepPartTable.AddString250Field("partFileName", false).AddScriptComments("Original LDraw part filename from the type 1 line (e.g. '3001.dat')");
            modelStepPartTable.AddIntField("colorCode", false).AddScriptComments("LDraw color code used in the file");

            // Position from the LDraw type 1 line
            modelStepPartTable.AddSingleField("positionX").AddScriptComments("X position from the LDraw type 1 reference line (LDU)");
            modelStepPartTable.AddSingleField("positionY").AddScriptComments("Y position from the LDraw type 1 reference line (LDU)");
            modelStepPartTable.AddSingleField("positionZ").AddScriptComments("Z position from the LDraw type 1 reference line (LDU)");
            modelStepPartTable.AddString500Field("transformMatrix", false).AddScriptComments("3x3 rotation matrix as space-delimited floats (a b c d e f g h i)");

            modelStepPartTable.AddSequenceField();
            modelStepPartTable.AddControlFields();

            #endregion


            #region LEGO Set Reference Data

            // -------------------------------------------------
            // LegoTheme — Hierarchical theme tree
            // -------------------------------------------------
            Database.Table legoThemeTable = database.AddTable("LegoTheme");
            legoThemeTable.comment = "Hierarchical tree of official LEGO themes (e.g. City → Police, Technic → Bionicle). Bulk-loaded from Rebrickable or similar sources.";
            legoThemeTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            legoThemeTable.AddIdField();
            legoThemeTable.AddNameAndDescriptionFields(true, true, false);

            legoThemeTable.AddForeignKeyField(legoThemeTable, true).AddScriptComments("Parent theme for hierarchical nesting (self-referencing FK, null = top-level)");

            legoThemeTable.AddIntField("rebrickableThemeId", false).AddScriptComments("Rebrickable theme ID — source of truth for theme identity");

            legoThemeTable.AddSequenceField();
            legoThemeTable.AddControlFields();


            // -------------------------------------------------
            // LegoSet — Official LEGO set definitions
            // -------------------------------------------------
            Database.Table legoSetTable = database.AddTable("LegoSet");
            legoSetTable.comment = "Official LEGO set definitions. Each row represents a distinct set release (e.g. 42131-1 Liebherr Crawler Crane).";
            legoSetTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            legoSetTable.AddIdField();

            //legoSetTable.AddNameField(true, true);
            legoSetTable.AddString500Field("name", false).AddScriptComments("For really long set names").CreateIndex();

            legoSetTable.AddString100Field("setNumber", false).AddScriptComments("Official set number including variant suffix (e.g. '42131-1', '10302-1')");
            legoSetTable.AddIntField("year", false).AddScriptComments("Release year of the set");
            legoSetTable.AddIntField("partCount", false).AddScriptComments("Total number of parts in the set (as listed by LEGO)");
            legoSetTable.AddForeignKeyField(legoThemeTable, true).AddScriptComments("The theme this set belongs to (null if theme not yet categorized)");
            legoSetTable.AddString250Field("imageUrl").AddScriptComments("URL to the set's official box art or primary image");
            legoSetTable.AddString250Field("brickLinkUrl").AddScriptComments("URL to the set's BrickLink catalogue page");
            legoSetTable.AddString250Field("rebrickableUrl").AddScriptComments("URL to the set's Rebrickable page");
            legoSetTable.AddString100Field("rebrickableSetNum", true).AddScriptComments("Explicit Rebrickable set number if it differs from setNumber");
            legoSetTable.AddDateTimeField("lastModifiedDate", true).AddScriptComments("Last modification date for incremental sync with Rebrickable");

            legoSetTable.AddControlFields();

            legoSetTable.AddUniqueConstraint(new List<string>() { "setNumber" }, false);


            // -------------------------------------------------
            // LegoSetPart — Parts inventory for each set
            // -------------------------------------------------
            Database.Table legoSetPartTable = database.AddTable("LegoSetPart");
            legoSetPartTable.comment = "Parts inventory for each official LEGO set. Maps set → part → colour → quantity.";
            legoSetPartTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            legoSetPartTable.AddIdField();

            legoSetPartTable.AddForeignKeyField(legoSetTable, false).AddScriptComments("The set this inventory line belongs to");
            legoSetPartTable.AddForeignKeyField(brickPartTable, false).AddScriptComments("The part included in this set");
            legoSetPartTable.AddForeignKeyField(brickColourTable, false).AddScriptComments("The colour of this part in the set");

            legoSetPartTable.AddIntField("quantity").AddScriptComments("Number of this part+colour combination included in the set");
            legoSetPartTable.AddBoolField("isSpare", false, false).AddScriptComments("Whether this is a spare part (included as extra in the bag, not used in the build)");

            legoSetPartTable.AddControlFields();


            // -------------------------------------------------
            // BrickPartRelationship — Relationships between parts
            // -------------------------------------------------
            Database.Table brickPartRelationshipTable = database.AddTable("BrickPartRelationship");
            brickPartRelationshipTable.comment = "Relationships between parts: alternates, molds, prints, pairs, sub-parts, and patterns. Bulk-loaded from Rebrickable part_relationships.csv.";
            brickPartRelationshipTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            brickPartRelationshipTable.AddIdField();

            brickPartRelationshipTable.AddForeignKeyField("childBrickPartId", brickPartTable, false).AddScriptComments("The child part in the relationship");
            brickPartRelationshipTable.AddForeignKeyField("parentBrickPartId", brickPartTable, false).AddScriptComments("The parent part in the relationship");
            brickPartRelationshipTable.AddString50Field("relationshipType", false).AddScriptComments("Type of relationship: Print, Pair, SubPart, Mold, Pattern, or Alternate");

            brickPartRelationshipTable.AddControlFields();


            // -------------------------------------------------
            // BrickElement — LEGO element IDs (part + colour combinations)
            // -------------------------------------------------
            Database.Table brickElementTable = database.AddTable("BrickElement");
            brickElementTable.comment = "LEGO element IDs representing specific part+colour combinations. Used for cross-referencing with official LEGO catalogues and BrickLink.";
            brickElementTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            brickElementTable.AddIdField();

            brickElementTable.AddString50Field("elementId", false).AddScriptComments("Official LEGO element ID (unique identifier for a specific part+colour combination)");
            brickElementTable.AddForeignKeyField(brickPartTable, false).AddScriptComments("The part this element represents");
            brickElementTable.AddForeignKeyField(brickColourTable, false).AddScriptComments("The colour of this element");
            brickElementTable.AddString50Field("designId", true).AddScriptComments("LEGO design ID (null if not available)");

            brickElementTable.AddControlFields();

            brickElementTable.AddUniqueConstraint(new List<string>() { "elementId" }, false);


            // -------------------------------------------------
            // LegoMinifig — Official LEGO minifigure definitions
            // -------------------------------------------------
            Database.Table legoMinifigTable = database.AddTable("LegoMinifig");
            legoMinifigTable.comment = "Official LEGO minifigure definitions. Each row represents a distinct minifig (e.g. fig-000001 Han Solo).";
            legoMinifigTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            legoMinifigTable.AddIdField();
            // Use STRING_500 for name because Rebrickable minifig names can be very long
            legoMinifigTable.AddString500Field("name", false).AddScriptComments("Minifig name — can be long descriptive text from Rebrickable");

            legoMinifigTable.AddString100Field("figNumber", false).AddScriptComments("Rebrickable minifig number (e.g. 'fig-000001')");
            legoMinifigTable.AddIntField("partCount", false).AddScriptComments("Total number of parts in the minifig");
            legoMinifigTable.AddString250Field("imageUrl").AddScriptComments("URL to the minifig's image");

            legoMinifigTable.AddControlFields();

            legoMinifigTable.AddUniqueConstraint(new List<string>() { "figNumber" }, false);


            // -------------------------------------------------
            // LegoSetMinifig — Minifigs included in each set
            // -------------------------------------------------
            Database.Table legoSetMinifigTable = database.AddTable("LegoSetMinifig");
            legoSetMinifigTable.comment = "Minifigs included in each official LEGO set's inventory, with quantities.";
            legoSetMinifigTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            legoSetMinifigTable.AddIdField();

            legoSetMinifigTable.AddForeignKeyField(legoSetTable, false).AddScriptComments("The set this minifig belongs to");
            legoSetMinifigTable.AddForeignKeyField(legoMinifigTable, false).AddScriptComments("The minifig included in the set");
            legoSetMinifigTable.AddIntField("quantity").AddScriptComments("Number of this minifig included in the set");

            legoSetMinifigTable.AddControlFields();


            // -------------------------------------------------
            // LegoSetSubset — Sets included within other sets
            // -------------------------------------------------
            Database.Table legoSetSubsetTable = database.AddTable("LegoSetSubset");
            legoSetSubsetTable.comment = "Sets included within other sets (e.g. polybags inside a larger set). Derived from Rebrickable inventory_sets.csv.";
            legoSetSubsetTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            legoSetSubsetTable.AddIdField();

            legoSetSubsetTable.AddForeignKeyField("parentLegoSetId", legoSetTable, false).AddScriptComments("The parent set that contains the subset");
            legoSetSubsetTable.AddForeignKeyField("childLegoSetId", legoSetTable, false).AddScriptComments("The subset included within the parent set");
            legoSetSubsetTable.AddIntField("quantity").AddScriptComments("Number of copies of the subset included in the parent");

            legoSetSubsetTable.AddControlFields();

            #endregion


            #region User Collection and Inventory

            // -------------------------------------------------
            // UserCollection — Named part collection / palette
            // -------------------------------------------------
            Database.Table userCollectionTable = database.AddTable("UserCollection");
            userCollectionTable.comment = "A user's named part collection or palette. Users can have multiple collections (e.g. 'My Inventory', 'Technic Parts', 'Parts for MOC #5').";
            userCollectionTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COLLECTION_WRITER_PERMISSION_LEVEL);
            userCollectionTable.customWriteAccessRole = BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME;
            userCollectionTable.AddIdField();
            userCollectionTable.AddMultiTenantSupport();
            userCollectionTable.AddNameAndDescriptionFields(true, true, false);

            userCollectionTable.AddBoolField("isDefault", false, false).AddScriptComments("Whether this is the user's primary / default collection");

            userCollectionTable.AddVersionControl();
            userCollectionTable.AddControlFields();


            // -------------------------------------------------
            // UserCollectionPart — Parts in a collection with quantity tracking
            // -------------------------------------------------
            Database.Table userCollectionPartTable = database.AddTable("UserCollectionPart");
            userCollectionPartTable.comment = "Individual part+colour entries within a user collection, with quantity owned and quantity currently allocated to projects.";
            userCollectionPartTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COLLECTION_WRITER_PERMISSION_LEVEL);
            userCollectionPartTable.customWriteAccessRole = BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME;
            userCollectionPartTable.AddIdField();
            userCollectionPartTable.AddMultiTenantSupport();

            userCollectionPartTable.AddForeignKeyField(userCollectionTable, false).AddScriptComments("The collection this part entry belongs to");
            userCollectionPartTable.AddForeignKeyField(brickPartTable, false).AddScriptComments("The part definition");
            userCollectionPartTable.AddForeignKeyField(brickColourTable, false).AddScriptComments("The specific colour of this part");

            userCollectionPartTable.AddIntField("quantityOwned").AddScriptComments("Total number of this part+colour the user owns");
            userCollectionPartTable.AddIntField("quantityUsed").AddScriptComments("Number currently allocated to projects (for build-with-what-you-own filtering)");

            userCollectionPartTable.AddControlFields();

            userCollectionPartTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "userCollectionId", "brickPartId", "brickColourId" }, false);


            // -------------------------------------------------
            // UserWishlistItem — Parts the user wants to acquire
            // -------------------------------------------------
            Database.Table userWishlistItemTable = database.AddTable("UserWishlistItem");
            userWishlistItemTable.comment = "Parts the user wants to acquire. Can optionally specify a colour or leave null for any colour.";
            userWishlistItemTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COLLECTION_WRITER_PERMISSION_LEVEL);
            userWishlistItemTable.customWriteAccessRole = BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME;
            userWishlistItemTable.AddIdField();
            userWishlistItemTable.AddMultiTenantSupport();

            userWishlistItemTable.AddForeignKeyField(userCollectionTable, false).AddScriptComments("The collection this wishlist item is associated with");
            userWishlistItemTable.AddForeignKeyField(brickPartTable, false).AddScriptComments("The desired part");
            userWishlistItemTable.AddForeignKeyField(brickColourTable, true).AddScriptComments("The desired colour (null = any colour)");

            userWishlistItemTable.AddIntField("quantityDesired").AddScriptComments("Number of this part the user wants to acquire");
            userWishlistItemTable.AddTextField("notes").AddScriptComments("Free-form notes about the wishlist item (e.g. source, priority)");

            userWishlistItemTable.AddControlFields();


            // -------------------------------------------------
            // UserCollectionSetImport — Track which sets were imported into a collection
            // -------------------------------------------------
            Database.Table userCollectionSetImportTable = database.AddTable("UserCollectionSetImport");
            userCollectionSetImportTable.comment = "Tracks which official LEGO sets have been imported into a user's collection, with quantity (e.g. 2 copies of set 42131).";
            userCollectionSetImportTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COLLECTION_WRITER_PERMISSION_LEVEL);
            userCollectionSetImportTable.customWriteAccessRole = BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME;
            userCollectionSetImportTable.AddIdField();
            userCollectionSetImportTable.AddMultiTenantSupport();

            userCollectionSetImportTable.AddForeignKeyField(userCollectionTable, false).AddScriptComments("The collection the set was imported into");
            userCollectionSetImportTable.AddForeignKeyField(legoSetTable, false).AddScriptComments("The set that was imported");

            userCollectionSetImportTable.AddIntField("quantity").AddScriptComments("Number of copies of this set imported (e.g. user owns 2 copies)");
            userCollectionSetImportTable.AddDateTimeField("importedDate").AddScriptComments("Date/time the set was imported into the collection");

            userCollectionSetImportTable.AddControlFields();

            userCollectionSetImportTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "userCollectionId", "legoSetId" }, false);


            // -------------------------------------------------
            // RebrickableUserLink — User's Rebrickable credentials and sync configuration
            // -------------------------------------------------
            Database.Table rebrickableUserLinkTable = database.AddTable("RebrickableUserLink");
            rebrickableUserLinkTable.comment = "Stores each user's Rebrickable credentials/token and sync configuration. One link per tenant. Supports three auth modes: ApiToken, EncryptedCredentials, SessionOnly.";
            rebrickableUserLinkTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COLLECTION_WRITER_PERMISSION_LEVEL);
            rebrickableUserLinkTable.customWriteAccessRole = BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME;
            rebrickableUserLinkTable.AddIdField();
            rebrickableUserLinkTable.AddMultiTenantSupport();

            rebrickableUserLinkTable.AddString100Field("rebrickableUsername", false).AddScriptComments("User's Rebrickable username for display and reference");
            rebrickableUserLinkTable.AddString500Field("encryptedApiToken", false).AddScriptComments("Encrypted Rebrickable user token — used for API calls on behalf of the user");
            rebrickableUserLinkTable.AddString50Field("authMode", false).AddScriptComments("Auth trust level: ApiToken, EncryptedCredentials, SessionOnly");
            rebrickableUserLinkTable.AddString500Field("encryptedPassword", true).AddScriptComments("Encrypted Rebrickable password — only used in EncryptedCredentials auth mode (null otherwise)");
            rebrickableUserLinkTable.AddBoolField("syncEnabled", false, true).AddScriptComments("Whether automatic sync is enabled for this user");
            rebrickableUserLinkTable.AddString50Field("syncDirectionFlags", false).AddScriptComments("Integration mode: None, RealTime, PushOnly, ImportOnly");
            rebrickableUserLinkTable.AddIntField("pullIntervalMinutes", true).AddScriptComments("Periodic pull interval in minutes for RealTime mode (null = manual only)");
            rebrickableUserLinkTable.AddDateTimeField("lastSyncDate", true).AddScriptComments("Date/time of last successful sync with Rebrickable (legacy — kept for compatibility)");
            rebrickableUserLinkTable.AddDateTimeField("lastPullDate", true).AddScriptComments("Date/time of last successful pull from Rebrickable");
            rebrickableUserLinkTable.AddDateTimeField("lastPushDate", true).AddScriptComments("Date/time of last successful push to Rebrickable");
            rebrickableUserLinkTable.AddTextField("lastSyncError").AddScriptComments("Last sync error message for display to the user (null = no error)");
            rebrickableUserLinkTable.AddIntField("tokenExpiryDays", true).AddScriptComments("User-configurable auto-clear interval in days (null = never auto-clear)");
            rebrickableUserLinkTable.AddDateTimeField("tokenStoredDate", true).AddScriptComments("When the token was last stored or refreshed — used with tokenExpiryDays for auto-expiry");

            rebrickableUserLinkTable.AddControlFields();

            rebrickableUserLinkTable.AddUniqueConstraint(new List<string>() { "tenantGuid" }, false);


            // -------------------------------------------------
            // RebrickableTransaction — Audit log for all Rebrickable API calls - Not writeable through standard data controllers.  Fully system managed data.
            // -------------------------------------------------
            Database.Table rebrickableTransactionTable = database.AddTable("RebrickableTransaction");
            rebrickableTransactionTable.comment = "Full audit log of every Rebrickable API call BMC makes on behalf of a user. Enables the Communications Panel for total transparency. Every push, pull, login, and error is recorded.";
            rebrickableTransactionTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            rebrickableTransactionTable.SetTableToBeReadonlyForControllerCreationPurposes();
            rebrickableTransactionTable.AddIdField();
            rebrickableTransactionTable.AddMultiTenantSupport();

            rebrickableTransactionTable.AddDateTimeField("transactionDate").AddScriptComments("Date/time the API call was made");
            rebrickableTransactionTable.AddString50Field("direction", false).AddScriptComments("Direction of data flow: Push, Pull");
            rebrickableTransactionTable.AddString50Field("httpMethod", false).AddScriptComments("HTTP method used: GET, POST, PUT, PATCH, DELETE");
            rebrickableTransactionTable.AddString500Field("endpoint", false).AddScriptComments("The Rebrickable API URL that was called");
            rebrickableTransactionTable.AddTextField("requestSummary").AddScriptComments("Human-readable description of the operation, e.g. 'Added set 42131-1 x1'");
            rebrickableTransactionTable.AddIntField("responseStatusCode").AddScriptComments("HTTP status code returned by Rebrickable");
            rebrickableTransactionTable.AddTextField("responseBody").AddScriptComments("Raw response body from Rebrickable (for debugging — may be null for large responses)");
            rebrickableTransactionTable.AddBoolField("success", false, true).AddScriptComments("Whether the API call completed successfully");
            rebrickableTransactionTable.AddTextField("errorMessage").AddScriptComments("Error details if the call failed (null on success)");
            rebrickableTransactionTable.AddString100Field("triggeredBy", false).AddScriptComments("What initiated this call: UserAction, PeriodicSync, ManualPull, SessionLogin");
            rebrickableTransactionTable.AddIntField("recordCount", true).AddScriptComments("Number of rows retrieved or affected by this API call");

            rebrickableTransactionTable.AddControlFields();


            // -------------------------------------------------
            // RebrickableSyncQueue — Outbound sync message queue for resilient Rebrickable API writes
            // -------------------------------------------------
            Database.Table rebrickableSyncQueueTable = database.AddTable("RebrickableSyncQueue");
            rebrickableSyncQueueTable.comment = "Outbound sync queue for resilient Rebrickable API writes. When a user modifies collection data locally, a queue entry is created. A background service picks up pending items and pushes them to Rebrickable. Retries on failure with exponential backoff.";
            rebrickableSyncQueueTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            rebrickableSyncQueueTable.SetTableToBeReadonlyForControllerCreationPurposes();
            rebrickableSyncQueueTable.AddIdField();
            rebrickableSyncQueueTable.AddMultiTenantSupport();

            rebrickableSyncQueueTable.AddString50Field("operationType", false).AddScriptComments("Sync operation: Create, Update, Delete");
            rebrickableSyncQueueTable.AddString50Field("entityType", false).AddScriptComments("Entity being synced: SetList, SetListItem, PartList, PartListItem, LostPart");
            rebrickableSyncQueueTable.AddLongField("entityId", false).AddScriptComments("Database ID of the entity being synced");
            rebrickableSyncQueueTable.AddTextField("payload").AddScriptComments("JSON-serialized data needed by the Rebrickable API call");
            rebrickableSyncQueueTable.AddString50Field("status", false).AddScriptComments("Queue status: Pending, InProgress, Completed, Failed, Abandoned");
            rebrickableSyncQueueTable.AddDateTimeField("createdDate").AddScriptComments("When this queue entry was created");
            rebrickableSyncQueueTable.AddDateTimeField("lastAttemptDate", true).AddScriptComments("When the last processing attempt occurred (null = never attempted)");
            rebrickableSyncQueueTable.AddDateTimeField("completedDate", true).AddScriptComments("When processing completed successfully (null = not yet completed)");
            rebrickableSyncQueueTable.AddIntField("attemptCount", false).AddScriptComments("Number of processing attempts so far");
            rebrickableSyncQueueTable.AddIntField("maxAttempts", false).AddScriptComments("Maximum retry attempts before marking as Abandoned (default: 5)");
            rebrickableSyncQueueTable.AddTextField("errorMessage").AddScriptComments("Last error message from a failed attempt (null on success)");
            rebrickableSyncQueueTable.AddTextField("responseBody").AddScriptComments("Last response body from Rebrickable for debugging (null on success)");

            rebrickableSyncQueueTable.AddControlFields();


            // -------------------------------------------------
            // UserPartList — Named part lists (Rebrickable partlists mirror)
            // -------------------------------------------------
            Database.Table userPartListTable = database.AddTable("UserPartList");
            userPartListTable.comment = "Named part lists, mirroring Rebrickable's partlists/ endpoint. Users can have multiple named lists for organizing parts.";
            userPartListTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COLLECTION_WRITER_PERMISSION_LEVEL);
            userPartListTable.customWriteAccessRole = BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME;
            userPartListTable.AddIdField();
            userPartListTable.AddMultiTenantSupport();

            userPartListTable.AddNameField(true, true);
            userPartListTable.AddBoolField("isBuildable", false, false).AddScriptComments("Whether this list represents buildable parts (for build matching)");
            userPartListTable.AddIntField("rebrickableListId", true).AddScriptComments("Rebrickable list ID for bidirectional sync (null = BMC-only list)");

            userPartListTable.AddVersionControl();
            userPartListTable.AddControlFields();


            // -------------------------------------------------
            // UserPartListItem — Parts within a named part list
            // -------------------------------------------------
            Database.Table userPartListItemTable = database.AddTable("UserPartListItem");
            userPartListItemTable.comment = "Individual part+colour entries within a user's named part list. Mirrors Rebrickable's partlists/{id}/parts/ endpoint.";
            userPartListItemTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COLLECTION_WRITER_PERMISSION_LEVEL);
            userPartListItemTable.customWriteAccessRole = BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME;
            userPartListItemTable.AddIdField();
            userPartListItemTable.AddMultiTenantSupport();

            userPartListItemTable.AddForeignKeyField(userPartListTable, false).AddScriptComments("The part list this item belongs to");
            userPartListItemTable.AddForeignKeyField(brickPartTable, false).AddScriptComments("The part definition");
            userPartListItemTable.AddForeignKeyField(brickColourTable, false).AddScriptComments("The specific colour of this part");
            userPartListItemTable.AddIntField("quantity", false).AddScriptComments("Number of this part+colour in the list");

            userPartListItemTable.AddControlFields();

            userPartListItemTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "userPartListId", "brickPartId", "brickColourId" }, false);


            // -------------------------------------------------
            // UserSetList — Named set lists (Rebrickable setlists mirror)
            // 1
            Database.Table userSetListTable = database.AddTable("UserSetList");
            userSetListTable.comment = "Named set lists, mirroring Rebrickable's setlists/ endpoint. Users can have multiple named lists for organizing sets.";
            userSetListTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COLLECTION_WRITER_PERMISSION_LEVEL);
            userSetListTable.customWriteAccessRole = BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME;
            userSetListTable.AddIdField();
            userSetListTable.AddMultiTenantSupport();

            userSetListTable.AddNameField(true, true);
            userSetListTable.AddBoolField("isBuildable", false, false).AddScriptComments("Whether this list represents buildable sets (for build matching)");
            userSetListTable.AddIntField("rebrickableListId", true).AddScriptComments("Rebrickable list ID for bidirectional sync (null = BMC-only list)");

            userSetListTable.AddVersionControl();
            userSetListTable.AddControlFields();


            // -------------------------------------------------
            // UserSetListItem — Sets within a named set list
            // -------------------------------------------------
            Database.Table userSetListItemTable = database.AddTable("UserSetListItem");
            userSetListItemTable.comment = "Individual set entries within a user's named set list. Mirrors Rebrickable's setlists/{id}/sets/ endpoint.";
            userSetListItemTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COLLECTION_WRITER_PERMISSION_LEVEL);
            userSetListItemTable.customWriteAccessRole = BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME;
            userSetListItemTable.AddIdField();
            userSetListItemTable.AddMultiTenantSupport();

            userSetListItemTable.AddForeignKeyField(userSetListTable, false).AddScriptComments("The set list this item belongs to");
            userSetListItemTable.AddForeignKeyField(legoSetTable, false).AddScriptComments("The set in this list");
            userSetListItemTable.AddIntField("quantity", false, 1).AddScriptComments("Number of copies of this set in the list");
            userSetListItemTable.AddBoolField("includeSpares", false, true).AddScriptComments("Whether to include spare parts from this set in build matching");

            userSetListItemTable.AddControlFields();

            userSetListItemTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "userSetListId", "legoSetId" }, false);


            // -------------------------------------------------
            // UserLostPart — Parts lost from sets (Rebrickable lost_parts mirror)
            // -------------------------------------------------
            Database.Table userLostPartTable = database.AddTable("UserLostPart");
            userLostPartTable.comment = "Parts lost from sets, mirroring Rebrickable's lost_parts/ endpoint. Tracks which parts are missing from a user's collection.";
            userLostPartTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COLLECTION_WRITER_PERMISSION_LEVEL);
            userLostPartTable.customWriteAccessRole = BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME;
            userLostPartTable.AddIdField();
            userLostPartTable.AddMultiTenantSupport();

            userLostPartTable.AddForeignKeyField(brickPartTable, false).AddScriptComments("The part that was lost");
            userLostPartTable.AddForeignKeyField(brickColourTable, false).AddScriptComments("The colour of the lost part");
            userLostPartTable.AddForeignKeyField(legoSetTable, true).AddScriptComments("The set the part was lost from (null if unknown)");
            userLostPartTable.AddIntField("lostQuantity", false).AddScriptComments("Number of this part+colour that were lost");
            userLostPartTable.AddIntField("rebrickableInvPartId", true).AddScriptComments("Rebrickable inventory_part ID for bidirectional sync (null = BMC-only entry)");

            userLostPartTable.AddControlFields();

            #endregion


            #region Building Instructions and Manuals

            // -------------------------------------------------
            // BuildManual — A complete instruction booklet for a project
            // -------------------------------------------------
            Database.Table buildManualTable = database.AddTable("BuildManual");
            buildManualTable.comment = "A complete instruction booklet for a building project. A project can have multiple manuals (e.g. one per bag/booklet).";
            buildManualTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_INSTRUCTION_WRITER_PERMISSION_LEVEL);
            buildManualTable.customWriteAccessRole = BMC_INSTRUCTION_WRITER_CUSTOM_ROLE_NAME;
            buildManualTable.AddIdField();
            buildManualTable.AddMultiTenantSupport();

            buildManualTable.AddForeignKeyField(projectTable, false).AddScriptComments("The project this manual documents");
            buildManualTable.AddNameAndDescriptionFields(true, true, false);

            buildManualTable.AddSingleField("pageWidthMm").AddScriptComments("Page width in millimetres for PDF/print layout (e.g. 210 for A4)");
            buildManualTable.AddSingleField("pageHeightMm").AddScriptComments("Page height in millimetres for PDF/print layout (e.g. 297 for A4)");
            buildManualTable.AddBoolField("isPublished", false, false).AddScriptComments("Whether this manual is marked as published/final");

            buildManualTable.AddVersionControl();
            buildManualTable.AddControlFields();


            // -------------------------------------------------
            // BuildManualPage — A single page in the instruction booklet
            // -------------------------------------------------
            Database.Table buildManualPageTable = database.AddTable("BuildManualPage");
            buildManualPageTable.comment = "A single page within a build manual. Contains one or more build steps.";
            buildManualPageTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_INSTRUCTION_WRITER_PERMISSION_LEVEL);
            buildManualPageTable.customWriteAccessRole = BMC_INSTRUCTION_WRITER_CUSTOM_ROLE_NAME;
            buildManualPageTable.AddIdField();
            buildManualPageTable.AddMultiTenantSupport();

            buildManualPageTable.AddForeignKeyField(buildManualTable, false).AddScriptComments("The manual this page belongs to");

            buildManualPageTable.AddIntField("pageNum").AddScriptComments("Sequential page number within the manual.  Note purposely not called pageNumber to not clash with code generated parameter");
            buildManualPageTable.AddString250Field("title").AddScriptComments("Optional page title (e.g. 'Bag 1', 'Chassis Assembly')");
            buildManualPageTable.AddTextField("notes").AddScriptComments("Optional internal notes about this page");

            buildManualPageTable.AddControlFields();


            // -------------------------------------------------
            // BuildManualStep — A single build step on a page
            // -------------------------------------------------
            Database.Table buildManualStepTable = database.AddTable("BuildManualStep");
            buildManualStepTable.comment = "A single build step within a manual page. Defines the camera angle and display options for that step's rendered view.";
            buildManualStepTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_INSTRUCTION_WRITER_PERMISSION_LEVEL);
            buildManualStepTable.customWriteAccessRole = BMC_INSTRUCTION_WRITER_CUSTOM_ROLE_NAME;
            buildManualStepTable.AddIdField();
            buildManualStepTable.AddMultiTenantSupport();

            buildManualStepTable.AddForeignKeyField(buildManualPageTable, false).AddScriptComments("The page this step appears on");

            buildManualStepTable.AddIntField("stepNumber").AddScriptComments("Sequential step number within the page");

            // Camera position for the step's rendered view (nullable — set when user positions the camera)
            buildManualStepTable.AddSingleField("cameraPositionX", true).AddScriptComments("Camera X position for this step's rendered view");
            buildManualStepTable.AddSingleField("cameraPositionY", true).AddScriptComments("Camera Y position for this step's rendered view");
            buildManualStepTable.AddSingleField("cameraPositionZ", true).AddScriptComments("Camera Z position for this step's rendered view");
            buildManualStepTable.AddSingleField("cameraTargetX", true).AddScriptComments("Camera look-at target X for this step");
            buildManualStepTable.AddSingleField("cameraTargetY", true).AddScriptComments("Camera look-at target Y for this step");
            buildManualStepTable.AddSingleField("cameraTargetZ", true).AddScriptComments("Camera look-at target Z for this step");
            buildManualStepTable.AddSingleField("cameraZoom", true).AddScriptComments("Camera zoom / field of view for this step");

            buildManualStepTable.AddBoolField("showExplodedView", false, false).AddScriptComments("Whether to render the step with newly-added parts pulled apart for clarity");
            buildManualStepTable.AddSingleField("explodedDistance", true).AddScriptComments("Distance in LDU to pull apart exploded parts (null = use default)");

            buildManualStepTable.AddControlFields();


            // -------------------------------------------------
            // BuildStepPart — A part placed during a specific step
            // -------------------------------------------------
            Database.Table buildStepPartTable = database.AddTable("BuildStepPart");
            buildStepPartTable.comment = "Maps which placed bricks are added during a specific build step. Links to the actual PlacedBrick in the project.";
            buildStepPartTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_INSTRUCTION_WRITER_PERMISSION_LEVEL);
            buildStepPartTable.customWriteAccessRole = BMC_INSTRUCTION_WRITER_CUSTOM_ROLE_NAME;
            buildStepPartTable.AddIdField();
            buildStepPartTable.AddMultiTenantSupport();

            buildStepPartTable.AddForeignKeyField(buildManualStepTable, false).AddScriptComments("The build step this part is added during");
            buildStepPartTable.AddForeignKeyField(placedBrickTable, false).AddScriptComments("The placed brick in the project that is added in this step");

            buildStepPartTable.AddControlFields();


            // -------------------------------------------------
            // BuildStepAnnotationType — Lookup of annotation types
            // -------------------------------------------------
            Database.Table buildStepAnnotationTypeTable = database.AddTable("BuildStepAnnotationType");
            buildStepAnnotationTypeTable.comment = "Lookup table of annotation types available for build steps (Arrow, Callout, Label, Quantity Callout, Submodel Callout).";
            buildStepAnnotationTypeTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            buildStepAnnotationTypeTable.AddIdField();
            buildStepAnnotationTypeTable.AddNameAndDescriptionFields(true, true, false);

            buildStepAnnotationTypeTable.AddSequenceField();
            buildStepAnnotationTypeTable.AddControlFields();

            // Seed data — core annotation types
            buildStepAnnotationTypeTable.AddData(new Dictionary<string, string> { { "name", "Arrow" }, { "description", "Directional arrow indicating placement direction or connection point" }, { "sequence", "1" }, { "objectGuid", "ba100001-0001-4000-8000-000000000001" } });
            buildStepAnnotationTypeTable.AddData(new Dictionary<string, string> { { "name", "Callout" }, { "description", "Callout box highlighting a sub-assembly built separately" }, { "sequence", "2" }, { "objectGuid", "ba100001-0001-4000-8000-000000000002" } });
            buildStepAnnotationTypeTable.AddData(new Dictionary<string, string> { { "name", "Label" }, { "description", "Text label providing additional context or instruction" }, { "sequence", "3" }, { "objectGuid", "ba100001-0001-4000-8000-000000000003" } });
            buildStepAnnotationTypeTable.AddData(new Dictionary<string, string> { { "name", "Quantity Callout" }, { "description", "Quantity indicator showing how many of a part are needed in this step" }, { "sequence", "4" }, { "objectGuid", "ba100001-0001-4000-8000-000000000004" } });
            buildStepAnnotationTypeTable.AddData(new Dictionary<string, string> { { "name", "Submodel Callout" }, { "description", "Callout referencing a submodel that should be built as part of this step" }, { "sequence", "5" }, { "objectGuid", "ba100001-0001-4000-8000-000000000005" } });


            // -------------------------------------------------
            // BuildStepAnnotation — Arrows, callouts, labels on a step
            // -------------------------------------------------
            Database.Table buildStepAnnotationTable = database.AddTable("BuildStepAnnotation");
            buildStepAnnotationTable.comment = "Visual annotations (arrows, callouts, labels) placed on a build step's rendered view.";
            buildStepAnnotationTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_INSTRUCTION_WRITER_PERMISSION_LEVEL);
            buildStepAnnotationTable.customWriteAccessRole = BMC_INSTRUCTION_WRITER_CUSTOM_ROLE_NAME;
            buildStepAnnotationTable.AddIdField();
            buildStepAnnotationTable.AddMultiTenantSupport();

            buildStepAnnotationTable.AddForeignKeyField(buildManualStepTable, false).AddScriptComments("The build step this annotation belongs to");
            buildStepAnnotationTable.AddForeignKeyField(buildStepAnnotationTypeTable, false).AddScriptComments("The type of annotation (Arrow, Callout, Label, etc.)");

            // 2D position on the rendered page
            buildStepAnnotationTable.AddSingleField("positionX").AddScriptComments("X position on the rendered page (normalised 0-1 or pixel coordinates)");
            buildStepAnnotationTable.AddSingleField("positionY").AddScriptComments("Y position on the rendered page");
            buildStepAnnotationTable.AddSingleField("width", true).AddScriptComments("Width of the annotation element (null = auto-size)");
            buildStepAnnotationTable.AddSingleField("height", true).AddScriptComments("Height of the annotation element (null = auto-size)");

            buildStepAnnotationTable.AddTextField("text").AddScriptComments("Optional text content for labels and callouts");
            buildStepAnnotationTable.AddForeignKeyField(placedBrickTable, true).AddScriptComments("Optional target placed brick that this annotation points to or highlights");

            buildStepAnnotationTable.AddControlFields();

            #endregion


            #region Rendering and Export

            // -------------------------------------------------
            // RenderPreset — Named rendering configuration presets
            // -------------------------------------------------
            Database.Table renderPresetTable = database.AddTable("RenderPreset");
            renderPresetTable.comment = "Reusable rendering presets that define resolution, lighting, and quality settings for producing images of models.";
            renderPresetTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            renderPresetTable.AddIdField();
            renderPresetTable.AddMultiTenantSupport();
            renderPresetTable.AddNameAndDescriptionFields(true, true, false);

            renderPresetTable.AddIntField("resolutionWidth").AddScriptComments("Output image width in pixels");
            renderPresetTable.AddIntField("resolutionHeight").AddScriptComments("Output image height in pixels");
            renderPresetTable.AddHTMLColorField("backgroundColorHex").AddScriptComments("Background colour in hex (e.g. #FFFFFF for white, #000000 for black)");
            renderPresetTable.AddBoolField("enableShadows", false, true).AddScriptComments("Whether to render drop shadows");
            renderPresetTable.AddBoolField("enableReflections", false, false).AddScriptComments("Whether to render environment reflections on metallic/chrome parts");
            renderPresetTable.AddString100Field("lightingMode").AddScriptComments("Lighting preset name: studio, outdoor, dramatic, custom");
            renderPresetTable.AddIntField("antiAliasLevel").AddScriptComments("Anti-aliasing level (1=none, 2=2x, 4=4x, 8=8x)");

            renderPresetTable.AddControlFields();


            // -------------------------------------------------
            // ProjectRender — Saved renders produced from a project
            // -------------------------------------------------
            Database.Table projectRenderTable = database.AddTable("ProjectRender");
            projectRenderTable.comment = "Records of rendered images produced from a project, with the preset used and output metadata.";
            projectRenderTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            projectRenderTable.AddIdField();
            projectRenderTable.AddMultiTenantSupport();

            projectRenderTable.AddForeignKeyField(projectTable, false).AddScriptComments("The project this render was produced from");
            projectRenderTable.AddForeignKeyField(renderPresetTable, true).AddScriptComments("The render preset used (null = custom/one-off settings)");
            projectRenderTable.AddNameField(true, true);

            projectRenderTable.AddString250Field("outputFilePath").AddScriptComments("Relative path to the rendered image file");
            projectRenderTable.AddIntField("resolutionWidth").AddScriptComments("Actual output width in pixels");
            projectRenderTable.AddIntField("resolutionHeight").AddScriptComments("Actual output height in pixels");
            projectRenderTable.AddDateTimeField("renderedDate").AddScriptComments("Date/time the render was produced");
            projectRenderTable.AddSingleField("renderDurationSeconds").AddScriptComments("Time taken to produce the render in seconds");

            projectRenderTable.AddControlFields();


            // -------------------------------------------------
            // ExportFormat — Lookup table of supported export formats
            // -------------------------------------------------
            Database.Table exportFormatTable = database.AddTable("ExportFormat");
            exportFormatTable.comment = "Lookup table of supported file export formats for models, instructions, and parts lists.";
            exportFormatTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            exportFormatTable.AddIdField();
            exportFormatTable.AddNameAndDescriptionFields(true, true, false);

            exportFormatTable.AddString50Field("fileExtension").AddScriptComments("File extension including dot (e.g. '.ldr', '.pdf', '.xml')");

            exportFormatTable.AddSequenceField();
            exportFormatTable.AddControlFields();

            // Seed data — supported export formats
            exportFormatTable.AddData(new Dictionary<string, string> { { "name", "LDraw" }, { "description", "LDraw single-model file format" }, { "fileExtension", ".ldr" }, { "sequence", "1" }, { "objectGuid", "ef100001-0001-4000-8000-000000000001" } });
            exportFormatTable.AddData(new Dictionary<string, string> { { "name", "LDraw Multi-Part" }, { "description", "LDraw multi-part document containing submodels" }, { "fileExtension", ".mpd" }, { "sequence", "2" }, { "objectGuid", "ef100001-0001-4000-8000-000000000002" } });
            exportFormatTable.AddData(new Dictionary<string, string> { { "name", "Wavefront OBJ" }, { "description", "Wavefront OBJ 3D model format for Blender and other 3D tools" }, { "fileExtension", ".obj" }, { "sequence", "3" }, { "objectGuid", "ef100001-0001-4000-8000-000000000003" } });
            exportFormatTable.AddData(new Dictionary<string, string> { { "name", "COLLADA" }, { "description", "COLLADA 3D asset exchange format" }, { "fileExtension", ".dae" }, { "sequence", "4" }, { "objectGuid", "ef100001-0001-4000-8000-000000000004" } });
            exportFormatTable.AddData(new Dictionary<string, string> { { "name", "BrickLink XML" }, { "description", "BrickLink wanted-list XML format for ordering parts" }, { "fileExtension", ".xml" }, { "sequence", "5" }, { "objectGuid", "ef100001-0001-4000-8000-000000000005" } });
            exportFormatTable.AddData(new Dictionary<string, string> { { "name", "Rebrickable CSV" }, { "description", "Rebrickable-compatible CSV parts list" }, { "fileExtension", ".csv" }, { "sequence", "6" }, { "objectGuid", "ef100001-0001-4000-8000-000000000006" } });
            exportFormatTable.AddData(new Dictionary<string, string> { { "name", "PDF Instructions" }, { "description", "PDF export of build manual instructions" }, { "fileExtension", ".pdf" }, { "sequence", "7" }, { "objectGuid", "ef100001-0001-4000-8000-000000000007" } });


            // -------------------------------------------------
            // ProjectExport — Log of exports produced from a project
            // -------------------------------------------------
            Database.Table projectExportTable = database.AddTable("ProjectExport");
            projectExportTable.comment = "Log of exports produced from a project, tracking what was exported and when.";
            projectExportTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_BUILDER_WRITER_PERMISSION_LEVEL);
            projectExportTable.AddIdField();
            projectExportTable.AddMultiTenantSupport();

            projectExportTable.AddForeignKeyField(projectTable, false).AddScriptComments("The project this export was produced from");
            projectExportTable.AddForeignKeyField(exportFormatTable, false).AddScriptComments("The format used for the export");
            projectExportTable.AddNameField(true, true);

            projectExportTable.AddString250Field("outputFilePath").AddScriptComments("Relative path to the exported file");
            projectExportTable.AddDateTimeField("exportedDate").AddScriptComments("Date/time the export was produced");
            projectExportTable.AddBoolField("includeInstructions", false, false).AddScriptComments("Whether build instructions were included in the export");
            projectExportTable.AddBoolField("includePartsList", false, false).AddScriptComments("Whether a bill of materials / parts list was included in the export");

            projectExportTable.AddControlFields();

            #endregion


            #region User Profiles and Identity

            // -------------------------------------------------
            // UserProfile — Public builder profile, one per tenant
            // -------------------------------------------------
            Database.Table userProfileTable = database.AddTable("UserProfile");
            userProfileTable.comment = "Public builder profile for community features. One profile per tenant (user). Decoupled from Foundation user/tenant tables to keep BMC concerns independent.";
            userProfileTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            userProfileTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            userProfileTable.AddIdField();
            userProfileTable.AddMultiTenantSupport();

            userProfileTable.AddString100Field("displayName", false).AddScriptComments("Public display name shown in the community (distinct from auth username)");
            userProfileTable.AddTextField("bio").AddScriptComments("Free-form biography / about-me text");
            userProfileTable.AddString100Field("location").AddScriptComments("User's declared location (city, country, or free-form)");
            userProfileTable.AddBinaryDataFields("avatar");      // avatarFileName, avatarSize, avatarData, avatarMimeType
            userProfileTable.AddBinaryDataFields("banner");       // bannerFileName, bannerSize, bannerData, bannerMimeType
            userProfileTable.AddString250Field("websiteUrl").AddScriptComments("Optional personal website or portfolio URL");
            userProfileTable.AddBoolField("isPublic", false, true).AddScriptComments("Whether this profile is visible to unauthenticated visitors");
            userProfileTable.AddDateTimeField("memberSinceDate", true).AddScriptComments("Date the user first created their profile (for display purposes)");

            userProfileTable.AddVersionControl();
            userProfileTable.AddControlFields();


            // -------------------------------------------------
            // UserProfileLinkType — Lookup of external link types
            // -------------------------------------------------
            Database.Table userProfileLinkTypeTable = database.AddTable("UserProfileLinkType");
            userProfileLinkTypeTable.comment = "Lookup table of external link types a user can add to their profile (e.g. BrickLink Store, Flickr, YouTube, Instagram, Rebrickable).";
            userProfileLinkTypeTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            userProfileLinkTypeTable.AddIdField();
            userProfileLinkTypeTable.AddNameAndDescriptionFields(true, true, false);

            userProfileLinkTypeTable.AddString100Field("iconCssClass").AddScriptComments("CSS class for the link type icon (e.g. 'fab fa-youtube')");

            userProfileLinkTypeTable.AddSequenceField();
            userProfileLinkTypeTable.AddControlFields();

            // Seed data — common AFOL community link types
            userProfileLinkTypeTable.AddData(new Dictionary<string, string> { { "name", "BrickLink Store" }, { "description", "Link to the user's BrickLink seller store" }, { "iconCssClass", "fas fa-store" }, { "sequence", "1" }, { "objectGuid", "a0100001-0001-4000-8000-000000000001" } });
            userProfileLinkTypeTable.AddData(new Dictionary<string, string> { { "name", "Rebrickable" }, { "description", "Link to the user's Rebrickable profile" }, { "iconCssClass", "fas fa-cubes" }, { "sequence", "2" }, { "objectGuid", "a0100001-0001-4000-8000-000000000002" } });
            userProfileLinkTypeTable.AddData(new Dictionary<string, string> { { "name", "Flickr" }, { "description", "Link to the user's Flickr photostream" }, { "iconCssClass", "fab fa-flickr" }, { "sequence", "3" }, { "objectGuid", "a0100001-0001-4000-8000-000000000003" } });
            userProfileLinkTypeTable.AddData(new Dictionary<string, string> { { "name", "YouTube" }, { "description", "Link to the user's YouTube channel" }, { "iconCssClass", "fab fa-youtube" }, { "sequence", "4" }, { "objectGuid", "a0100001-0001-4000-8000-000000000004" } });
            userProfileLinkTypeTable.AddData(new Dictionary<string, string> { { "name", "Instagram" }, { "description", "Link to the user's Instagram profile" }, { "iconCssClass", "fab fa-instagram" }, { "sequence", "5" }, { "objectGuid", "a0100001-0001-4000-8000-000000000005" } });
            userProfileLinkTypeTable.AddData(new Dictionary<string, string> { { "name", "Personal Website" }, { "description", "Link to the user's personal website or blog" }, { "iconCssClass", "fas fa-globe" }, { "sequence", "6" }, { "objectGuid", "a0100001-0001-4000-8000-000000000006" } });
            userProfileLinkTypeTable.AddData(new Dictionary<string, string> { { "name", "Eurobricks" }, { "description", "Link to the user's Eurobricks forum profile" }, { "iconCssClass", "fas fa-comments" }, { "sequence", "7" }, { "objectGuid", "a0100001-0001-4000-8000-000000000007" } });


            // -------------------------------------------------
            // UserProfileLink — External links on a user's profile
            // -------------------------------------------------
            Database.Table userProfileLinkTable = database.AddTable("UserProfileLink");
            userProfileLinkTable.comment = "External links displayed on a user's public profile (BrickLink store, Flickr, YouTube, etc.).";
            userProfileLinkTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            userProfileLinkTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            userProfileLinkTable.AddIdField();
            userProfileLinkTable.AddMultiTenantSupport();

            userProfileLinkTable.AddForeignKeyField(userProfileTable, false).AddScriptComments("The profile this link belongs to");
            userProfileLinkTable.AddForeignKeyField(userProfileLinkTypeTable, false).AddScriptComments("The type of link (BrickLink, YouTube, etc.)");
            userProfileLinkTable.AddString500Field("url", false).AddScriptComments("The full URL to the external resource");
            userProfileLinkTable.AddString100Field("displayLabel").AddScriptComments("Optional custom label to display instead of the URL (e.g. 'My BL Store')");

            userProfileLinkTable.AddSequenceField();
            userProfileLinkTable.AddControlFields();


            // -------------------------------------------------
            // UserProfilePreferredTheme — User's favourite LEGO themes
            // -------------------------------------------------
            Database.Table userProfilePreferredThemeTable = database.AddTable("UserProfilePreferredTheme");
            userProfilePreferredThemeTable.comment = "Junction table linking a user profile to their preferred LEGO themes (e.g. Star Wars, Technic, City). Used to personalise the experience and display theme interests on the public profile.";
            userProfilePreferredThemeTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            userProfilePreferredThemeTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            userProfilePreferredThemeTable.AddIdField();
            userProfilePreferredThemeTable.AddMultiTenantSupport();

            userProfilePreferredThemeTable.AddForeignKeyField(userProfileTable, false).AddScriptComments("The profile this preference belongs to");
            userProfilePreferredThemeTable.AddForeignKeyField(legoThemeTable, false).AddScriptComments("The LEGO theme the user prefers");

            userProfilePreferredThemeTable.AddSequenceField();
            userProfilePreferredThemeTable.AddControlFields();

            userProfilePreferredThemeTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "userProfileId", "legoThemeId" }, false);


            // -------------------------------------------------
            // UserSetOwnership — Track owned sets with status
            // -------------------------------------------------
            Database.Table userSetOwnershipTable = database.AddTable("UserSetOwnership");
            userSetOwnershipTable.comment = "Tracks a user's relationship with official LEGO sets for their collector showcase. Distinct from UserCollectionSetImport which tracks parts inventory.";
            userSetOwnershipTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COLLECTION_WRITER_PERMISSION_LEVEL);
            userSetOwnershipTable.customWriteAccessRole = BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME;
            userSetOwnershipTable.AddIdField();
            userSetOwnershipTable.AddMultiTenantSupport();

            userSetOwnershipTable.AddForeignKeyField(legoSetTable, false).AddScriptComments("The official LEGO set");
            userSetOwnershipTable.AddString50Field("status", false).AddScriptComments("Ownership status: Owned, Built, Wanted, WishList, ForDisplay, ForSale");
            userSetOwnershipTable.AddDateTimeField("acquiredDate", true).AddScriptComments("Date the user acquired this set (null if unknown or wanted)");
            userSetOwnershipTable.AddIntField("personalRating", true).AddScriptComments("User's personal rating of the set (1-5 stars, null if not rated)");
            userSetOwnershipTable.AddTextField("notes").AddScriptComments("Free-form notes about this set (e.g. condition, where purchased, modifications)");
            userSetOwnershipTable.AddIntField("quantity", false, 1).AddScriptComments("Number of copies owned");
            userSetOwnershipTable.AddBoolField("isPublic", false, true).AddScriptComments("Whether this ownership record is visible on the user's public profile");

            userSetOwnershipTable.AddControlFields();

            userSetOwnershipTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "legoSetId" }, false);


            // -------------------------------------------------
            // UserProfileStat — Cached aggregate stats for fast display
            // -------------------------------------------------
            Database.Table userProfileStatTable = database.AddTable("UserProfileStat");
            userProfileStatTable.comment = "Cached aggregate statistics for a user's profile. Periodically recalculated by background worker to avoid expensive real-time queries.";
            userProfileStatTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            userProfileStatTable.AddIdField();
            userProfileStatTable.AddMultiTenantSupport();

            userProfileStatTable.AddForeignKeyField(userProfileTable, false).AddScriptComments("The profile these stats belong to");
            userProfileStatTable.AddIntField("totalPartsOwned", false, 0).AddScriptComments("Total number of individual parts across all collections");
            userProfileStatTable.AddIntField("totalUniquePartsOwned", false, 0).AddScriptComments("Total number of unique part+colour combinations owned");
            userProfileStatTable.AddIntField("totalSetsOwned", false, 0).AddScriptComments("Total number of sets with Owned or Built status");
            userProfileStatTable.AddIntField("totalMocsPublished", false, 0).AddScriptComments("Total number of MOCs published to the gallery");
            userProfileStatTable.AddIntField("totalFollowers", false, 0).AddScriptComments("Number of users following this profile");
            userProfileStatTable.AddIntField("totalFollowing", false, 0).AddScriptComments("Number of users this profile is following");
            userProfileStatTable.AddIntField("totalLikesReceived", false, 0).AddScriptComments("Total likes received across all published MOCs");
            userProfileStatTable.AddIntField("totalAchievementPoints", false, 0).AddScriptComments("Sum of achievement point values earned");
            userProfileStatTable.AddDateTimeField("lastCalculatedDate", true).AddScriptComments("When these stats were last recalculated by the background worker");

            userProfileStatTable.AddControlFields();

            #endregion


            #region Social Graph

            // -------------------------------------------------
            // UserFollow — Follow relationships between users
            // -------------------------------------------------
            Database.Table userFollowTable = database.AddTable("UserFollow");
            userFollowTable.comment = "Follow relationships between users. A follower subscribes to activity updates from the followed user.";
            userFollowTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            userFollowTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            userFollowTable.AddIdField();

            userFollowTable.AddGuidField("followerTenantGuid", false).AddScriptComments("Tenant GUID of the user who is following");
            userFollowTable.AddGuidField("followedTenantGuid", false).AddScriptComments("Tenant GUID of the user being followed");
            userFollowTable.AddDateTimeField("followedDate", false).AddScriptComments("Date/time the follow relationship was created");

            userFollowTable.AddControlFields();

            userFollowTable.AddUniqueConstraint(new List<string>() { "followerTenantGuid", "followedTenantGuid" }, false);


            // -------------------------------------------------
            // ActivityEventType — Lookup of activity event types
            // -------------------------------------------------
            Database.Table activityEventTypeTable = database.AddTable("ActivityEventType");
            activityEventTypeTable.comment = "Lookup table of activity event types that appear in users' activity feeds.";
            activityEventTypeTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            activityEventTypeTable.AddIdField();
            activityEventTypeTable.AddNameAndDescriptionFields(true, true, false);

            activityEventTypeTable.AddString100Field("iconCssClass").AddScriptComments("CSS class for the event type icon in the activity feed");
            activityEventTypeTable.AddHTMLColorField("accentColor", true).AddScriptComments("Optional accent colour for this event type in the feed");

            activityEventTypeTable.AddSequenceField();
            activityEventTypeTable.AddControlFields();

            // Seed data — core activity event types
            activityEventTypeTable.AddData(new Dictionary<string, string> { { "name", "PublishedMoc" }, { "description", "User published a MOC to the gallery" }, { "iconCssClass", "fas fa-rocket" }, { "sequence", "1" }, { "objectGuid", "ae100001-0001-4000-8000-000000000001" } });
            activityEventTypeTable.AddData(new Dictionary<string, string> { { "name", "AddedSet" }, { "description", "User added a set to their collection" }, { "iconCssClass", "fas fa-box-open" }, { "sequence", "2" }, { "objectGuid", "ae100001-0001-4000-8000-000000000002" } });
            activityEventTypeTable.AddData(new Dictionary<string, string> { { "name", "EarnedAchievement" }, { "description", "User earned an achievement" }, { "iconCssClass", "fas fa-trophy" }, { "sequence", "3" }, { "objectGuid", "ae100001-0001-4000-8000-000000000003" } });
            activityEventTypeTable.AddData(new Dictionary<string, string> { { "name", "JoinedChallenge" }, { "description", "User submitted an entry to a build challenge" }, { "iconCssClass", "fas fa-flag-checkered" }, { "sequence", "4" }, { "objectGuid", "ae100001-0001-4000-8000-000000000004" } });
            activityEventTypeTable.AddData(new Dictionary<string, string> { { "name", "SharedInstruction" }, { "description", "User published build instructions" }, { "iconCssClass", "fas fa-book" }, { "sequence", "5" }, { "objectGuid", "ae100001-0001-4000-8000-000000000005" } });
            activityEventTypeTable.AddData(new Dictionary<string, string> { { "name", "CollectionMilestone" }, { "description", "User reached a collection milestone" }, { "iconCssClass", "fas fa-gem" }, { "sequence", "6" }, { "objectGuid", "ae100001-0001-4000-8000-000000000006" } });
            activityEventTypeTable.AddData(new Dictionary<string, string> { { "name", "FollowedUser" }, { "description", "User followed another builder" }, { "iconCssClass", "fas fa-user-plus" }, { "sequence", "7" }, { "objectGuid", "ae100001-0001-4000-8000-000000000007" } });


            // -------------------------------------------------
            // ActivityEvent — Activity feed events
            // -------------------------------------------------
            Database.Table activityEventTable = database.AddTable("ActivityEvent");
            activityEventTable.comment = "Individual activity feed events generated by user actions. Used to build the community activity feed and individual user activity histories.";
            activityEventTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            activityEventTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            activityEventTable.AddIdField(true, true);
            activityEventTable.AddMultiTenantSupport();

            activityEventTable.AddForeignKeyField(activityEventTypeTable, false).AddScriptComments("The type of activity event");
            activityEventTable.AddString250Field("title", false).AddScriptComments("Short display title for the event (e.g. 'Published Technic Crane MOC')");
            activityEventTable.AddTextField("description").AddScriptComments("Optional longer description or context for the event");
            activityEventTable.AddString100Field("relatedEntityType").AddScriptComments("Type name of the related entity (e.g. 'PublishedMoc', 'LegoSet', 'Achievement')");
            activityEventTable.AddLongField("relatedEntityId", true).AddScriptComments("ID of the related entity for deep linking (null if not applicable)");
            activityEventTable.AddDateTimeField("eventDate", false).AddScriptComments("Date/time the activity occurred");
            activityEventTable.AddBoolField("isPublic", false, true).AddScriptComments("Whether this event is visible on the public activity feed");

            activityEventTable.AddControlFields();

            #endregion


            #region Content Sharing and Gallery

            // -------------------------------------------------
            // PublishedMoc — A MOC published to the community gallery
            // -------------------------------------------------
            Database.Table publishedMocTable = database.AddTable("PublishedMoc");
            publishedMocTable.comment = "A MOC (My Own Creation) published to the community gallery. Links to the underlying project for parts list and 3D model data.";
            publishedMocTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            publishedMocTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            publishedMocTable.AddIdField();
            publishedMocTable.AddMultiTenantSupport();

            publishedMocTable.AddForeignKeyField(projectTable, false).AddScriptComments("The underlying project containing the model data");
            publishedMocTable.AddNameField(true, true).AddScriptComments("Public-facing title of the MOC");
            publishedMocTable.AddTextField("description").AddScriptComments("Rich description of the MOC, build story, or design notes");
            publishedMocTable.AddString250Field("thumbnailImagePath").AddScriptComments("Relative path to the primary thumbnail image");
            publishedMocTable.AddTextField("tags").AddScriptComments("Comma-separated tags for search and categorization (e.g. 'technic, crane, vehicle')");
            publishedMocTable.AddBoolField("isPublished", false, false).AddScriptComments("Whether this MOC is visible in the public gallery (draft vs published)");
            publishedMocTable.AddBoolField("isFeatured", false, false).AddScriptComments("Whether this MOC is featured / editor's pick (set by moderators)");
            publishedMocTable.AddDateTimeField("publishedDate", true).AddScriptComments("Date/time the MOC was first published");
            publishedMocTable.AddIntField("viewCount", false, 0).AddScriptComments("Number of times this MOC has been viewed");
            publishedMocTable.AddIntField("likeCount", false, 0).AddScriptComments("Cached like count for fast sorting and display");
            publishedMocTable.AddIntField("commentCount", false, 0).AddScriptComments("Cached comment count for fast display");
            publishedMocTable.AddIntField("favouriteCount", false, 0).AddScriptComments("Cached favourite/bookmark count for fast display");
            publishedMocTable.AddIntField("partCount", true).AddScriptComments("Cached total part count from the underlying project");
            publishedMocTable.AddBoolField("allowForking", false, true).AddScriptComments("Whether other users can fork (copy) this MOC as a starting point");

            publishedMocTable.AddVersionControl();
            publishedMocTable.AddControlFields();


            // -------------------------------------------------
            // PublishedMocImage — Gallery images for a published MOC
            // -------------------------------------------------
            Database.Table publishedMocImageTable = database.AddTable("PublishedMocImage");
            publishedMocImageTable.comment = "Additional gallery images for a published MOC. The thumbnail is on the PublishedMoc itself; these are supplementary views and renders.";
            publishedMocImageTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            publishedMocImageTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            publishedMocImageTable.AddIdField();
            publishedMocImageTable.AddMultiTenantSupport();

            publishedMocImageTable.AddForeignKeyField(publishedMocTable, false).AddScriptComments("The published MOC this image belongs to");
            publishedMocImageTable.AddString250Field("imagePath", false).AddScriptComments("Relative path to the image file");
            publishedMocImageTable.AddString250Field("caption").AddScriptComments("Optional caption describing the image or the angle shown");

            publishedMocImageTable.AddSequenceField();
            publishedMocImageTable.AddControlFields();


            // -------------------------------------------------
            // MocLike — Likes on published MOCs
            // -------------------------------------------------
            Database.Table mocLikeTable = database.AddTable("MocLike");
            mocLikeTable.comment = "User likes on published MOCs. One like per user per MOC.";
            mocLikeTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            mocLikeTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            mocLikeTable.AddIdField();

            mocLikeTable.AddForeignKeyField(publishedMocTable, false).AddScriptComments("The MOC being liked");
            mocLikeTable.AddGuidField("likerTenantGuid", false).AddScriptComments("Tenant GUID of the user who liked");
            mocLikeTable.AddDateTimeField("likedDate", false).AddScriptComments("Date/time the like was registered");

            mocLikeTable.AddControlFields();

            mocLikeTable.AddUniqueConstraint(new List<string>() { "publishedMocId", "likerTenantGuid" }, false);


            // -------------------------------------------------
            // MocComment — Comments on published MOCs
            // -------------------------------------------------
            Database.Table mocCommentTable = database.AddTable("MocComment");
            mocCommentTable.comment = "User comments on published MOCs. Supports threaded replies via self-referencing parent FK.";
            mocCommentTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            mocCommentTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            mocCommentTable.AddIdField();

            mocCommentTable.AddForeignKeyField(publishedMocTable, false).AddScriptComments("The MOC being commented on");
            mocCommentTable.AddGuidField("commenterTenantGuid", false).AddScriptComments("Tenant GUID of the user who posted the comment");
            mocCommentTable.AddTextField("commentText", false).AddScriptComments("The comment content");
            mocCommentTable.AddDateTimeField("postedDate", false).AddScriptComments("Date/time the comment was posted");
            mocCommentTable.AddForeignKeyField(mocCommentTable, true).AddScriptComments("Optional parent comment for threaded replies (null = top-level comment)");
            mocCommentTable.AddBoolField("isEdited", false, false).AddScriptComments("Whether this comment has been edited after posting");
            mocCommentTable.AddBoolField("isHidden", false, false).AddScriptComments("Whether this comment has been hidden by a moderator");

            mocCommentTable.AddControlFields();


            // -------------------------------------------------
            // MocFavourite — User's favourited/bookmarked MOCs
            // -------------------------------------------------
            Database.Table mocFavouriteTable = database.AddTable("MocFavourite");
            mocFavouriteTable.comment = "User's favourited (bookmarked) MOCs for quick access. Separate from likes — favourites are private bookmarks, likes are public endorsements.";
            mocFavouriteTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            mocFavouriteTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            mocFavouriteTable.AddIdField();

            mocFavouriteTable.AddForeignKeyField(publishedMocTable, false).AddScriptComments("The MOC being favourited");
            mocFavouriteTable.AddGuidField("userTenantGuid", false).AddScriptComments("Tenant GUID of the user who favourited");
            mocFavouriteTable.AddDateTimeField("favouritedDate", false).AddScriptComments("Date/time the favourite was added");

            mocFavouriteTable.AddControlFields();

            mocFavouriteTable.AddUniqueConstraint(new List<string>() { "publishedMocId", "userTenantGuid" }, false);


            // -------------------------------------------------
            // SharedInstruction — Published instruction manuals
            // -------------------------------------------------
            Database.Table sharedInstructionTable = database.AddTable("SharedInstruction");
            sharedInstructionTable.comment = "Published instruction manuals shared with the community. Can be BMC-native format (linked to BuildManual), uploaded PDF, or image-based.";
            sharedInstructionTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            sharedInstructionTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            sharedInstructionTable.AddIdField();
            sharedInstructionTable.AddMultiTenantSupport();

            sharedInstructionTable.AddForeignKeyField(buildManualTable, true).AddScriptComments("Optional link to a BMC-native BuildManual (null for uploaded PDF/image instructions)");
            sharedInstructionTable.AddForeignKeyField(publishedMocTable, true).AddScriptComments("Optional link to the published MOC these instructions are for");
            sharedInstructionTable.AddNameField(true, true).AddScriptComments("Public-facing title of the instruction document");
            sharedInstructionTable.AddTextField("description").AddScriptComments("Description of what these instructions cover");
            sharedInstructionTable.AddString50Field("formatType", false).AddScriptComments("Format of the instruction: BMCNative, PDF, ImageSet");
            sharedInstructionTable.AddString250Field("filePath").AddScriptComments("Relative path to the instruction file (PDF) or folder (image set). Null for BMC-native.");
            sharedInstructionTable.AddBoolField("isPublished", false, false).AddScriptComments("Whether these instructions are visible in the community");
            sharedInstructionTable.AddDateTimeField("publishedDate", true).AddScriptComments("Date/time the instructions were first published");
            sharedInstructionTable.AddIntField("downloadCount", false, 0).AddScriptComments("Number of times these instructions have been downloaded");
            sharedInstructionTable.AddIntField("pageCount", true).AddScriptComments("Total number of pages (for display purposes)");

            sharedInstructionTable.AddVersionControl();
            sharedInstructionTable.AddControlFields();

            #endregion


            #region Gamification and Achievements

            // -------------------------------------------------
            // AchievementCategory — Groups of achievements
            // -------------------------------------------------
            Database.Table achievementCategoryTable = database.AddTable("AchievementCategory");
            achievementCategoryTable.comment = "Groups of achievements for organization and display (e.g. Collection, Building, Social, Exploration).";
            achievementCategoryTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            achievementCategoryTable.AddIdField();
            achievementCategoryTable.AddNameAndDescriptionFields(true, true, false);

            achievementCategoryTable.AddString100Field("iconCssClass").AddScriptComments("CSS class for the category icon");

            achievementCategoryTable.AddSequenceField();
            achievementCategoryTable.AddControlFields();

            // Seed data — core achievement categories
            achievementCategoryTable.AddData(new Dictionary<string, string> { { "name", "Collection" }, { "description", "Achievements related to building and managing your parts collection" }, { "iconCssClass", "fas fa-cubes" }, { "sequence", "1" }, { "objectGuid", "ac100001-0001-4000-8000-000000000001" } });
            achievementCategoryTable.AddData(new Dictionary<string, string> { { "name", "Building" }, { "description", "Achievements related to creating and publishing MOCs" }, { "iconCssClass", "fas fa-hammer" }, { "sequence", "2" }, { "objectGuid", "ac100001-0001-4000-8000-000000000002" } });
            achievementCategoryTable.AddData(new Dictionary<string, string> { { "name", "Social" }, { "description", "Achievements related to community engagement and social interactions" }, { "iconCssClass", "fas fa-users" }, { "sequence", "3" }, { "objectGuid", "ac100001-0001-4000-8000-000000000003" } });
            achievementCategoryTable.AddData(new Dictionary<string, string> { { "name", "Exploration" }, { "description", "Achievements related to exploring the parts catalog and set database" }, { "iconCssClass", "fas fa-compass" }, { "sequence", "4" }, { "objectGuid", "ac100001-0001-4000-8000-000000000004" } });
            achievementCategoryTable.AddData(new Dictionary<string, string> { { "name", "Challenge" }, { "description", "Achievements earned by competing in build challenges" }, { "iconCssClass", "fas fa-medal" }, { "sequence", "5" }, { "objectGuid", "ac100001-0001-4000-8000-000000000005" } });


            // -------------------------------------------------
            // Achievement — Achievement definitions
            // -------------------------------------------------
            Database.Table achievementTable = database.AddTable("Achievement");
            achievementTable.comment = "Individual achievement definitions. Each achievement has criteria, point value, and rarity classification.";
            achievementTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            achievementTable.AddIdField();

            achievementTable.AddForeignKeyField(achievementCategoryTable, false).AddScriptComments("The category this achievement belongs to");
            achievementTable.AddNameAndDescriptionFields(true, true, false);
            achievementTable.AddString100Field("iconCssClass").AddScriptComments("CSS class for the achievement icon/badge");
            achievementTable.AddString250Field("iconImagePath").AddScriptComments("Optional path to a custom badge image (overrides CSS icon)");
            achievementTable.AddTextField("criteria").AddScriptComments("Human-readable description of how to earn this achievement");
            achievementTable.AddString250Field("criteriaCode").AddScriptComments("Machine-readable criteria code for automatic detection (e.g. 'parts_owned >= 10000')");
            achievementTable.AddIntField("pointValue", false, 10).AddScriptComments("Point value when earned — contributes to the user's total achievement score");
            achievementTable.AddString50Field("rarity", false).AddScriptComments("Rarity classification: Common, Uncommon, Rare, Epic, Legendary");
            achievementTable.AddBoolField("isActive", false, true).AddScriptComments("Whether this achievement can currently be earned");

            achievementTable.AddSequenceField();
            achievementTable.AddControlFields();

            // Seed data — starter achievements
            achievementTable.AddData(new Dictionary<string, string> { { "name", "First Brick" }, { "description", "Added your first part to your collection" }, { "iconCssClass", "fas fa-cube" }, { "criteria", "Add at least 1 part to any collection" }, { "criteriaCode", "parts_owned >= 1" }, { "pointValue", "5" }, { "rarity", "Common" }, { "isActive", "true" }, { "sequence", "1" }, { "link:AchievementCategory:name:achievementCategoryId", "Collection" }, { "objectGuid", "a1100001-0001-4000-8000-000000000001" } });
            achievementTable.AddData(new Dictionary<string, string> { { "name", "Brick Enthusiast" }, { "description", "Own 1,000 parts across all collections" }, { "iconCssClass", "fas fa-cubes" }, { "criteria", "Total parts owned reaches 1,000" }, { "criteriaCode", "parts_owned >= 1000" }, { "pointValue", "25" }, { "rarity", "Uncommon" }, { "isActive", "true" }, { "sequence", "2" }, { "link:AchievementCategory:name:achievementCategoryId", "Collection" }, { "objectGuid", "a1100001-0001-4000-8000-000000000002" } });
            achievementTable.AddData(new Dictionary<string, string> { { "name", "Brick Master" }, { "description", "Own 10,000 parts across all collections" }, { "iconCssClass", "fas fa-warehouse" }, { "criteria", "Total parts owned reaches 10,000" }, { "criteriaCode", "parts_owned >= 10000" }, { "pointValue", "100" }, { "rarity", "Rare" }, { "isActive", "true" }, { "sequence", "3" }, { "link:AchievementCategory:name:achievementCategoryId", "Collection" }, { "objectGuid", "a1100001-0001-4000-8000-000000000003" } });
            achievementTable.AddData(new Dictionary<string, string> { { "name", "First Creation" }, { "description", "Published your first MOC to the gallery" }, { "iconCssClass", "fas fa-rocket" }, { "criteria", "Publish at least 1 MOC to the gallery" }, { "criteriaCode", "mocs_published >= 1" }, { "pointValue", "15" }, { "rarity", "Common" }, { "isActive", "true" }, { "sequence", "10" }, { "link:AchievementCategory:name:achievementCategoryId", "Building" }, { "objectGuid", "a1100001-0001-4000-8000-000000000010" } });
            achievementTable.AddData(new Dictionary<string, string> { { "name", "Community Builder" }, { "description", "Gained 10 followers" }, { "iconCssClass", "fas fa-user-friends" }, { "criteria", "Reach 10 followers on your profile" }, { "criteriaCode", "followers >= 10" }, { "pointValue", "20" }, { "rarity", "Uncommon" }, { "isActive", "true" }, { "sequence", "20" }, { "link:AchievementCategory:name:achievementCategoryId", "Social" }, { "objectGuid", "a1100001-0001-4000-8000-000000000020" } });


            // -------------------------------------------------
            // UserAchievement — Achievements earned by users
            // -------------------------------------------------
            Database.Table userAchievementTable = database.AddTable("UserAchievement");
            userAchievementTable.comment = "Records of achievements earned by users. Created when a user meets an achievement's criteria.";
            userAchievementTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            userAchievementTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            userAchievementTable.AddIdField();
            userAchievementTable.AddMultiTenantSupport();

            userAchievementTable.AddForeignKeyField(achievementTable, false).AddScriptComments("The achievement earned");
            userAchievementTable.AddDateTimeField("earnedDate", false).AddScriptComments("Date/time the achievement was earned");
            userAchievementTable.AddBoolField("isDisplayed", false, true).AddScriptComments("Whether this achievement is displayed on the user's public profile showcase");

            userAchievementTable.AddControlFields();

            userAchievementTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "achievementId" }, false);


            // -------------------------------------------------
            // UserBadge — Display badges for user profiles
            // -------------------------------------------------
            Database.Table userBadgeTable = database.AddTable("UserBadge");
            userBadgeTable.comment = "Special display badges that can be awarded to users by moderators or earned through special events. Displayed prominently on user profiles.";
            userBadgeTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            userBadgeTable.AddIdField();
            userBadgeTable.AddNameAndDescriptionFields(true, true, false);

            userBadgeTable.AddString100Field("iconCssClass").AddScriptComments("CSS class for the badge icon");
            userBadgeTable.AddString250Field("iconImagePath").AddScriptComments("Optional path to a custom badge image");
            userBadgeTable.AddHTMLColorField("badgeColor", true).AddScriptComments("Optional accent colour for the badge display");
            userBadgeTable.AddBoolField("isAutomatic", false, false).AddScriptComments("Whether this badge is automatically awarded (vs. manually by moderators)");
            userBadgeTable.AddString250Field("automaticCriteriaCode").AddScriptComments("Machine-readable criteria for automatic badges (null for manual badges)");

            userBadgeTable.AddSequenceField();
            userBadgeTable.AddControlFields();

            // Seed data — starter badges
            userBadgeTable.AddData(new Dictionary<string, string> { { "name", "Early Adopter" }, { "description", "Joined the BMC community during the early access period" }, { "iconCssClass", "fas fa-star" }, { "isAutomatic", "false" }, { "sequence", "1" }, { "objectGuid", "ab100001-0001-4000-8000-000000000001" } });
            userBadgeTable.AddData(new Dictionary<string, string> { { "name", "Verified Builder" }, { "description", "Identity verified by the BMC team" }, { "iconCssClass", "fas fa-check-circle" }, { "isAutomatic", "false" }, { "sequence", "2" }, { "objectGuid", "ab100001-0001-4000-8000-000000000002" } });
            userBadgeTable.AddData(new Dictionary<string, string> { { "name", "Top Contributor" }, { "description", "One of the most active community contributors this month" }, { "iconCssClass", "fas fa-crown" }, { "isAutomatic", "false" }, { "sequence", "3" }, { "objectGuid", "ab100001-0001-4000-8000-000000000003" } });
            userBadgeTable.AddData(new Dictionary<string, string> { { "name", "Challenge Winner" }, { "description", "Won a community build challenge" }, { "iconCssClass", "fas fa-award" }, { "isAutomatic", "false" }, { "sequence", "4" }, { "objectGuid", "ab100001-0001-4000-8000-000000000004" } });
            userBadgeTable.AddData(new Dictionary<string, string> { { "name", "Moderator" }, { "description", "Community moderator trusted to help maintain quality" }, { "iconCssClass", "fas fa-shield-alt" }, { "isAutomatic", "false" }, { "sequence", "5" }, { "objectGuid", "ab100001-0001-4000-8000-000000000005" } });


            // -------------------------------------------------
            // UserBadgeAssignment — Badges awarded to users
            // -------------------------------------------------
            Database.Table userBadgeAssignmentTable = database.AddTable("UserBadgeAssignment");
            userBadgeAssignmentTable.comment = "Maps badges to users. A badge can be awarded multiple times conceptually, but one unique assignment per user per badge.";
            userBadgeAssignmentTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            userBadgeAssignmentTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            userBadgeAssignmentTable.AddIdField();
            userBadgeAssignmentTable.AddMultiTenantSupport();

            userBadgeAssignmentTable.AddForeignKeyField(userBadgeTable, false).AddScriptComments("The badge awarded");
            userBadgeAssignmentTable.AddDateTimeField("awardedDate", false).AddScriptComments("Date/time the badge was awarded");
            userBadgeAssignmentTable.AddGuidField("awardedByTenantGuid", true).AddScriptComments("Tenant GUID of the moderator who awarded the badge (null for automatic badges)");
            userBadgeAssignmentTable.AddTextField("reason").AddScriptComments("Optional reason or context for awarding the badge");
            userBadgeAssignmentTable.AddBoolField("isDisplayed", false, true).AddScriptComments("Whether this badge is displayed on the user's profile");

            userBadgeAssignmentTable.AddControlFields();

            userBadgeAssignmentTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "userBadgeId" }, false);


            // -------------------------------------------------
            // BuildChallenge — Community build challenges
            // -------------------------------------------------
            Database.Table buildChallengeTable = database.AddTable("BuildChallenge");
            buildChallengeTable.comment = "Community build challenges with themes, rules, and time windows. Created by moderators or admins.";
            buildChallengeTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_MODERATOR_PERMISSION_LEVEL);
            buildChallengeTable.customWriteAccessRole = BMC_MODERATOR_CUSTOM_ROLE_NAME;
            buildChallengeTable.AddIdField();

            buildChallengeTable.AddNameField(true, true).AddScriptComments("Title of the challenge (e.g. 'Under 100 Parts Technic Vehicle')");
            buildChallengeTable.AddTextField("description").AddScriptComments("Full description of the challenge theme and goals");
            buildChallengeTable.AddTextField("rules").AddScriptComments("Detailed rules and constraints for entries");
            buildChallengeTable.AddString250Field("thumbnailImagePath").AddScriptComments("Promotional image for the challenge");
            buildChallengeTable.AddDateTimeField("startDate", false).AddScriptComments("When submissions open");
            buildChallengeTable.AddDateTimeField("endDate", false).AddScriptComments("When submissions close");
            buildChallengeTable.AddDateTimeField("votingEndDate", true).AddScriptComments("When community voting closes (null if no voting period)");
            buildChallengeTable.AddBoolField("isActive", false, true).AddScriptComments("Whether this challenge is currently active and accepting entries");
            buildChallengeTable.AddBoolField("isFeatured", false, false).AddScriptComments("Whether this challenge should be prominently displayed on the landing page");
            buildChallengeTable.AddIntField("entryCount", false, 0).AddScriptComments("Cached count of submitted entries");
            buildChallengeTable.AddIntField("maxPartsLimit", true).AddScriptComments("Optional maximum part count constraint for entries (null = no limit)");

            buildChallengeTable.AddVersionControl();
            buildChallengeTable.AddControlFields();


            // -------------------------------------------------
            // BuildChallengeEntry — User entries into a challenge
            // -------------------------------------------------
            Database.Table buildChallengeEntryTable = database.AddTable("BuildChallengeEntry");
            buildChallengeEntryTable.comment = "User-submitted entries into a build challenge. Links to a published MOC.";
            buildChallengeEntryTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            buildChallengeEntryTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            buildChallengeEntryTable.AddIdField();
            buildChallengeEntryTable.AddMultiTenantSupport();

            buildChallengeEntryTable.AddForeignKeyField(buildChallengeTable, false).AddScriptComments("The challenge being entered");
            buildChallengeEntryTable.AddForeignKeyField(publishedMocTable, false).AddScriptComments("The published MOC submitted as an entry");
            buildChallengeEntryTable.AddDateTimeField("submittedDate", false).AddScriptComments("Date/time the entry was submitted");
            buildChallengeEntryTable.AddTextField("entryNotes").AddScriptComments("Optional notes from the builder about their entry");
            buildChallengeEntryTable.AddIntField("voteCount", false, 0).AddScriptComments("Cached community vote count");
            buildChallengeEntryTable.AddBoolField("isWinner", false, false).AddScriptComments("Whether this entry was selected as a winner");
            buildChallengeEntryTable.AddBoolField("isDisqualified", false, false).AddScriptComments("Whether this entry was disqualified by moderators");

            buildChallengeEntryTable.AddControlFields();

            buildChallengeEntryTable.AddUniqueConstraint(new List<string>() { "tenantGuid", "buildChallengeId" }, false);

            #endregion


            #region Moderation and Admin

            // -------------------------------------------------
            // ContentReportReason — Lookup of report reasons
            // -------------------------------------------------
            Database.Table contentReportReasonTable = database.AddTable("ContentReportReason");
            contentReportReasonTable.comment = "Lookup table of reasons a user can report community content (Spam, Inappropriate, Copyright, etc.).";
            contentReportReasonTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            contentReportReasonTable.AddIdField();
            contentReportReasonTable.AddNameAndDescriptionFields(true, true, false);

            contentReportReasonTable.AddSequenceField();
            contentReportReasonTable.AddControlFields();

            // Seed data — standard report reasons
            contentReportReasonTable.AddData(new Dictionary<string, string> { { "name", "Spam" }, { "description", "Content is spam, advertising, or promotional" }, { "sequence", "1" }, { "objectGuid", "c4100001-0001-4000-8000-000000000001" } });
            contentReportReasonTable.AddData(new Dictionary<string, string> { { "name", "Inappropriate" }, { "description", "Content is offensive, vulgar, or inappropriate" }, { "sequence", "2" }, { "objectGuid", "c4100001-0001-4000-8000-000000000002" } });
            contentReportReasonTable.AddData(new Dictionary<string, string> { { "name", "Copyright" }, { "description", "Content violates copyright or intellectual property" }, { "sequence", "3" }, { "objectGuid", "c4100001-0001-4000-8000-000000000003" } });
            contentReportReasonTable.AddData(new Dictionary<string, string> { { "name", "Harassment" }, { "description", "Content constitutes harassment or bullying" }, { "sequence", "4" }, { "objectGuid", "c4100001-0001-4000-8000-000000000004" } });
            contentReportReasonTable.AddData(new Dictionary<string, string> { { "name", "Misinformation" }, { "description", "Content contains misleading or false information" }, { "sequence", "5" }, { "objectGuid", "c4100001-0001-4000-8000-000000000005" } });
            contentReportReasonTable.AddData(new Dictionary<string, string> { { "name", "Other" }, { "description", "Other reason not covered above" }, { "sequence", "99" }, { "objectGuid", "c4100001-0001-4000-8000-000000000099" } });


            // -------------------------------------------------
            // ContentReport — User-submitted content reports
            // -------------------------------------------------
            Database.Table contentReportTable = database.AddTable("ContentReport");
            contentReportTable.comment = "User-submitted reports of problematic community content. Reviewed by moderators via the BMC Admin project.";
            contentReportTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            contentReportTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            contentReportTable.AddIdField();

            contentReportTable.AddForeignKeyField(contentReportReasonTable, false).AddScriptComments("The reason for the report");
            contentReportTable.AddGuidField("reporterTenantGuid", false).AddScriptComments("Tenant GUID of the user submitting the report");
            contentReportTable.AddString100Field("reportedEntityType", false).AddScriptComments("Type of the reported content (e.g. 'PublishedMoc', 'MocComment', 'UserProfile')");
            contentReportTable.AddLongField("reportedEntityId", false).AddScriptComments("ID of the reported entity");
            contentReportTable.AddTextField("description").AddScriptComments("Additional details provided by the reporter");
            contentReportTable.AddString50Field("status", false).AddScriptComments("Report status: Pending, UnderReview, Dismissed, ActionTaken");
            contentReportTable.AddDateTimeField("reportedDate", false).AddScriptComments("Date/time the report was submitted");
            contentReportTable.AddDateTimeField("reviewedDate", true).AddScriptComments("Date/time a moderator reviewed the report (null if pending)");
            contentReportTable.AddGuidField("reviewerTenantGuid", true).AddScriptComments("Tenant GUID of the moderator who reviewed (null if pending)");
            contentReportTable.AddTextField("reviewNotes").AddScriptComments("Moderator notes on the review decision");

            contentReportTable.AddControlFields();


            // -------------------------------------------------
            // ModerationAction — Log of moderator actions
            // -------------------------------------------------
            Database.Table moderationActionTable = database.AddTable("ModerationAction");
            moderationActionTable.comment = "Audit log of actions taken by moderators. Immutable record for accountability.";
            moderationActionTable.SetMinimumPermissionLevels(BMC_MODERATOR_PERMISSION_LEVEL, BMC_MODERATOR_PERMISSION_LEVEL);
            moderationActionTable.customWriteAccessRole = BMC_MODERATOR_CUSTOM_ROLE_NAME;
            moderationActionTable.AddIdField(true, true);

            moderationActionTable.AddGuidField("moderatorTenantGuid", false).AddScriptComments("Tenant GUID of the moderator who took the action");
            moderationActionTable.AddString100Field("actionType", false).AddScriptComments("Type of action: Warning, ContentRemoved, ContentHidden, UserSuspended, UserBanned, BadgeAwarded");
            moderationActionTable.AddGuidField("targetTenantGuid", true).AddScriptComments("Tenant GUID of the user the action was taken against (null for content-only actions)");
            moderationActionTable.AddString100Field("targetEntityType").AddScriptComments("Type of the target entity (e.g. 'PublishedMoc', 'MocComment', 'UserProfile')");
            moderationActionTable.AddLongField("targetEntityId", true).AddScriptComments("ID of the target entity (null for user-level actions)");
            moderationActionTable.AddTextField("reason").AddScriptComments("Reason for the moderation action");
            moderationActionTable.AddDateTimeField("actionDate", false).AddScriptComments("Date/time the action was taken");
            moderationActionTable.AddForeignKeyField(contentReportTable, true).AddScriptComments("Optional link to the content report that triggered this action");

            moderationActionTable.AddControlFields();


            // -------------------------------------------------
            // PlatformAnnouncement — Admin announcements for landing page
            // -------------------------------------------------
            Database.Table platformAnnouncementTable = database.AddTable("PlatformAnnouncement");
            platformAnnouncementTable.comment = "Admin-created announcements displayed on the public landing page and/or dashboard. Time-windowed with priority ordering.";
            platformAnnouncementTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_MODERATOR_PERMISSION_LEVEL);
            platformAnnouncementTable.customWriteAccessRole = BMC_MODERATOR_CUSTOM_ROLE_NAME;
            platformAnnouncementTable.AddIdField();

            platformAnnouncementTable.AddNameField(true, true).AddScriptComments("Announcement headline/title");
            platformAnnouncementTable.AddTextField("body").AddScriptComments("Full announcement content (supports markdown or HTML)");
            platformAnnouncementTable.AddString50Field("announcementType").AddScriptComments("Type for styling: Info, Warning, Celebration, Maintenance");
            platformAnnouncementTable.AddDateTimeField("startDate", false).AddScriptComments("When the announcement becomes visible");
            platformAnnouncementTable.AddDateTimeField("endDate", true).AddScriptComments("When the announcement expires (null = no expiry)");
            platformAnnouncementTable.AddBoolField("isActive", false, true).AddScriptComments("Whether the announcement is currently active");
            platformAnnouncementTable.AddIntField("priority", false, 0).AddScriptComments("Display priority (higher = more prominent)");
            platformAnnouncementTable.AddBoolField("showOnLandingPage", false, true).AddScriptComments("Whether to show on the public landing page");
            platformAnnouncementTable.AddBoolField("showOnDashboard", false, true).AddScriptComments("Whether to show on the authenticated user dashboard");

            platformAnnouncementTable.AddControlFields();

            #endregion


            #region Public API Management

            // -------------------------------------------------
            // ApiKey — API keys for public API consumers
            // -------------------------------------------------
            Database.Table apiKeyTable = database.AddTable("ApiKey");
            apiKeyTable.comment = "API keys issued to users or external integrators for accessing the BMC Public API. Keys are stored as hashes for security.";
            apiKeyTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_COMMUNITY_WRITER_PERMISSION_LEVEL);
            apiKeyTable.customWriteAccessRole = BMC_COMMUNITY_WRITER_CUSTOM_ROLE_NAME;
            apiKeyTable.AddIdField();
            apiKeyTable.AddMultiTenantSupport();

            apiKeyTable.AddString250Field("keyHash", false).AddScriptComments("SHA-256 hash of the API key (the plain key is shown once at creation, then discarded)");
            apiKeyTable.AddString100Field("keyPrefix", false).AddScriptComments("First 8 characters of the key for identification without exposing the full key");
            apiKeyTable.AddNameField(true, true).AddScriptComments("User-defined name for the key (e.g. 'My BrickLink Integration')");
            apiKeyTable.AddTextField("description").AddScriptComments("Optional description of what this key is used for");
            apiKeyTable.AddBoolField("isActive", false, true).AddScriptComments("Whether this key is active and can authenticate requests");
            apiKeyTable.AddDateTimeField("createdDate", false).AddScriptComments("Date/time the key was created");
            apiKeyTable.AddDateTimeField("lastUsedDate", true).AddScriptComments("Date/time the key was last used to make a request");
            apiKeyTable.AddDateTimeField("expiresDate", true).AddScriptComments("Optional expiry date (null = no expiry)");
            apiKeyTable.AddIntField("rateLimitPerHour", false, 1000).AddScriptComments("Maximum API requests allowed per hour with this key");

            apiKeyTable.AddControlFields();


            // -------------------------------------------------
            // ApiRequestLog — Audit log of API requests
            // -------------------------------------------------
            Database.Table apiRequestLogTable = database.AddTable("ApiRequestLog");
            apiRequestLogTable.comment = "Audit log of requests made through the BMC Public API. Used for rate limiting, usage analytics, and abuse detection.";
            apiRequestLogTable.SetMinimumPermissionLevels(BMC_MODERATOR_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            apiRequestLogTable.AddIdField(true, true);

            apiRequestLogTable.AddForeignKeyField(apiKeyTable, false).AddScriptComments("The API key used for this request");
            apiRequestLogTable.AddString250Field("endpoint", false).AddScriptComments("The API endpoint that was called (e.g. '/api/v1/parts/3001')");
            apiRequestLogTable.AddString10Field("httpMethod", false).AddScriptComments("HTTP method (GET, POST, PUT, DELETE)");
            apiRequestLogTable.AddIntField("responseStatus", false).AddScriptComments("HTTP response status code (200, 401, 429, etc.)");
            apiRequestLogTable.AddDateTimeField("requestDate", false).AddScriptComments("Date/time of the request");
            apiRequestLogTable.AddIntField("durationMs", true).AddScriptComments("Request processing duration in milliseconds");
            apiRequestLogTable.AddString100Field("clientIpAddress").AddScriptComments("IP address of the client making the request");

            apiRequestLogTable.AddControlFields();

            #endregion


            #region Registration

            // -------------------------------------------------
            // PendingRegistration — Tracks the two-step registration process
            // -------------------------------------------------
            Database.Table pendingRegistrationTable = database.AddTable("PendingRegistration");
            pendingRegistrationTable.comment = "Tracks self-service user registrations through the two-step email verification process. Stores pending registrations until email verification is completed, then provisions the SecurityUser, SecurityTenant, and UserProfile. Designed for auditing and reporting on the registration funnel.";
            pendingRegistrationTable.SetMinimumPermissionLevels(BMC_MODERATOR_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            pendingRegistrationTable.AddIdField();

            pendingRegistrationTable.AddString250Field("accountName", false).AddScriptComments("The requested username for the new account").CreateIndex();
            pendingRegistrationTable.AddString100Field("emailAddress", false).AddScriptComments("The email address to verify").CreateIndex();
            pendingRegistrationTable.AddString250Field("displayName", true).AddScriptComments("Optional display name for the profile (defaults to accountName if not provided)");
            pendingRegistrationTable.AddString250Field("passwordHash", false).AddScriptComments("Pre-hashed password stored during the pending period");

            pendingRegistrationTable.AddString50Field("verificationCode", false).AddScriptComments("The code or token sent to the user for verification (email, SMS, OTP)").CreateIndex();
            pendingRegistrationTable.AddDateTimeField("codeExpiresAt", false).AddScriptComments("When the verification code expires (default 15 minutes from creation)").CreateIndex();
            pendingRegistrationTable.AddIntField("verificationAttempts", false, 0).AddScriptComments("Number of times the user has attempted to enter the verification code");

            pendingRegistrationTable.AddString50Field("status", false).AddScriptComments("Registration status: Pending, Verified, Provisioned, Expired, Failed").CreateIndex();
            pendingRegistrationTable.AddDateTimeField("createdAt", false).AddScriptComments("When the registration was initiated").CreateIndex();
            pendingRegistrationTable.AddDateTimeField("verifiedAt", true).AddScriptComments("When the verification code was successfully validated");
            pendingRegistrationTable.AddDateTimeField("provisionedAt", true).AddScriptComments("When the SecurityUser and SecurityTenant were created");

            pendingRegistrationTable.AddString100Field("ipAddress", true).AddScriptComments("Client IP address for security auditing");
            pendingRegistrationTable.AddString500Field("userAgent", true).AddScriptComments("Client user agent for security auditing");
            pendingRegistrationTable.AddString50Field("verificationChannel", true).AddScriptComments("Channel used for verification: Email, SMS, OTP (default Email)");
            pendingRegistrationTable.AddString1000Field("failureReason", true).AddScriptComments("Reason for failure if status is Failed");
            pendingRegistrationTable.AddIntField("provisionedSecurityUserId", true).AddScriptComments("The SecurityUser.id created on successful provisioning, for cross-referencing");

            pendingRegistrationTable.AddControlFields();

            // Index for the common lookup: find pending registrations that haven't expired
            pendingRegistrationTable.CreateIndexForFields(new List<string>() { "status", "codeExpiresAt", "active", "deleted" });

            #endregion
        }
    }
}
