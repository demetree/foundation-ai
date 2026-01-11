# CodeGenerationFramework

This is a *.Net Framework 4.8* class library that extends CodeGenerationCommon to added .Net Framework specific features, and to add the 
ability to generate code that is intended to work in .Net Framework

It is to be referenced by Foundation *Framework* projects, particularly those that generate the application database, and template code.


Note that Foundation DB Context classes for Contexts used by foundation need this in their constructors


    // Disable the database initialization, so no attempt to create databases will be made
    System.Data.Entity.Database.SetInitializer<CONTENTNAMEContext>(null);
 
    // Prevent automatic migrations.
    this.Configuration.AutoDetectChangesEnabled = false;
    this.Configuration.ProxyCreationEnabled = false;
 
    // Don't lazy load
    this.Configuration.LazyLoadingEnabled = false;