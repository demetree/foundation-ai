# Foundation.IndexedDB

This is a .Net Core (.Net 10) class library that provides objects similar to the JavaSript IndexedDB API.

# Core Idea:
The program provides an abstraction layer over a SQLite database, making it behave like a client-side IndexedDB in a .NET environment. This means you can define "object stores" (tables), add/get/put/delete "objects" (records), and query them using "indexes" and "key ranges," all with a familiar API for developers accustomed to IndexedDB or Dexie.js.
Key Classes and Their Roles:

## IDBFactory:

This is the entry point for creating or opening databases.
It manages the creation of the underlying SQLite file (.sqlite).
The OpenAsync method handles database versioning and calls an upgradeNeededHandler when the database schema needs to be updated (similar to onupgradeneeded in IndexedDB).
Provides a DeleteDatabase method to remove a database.


## IDBDatabase:

Represents a single database.
Holds a reference to the Entity Framework DbContext (IDBContext).
Manages ObjectStoreConfig for each object store, which defines its KeyPath, AutoIncrement status, and Indexes.
Allows access to IDBObjectStore instances for specific store names.
Provides Transaction management (read-only or read-write).
Handles database metadata (like version and nextKey_ for auto-increment).

## IDBObjectStore:
Represents a table (object store) within the database.
Provides methods for basic CRUD operations: Add, Put (upsert), Get, Delete, Clear.
Manages the primary key generation (AutoIncrement) and extraction from objects (KeyPath).
Allows creation and deletion of secondary Indexes.
Provides OpenCursor for iterating over records.
Count method to get the number of records.

## IDBIndex:
Represents a secondary index defined on an object store.
Allows efficient querying (Get, GetAll, OpenCursor) based on the indexed property.
It uses json_extract SQLite function to query nested properties within the JSON ValueJson column.

## IDBCursor<T>:
Enables iteration over a set of records, similar to IndexedDB cursors.
Provides ContinueAsync to move to the next record.
Exposes Key and Value of the current record.

## IDBKeyRange:
A helper class for defining ranges for queries (e.g., Only, LowerBound, UpperBound, Bound).
Used by IDBIndex and IDBObjectStore to filter records efficiently.

## IDBTransaction:
Encapsulates a database transaction.
Supports ReadOnly and ReadWrite modes.
In ReadWrite mode, it wraps an EF Core IDbContextTransaction and requires explicit Commit or Abort. If not explicitly handled, it will rollback on Dispose.
Ensures that only one read-write transaction can be active at a time for simplicity.

## DexterDatabase (and related Dexter* classes):
These are higher-level, more "Dexie-like" wrappers built on top of the IDB* core classes.

### DexterDatabase: 
An abstract base class for defining your specific database schema. You'd inherit from this to create your database with strongly-typed tables.
### DexterVersionBuilder: 
Used within DexterDatabase to define object stores and their indexes during schema upgrades. It parses a schema string (e.g., "++id, name, &email") to configure KeyPath, AutoIncrement, and Unique indexes.
### DexterTable<T, TKey>: 
Provides a strongly-typed API for interacting with an object store, similar to a db.table('users') in Dexie.
##DexterWhereClause<T, TKey, TProperty>: 
Implements the where clause for filtering queries, offering methods like Equals, Above, Below, Between, StartsWith. This is where the IDBIndex is utilized.
### DexterCollection<T, TKey>: 
Represents the result of a query, allowing further operations like First, ToArray, Limit, and Count.

## IDBContext (Entity Framework Core DbContext):
The actual EF Core context that interacts with SQLite.
Defines two DbSets: Data (to store the actual records as JSON) and Metadata (for internal database information like version and object store configurations).
Data table has id (PK), storeName, keyJson (serialized key), and valueJson (serialized record).
Metadata table stores key-value pairs for configurations.

## IDBCommon:
Holds shared JsonSerializerOptions (camelCase, ignore nulls, etc.) and custom exception types.
How it Works (Simplified Flow):

### Initialization: 
You'd create an instance of IDBFactory and call OpenAsync to get an IDBDatabase instance. 

During OpenAsync, the upgradeNeededHandler is invoked if the database version changes, allowing you to define your object stores and indexes using DexterVersionBuilder.

#### Schema Definition: 
In your DexterDatabase subclass, you'd call Version(yourVersion).DefineStores(...) to set up your tables and their indexes. 

This creates the necessary metadata in the Metadata table and sets up SQLite indexes on the Data table using json_extract.

#### Data Operations:

You get a DexterTable<T, TKey> from your DexterDatabase subclass.

When you Add or Put an object, it's serialized to JSON and stored in the valueJson column of the Data table. The primary key is either provided, auto-incremented, or extracted from the object itself and stored in keyJson.

When you Get or query using Where, the system constructs SQL queries that use json_extract to query against the indexed properties within the valueJson column or keyJson directly.

IDBKeyRange objects are translated into SQL WHERE clauses (e.g., WHERE json_extract(...) > value).

IDBCursor provides a way to stream results rather than loading everything into memory at once.

# Key Technologies Used:
-  C# and .NET: 
The primary programming language and framework.
-  SQLite: 
The embedded relational database used for persistence.
-  Entity Framework Core: 
An ORM that simplifies interaction with the SQLite database.
- System.Text.Json: 
For serializing and deserializing C# objects to/from JSON, which is how the actual data is stored in SQLite.

# Advantages of this Approach:

- Familiar API: Provides a strongly-typed, fluent API that mimics client-side IndexedDB and Dexie.js, making it potentially easier for web developers to adopt in C# applications.

- Strongly-Typed Data: Unlike raw IndexedDB, you work with C# objects directly.

- Persistence: Leverages SQLite for robust and persistent storage.

- Querying Capabilities: Utilizes SQLite's json_extract and indexing capabilities for efficient querying on JSON data.

- Transactions: Proper transaction management ensures data integrity.

# Potential Considerations/Improvements:

- Performance: While json_extract with indexes helps, heavy querying on deeply nested JSON might still have performance implications compared to a fully normalized relational schema.

- Complex Indexing: The current DefineStores parsing for indexes is relatively simple. Real-world Dexie allows compound indexes ([firstName+lastName]) and multi-entry indexes (*tags).
 
- Query Expressiveness: The DexterWhereClause is a good start but could be expanded with more Dexie-like query operators (anyOf, notEqual, or, and).

- Concurrency: The single read-write transaction constraint simplifies things but might need more sophisticated handling for highly concurrent scenarios.

- Schema Migration: While upgradeNeededHandler is there, the actual logic for migrating data when a KeyPath or index definition changes is left to the implementer.

- Error Handling: The exception hierarchy is good, but ensuring all possible IndexedDB-specific error codes are mapped or handled consistently could be useful.

# Summary
In summary, this is a well-structured and ambitious project that provides a modern, convenient way to interact with a persistent local data store in C# applications, borrowing heavily from the successful patterns of client-side IndexedDB frameworks.