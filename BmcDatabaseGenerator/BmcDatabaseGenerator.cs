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
        private const int BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL = 255;      // Foundation / system admin only

        // ── Custom role names ──
        private const string BMC_CATALOG_WRITER_CUSTOM_ROLE_NAME = "BMC Catalog Writer";
        private const string BMC_COLLECTION_WRITER_CUSTOM_ROLE_NAME = "BMC Collection Writer";
        private const string BMC_INSTRUCTION_WRITER_CUSTOM_ROLE_NAME = "BMC Instruction Writer";


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


            #region Master Data - Part Categories, Connector Types, Colours

            // -------------------------------------------------
            // BrickCategory — Master list of part families
            // -------------------------------------------------
            Database.Table brickCategoryTable = database.AddTable("BrickCategory");
            brickCategoryTable.comment = "Master list of part categories (Beam, Plate, Gear, Axle, Motor, Pneumatic, etc.)";
            brickCategoryTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            brickCategoryTable.AddIdField();
            brickCategoryTable.AddNameAndDescriptionFields(true, true, false);
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

            brickColourTable.AddIntField("ldrawColourCode", false).AddScriptComments("LDraw standard colour code number");
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

            brickColourTable.AddUniqueConstraint(new List<string>() { "ldrawColourCode" }, false);

            // Seed common solid colours (edge colours, finish FK, and LEGO IDs from LDConfig.ldr)
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Black" }, { "ldrawColourCode", "0" }, { "hexRgb", "#1B2A34" }, { "hexEdgeColour", "#808080" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "26" }, { "sequence", "1" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000001" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Blue" }, { "ldrawColourCode", "1" }, { "hexRgb", "#1E5AA8" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "23" }, { "sequence", "2" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000002" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Green" }, { "ldrawColourCode", "2" }, { "hexRgb", "#00852B" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "28" }, { "sequence", "3" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000003" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Red" }, { "ldrawColourCode", "4" }, { "hexRgb", "#B40000" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "21" }, { "sequence", "4" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000004" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Yellow" }, { "ldrawColourCode", "14" }, { "hexRgb", "#FAC80A" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "24" }, { "sequence", "5" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000005" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "White" }, { "ldrawColourCode", "15" }, { "hexRgb", "#F4F4F4" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "1" }, { "sequence", "6" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000006" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Light Bluish Grey" }, { "ldrawColourCode", "71" }, { "hexRgb", "#969696" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "194" }, { "sequence", "7" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000007" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Dark Bluish Grey" }, { "ldrawColourCode", "72" }, { "hexRgb", "#646464" }, { "hexEdgeColour", "#333333" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "legoColourId", "199" }, { "sequence", "8" }, { "link:ColourFinish:name:colourFinishId", "Solid" }, { "objectGuid", "c0100001-0001-4000-8000-000000000008" } });

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

            brickPartTable.AddString100Field("ldrawPartId", false).AddScriptComments("LDraw part ID (e.g. 3001, 32523) — the canonical identifier in the LDraw parts library");
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

            // Geometry reference
            brickPartTable.AddString250Field("geometryFilePath").AddScriptComments("Relative path to the LDraw .dat geometry file");

            // Technic-specific properties
            brickPartTable.AddIntField("toothCount", true).AddScriptComments("For gears: number of teeth. Null for non-gear parts.");
            brickPartTable.AddSingleField("gearRatio", true).AddScriptComments("For gears: effective gear ratio relative to a base gear. Null for non-gear parts.");

            brickPartTable.AddVersionControl();
            brickPartTable.AddControlFields();

            brickPartTable.AddUniqueConstraint(new List<string>() { "ldrawPartId" }, false);


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

            // Source side of the connection
            brickConnectionTable.AddLongField("sourcePlacedBrickId").AddScriptComments("FK to the source PlacedBrick");
            brickConnectionTable.AddLongField("sourceConnectorId").AddScriptComments("FK to the BrickPartConnector on the source brick");

            // Target side of the connection
            brickConnectionTable.AddLongField("targetPlacedBrickId").AddScriptComments("FK to the target PlacedBrick");
            brickConnectionTable.AddLongField("targetConnectorId").AddScriptComments("FK to the BrickPartConnector on the target brick");

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

            legoThemeTable.AddSequenceField();
            legoThemeTable.AddControlFields();


            // -------------------------------------------------
            // LegoSet — Official LEGO set definitions
            // -------------------------------------------------
            Database.Table legoSetTable = database.AddTable("LegoSet");
            legoSetTable.comment = "Official LEGO set definitions. Each row represents a distinct set release (e.g. 42131-1 Liebherr Crawler Crane).";
            legoSetTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            legoSetTable.AddIdField();
            legoSetTable.AddNameField(true, true);

            legoSetTable.AddString100Field("setNumber", false).AddScriptComments("Official set number including variant suffix (e.g. '42131-1', '10302-1')");
            legoSetTable.AddIntField("year", false).AddScriptComments("Release year of the set");
            legoSetTable.AddIntField("partCount", false).AddScriptComments("Total number of parts in the set (as listed by LEGO)");
            legoSetTable.AddForeignKeyField(legoThemeTable, true).AddScriptComments("The theme this set belongs to (null if theme not yet categorized)");
            legoSetTable.AddString250Field("imageUrl").AddScriptComments("URL to the set's official box art or primary image");
            legoSetTable.AddString250Field("brickLinkUrl").AddScriptComments("URL to the set's BrickLink catalogue page");
            legoSetTable.AddString250Field("rebrickableUrl").AddScriptComments("URL to the set's Rebrickable page");

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
        }
    }
}
