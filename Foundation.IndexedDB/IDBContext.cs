using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Foundation.IndexedDB
{
    /// <summary>
    /// 
    /// This is a DbContext for IndexedDB-like storage using Entity Framework Core.
    /// 
    /// Note that additional indexes are created dynamically in the IDBDatabase class.
    /// 
    /// </summary>
    internal class IDBContext : DbContext
    {
        public DbSet<Data> Data { get; set; }
        public DbSet<Metadata> Metadata { get; set; }

        public IDBContext(DbContextOptions<IDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Data>(entry =>
            {
                entry.HasKey(e => e.id).HasName("PK_Data_id");
                entry.HasIndex(e => new { e.storeName, e.keyJson }).IsUnique();
                entry.ToTable("Data");

            });

            modelBuilder.Entity<Metadata>(entry =>
            {
                entry.HasKey(e => e.Key).HasName("PK_Metadata_key");

                entry.ToTable("Metadata");
            });
        }
    }

    public class Data
    {
        public long id { get; set; }
        public string storeName { get; set; }
        public string keyJson { get; set; }
        public string valueJson { get; set; }
    }

    public class Metadata
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}