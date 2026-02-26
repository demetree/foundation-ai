--
-- Migration: Add IpAddressLocation table and link from LoginAttempt
-- Date: 2026-02-26
-- Feature: Login geolocation visualization
--
-- This script adds:
--   1. New IpAddressLocation cache table (one record per unique IP)
--   2. ipAddressLocationId FK column on LoginAttempt (nullable, populated by background worker)
--

CREATE TABLE [Security].[IpAddressLocation]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[ipAddress] NVARCHAR(50) NOT NULL,
	[countryCode] NVARCHAR(10) NULL,
	[countryName] NVARCHAR(100) NULL,
	[city] NVARCHAR(100) NULL,
	[latitude] FLOAT NULL,
	[longitude] FLOAT NULL,
	[lastLookupDate] DATETIME2(7) NOT NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.
)
GO

-- Unique index on ipAddress for fast lookups and deduplication.
CREATE UNIQUE INDEX [I_IpAddressLocation_ipAddress] ON [Security].[IpAddressLocation] ([ipAddress])
GO

-- Standard id/active/deleted index.
CREATE INDEX [I_IpAddressLocation_id_active_deleted] ON [Security].[IpAddressLocation] ([id], [active], [deleted])
GO


--
-- Add FK column on LoginAttempt pointing to IpAddressLocation.
-- Nullable because records are linked asynchronously by the IpAddressLocationWorker.
--
ALTER TABLE [Security].[LoginAttempt] ADD [ipAddressLocationId] INT NULL
GO

ALTER TABLE [Security].[LoginAttempt] ADD CONSTRAINT [FK_LoginAttempt_IpAddressLocation_ipAddressLocationId] FOREIGN KEY ([ipAddressLocationId]) REFERENCES [Security].[IpAddressLocation] ([id])
GO

-- Index on the LoginAttempt table's ipAddressLocationId field.
CREATE INDEX [I_LoginAttempt_ipAddressLocationId] ON [Security].[LoginAttempt] ([ipAddressLocationId])
GO
