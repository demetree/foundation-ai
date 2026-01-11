using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Foundation.Entity;
using System.Threading;


namespace Foundation.ChangeHistory
{
    /// <summary>
    /// Exception class that indicates that the input is out of data when compared with the current version in the database
    /// </summary>
    public class VersionNotCurrentException : Exception
    {
        public VersionNotCurrentException(string message) : base(message) { }
        public VersionNotCurrentException(string message, Exception inner) : base(message, inner) { }
    }


    /// <summary>
    /// 
    /// This is a simple user object that is used to represent a user in the ChangeHistory records.
    /// 
    /// It is agnostic to the source of the user information, so it can be used with systems linking change history records to security user records (such as those not using the data visibility features of the Foundation).
    /// 
    /// It will also support the Foundation's data visibility user table that will be in data visibility.  
    /// 
    /// </summary>
    public class ChangeHistoryUser
    {
        public int id { get; set; }         // This is the id of the user in the source system, such as the Foundation's data visibility user table, or the security module user table.  The one that it is will depend on the implementing system.
        public string userName { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
    }

    /// <summary>
    /// 
    /// ChangeHistory tables must implement this interface to be supported by this Toolset
    /// 
    /// </summary>
    public interface IChangeHistoryEntity
    {
        long primaryId { get; set; }
        int versionNumber { get; set; }
        DateTime timeStamp { get; set; }
        string data { get; set; }
        int userId { get; set; }
    }


    /// <summary>
    /// 
    /// This provides a record of a version of an entity, including the user who made the change, the version number, the timestamp, and the data at that version.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VersionInformation<T> where T : class
    { 
        public ChangeHistoryUser user { get; set; }  

        public int versionNumber { get; set; }

        public DateTime timeStamp { get; set; }

        public T data { get; set; }

    }

    /// <summary>
    /// 
    /// This interface is to be implemented by entities that are tracked for version history, and it will extend them
    /// to have a set of methods to retrieve version history and metrics about the entity's versions.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IVersionTrackedEntity<T> where T : class, IAnonymousConvertible
    {

        /// <summary>
        /// Gets meta data and optionally the entity data about the entity's version history using the version of the entity as the basis for the query.
        /// 
        /// Use this to get the update user/time metadata for this version.  IncludingData here is optional and default to false, as it is probably redundant in most cases 
        /// unless the entity you're working with might have unsaved changes.
        /// 
        /// </summary>
        /// <param name="includeData"></param>
        /// <returns></returns>
        public Task<VersionInformation<T>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets meta data and optionally the entity data about the first version of the entity.  Equivalent to GetVersionAsync(1, includeData), but name is a bit more concise.
        /// </summary>
        /// <param name="includeData"></param>
        /// <returns></returns>
        public Task<VersionInformation<T>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default);


        /// <summary>
        /// Gets meta data and optionally the entity data about a specific version of the entity.
        /// </summary>
        /// <param name="versionNumber"></param>
        /// <param name="includeData"></param>
        /// <returns></returns>
        public Task<VersionInformation<T>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default);


        /// <summary>
        /// Gets meta data and optionally the entity data about the version of the entity at the provided point in time.
        /// </summary>
        /// <param name="versionNumber"></param>
        /// <param name="includeData"></param>
        /// <returns></returns>
        public Task<VersionInformation<T>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default);


        /// <summary>
        /// Gets the full version history of the entity.
        /// </summary>
        /// <param name="includeData"></param>
        /// <returns></returns>
        public Task<List<VersionInformation<T>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default);
    }


    /// <summary>
    /// 
    /// This provides a simple audit entry for the change history records.
    /// 
    /// It does not include any data fields - Just the version meta data
    /// 
    /// </summary>
    public class AuditEntry
    {
        public int versionNumber { get; set; }
        public DateTime timeStamp { get; set; }
        public int? userId { get; set; }
    }


    /// <summary>
    /// 
    /// This class provides tools to easily interact with the Foundation's version control features of any table in the Foundation system
    /// that is modelled with a version control 'ChangeHistory' table.
    /// 
    /// It expects standard foundation field naming practices, and is coded for them.
    /// 
    /// </summary>
    /// <typeparam name="TPrimary"></typeparam>
    /// <typeparam name="TChangeHistory"></typeparam>
    public class ChangeHistoryToolset<TPrimary, TChangeHistory> where TPrimary : class, IAnonymousConvertible
                                                                where TChangeHistory : class, IChangeHistoryEntity, new()
    {
        private const string TENANT_GUID_FIELD_NAME = "tenantGuid";
        private const string USER_ID_FIELD_NAME = "userId";
        private const string VERSION_NUMBER_FIELD_NAME = "versionNumber";


        private readonly DbContext _context;
        private readonly DbSet<TPrimary> _primarySet;
        private readonly DbSet<TChangeHistory> _changeHistorySet;
        private readonly JsonSerializerOptions _jsonOptions;

        //
        // This is provided during construction, and used by all async functions during lift of object
        //
        private CancellationToken _cancellationToken;
        
        private static string _schemaName = null;

        /// <summary>
        /// 
        ///  
        /// This gets the user id key to use when adding new change history records.  
        /// 
        /// A change history toolset initialized without a userId can only be used for reading.  It will throw an exception on writing. 
        /// 
        /// 
        /// The table that the user Id indexes is variable, based on the table context.  Note that it is outside of the domain of this class 
        /// to determine this, as I do not want to need to reference the Security schema, or try to find user
        /// tables in arbitrary schemas, and/or test for the presence of data visibility fields in here to make that call.  (Though that would work too).
        ///
        /// The entity extensions generated by the code generator
        /// 
        /// 
        /// The user id value will be a reference to Security.SecurityUser table when:
        ///
        /// - Used on any tables in non-multi tenanted systems 
        /// - Used on tables in multi tenanted systems WITHOUT data visibility control fields.
        ///
        /// OR 
        ///
        /// It will be reference to the <Schema>.User table when
        ///    
        /// - Used on tables in multi tenanted systems WITH data visibility control fields.
        ///
        /// 
        /// In either case, this class just treats it as an int value, and hides its source from the user.
        ///
        /// 
        /// </summary>
        private readonly int? _userId = null;       // When null, only read actions are allowed.  When provided, discretion must be used 
        private readonly bool? _insideTransaction = null;   // used for write mode only

        /// <summary>
        /// 
        /// Use this class to manage the creation and updating of entities backed by ChangeHistory tables.  It also provides Rollback to version, and history inquiry support methods too.
        /// 
        /// This constructor is to be used for Read/Write purposes.
        /// 
        /// This will take care of all of the configuration related to managing change history.
        /// 
        /// The default state of the insideTransaction parameter is false, which is fine for small units of work, as it wraps up work in its own transaction.
        /// 
        /// If a value of true is provided for insideTransaction then this class will not create its own transaction to wrap its work.
        /// 
        /// However, if you're using this in the context of a bigger transaction then provide false to the transaction parameter so the out transaction will be used.
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="insideTransaction"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ChangeHistoryToolset(DbContext dbContext, int userId, bool insideTransaction = false, CancellationToken cancellationToken = default)
        {
            if (dbContext == null)
            {
                throw new ArgumentException("dbContext parameter cannot be null.");
            }

            _context = dbContext;
            _cancellationToken = cancellationToken;
            
            // Variables needed for writing.
            _userId = userId;
            _insideTransaction = insideTransaction;

            _primarySet = dbContext.Set<TPrimary>();
            _changeHistorySet = dbContext.Set<TChangeHistory>();

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            };
        }


        /// <summary>
        /// 
        /// This constructor is to be used for Read only purposes to make inquiries on change history for an entity, but not add or update anything.
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ChangeHistoryToolset(DbContext dbContext, CancellationToken cancellationToken = default)
        {
            if (dbContext == null)
            {
                throw new ArgumentException("dbContext parameter cannot be null.");
            }

            _context = dbContext;
            _cancellationToken = cancellationToken;

            // Ensure write mode variables are null.
            _userId = null;
            _insideTransaction = null;

            _primarySet = dbContext.Set<TPrimary>();
            _changeHistorySet = dbContext.Set<TChangeHistory>();

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            };
        }


        /// <summary>
        /// 
        /// This is a general save method that will either add or update, depending on the state of the id property.
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<TPrimary> SaveEntityAsync(TPrimary entity)
        {
            if (_userId == null || _insideTransaction == null)
            {
                throw new Exception("Change History Toolset must be initialized with write mode constructor to use this function.");
            }


            long id = GetPrimaryId(entity);

            if (id == 0)
            {
                return await AddEntityAsync(entity).ConfigureAwait(false);
            }
            else
            {
                return await UpdateEntityAsync(entity).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 
        /// Saves a new entity to the primary table and creates a change history record.
        /// 
        /// It is assumed that the tenant guid property is set before this is called.
        /// 
        /// Be mindful of the meaning of the user id record.  
        /// 
        ///     For systems that are not using the Foundation's data visibility features, this will be the user ID from the security module's securityUser table
        ///     
        ///     For systems are are using the Foundations's data visibility features, this will be the user id from the module's own user table.
        ///
        /// </summary>
        /// <param name="entity">The new entity to save.</param>
        /// <param name="userId">The ID of the user creating the entity.</param>
        /// <returns>The saved entity, or null if the operation fails.</returns>
        public async Task<TPrimary> AddEntityAsync(TPrimary entity)
        {
            if (_userId == null || _insideTransaction == null)
            {
                throw new Exception("Change History Toolset must be initialized with write mode constructor to use this function.");
            }


            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            //
            // Make sure that the id property has a value of 0 when adding
            //
            long idValue = GetPrimaryId(entity);

            if (idValue != 0)
            {
                throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of {idValue} but 0 is expected when adding.");
            }


            //
            // Set initial version number
            //
            PropertyInfo versionProp = typeof(TPrimary).GetProperty(VERSION_NUMBER_FIELD_NAME);

            if (versionProp == null)
            {
                throw new Exception($"Entity {typeof(TPrimary).Name} has no {VERSION_NUMBER_FIELD_NAME} property.");
            }

            //
            // Set the version number to 1
            //
            versionProp.SetValue(entity, 1);

            //
            // Get tenantGuid from the primary entity if present
            //
            Guid tenantGuid = Guid.Empty;
            PropertyInfo tenantProp = typeof(TPrimary).GetProperty(TENANT_GUID_FIELD_NAME);
            if (tenantProp != null)
            {
                tenantGuid = (Guid)tenantProp.GetValue(entity);
            }

            //
            // Initialize transaction variable
            //
            IDbContextTransaction transaction = null;


            try
            {
                // Start transaction only if not already inside one
                if (_insideTransaction == false)
                {
                    transaction = await _context.Database.BeginTransactionAsync(_cancellationToken).ConfigureAwait(false);
                }

                //
                // Add to primary table
                //
                _primarySet.Add(entity);

                //
                // Save entity to generate its ID
                //
                await _context.SaveChangesAsync(_cancellationToken).ConfigureAwait(false);

                //
                // Serialize entity state to use on change history record
                //
                // Note that the object that we serialize is an anonymous copy with first level sub objects, so we don't need to nullify
                // any collections or other things to get only want we want to serialize into the history state
                //
                string jsonData;

                jsonData = JsonSerializer.Serialize(entity.ToAnonymousWithFirstLevelSubObjects(), _jsonOptions);

                //
                // Create change history record
                //
                TChangeHistory changeHistory = new TChangeHistory
                {
                    primaryId = GetPrimaryId(entity),
                    versionNumber = 1,
                    timeStamp = DateTime.UtcNow,
                    data = jsonData
                };

                //
                // Set UserId and TenantGuid on the change history record
                //
                PropertyInfo userIdProp = typeof(TChangeHistory).GetProperty(USER_ID_FIELD_NAME);
                if (userIdProp != null)
                {
                    userIdProp.SetValue(changeHistory, _userId);
                }

                PropertyInfo changeTenantProp = typeof(TChangeHistory).GetProperty(TENANT_GUID_FIELD_NAME);
                if (changeTenantProp != null)
                {
                    changeTenantProp.SetValue(changeHistory, tenantGuid);
                }

                //
                // Add change history record
                //
                _changeHistorySet.Add(changeHistory);

                //
                // Save all the changes
                //
                await _context.SaveChangesAsync(_cancellationToken).ConfigureAwait(false);

                //
                // Commit the transaction if necessary
                //
                if (transaction != null)
                {
                    await transaction.CommitAsync(_cancellationToken).ConfigureAwait(false);
                }

                return entity;
            }
            catch (Exception ex)
            {
                // Todo - add logging here
                throw;
            }
            finally
            {
                // Ensure transaction is disposed if it was created
                if (transaction != null)
                {
                    await transaction.DisposeAsync().ConfigureAwait(false);
                }
            }
        }


        /// <summary>
        /// 
        /// Updates an existing entity in the primary table and creates a change history record.
        /// 
        /// Be mindful of the meaning of the user id record.  
        /// 
        ///     For systems that are not using the Foundation's data visibility features, this will be the user ID from the security module's securityUser table
        ///     
        ///     For systems are are using the Foundations's data visibility features, this will be the user id from the module's own user table.
        /// 
        /// </summary>
        /// <param name="primaryId">The ID of the entity to update.</param>
        /// <param name="updatedEntity">The updated entity data.</param>
        /// <param name="tenantGuid">The tenant GUID for data isolation.</param>
        /// <param name="userId">The ID of the user updating the entity.</param>
        /// <returns>The updated entity, or null if the operation fails.</returns>
        public async Task<TPrimary> UpdateEntityAsync(TPrimary updatedEntity)
        {
            if (_userId == null || _insideTransaction == null)
            {
                throw new Exception("Change History Toolset must be initialized with write mode constructor to use this function.");
            }


            if (updatedEntity == null)
            {
                throw new ArgumentNullException(nameof(updatedEntity));
            }

            //
            //  Find existing entity  - Get it's ID property and Find the existing record by using the appropriate id data type of int or long.
            //
            TPrimary existingEntity;

            PropertyInfo idProperty = typeof(TPrimary).GetProperty("id");
            if (idProperty == null)
            {
                throw new InvalidOperationException($"Entity of type {typeof(TPrimary).Name} does not have an 'id' property.");
            }

            //
            // Note we're not using GetPrimaryId() on purpose here because we care about the actual field type in the next steps.
            //
            object idValue = idProperty.GetValue(updatedEntity);
            if (idValue == null)
            {
                throw new InvalidOperationException($"The 'id' property of entity {typeof(TPrimary).Name} is null.");
            }

            long primaryIdForVersionHistory = 0;    // Used for creating history record.

            //
            // Load the existing entity using the the right data type as input to the .FindAsync method.
            //
            if (idProperty.PropertyType == typeof(int))
            {
                int intId = (int)idValue;

                if (intId == 0)
                {
                    throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of 0 but, a non zero value is expected when updating.");
                }

                existingEntity = await _primarySet.FindAsync(intId, _cancellationToken).ConfigureAwait(false);

                primaryIdForVersionHistory = intId;
            }
            else if (idProperty.PropertyType == typeof(long))
            {
                long longId = (long)idValue;

                if (longId == 0)
                {
                    throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of 0 but, a non zero value is expected when updating.");
                }

                existingEntity = await _primarySet.FindAsync(longId, _cancellationToken).ConfigureAwait(false);

                primaryIdForVersionHistory = longId;
            }
            else
            {
                throw new InvalidOperationException($"The 'id' property of type {idProperty.PropertyType.Name} is not supported. Only int and long are allowed.");
            }

            
            if (existingEntity == null)
            {
                throw new Exception($"Record with id of {primaryIdForVersionHistory} not found for type {typeof(TPrimary).Name}.");
            }

            //
            // Get tenant guid if property exists.
            PropertyInfo tenantProp = typeof(TPrimary).GetProperty(TENANT_GUID_FIELD_NAME);

            Guid tenantGuid = Guid.Empty;
            if (tenantProp != null)
            {
                tenantGuid = (Guid)tenantProp.GetValue(existingEntity);
            }

            // Get current version number
            PropertyInfo versionProp = typeof(TPrimary).GetProperty(VERSION_NUMBER_FIELD_NAME);

            if (versionProp == null)
            {
                throw new Exception($"Entity {typeof(TPrimary).Name} has no versionNumber property.");
            }


            int currentVersion = (int)versionProp.GetValue(existingEntity);
            int versionFromInput = (int)versionProp.GetValue(updatedEntity);

            //
            // Check the version of the entity coming in to be updated.  If it's not the same as the current version from the database, then throw an error
            //
            if (currentVersion != versionFromInput)
            {
                throw new VersionNotCurrentException($"Current version is {currentVersion} and version from input is {versionFromInput}.");
            }

            //
            // Kick up the version number by 1
            //
            int newVersion = currentVersion + 1;


            // Initialize transaction variable
            IDbContextTransaction transaction = null;

            try
            {
                // Start transaction only if not already inside one
                if (_insideTransaction == false)
                {
                    transaction = await _context.Database.BeginTransactionAsync(_cancellationToken).ConfigureAwait(false);
                }

                // Update entity
                _context.Entry(existingEntity).CurrentValues.SetValues(updatedEntity);

                // Set new version number
                versionProp.SetValue(existingEntity, newVersion);

                // Save changes to primary table
                await _context.SaveChangesAsync(_cancellationToken).ConfigureAwait(false);

                // Serialize updated state
                string jsonData;
                try
                {
                    jsonData = JsonSerializer.Serialize(existingEntity.ToAnonymousWithFirstLevelSubObjects(), _jsonOptions);
                }
                catch (JsonException ex)
                {
                    // ToDo - Add logging here
                    throw;
                }

                //
                // Create change history record
                //
                var changeHistory = new TChangeHistory
                {
                    primaryId = primaryIdForVersionHistory,
                    versionNumber = newVersion,
                    timeStamp = DateTime.UtcNow,
                    data = jsonData
                };

                //
                // Set UserId and TenantGuid if necessary
                //
                PropertyInfo userIdProp = typeof(TChangeHistory).GetProperty(USER_ID_FIELD_NAME);
                if (userIdProp != null)
                {
                    userIdProp.SetValue(changeHistory, _userId);
                }

                PropertyInfo changeTenantProp = typeof(TChangeHistory).GetProperty(TENANT_GUID_FIELD_NAME);
                if (changeTenantProp != null)
                {
                    changeTenantProp.SetValue(changeHistory, tenantGuid);
                }


                //
                // Add change history record
                //
                _changeHistorySet.Add(changeHistory);


                //
                // Save the changes
                //
                await _context.SaveChangesAsync(_cancellationToken).ConfigureAwait(false);

                //
                // Commit the transaction if necessary
                //
                if (_insideTransaction == false)
                {
                    await transaction.CommitAsync(_cancellationToken).ConfigureAwait(false);
                }

                return existingEntity;
            }
            catch (Exception ex)
            {
                // ToDo - Add logging here
                throw;
            }
            finally
            {
                // Ensure transaction is disposed if it was created
                if (transaction != null)
                {
                    await transaction.DisposeAsync().ConfigureAwait(false);
                }
            }
        }


        /// <summary>
        /// 
        /// This returns a clone of the entity as it was at a point in time.
        /// 
        /// Note that the entity returned is not attached to the EF Context, so any changes to it won't be tracked by the context.
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="pointInTime"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<TPrimary> GetStateAtPointInTimeAsync(TPrimary entity, DateTime pointInTime)
        {
            return await GetVersionAsync(entity, pointInTime).ConfigureAwait(false);
        }


        //
        // Named Metric accessor Functions
        //
        // Note that these have all be updated to use the audit accessors.  This is a more efficient way to pull 
        // the metadata as compared to the original solution which replied on loading the complete change history record each time.
        //


        //
        /// <summary>
        /// 
        /// Gets the creation timestamp of the first first of an entity
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<DateTime?> GetCreationTimeAsync(TPrimary entity)
        {
            AuditEntry auditEntry = await GetAuditForVersion(entity, 1).ConfigureAwait(false);

            if (auditEntry != null)
            {
                return auditEntry.timeStamp;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 
        /// Gets the update time for a particular version of an entity
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="versionNumber"></param>
        /// <returns></returns>
        public async Task<DateTime?> GetUpdateTimeAsync(TPrimary entity, int versionNumber)
        {
            AuditEntry auditEntry = await GetAuditForVersion(entity, versionNumber).ConfigureAwait(false);

            if (auditEntry != null)
            {
                return auditEntry.timeStamp;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// Gets the id of the user who created an entity
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int?> GetCreatedByUserIdAsync(TPrimary entity)
        {
            AuditEntry auditEntry = await GetAuditForVersion(entity, 1).ConfigureAwait(false);

            if (auditEntry != null)
            {
                return auditEntry.userId;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 
        /// Gets the id of the user who updated an entity for a particular version
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="versionNumber"></param>
        /// <returns></returns>
        public async Task<int?> GetUpdatedByUserIdAsync(TPrimary entity, int versionNumber)
        {
            AuditEntry auditEntry = await GetAuditForVersion(entity, versionNumber).ConfigureAwait(false);

            if (auditEntry != null )
            {
                return auditEntry.userId;
            }
            else
            {
                return null;
            }
        }


        //
        // Most recent accessors
        //
        public async Task<int?> GetLatestVersionNumberAsync(TPrimary entity)
        {
            List<AuditEntry> allAudits = await GetAuditTrailAsync(entity).ConfigureAwait(false);

            if (allAudits != null && allAudits.Count > 0)
            {
                return allAudits[allAudits.Count - 1].versionNumber;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// Gets the ID of the user who made the most recent update to the entity.
        /// 
        /// If you know you are  using the current version number for an entity (ie. a regular read from a table, its version number will be the latest.
        /// 
        /// However, I think there's value in having clear methods to get most recent, so these will do that.
        /// 
        /// </summary>
        /// <param name="entity">The entity to query.</param>
        /// <returns>The user ID of the most recent update, or null if no history exists.</returns>
        public async Task<int?> GetMostRecentUpdateUserIdAsync(TPrimary entity)
        {
            List<AuditEntry> allAudits = await GetAuditTrailAsync(entity).ConfigureAwait(false);

            if (allAudits != null && allAudits.Count > 0)
            {
                return allAudits[allAudits.Count - 1].userId;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// Gets the timestamp of the most recent update to an entity.  
        /// 
        /// </summary>
        /// <param name="entity">The entity to query.</param>
        /// <returns>The timestamp of the most recent update, or null if no history exists.</returns>
        public async Task<DateTime?> GetMostRecentUpdateTimeAsync(TPrimary entity)
        {
            List<AuditEntry> allAudits = await GetAuditTrailAsync(entity).ConfigureAwait(false);

            if (allAudits != null && allAudits.Count > 0)
            {
                return allAudits[allAudits.Count - 1].timeStamp;
            }
            else
            { 
                return null;
            }
        }


        
        /// <summary>
        /// 
        /// This retrieves an object from the database at the specified version number.  
        /// 
        /// </summary>
        /// <param name="primaryId"></param>
        /// <param name="versionNumber"></param>
        /// <param name="tenantGuid"></param>
        /// <returns></returns>
        public async Task<TPrimary> GetVersionAsync(TPrimary entity, int versionNumber)
        {
            TChangeHistory historyRecord = await GetVersionChangeHistoryAsync(entity, versionNumber).ConfigureAwait(false);

            if (historyRecord == null || string.IsNullOrEmpty(historyRecord.data))
            {
                return null;
            }

            try
            {
                TPrimary deserializedData = JsonSerializer.Deserialize<TPrimary>(historyRecord.data, _jsonOptions);
                return deserializedData;
            }
            catch (Exception ex)
            {
                // ToDo - Add logging here
                return null;
            }
        }

        /// <summary>
        /// 
        /// This retrieves an object from the database at the specified point in time.
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="pointIntTime"></param>
        /// <returns></returns>
        public async Task<TPrimary> GetVersionAsync(TPrimary entity, DateTime pointIntTime)
        {
            TChangeHistory historyRecord = await GetVersionChangeHistoryAsync(entity, pointIntTime).ConfigureAwait(false);

            if (historyRecord == null || string.IsNullOrEmpty(historyRecord.data))
            {
                return null;
            }

            try
            {
                TPrimary deserializedData = JsonSerializer.Deserialize<TPrimary>(historyRecord.data, _jsonOptions);
                return deserializedData;
            }
            catch (Exception ex)
            {
                // ToDo - Add logging here
                return null;
            }
        }


        /// <summary>
        /// 
        /// This gets all the record history available and returns as a list of the original type
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<List<TPrimary>> GetAllVersionsAsync(TPrimary entity)
        {
            List<TChangeHistory> historyRecordList = await GetAllVersionsChangeHistoryAsync(entity).ConfigureAwait(false);

            if (historyRecordList == null || historyRecordList.Count == 0)
            {
                return null;
            }

            try
            {
                List<TPrimary> output = new List<TPrimary>();

                foreach (TChangeHistory historyRecord in historyRecordList)
                {
                    TPrimary deserializedData = JsonSerializer.Deserialize<TPrimary>(historyRecord.data, _jsonOptions);
                    
                    output.Add(deserializedData);
                }

                return output;
            }
            catch (Exception ex)
            {
                // ToDo - Add logging here
                return null;
            }
        }


        /// <summary>
        /// 
        /// This gets all the change history records for an entity.  Note that all data will be returned.  Could be costly for records 
        /// with large content.
        /// 
        /// Use the GetAudit... functions for faster access to just metadata
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        private async Task<List<TChangeHistory>> GetAllVersionsChangeHistoryAsync(TPrimary entity)
        {
            PropertyInfo idProperty = typeof(TPrimary).GetProperty("id");

            if (idProperty == null)
            {
                throw new InvalidOperationException($"Entity of type {typeof(TPrimary).Name} does not have an 'id' property.");
            }

            //
            // Note we're not using GetPrimaryId() on purpose here because we care about the actual field type in the next steps.
            //
            object idValue = idProperty.GetValue(entity);
            if (idValue == null)
            {
                throw new InvalidOperationException($"The 'id' property of entity {typeof(TPrimary).Name} is null.");
            }

            string foreignKeyFieldName = StringUtility.CamelCase(typeof(TPrimary).Name) + "Id";


            //
            // Get tenantGuid from the primary entity if present
            //
            Guid tenantGuid = Guid.Empty;
            PropertyInfo tenantProp = typeof(TPrimary).GetProperty(TENANT_GUID_FIELD_NAME);
            if (tenantProp != null)
            {
                tenantGuid = (Guid)tenantProp.GetValue(entity);
            }

            //
            // Get the history record using a lookup by the data type of the entity primary key
            //
            if (idProperty.PropertyType == typeof(int))
            {
                int intId = (int)idValue;

                if (intId == 0)
                {
                    throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of 0 but, a non zero value is expected when getting all versions.");
                }

                //
                // Get the history records using an int key
                //
                IQueryable<TChangeHistory> query = _changeHistorySet.Where(ch => EF.Property<int>(ch, foreignKeyFieldName) == intId);

                if (tenantGuid != Guid.Empty)
                {
                    // If tenantGuid is set, filter by it
                    query = query.Where(ch => EF.Property<Guid>(ch, TENANT_GUID_FIELD_NAME) == tenantGuid);
                }

                query = query.OrderBy(ch => ch.versionNumber);

                return await query.AsNoTracking().ToListAsync(_cancellationToken).ConfigureAwait(false);

            }
            else if (idProperty.PropertyType == typeof(long))
            {
                long longId = (long)idValue;

                if (longId == 0)
                {
                    throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of 0 but, a non zero value is expected when updating.");
                }

                //
                // Get the history records using a long key
                //
                IQueryable<TChangeHistory> query = _changeHistorySet.Where(ch => EF.Property<long>(ch, foreignKeyFieldName) == longId);

                if (tenantGuid != Guid.Empty)
                {
                    // If tenantGuid is set, filter by it
                    query = query.Where(ch => EF.Property<Guid>(ch, TENANT_GUID_FIELD_NAME) == tenantGuid);
                }


                query = query.OrderBy(ch => ch.versionNumber);

                return await query.AsNoTracking().ToListAsync(_cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException($"The 'id' property of type {idProperty.PropertyType.Name} is not supported. Only int and long are allowed.");
            }
        }


        /// <summary>
        /// 
        /// This gets the change history record for the provided version number.
        /// 
        /// Note that I have tried hard to make this function selectively pull the data field from the TChangeHistory table.
        /// 
        /// I tried with a parameter that added minimal field selection from the IChangeHistory base (less the data field), but that did not worko.
        /// 
        /// The query that executed pulled the data field regardless.  Likely due to the multi level abstraction here.  Regardless,
        /// it is clear that this will always pull the full change history record.  This is fine for when you want the data, but gets slow
        /// when the data field has a lot of bytes in it, especially when all you want is the user and timestamp.
        /// 
        /// To work around this, I'm going to make a new method to use in to get the selected field raw date with SQL directly
        /// to use in the GetAudit...  Series of functions.  That will cherry pick the desired data points directly, at the cost
        /// of needing backing database type awareness for SQL formatting....
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="versionNumber"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        private async Task<TChangeHistory> GetVersionChangeHistoryAsync(TPrimary entity, int versionNumber)
        {
            if (versionNumber < 0)
            {
                throw new ArgumentException("Invalid version number.");
            }


            PropertyInfo idProperty = typeof(TPrimary).GetProperty("id");

            if (idProperty == null)
            {
                throw new InvalidOperationException($"Entity of type {typeof(TPrimary).Name} does not have an 'id' property.");
            }

            //
            // Note we're not using GetPrimaryId() on purpose here because we care about the actual field type in the next steps.
            //
            object idValue = idProperty.GetValue(entity);
            if (idValue == null)
            {
                throw new InvalidOperationException($"The 'id' property of entity {typeof(TPrimary).Name} is null.");
            }

            string foreignKeyFieldName = StringUtility.CamelCase(typeof(TPrimary).Name) + "Id";

            //
            // Get tenantGuid from the primary entity if present
            //
            Guid tenantGuid = Guid.Empty;
            PropertyInfo tenantProp = typeof(TPrimary).GetProperty(TENANT_GUID_FIELD_NAME);
            if (tenantProp != null)
            {
                tenantGuid = (Guid)tenantProp.GetValue(entity);
            }


            //
            // Get the history record using a lookup by the data type of the entity primary key and version number
            //
            if (idProperty.PropertyType == typeof(int))
            {
                int intId = (int)idValue;

                if (intId == 0)
                {
                    throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of 0 but, a non zero value is expected when getting all versions.");
                }

                //
                // Get the history records using an int key
                //
                IQueryable<TChangeHistory> query = _changeHistorySet.Where(ch => EF.Property<int>(ch, foreignKeyFieldName) == intId)
                                                                    .Where(ch => ch.versionNumber == versionNumber)
                                                                    .OrderByDescending(ch => ch.timeStamp);

                if (tenantGuid != Guid.Empty)
                {
                    // If tenantGuid is set, filter by it
                    query = query.Where(ch => EF.Property<Guid>(ch, TENANT_GUID_FIELD_NAME) == tenantGuid);
                }

                //string querySQL = query.ToQueryString();

                return await query.AsNoTracking().FirstOrDefaultAsync(_cancellationToken).ConfigureAwait(false);

            }
            else if (idProperty.PropertyType == typeof(long))
            {
                long longId = (long)idValue;

                if (longId == 0)
                {
                    throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of 0 but, a non zero value is expected when updating.");
                }

                //
                // Get the history records using a long key
                //

                IQueryable<TChangeHistory> query = _changeHistorySet.Where(ch => EF.Property<long>(ch, foreignKeyFieldName) == longId)
                                                                    .Where(ch => ch.versionNumber == versionNumber)
                                                                    .OrderByDescending(ch => ch.timeStamp);

                if (tenantGuid != Guid.Empty)
                {
                    // If tenantGuid is set, filter by it
                    query = query.Where(ch => EF.Property<Guid>(ch, TENANT_GUID_FIELD_NAME) == tenantGuid);
                }

                return await query.AsNoTracking().FirstOrDefaultAsync(_cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException($"The 'id' property of type {idProperty.PropertyType.Name} is not supported. Only int and long are allowed.");
            }
        }


        /// <summary>
        /// 
        /// This gets the change history record for the provided point in time.
        /// 
        /// Note that I have tried hard to make this function selectively pull the data field from the TChangeHistory table.
        /// 
        /// I tried with a parameter that added minimal field selection from the IChangeHistory base (less the data field), but that did not worko.
        /// 
        /// The query that executed pulled the data field regardless.  Likely due to the multi level abstraction here.  Regardless,
        /// it is clear that this will always pull the full change history record.  This is fine for when you want the data, but gets slow
        /// when the data field has a lot of bytes in it, especially when all you want is the user and timestamp.
        /// 
        /// To work around this, I'm going to make a new method to use in to get the selected field raw date with SQL directly
        /// to use in the GetAudit...  Series of functions.  That will cherry pick the desired data points directly, at the cost
        /// of needing backing database type awareness for SQL formatting....
        /// 
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="pointIntime"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        private async Task<TChangeHistory> GetVersionChangeHistoryAsync(TPrimary entity, DateTime pointInTime)
        {
            PropertyInfo idProperty = typeof(TPrimary).GetProperty("id");

            if (idProperty == null)
            {
                throw new InvalidOperationException($"Entity of type {typeof(TPrimary).Name} does not have an 'id' property.");
            }

            //
            // Note we're not using GetPrimaryId() on purpose here because we care about the actual field type in the next steps.
            //
            object idValue = idProperty.GetValue(entity);
            if (idValue == null)
            {
                throw new InvalidOperationException($"The 'id' property of entity {typeof(TPrimary).Name} is null.");
            }

            string foreignKeyFieldName = StringUtility.CamelCase(typeof(TPrimary).Name) + "Id";

            //
            // Get tenantGuid from the primary entity if present
            //
            Guid tenantGuid = Guid.Empty;
            PropertyInfo tenantProp = typeof(TPrimary).GetProperty(TENANT_GUID_FIELD_NAME);
            if (tenantProp != null)
            {
                tenantGuid = (Guid)tenantProp.GetValue(entity);
            }

            //
            // Get the history record using a lookup by the data type of the entity primary key and version number
            //
            if (idProperty.PropertyType == typeof(int))
            {
                //
                // Get the history records using an int key
                //
                int intId = (int)idValue;
                if (intId == 0)
                {
                    throw new InvalidOperationException($"Entity {typeof(TPrimary).Name} has an id value of 0, but a non-zero value is expected.");
                }

                IQueryable<TChangeHistory> query = _changeHistorySet.Where(ch => EF.Property<int>(ch, foreignKeyFieldName) == intId &&
                                                                     ch.timeStamp <= pointInTime);

                if (tenantGuid != Guid.Empty)
                {
                    // If tenantGuid is set, filter by it
                    query = query.Where(ch => EF.Property<Guid>(ch, TENANT_GUID_FIELD_NAME) == tenantGuid);
                }

                query = query.OrderByDescending(ch => ch.timeStamp);

                return await query.AsNoTracking().FirstOrDefaultAsync(_cancellationToken).ConfigureAwait(false);
            }
            else if (idProperty.PropertyType == typeof(long))
            {
                //
                // Get the history records using a long key
                //
                long longId = (long)idValue;

                if (longId == 0)
                {
                    throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of 0 but, a non zero value is expected when updating.");
                }


                IQueryable<TChangeHistory> query = _changeHistorySet.Where(ch => EF.Property<long>(ch, foreignKeyFieldName) == longId &&
                                                     ch.timeStamp <= pointInTime);

                if (tenantGuid != Guid.Empty)
                {
                    // If tenantGuid is set, filter by it
                    query = query.Where(ch => EF.Property<Guid>(ch, TENANT_GUID_FIELD_NAME) == tenantGuid);
                }

                query = query.OrderByDescending(ch => ch.timeStamp);

                return await query.AsNoTracking().FirstOrDefaultAsync(_cancellationToken).ConfigureAwait(false);

            }
            else
            {
                throw new InvalidOperationException($"The 'id' property of type {idProperty.PropertyType.Name} is not supported. Only int and long are allowed.");
            }
        }


        public async Task<Dictionary<string, (object OldValue, object NewValue)>> CompareVersionsAsync(TPrimary entity,
                                                                                                       int oldVersionNumber,
                                                                                                       int newVersionNumber)
        {
            TChangeHistory oldRecord = await GetVersionChangeHistoryAsync(entity, oldVersionNumber).ConfigureAwait(false);
            TChangeHistory newRecord = await GetVersionChangeHistoryAsync(entity, newVersionNumber).ConfigureAwait(false);

            if (oldRecord == null || newRecord == null)
            {
                return null;
            }

            TPrimary oldData = JsonSerializer.Deserialize<TPrimary>(oldRecord.data, _jsonOptions);
            TPrimary newData = JsonSerializer.Deserialize<TPrimary>(newRecord.data, _jsonOptions);

            Dictionary<string, (object, object)> differences = new Dictionary<string, (object, object)>();
            PropertyInfo[] properties = typeof(TPrimary).GetProperties();

            //
            // Look for differences in each property except for version number.
            //
            foreach (var prop in properties)
            {
                if (prop.Name == "versionNumber")
                {
                    continue;
                }

                object oldValue = prop.GetValue(oldData);
                object newValue = prop.GetValue(newData);

                if (!Equals(oldValue, newValue))
                {
                    differences.Add(prop.Name, (oldValue, newValue));
                }
            }

            return differences;
        }



        public async Task<TPrimary> RollbackToVersionAsync(TPrimary entityToRollback, int versionNumberToRollbackTo)
        {
            if (_userId == null || _insideTransaction == null)
            {
                throw new Exception("Change History Toolset must be initialized with write mode constructor to use this function.");
            }


            PropertyInfo idProperty = typeof(TPrimary).GetProperty("id");
            if (idProperty == null)
            {
                throw new InvalidOperationException($"Entity of type {typeof(TPrimary).Name} does not have an 'id' property.");
            }

            PropertyInfo versionProp = typeof(TPrimary).GetProperty(VERSION_NUMBER_FIELD_NAME);

            if (versionProp == null)
            {
                throw new Exception($"Entity {typeof(TPrimary).Name} has no versionNumber property.");
            }


            //
            // Note we're not using GetPrimaryId() on purpose here because we care about the actual field type in the next steps.
            //
            object idValue = idProperty.GetValue(entityToRollback);
            if (idValue == null)
            {
                throw new InvalidOperationException($"The 'id' property of entity {typeof(TPrimary).Name} is null.");
            }



            string foreignKeyFieldName = StringUtility.CamelCase(typeof(TPrimary).Name) + "Id";


            //
            // Get tenantGuid from the primary entity if present
            //
            Guid tenantGuid = Guid.Empty;
            PropertyInfo tenantProp = typeof(TPrimary).GetProperty(TENANT_GUID_FIELD_NAME);
            if (tenantProp != null)
            {
                tenantGuid = (Guid)tenantProp.GetValue(entityToRollback);
            }

            //
            // Load the history record, and a fresh copy of the entity from the database.
            //
            TChangeHistory historyRecord;
            TPrimary entityFromDatabase;

            if (idProperty.PropertyType == typeof(int))
            {
                int intId = (int)idValue;

                if (intId == 0)
                {
                    throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of 0 but, a non zero value is expected when rolling back.");
                }


                IQueryable<TChangeHistory> query = _changeHistorySet.Where(ch => EF.Property<int>(ch, foreignKeyFieldName) == intId &&
                                                              ch.versionNumber == versionNumberToRollbackTo);


                if (tenantGuid != Guid.Empty)
                {
                    // If tenantGuid is set, filter by it
                    query = query.Where(ch => EF.Property<Guid>(ch, TENANT_GUID_FIELD_NAME) == tenantGuid);
                }

                query = query.OrderByDescending(ch => ch.timeStamp);


                historyRecord = await query.AsNoTracking().FirstOrDefaultAsync(_cancellationToken).ConfigureAwait(false);

                if (historyRecord == null)
                {
                    throw new Exception($"Could not find history record for version {versionNumberToRollbackTo} for {typeof(TPrimary).Name} with id of {intId}");
                }

                entityFromDatabase = await _primarySet.FindAsync(intId, _cancellationToken).ConfigureAwait(false);

            }
            else if (idProperty.PropertyType == typeof(long))
            {
                long longId = (long)idValue;

                if (longId == 0)
                {
                    throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of 0 but, a non zero value is expected when rolling back.");
                }

                IQueryable<TChangeHistory> query = _changeHistorySet.Where(ch => EF.Property<long>(ch, foreignKeyFieldName) == longId &&
                                                                           ch.versionNumber == versionNumberToRollbackTo);

                if (tenantGuid != Guid.Empty)
                {
                    // If tenantGuid is set, filter by it
                    query = query.Where(ch => EF.Property<Guid>(ch, TENANT_GUID_FIELD_NAME) == tenantGuid);
                }

                query = query.OrderByDescending(ch => ch.timeStamp);

                historyRecord = await query.AsNoTracking().FirstOrDefaultAsync(_cancellationToken).ConfigureAwait(false);

                if (historyRecord == null)
                {
                    throw new Exception($"Could not find history record for version {versionNumberToRollbackTo} for {typeof(TPrimary).Name} with id of {longId}");
                }

                entityFromDatabase = await _primarySet.FindAsync(longId, _cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException($"The 'id' property of type {idProperty.PropertyType.Name} is not supported. Only int and long are allowed.");
            }


            //
            // Get the current version from the primary record
            //
            int currentVersion = (int)versionProp.GetValue(entityFromDatabase);
            int versionFromInput = (int)versionProp.GetValue(entityToRollback);
            //
            // Make sure that the entity that was provided as input has the same version number so we don't work on old data.
            //
            if (currentVersion != versionFromInput)
            {
                throw new VersionNotCurrentException($"Cannot rollback because current version is {currentVersion} and version from input is {versionFromInput}.");
            }


            //
            // Rebuild the state of the object as it was at the historical version
            //
            TPrimary restoredData = JsonSerializer.Deserialize<TPrimary>(historyRecord.data, _jsonOptions);
            if (restoredData == null)
            {
                throw new Exception($"Unable to restore objects state from history data.  Version from history is {versionNumberToRollbackTo}");
            }

            //
            // Set the state of the primary record back to what it was as of the restored data
            //
            _context.Entry(entityFromDatabase).CurrentValues.SetValues(restoredData);

            // 
            // Put the version number back to current 
            //
            versionProp.SetValue(entityFromDatabase, currentVersion);


            //
            // Reconstruct the tenant guid on the object, as it won't have come from the history.  That field is excluded.
            //
            if (tenantProp != null)
            {
                tenantProp.SetValue(entityFromDatabase, tenantGuid);
            }


            //
            // Call the update record to finish the update.  This will create a new version, and its history record using the historical data we provide.
            //
            return await UpdateEntityAsync(entityFromDatabase).ConfigureAwait(false);
        }


        /// <summary>
        /// 
        /// This gets the audit details for the specified version on an entity.
        /// 
        /// It does this efficiently by only querying the database for the meta data fields from the ChangeHistory table.
        /// 
        /// By doing it this way, the size of the data in the data field has no bearing on the performance. Compare this to the GetVersionChangeHistoryAsync function
        /// which gets all the data from the change history table, and it really slows down on mapping the data back into the object when there's a lot of data in it.
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="versionNumber"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<AuditEntry> GetAuditForVersion(TPrimary entity, int versionNumber)
        {
            if (versionNumber < 0)
            {
                throw new ArgumentException("Invalid version number.");
            }


            PropertyInfo idProperty = typeof(TPrimary).GetProperty("id");

            if (idProperty == null)
            {
                throw new InvalidOperationException($"Entity of type {typeof(TPrimary).Name} does not have an 'id' property.");
            }

            //
            // Note we're not using GetPrimaryId() on purpose here because we care about the actual field type in the next steps.
            //
            object idValue = idProperty.GetValue(entity);
            if (idValue == null)
            {
                throw new InvalidOperationException($"The 'id' property of entity {typeof(TPrimary).Name} is null.");
            }

            string tableName = typeof(TPrimary).Name;

            string foreignKeyFieldName = StringUtility.CamelCase(tableName) + "Id";

            //
            // Get tenantGuid from the primary entity if present
            //
            Guid tenantGuid = Guid.Empty;
            PropertyInfo tenantProp = typeof(TPrimary).GetProperty(TENANT_GUID_FIELD_NAME);
            if (tenantProp != null)
            {
                tenantGuid = (Guid)tenantProp.GetValue(entity);
            }

            long recordId = 0;

            //
            // Get the record Id.  Map either int or long into it.
            //
            if (idProperty.PropertyType == typeof(int))
            {
                int intId = (int)idValue;

                if (intId == 0)
                {
                    throw new Exception($"Entity {tableName} has an id value of 0 but, a non zero value is expected when getting all versions.");
                }

                recordId = intId;

            }
            else if (idProperty.PropertyType == typeof(long))
            {
                long longId = (long)idValue;

                if (longId == 0)
                {
                    throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of 0 but, a non zero value is expected when updating.");
                }

                recordId = longId;

            }
            else
            {
                throw new InvalidOperationException($"The 'id' property of type {idProperty.PropertyType.Name} is not supported. Only int and long are allowed.");
            }

            //
            // Create a SQL statement to pull only the ChangeHistory table meta data.
            //
            StringBuilder sqlBuilder = new StringBuilder();


            string providerName = _context.Database.ProviderName;

            // Write the SQL in the format needed for the backing provider.
            switch (providerName)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":


                    // Get the schema only once.  _schemaName is static
                    if (string.IsNullOrEmpty(_schemaName) == true)
                    {
                        _schemaName = await GetSQLServerTableSchemaAsync(tableName).ConfigureAwait(false);
                    }

                    sqlBuilder.Append($"SELECT TOP 1 versionNumber, timeStamp, userId FROM {_schemaName}.{tableName}ChangeHistory ");

                    sqlBuilder.Append($" WHERE {foreignKeyFieldName} = {recordId.ToString()} AND versionNumber = {versionNumber.ToString()}");

                    if (tenantGuid != Guid.Empty)
                    {
                        // If tenantGuid is set, filter by it
                        sqlBuilder.Append($" AND tenantGuid = '{tenantGuid.ToString()}'");
                    }

                    sqlBuilder.Append($" ORDER BY timeStamp DESC");

                    break;

                case "Npgsql.EntityFrameworkCore.PostgreSQL":

                    throw new NotImplementedException("Write this for PostgreSQL");

                case "MySql.EntityFrameworkCore":
                    throw new NotImplementedException("Write this for MySQL");

                case "Microsoft.EntityFrameworkCore.Sqlite":
                    throw new NotImplementedException("Write this for SQLite");

                default:
                    throw new NotImplementedException($"Write this for {providerName}");

            }

            //
            // Execute raw SQL and map to AuditEntry
            //
            AuditEntry auditEntry = await _context.Database.SqlQuery<AuditEntry>(FormattableStringFactory.Create(sqlBuilder.ToString())).FirstOrDefaultAsync(_cancellationToken).ConfigureAwait(false);

            return auditEntry;
        }


        /// <summary>
        /// 
        /// This gets the audit details for an entity at a point in time
        /// 
        /// It does this efficiently by only querying the database for the meta data fields from the ChangeHistory table.
        /// 
        /// By doing it this way, the size of the data in the data field has no bearing on the performance. Compare this to the GetVersionChangeHistoryAsync function
        /// which gets all the data from the change history table, and it really slows down on mapping the data back into the object when there's a lot of data in it.
        //
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="pointInTime"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<AuditEntry> GetAuditForTime(TPrimary entity, DateTime pointInTime)
        {
            PropertyInfo idProperty = typeof(TPrimary).GetProperty("id");

            if (idProperty == null)
            {
                throw new InvalidOperationException($"Entity of type {typeof(TPrimary).Name} does not have an 'id' property.");
            }

            //
            // Note we're not using GetPrimaryId() on purpose here because we care about the actual field type in the next steps.
            //
            object idValue = idProperty.GetValue(entity);
            if (idValue == null)
            {
                throw new InvalidOperationException($"The 'id' property of entity {typeof(TPrimary).Name} is null.");
            }

            string tableName = typeof(TPrimary).Name;

            string foreignKeyFieldName = StringUtility.CamelCase(tableName) + "Id";

            //
            // Get tenantGuid from the primary entity if present
            //
            Guid tenantGuid = Guid.Empty;
            PropertyInfo tenantProp = typeof(TPrimary).GetProperty(TENANT_GUID_FIELD_NAME);
            if (tenantProp != null)
            {
                tenantGuid = (Guid)tenantProp.GetValue(entity);
            }

            long recordId = 0;

            //
            // Get the record Id.  Map either int or long into it.
            //
            if (idProperty.PropertyType == typeof(int))
            {
                int intId = (int)idValue;

                if (intId == 0)
                {
                    throw new Exception($"Entity {tableName} has an id value of 0 but, a non zero value is expected when getting all versions.");
                }

                recordId = intId;

            }
            else if (idProperty.PropertyType == typeof(long))
            {
                long longId = (long)idValue;

                if (longId == 0)
                {
                    throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of 0 but, a non zero value is expected when updating.");
                }

                recordId = longId;

            }
            else
            {
                throw new InvalidOperationException($"The 'id' property of type {idProperty.PropertyType.Name} is not supported. Only int and long are allowed.");
            }

            //
            // Create a SQL statement to pull only the ChangeHistory table meta data.
            //
            StringBuilder sqlBuilder = new StringBuilder();


            string providerName = _context.Database.ProviderName;

            // Write the SQL in the format needed for the backing provider.
            switch (providerName)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":


                    // Get the schema only once.  _schemaName is static
                    if (string.IsNullOrEmpty(_schemaName) == true)
                    {
                        _schemaName = await GetSQLServerTableSchemaAsync(tableName).ConfigureAwait(false);
                    }

                    sqlBuilder.Append($"SELECT versionNumber, timeStamp, userId FROM {_schemaName}.{tableName}ChangeHistory ");

                    sqlBuilder.Append($" WHERE {foreignKeyFieldName} = {recordId.ToString()} AND timeStamp < {pointInTime.ToUniversalTime().ToString("s")}");

                    if (tenantGuid != Guid.Empty)
                    {
                        // If tenantGuid is set, filter by it
                        sqlBuilder.Append($" AND tenantGuid = '{tenantGuid.ToString()}'");
                    }

                    sqlBuilder.Append($" ORDER BY timeStamp DESC");

                    break;

                case "Npgsql.EntityFrameworkCore.PostgreSQL":

                    throw new NotImplementedException("Write this for PostgreSQL");

                case "MySql.EntityFrameworkCore":
                    throw new NotImplementedException("Write this for MySQL");

                case "Microsoft.EntityFrameworkCore.Sqlite":
                    throw new NotImplementedException("Write this for SQLite");

                default:
                    throw new NotImplementedException($"Write this for {providerName}");

            }

            //
            // Execute raw SQL and map to AuditEntry
            //
            AuditEntry auditEntry = await _context.Database.SqlQuery<AuditEntry>(FormattableStringFactory.Create(sqlBuilder.ToString())).FirstOrDefaultAsync(_cancellationToken).ConfigureAwait(false);

            return auditEntry;
        }


        /// <summary>
        /// 
        /// This gets all audit history available for an entity, ordered by version number
        /// 
        /// It does this efficiently by only querying the database for the meta data fields from the ChangeHistory table.
        /// 
        /// By doing it this way, the size of the data in the data field has no bearing on the performance. Compare this to the GetVersionChangeHistoryAsync function
        /// which gets all the data from the change history table, and it really slows down on mapping the data back into the object when there's a lot of data in it.
        //
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<AuditEntry>> GetAuditTrailAsync(TPrimary entity)
        {
            PropertyInfo idProperty = typeof(TPrimary).GetProperty("id");

            if (idProperty == null)
            {
                throw new InvalidOperationException($"Entity of type {typeof(TPrimary).Name} does not have an 'id' property.");
            }

            //
            // Note we're not using GetPrimaryId() on purpose here because we care about the actual field type in the next steps.
            //
            object idValue = idProperty.GetValue(entity);
            if (idValue == null)
            {
                throw new InvalidOperationException($"The 'id' property of entity {typeof(TPrimary).Name} is null.");
            }

            string tableName = typeof(TPrimary).Name;

            string foreignKeyFieldName = StringUtility.CamelCase(tableName) + "Id";

            //
            // Get tenantGuid from the primary entity if present
            //
            Guid tenantGuid = Guid.Empty;
            PropertyInfo tenantProp = typeof(TPrimary).GetProperty(TENANT_GUID_FIELD_NAME);
            if (tenantProp != null)
            {
                tenantGuid = (Guid)tenantProp.GetValue(entity);
            }

            long recordId = 0;

            //
            // Get the record Id.  Map either int or long into it.
            //
            if (idProperty.PropertyType == typeof(int))
            {
                int intId = (int)idValue;

                if (intId == 0)
                {
                    throw new Exception($"Entity {tableName} has an id value of 0 but, a non zero value is expected when getting all versions.");
                }

                recordId = intId;

            }
            else if (idProperty.PropertyType == typeof(long))
            {
                long longId = (long)idValue;

                if (longId == 0)
                {
                    throw new Exception($"Entity {typeof(TPrimary).Name} has an id value of 0 but, a non zero value is expected when updating.");
                }

                recordId = longId;

            }
            else
            {
                throw new InvalidOperationException($"The 'id' property of type {idProperty.PropertyType.Name} is not supported. Only int and long are allowed.");
            }

            //
            // Create a SQL statement to pull only the ChangeHistory table meta data.
            //
            StringBuilder sqlBuilder = new StringBuilder();


            string providerName = _context.Database.ProviderName;

            // Write the SQL in the format needed for the backing provider.
            switch (providerName)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":

                    // Get the schema only once.  _schemaName is static
                    if (string.IsNullOrEmpty(_schemaName) == true)
                    {
                        _schemaName = await GetSQLServerTableSchemaAsync(tableName).ConfigureAwait(false);
                    }

                    sqlBuilder.Append($"SELECT versionNumber, timeStamp, userId FROM {_schemaName}.{tableName}ChangeHistory ");

                    sqlBuilder.Append($" WHERE {foreignKeyFieldName} = {recordId.ToString()}");

                    if (tenantGuid != Guid.Empty)
                    {
                        // If tenantGuid is set, filter by it
                        sqlBuilder.Append($" AND tenantGuid = '{tenantGuid.ToString()}'");
                    }

                    sqlBuilder.Append($" ORDER BY versionNumber");

                    break;

                case "Npgsql.EntityFrameworkCore.PostgreSQL":

                    throw new NotImplementedException("Write this for PostgreSQL");

                case "MySql.EntityFrameworkCore":
                    throw new NotImplementedException("Write this for MySQL");

                case "Microsoft.EntityFrameworkCore.Sqlite":
                    throw new NotImplementedException("Write this for SQLite");

                default:
                    throw new NotImplementedException($"Write this for {providerName}");

            }

            //
            // Execute raw SQL and map to AuditEntry
            //
            List<AuditEntry> auditEntry = await _context.Database.SqlQuery<AuditEntry>(FormattableStringFactory.Create(sqlBuilder.ToString())).ToListAsync(_cancellationToken).ConfigureAwait(false);

            return auditEntry;
        }

                       
        /// <summary>
        /// 
        /// Helper method to extract and convert the id property to a long, whether or not it actually is a long.
        /// 
        /// </summary>
        /// <typeparam name="TPrimary"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private long GetPrimaryId<TPrimary>(TPrimary entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            PropertyInfo idProperty = typeof(TPrimary).GetProperty("id");

            if (idProperty == null)
            {
                throw new InvalidOperationException($"Entity of type {typeof(TPrimary).Name} does not have an 'id' property.");
            }

            object idValue = idProperty.GetValue(entity);

            if (idValue == null)
            {
                throw new InvalidOperationException($"The 'id' property of entity {typeof(TPrimary).Name} is null.");
            }

            return idProperty.PropertyType switch
            {
                Type intType when intType == typeof(int) => (int)idValue,
                Type longType when longType == typeof(long) => (long)idValue,
                _ => throw new InvalidOperationException($"The 'id' property of type {idProperty.PropertyType.Name} is not supported. Only int and long are allowed.")
            };
        }

        private async Task<string> GetSQLServerTableSchemaAsync(string tableName)
        {
            // SQL Server query to get schema
            string sql = "SELECT TABLE_SCHEMA AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @p0";

            string schema;

            try
            {
                schema = await _context.Database.SqlQueryRaw<string>(sql, tableName)
                                                .SingleOrDefaultAsync(_cancellationToken)
                                                .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                throw;
            }

            return schema ?? "dbo"; // Default to 'dbo' if not found
        }
    }
}