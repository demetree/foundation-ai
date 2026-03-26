using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using Foundation.Scheduler.Database;


namespace Scheduler.Tests
{
    /// <summary>
    /// A test-only subclass of SchedulerContext that relaxes NOT NULL constraints
    /// on string columns. This is necessary because:
    ///
    /// 1. The SchedulerContext entities are generated with #nullable disable, so
    ///    all string properties are non-nullable reference types.
    /// 2. SQL Server (production) uses nvarchar(max) NULL by default for these columns.
    /// 3. SQLite + EnsureCreated honours the EF model metadata strictly, making
    ///    every string property NOT NULL — causing INSERT failures when the business
    ///    logic doesn't populate optional fields like notes, description, etc.
    ///
    /// This subclass overrides OnModelCreating to mark all string properties as
    /// optional (IsRequired = false), matching the production SQL Server behaviour.
    /// </summary>
    public class TestSchedulerContext : SchedulerContext
    {
        public TestSchedulerContext(DbContextOptions<SchedulerContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //
            // Relax all string properties to allow NULL — mirrors SQL Server's
            // default behaviour and prevents spurious NOT NULL failures in tests.
            //
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(string)))
                {
                    property.IsNullable = true;
                }
            }
        }
    }
}
