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
        private const int BMC_CATALOG_WRITER_PERMISSION_LEVEL = 50;           // Admin: parts catalog, colours, connectors
        private const int BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL = 255;      // Foundation / system admin only

        // ── Custom role names ──
        private const string BMC_CATALOG_WRITER_CUSTOM_ROLE_NAME = "BMC Catalog Writer";


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

            // Seed data - core part categories
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Plate" }, { "description", "Standard plates of various sizes" }, { "sequence", "1" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000001" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Brick" }, { "description", "Standard bricks of various sizes" }, { "sequence", "2" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000002" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Beam" }, { "description", "Technic beams and liftarms" }, { "sequence", "10" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000010" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Pin" }, { "description", "Technic pins and connectors" }, { "sequence", "11" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000011" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Axle" }, { "description", "Technic axles of various lengths" }, { "sequence", "12" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000012" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Gear" }, { "description", "Technic gears of various tooth counts" }, { "sequence", "13" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000013" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Motor" }, { "description", "Powered motors (Power Functions, Powered Up)" }, { "sequence", "14" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000014" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Pneumatic" }, { "description", "Pneumatic cylinders, pumps, and tubing" }, { "sequence", "15" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000015" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Differential" }, { "description", "Differential gear assemblies" }, { "sequence", "16" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000016" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Panel" }, { "description", "Panels, fairings, and body pieces" }, { "sequence", "20" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000020" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Wheel" }, { "description", "Wheels, tyres, and rims" }, { "sequence", "21" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000021" } });
            brickCategoryTable.AddData(new Dictionary<string, string> { { "name", "Decorative" }, { "description", "Decorative, printed, and sticker parts" }, { "sequence", "30" }, { "objectGuid", "b1c10001-0001-4000-8000-000000000030" } });


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
            // BrickColour — Colour definitions compatible with LDraw standard
            // -------------------------------------------------
            Database.Table brickColourTable = database.AddTable("BrickColour");
            brickColourTable.comment = "Colour definitions for brick parts. Compatible with the LDraw colour standard.";
            brickColourTable.SetMinimumPermissionLevels(BMC_READER_PERMISSION_LEVEL, BMC_SUPER_ADMIN_WRITER_PERMISSION_LEVEL);
            brickColourTable.AddIdField();
            brickColourTable.AddNameField(true, true);

            brickColourTable.AddIntField("ldrawColourCode", false).AddScriptComments("LDraw standard colour code number");
            brickColourTable.AddString10Field("hexRgb").AddScriptComments("Hex RGB colour value (e.g. #FF0000)");
            brickColourTable.AddIntField("alpha").AddScriptComments("Alpha transparency value (0-255, 255 = fully opaque)");
            brickColourTable.AddBoolField("isTransparent", false, false).AddScriptComments("Whether this colour is transparent");
            brickColourTable.AddBoolField("isMetallic", false, false).AddScriptComments("Whether this colour has a metallic finish");

            brickColourTable.AddSequenceField();
            brickColourTable.AddControlFields();

            brickColourTable.AddUniqueConstraint(new List<string>() { "ldrawColourCode" }, false);

            // Seed a handful of common colours
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Black" }, { "ldrawColourCode", "0" }, { "hexRgb", "#1B2A34" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "sequence", "1" }, { "objectGuid", "c0100001-0001-4000-8000-000000000001" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Blue" }, { "ldrawColourCode", "1" }, { "hexRgb", "#1E5AA8" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "sequence", "2" }, { "objectGuid", "c0100001-0001-4000-8000-000000000002" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Green" }, { "ldrawColourCode", "2" }, { "hexRgb", "#00852B" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "sequence", "3" }, { "objectGuid", "c0100001-0001-4000-8000-000000000003" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Red" }, { "ldrawColourCode", "4" }, { "hexRgb", "#B40000" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "sequence", "4" }, { "objectGuid", "c0100001-0001-4000-8000-000000000004" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Yellow" }, { "ldrawColourCode", "14" }, { "hexRgb", "#FFD67F" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "sequence", "5" }, { "objectGuid", "c0100001-0001-4000-8000-000000000005" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "White" }, { "ldrawColourCode", "15" }, { "hexRgb", "#F4F4F4" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "sequence", "6" }, { "objectGuid", "c0100001-0001-4000-8000-000000000006" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Light Bluish Grey" }, { "ldrawColourCode", "71" }, { "hexRgb", "#A0A5A9" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "sequence", "7" }, { "objectGuid", "c0100001-0001-4000-8000-000000000007" } });
            brickColourTable.AddData(new Dictionary<string, string> { { "name", "Dark Bluish Grey" }, { "ldrawColourCode", "72" }, { "hexRgb", "#6C6E68" }, { "alpha", "255" }, { "isTransparent", "false" }, { "isMetallic", "false" }, { "sequence", "8" }, { "objectGuid", "c0100001-0001-4000-8000-000000000008" } });

            #endregion


            #region Parts Catalog

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
            brickPartTable.AddForeignKeyField(brickCategoryTable, true).AddScriptComments("The category this part belongs to");

            // Physical dimensions in LDraw units (1 LDU = 0.4mm)
            brickPartTable.AddSingleField("widthLdu", false).AddScriptComments("Part width in LDraw units");
            brickPartTable.AddSingleField("heightLdu", false).AddScriptComments("Part height in LDraw units");
            brickPartTable.AddSingleField("depthLdu", false).AddScriptComments("Part depth in LDraw units");

            // Physics properties
            brickPartTable.AddSingleField("massGrams", false).AddScriptComments("Part mass in grams (for physics simulation)");

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

            brickPartConnectorTable.AddForeignKeyField(brickPartTable, true).AddScriptComments("The part this connector belongs to");
            brickPartConnectorTable.AddForeignKeyField(connectorTypeTable, true).AddScriptComments("The type of connector (Stud, PinHole, AxleHole, etc.)");

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

            placedBrickTable.AddForeignKeyField(projectTable, true).AddScriptComments("The project this brick is placed in");
            placedBrickTable.AddForeignKeyField(brickPartTable, true).AddScriptComments("The part definition being placed");
            placedBrickTable.AddForeignKeyField(brickColourTable, true).AddScriptComments("The colour of this placed brick instance");

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

            brickConnectionTable.AddForeignKeyField(projectTable, true).AddScriptComments("The project this connection belongs to");

            // Source side of the connection
            brickConnectionTable.AddLongField("sourcePlacedBrickId").AddScriptComments("FK to the source PlacedBrick");
            brickConnectionTable.AddLongField("sourceConnectorId").AddScriptComments("FK to the BrickPartConnector on the source brick");

            // Target side of the connection
            brickConnectionTable.AddLongField("targetPlacedBrickId").AddScriptComments("FK to the target PlacedBrick");
            brickConnectionTable.AddLongField("targetConnectorId").AddScriptComments("FK to the BrickPartConnector on the target brick");

            brickConnectionTable.AddControlFields();

            #endregion
        }
    }
}
