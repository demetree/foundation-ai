# BmcDatabase

Entity Framework Core database context and entity classes for the BMC (Brick Machine Construction) system.

## Status

**Placeholder** — The EF Core context and entity classes will be generated using EF Core Power Tools after the BMC database is stood up from the generated SQL scripts.

## Pipeline

1. Run `BmcTools` → Option 1 to generate the SQL database creation scripts
2. Execute the SQL scripts against SQL Server to create the `BMC` database
3. Use EF Core Power Tools to reverse-engineer the database into this project
4. Run `BmcTools` → Option 2 to generate entity extension classes and API controllers
