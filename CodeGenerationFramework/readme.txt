This is the .Net Framework CodeGeneration class library that produces code for .Net Framework 4.8 projects

Note that Foundation DB Context classes for Contexts used by foundation need this in their constructors


    // Disable the database initialization, so no attempt to create databases will be made
    System.Data.Entity.Database.SetInitializer<CONTENTNAMEContext>(null);
 
    // Prevent automatic migrations.
    this.Configuration.AutoDetectChangesEnabled = false;
    this.Configuration.ProxyCreationEnabled = false;
 
    // Don't lazy load
    this.Configuration.LazyLoadingEnabled = false;