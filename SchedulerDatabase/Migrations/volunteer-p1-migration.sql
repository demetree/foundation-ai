-- =============================================
-- Volunteer P1 Schema Migration
-- =============================================
-- Run this script against the Scheduler database
-- to add the fields required by the P1 Volunteer
-- Hub implementation.
-- =============================================

-- 1. Add reminder tracking to EventResourceAssignment
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'EventResourceAssignment'
      AND COLUMN_NAME = 'reminderSentDateTime'
)
BEGIN
    ALTER TABLE EventResourceAssignment
    ADD reminderSentDateTime DATETIME2 NULL;

    PRINT 'Added reminderSentDateTime to EventResourceAssignment';
END
GO

-- 2. Add volunteer opportunity fields to ScheduledEvent
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ScheduledEvent'
      AND COLUMN_NAME = 'isOpenForVolunteers'
)
BEGIN
    ALTER TABLE ScheduledEvent
    ADD isOpenForVolunteers BIT NOT NULL DEFAULT 0;

    PRINT 'Added isOpenForVolunteers to ScheduledEvent';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ScheduledEvent'
      AND COLUMN_NAME = 'maxVolunteerSlots'
)
BEGIN
    ALTER TABLE ScheduledEvent
    ADD maxVolunteerSlots INT NULL;

    PRINT 'Added maxVolunteerSlots to ScheduledEvent';
END
GO

-- 3. Seed "Pending" VolunteerStatus (if not already present)
IF NOT EXISTS (
    SELECT 1 FROM VolunteerStatus WHERE [name] = 'Pending'
)
BEGIN
    INSERT INTO VolunteerStatus (
        [name], [description], [sequence], [color],
        [isActive], [preventsScheduling], [requiresApproval],
        [objectGuid], [active], [deleted]
    )
    VALUES (
        'Pending',
        'Volunteer registration awaiting admin approval',
        0,
        '#F59E0B',
        0,            -- isActive = false (pending isn't fully active)
        1,            -- preventsScheduling = true (can''t be scheduled until approved)
        1,            -- requiresApproval = true
        NEWID(),
        1,            -- active
        0             -- deleted
    );

    PRINT 'Inserted "Pending" VolunteerStatus';
END
GO

-- 4. Ensure "Active" VolunteerStatus exists (used after approval)
IF NOT EXISTS (
    SELECT 1 FROM VolunteerStatus WHERE [name] = 'Active'
)
BEGIN
    INSERT INTO VolunteerStatus (
        [name], [description], [sequence], [color],
        [isActive], [preventsScheduling], [requiresApproval],
        [objectGuid], [active], [deleted]
    )
    VALUES (
        'Active',
        'Active volunteer, eligible for scheduling',
        1,
        '#10B981',
        1,            -- isActive = true
        0,            -- preventsScheduling = false
        0,            -- requiresApproval = false
        NEWID(),
        1,
        0
    );

    PRINT 'Inserted "Active" VolunteerStatus';
END
GO

-- 5. Ensure "Inactive" VolunteerStatus exists (used for rejections)
IF NOT EXISTS (
    SELECT 1 FROM VolunteerStatus WHERE [name] = 'Inactive'
)
BEGIN
    INSERT INTO VolunteerStatus (
        [name], [description], [sequence], [color],
        [isActive], [preventsScheduling], [requiresApproval],
        [objectGuid], [active], [deleted]
    )
    VALUES (
        'Inactive',
        'Inactive or rejected volunteer',
        2,
        '#6B7280',
        0,
        1,
        0,
        NEWID(),
        1,
        0
    );

    PRINT 'Inserted "Inactive" VolunteerStatus';
END
GO

PRINT '✅ Volunteer P1 migration complete.';
