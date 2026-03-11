-- ============================================================================
-- SubmodelInstance — Tracks placement of submodel assemblies in parent models.
-- A single Submodel can have multiple instances (e.g. left/right wheel assemblies).
-- Stores the LDraw type-1 reference line transform data.
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'SubmodelInstance' AND s.name = 'BMC')
BEGIN
    CREATE TABLE [BMC].[SubmodelInstance] (
        [id]                INT              IDENTITY(1,1) NOT NULL,
        [tenantGuid]        UNIQUEIDENTIFIER NOT NULL,
        [submodelId]        INT              NOT NULL,
        [positionX]         REAL             NOT NULL DEFAULT 0,
        [positionY]         REAL             NOT NULL DEFAULT 0,
        [positionZ]         REAL             NOT NULL DEFAULT 0,
        [rotationX]         REAL             NOT NULL DEFAULT 0,
        [rotationY]         REAL             NOT NULL DEFAULT 0,
        [rotationZ]         REAL             NOT NULL DEFAULT 0,
        [rotationW]         REAL             NOT NULL DEFAULT 1,
        [colourCode]        INT              NOT NULL DEFAULT 16,
        [buildStepNumber]   INT              NOT NULL DEFAULT 1,
        [objectGuid]        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [active]            BIT              NOT NULL DEFAULT 1,
        [deleted]           BIT              NOT NULL DEFAULT 0,

        CONSTRAINT [PK_SubmodelInstance] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_SubmodelInstance_Submodel] FOREIGN KEY ([submodelId]) REFERENCES [BMC].[Submodel]([id]),
        CONSTRAINT [UQ_SubmodelInstance_objectGuid] UNIQUE ([objectGuid])
    );

    CREATE NONCLUSTERED INDEX [I_SubmodelInstance_tenantGuid]
        ON [BMC].[SubmodelInstance] ([tenantGuid]);

    CREATE NONCLUSTERED INDEX [I_SubmodelInstance_tenantGuid_active]
        ON [BMC].[SubmodelInstance] ([tenantGuid], [active]);

    CREATE NONCLUSTERED INDEX [I_SubmodelInstance_tenantGuid_submodelId]
        ON [BMC].[SubmodelInstance] ([tenantGuid], [submodelId]);

    PRINT 'Created BMC.SubmodelInstance table with indexes.';
END
ELSE
BEGIN
    PRINT 'BMC.SubmodelInstance table already exists — skipping.';
END
GO
