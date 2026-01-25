# Telemetry Database Generator

Database schema generator for the Foundation Telemetry Collection System.

## Overview

This module defines the database schema for storing historical system health metrics 
collected from monitored Foundation-based applications.

## Tables

| Table | Purpose |
|-------|---------|
| `TelemetryApplication` | Registry of monitored applications |
| `TelemetryCollectionRun` | Metadata about each collection cycle |
| `TelemetrySnapshot` | Core metrics per app per cycle (memory, CPU, threads) |
| `TelemetryDatabaseHealth` | Database connectivity status per snapshot |
| `TelemetryDiskHealth` | Disk space metrics per snapshot |
| `TelemetrySessionSnapshot` | Active user session counts per snapshot |
| `TelemetryErrorEvent` | Correlated audit error events |
| `TelemetryLogError` | Correlated log file error entries |

## Usage

Run the SchedulerTools/FoundationTools to generate SQL scripts from this generator.
