using Foundation.Auditor.Database;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cronos;

namespace Foundation.Auditor
{
    public sealed class AuditEngine
    {
        public enum AuditorMode
        {
            InProcess = 1,                              // slowest to the end user because it writes right away in the same thread
            DispatchToBackgroundImmediately = 2,        // much faster and almost immediate write because it pushes the log writing into the hangfire worker threads
            MemoryQueueWithOneMinuteFlush = 3           // way faster but messages won't write into the db for up to one minute and < 1 minute old ones will be lost on IIS restart
        }

        public enum AuditAccessType
        {
            WebBrowser = 1,
            APIRequest = 2,
            Ambiguous = 3
        }

        public enum ExternalCommunicationType
        {
            Email = 1,
            SMS = 2
        }

        public enum ExternalCommunicationRecipientType
        {
            To = 1,
            CC = 2,
            BCC = 3
        }

        public enum AuditType
        {
            Login = 1,
            Logout = 2,
            ReadList = 3,
            ReadListRedacted = 4,
            ReadEntity = 5,
            ReadEntityRedacted = 6,
            CreateEntity = 7,
            UpdateEntity = 8,
            DeleteEntity = 9,
            WriteList = 10,
            LoadPage = 11,
            ConfirmationRequested = 12,
            ConfirmationGranted = 13,
            ConfirmationDenied = 14,
            Search = 15,
            ContextSet = 16,
            UnauthorizedAccessAttempt = 17,
            Error = 18,
            Miscellaneous = 19
        }


        public class EventDetails
        {
            public EventDetails(DateTime startTime,
                DateTime stopTime,
                bool completedSuccessfully,
                AuditAccessType accessType,
                AuditType auditType,
                string user,
                string session,
                string source,
                string userAgent,
                string module,
                string moduleEntity,
                string resource,
                string hostSystem,
                string primaryKey,
                int? threadId,
                string message,
                string entityBeforeState,
                string entityAfterState,
                List<string> errorMessages)
            {
                this.startTime = startTime;
                this.stopTime = stopTime;
                this.completedSuccessfully = completedSuccessfully;
                this.accessType = accessType;
                this.auditType = auditType;
                this.user = user;
                this.session = session;
                this.source = source;
                this.userAgent = userAgent;
                this.module = module;
                this.moduleEntity = moduleEntity;
                this.resource = resource;
                this.hostSystem = hostSystem;
                this.primaryKey = primaryKey;
                this.threadId = threadId;
                this.message = message;
                this.entityBeforeState = entityBeforeState;
                this.entityAfterState = entityAfterState;
                this.errorMessages = errorMessages;
            }
            public DateTime startTime;
            public DateTime stopTime;
            public bool completedSuccessfully;
            public AuditAccessType accessType;
            public AuditType auditType;
            public string user;
            public string session;
            public string source;
            public string userAgent;
            public string module;
            public string moduleEntity;
            public string resource;
            public string hostSystem;
            public string primaryKey;
            public int? threadId;
            public string message;
            public string entityBeforeState;
            public string entityAfterState;
            public List<string> errorMessages;
        }

        public static void IntegrityCheckAuditTypeTable()
        {
            //
            // This should be called once at startup just to confirm that the startup state of the SecurityPrivilege table is what is expected.
            //
            using (AuditorContext db = new AuditorContext())
            {
                /* If using SQLite as the provider, and this blows up, do this:
                 * 
                  1.) Add the System.Data.SQLite package from NuGet.  It will add the required references.
                  2.) There will be 3 SQLite assemblies in the references.  Make them all 'Copy Local' = true
                  3.) Note that the |DataDirectory| varaible should be set in globals.asax by calling             Foundation.StartupBasics.SetStartupConfiguration();  as part of application_start
                  4.) Copy the App_Data\Auditor folder from another project in to get a shell auditor db.  You can also create a new auditor db file from th escripts and put it in the app_data\Auditor folder as well.
                  5.) Fix the web.config file like this:
                  6.) ** NOTE THAT THE CONNECTION STRING MUST HAVE THIS VALUE - ;BinaryGUID=false; ' that is needed to allow SQLite to treat guid fields as text instead of blobs, and without this, no constraints by guid types (as we do a lot) will ever work. so this is mission critical to include.

                Providers like this

                    <providers>
                      <provider invariantName="System.Data.SQLite" type="System.Data.SQLite.EF6.SQLiteProviderServices, System.Data.SQLite.EF6" />
                    </providers>

                DB Provider Factories like this:

                    <DbProviderFactories>
                        <remove invariant="System.Data.SQLite.EF6" />
                        <add name="SQLite Data Provider" invariant="System.Data.SQLite" description="Data Provider for SQLite" type="System.Data.SQLite.SQLiteFactory, System.Data.SQLite" />
                    </DbProviderFactories>

                Connection string like this:

                   <add name="AuditorContext" connectionString="data source=|DataDirectory|\Auditor\Auditor.db;BinaryGUID=false;" providerName="System.Data.SQLite" />  (or set the db path to be whatever you need)

                 
                 */
                List<Database.AuditType> allAuditTypes = (from x in db.AuditTypes select x).AsNoTracking().ToList();

                if (allAuditTypes.Count != 19)
                {
                    throw new Exception("Auditor System Integrity Error.  Incorrect number of Audit Types in Auditor database.");
                }

                List<Database.AuditType> validatedAuditTypes = (from x in allAuditTypes
                                                                where ((x.id == 1 && x.name == "Login") ||
                                                                (x.id == 2 && x.name == "Logout") ||
                                                                (x.id == 3 && x.name == "Read List") ||
                                                                (x.id == 4 && x.name == "Read List (Redacted)") ||
                                                                (x.id == 5 && x.name == "Read Entity") ||
                                                                (x.id == 6 && x.name == "Read Entity (Redacted)") ||
                                                                (x.id == 7 && x.name == "Create Entity") ||
                                                                (x.id == 8 && x.name == "Update Entity") ||
                                                                (x.id == 9 && x.name == "Delete Entity") ||
                                                                (x.id == 10 && x.name == "Write List") ||
                                                                (x.id == 11 && x.name == "Load Page") ||
                                                                (x.id == 12 && x.name == "Confirmation Requested") ||
                                                                (x.id == 13 && x.name == "Confirmation Granted") ||
                                                                (x.id == 14 && x.name == "Confirmation Denied") ||
                                                                (x.id == 15 && x.name == "Search") ||
                                                                (x.id == 16 && x.name == "Context Set") ||
                                                                (x.id == 17 && x.name == "Unauthorized Access Attempt") ||
                                                                (x.id == 18 && x.name == "Error") ||
                                                                (x.id == 19 && x.name == "Miscellaneous"))
                                                                select x).ToList();

                if (validatedAuditTypes.Count != 19)
                {
                    throw new Exception("Auditor System Integrity Error.  Audit Type configuration is incorrect in the Auditor database.");
                }
            }
        }


        public static void IntegrityCheckAuditAccessTypeTable()
        {
            //
            // This should be called once at startup just to confirm that the startup state of the SecurityPrivilege table is what is expected.
            //
            using (AuditorContext db = new AuditorContext())
            {
                List<Database.AuditAccessType> allAuditAccessTypes = (from x in db.AuditAccessTypes select x).AsNoTracking().ToList();

                if (allAuditAccessTypes.Count != 3)
                {
                    throw new Exception("Auditor System Integrity Error.  Incorrect number of Audit Access Types in Auditor database.");
                }

                List<Database.AuditAccessType> validatedAuditTypes = (from x in allAuditAccessTypes
                                                                      where ((x.id == 1 && x.name == "Web Browser") ||
                                                                      (x.id == 2 && x.name == "API Request") ||
                                                                      (x.id == 3 && x.name == "Ambiguous"))
                                                                      select x).ToList();

                if (validatedAuditTypes.Count != 3)
                {
                    throw new Exception("Auditor System Integrity Error.  Audit Access Type configuration is incorrect in the Auditor database.");
                }
            }
        }

        public void EnableAutoPurge(int auditorAutoPurgeDays)
        {
            if (auditorAutoPurgeDays <= 0)
            {
                return;
            }

            try
            {
                RecurringJob.AddOrUpdate(AUDITOR_AUTO_PURGE, () => PurgeAuditEvents(auditorAutoPurgeDays), RecurringJob.CRON_HOURLY);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        public void DisableAutoPurge()
        {
            RecurringJob.RemoveIfExists(AUDITOR_AUTO_PURGE);
        }

        public volatile List<EventDetails> queuedEvents;

        private AuditorMode auditorMode = AuditorMode.InProcess;      // default to in process writing.

        private static volatile AuditEngine instance;
        public static object syncRoot = new object();
        public static object memoryCacheSyncRoot = new object();
        public static object purgeSyncRoot = new object();

        private Dictionary<string, AuditUser> AuditUserCache;
        private Dictionary<string, AuditSource> AuditSourceCache;
        private Dictionary<string, AuditUserAgent> AuditUserAgentCache;
        private Dictionary<string, AuditModule> AuditModuleCache;
        private Dictionary<string, AuditModuleEntity> AuditModuleEntityCache;
        private Dictionary<string, AuditResource> AuditResourceCache;
        private Dictionary<string, AuditHostSystem> AuditHostSystemCache;
        private Dictionary<string, AuditSession> AuditSessionCache;

        private AuditEngine() { }

        private const string AUDITOR_MEMORY_QUEUE_FLUSH = "AuditorMemoryQueueFlush";
        private const string AUDITOR_AUTO_PURGE = "AuditorAutoPurge";

        //public static AuditorContext GetAuditorDB()
        //{
        //    AuditorContext db = new AuditorContext();

        //    return db;
        //}

        public void SetAuditorMode(string mode)
        {
            if (mode.Trim().ToUpper() == AuditorMode.MemoryQueueWithOneMinuteFlush.ToString().ToUpper())
            {
                SetAuditorMode(AuditorMode.MemoryQueueWithOneMinuteFlush);
            }
            else if (mode.Trim().ToUpper() == AuditorMode.DispatchToBackgroundImmediately.ToString().ToUpper())
            {
                SetAuditorMode(AuditorMode.DispatchToBackgroundImmediately);
            }
            else if (mode.Trim().ToUpper() == AuditorMode.InProcess.ToString().ToUpper())
            {
                SetAuditorMode(AuditorMode.InProcess);
            }
            else
            {
                // default to the dispatch to background immediately mode if the string can't be understood
                SetAuditorMode(AuditorMode.DispatchToBackgroundImmediately);
            }
        }

        public void SetAuditorMode(AuditorMode mode)
        {
            this.auditorMode = mode;

            try
            {
                if (this.auditorMode == AuditorMode.MemoryQueueWithOneMinuteFlush)
                {
                    RecurringJob.AddOrUpdate(AUDITOR_MEMORY_QUEUE_FLUSH, () => AuditEngine.FlushMemoryQueueToDatabase(), RecurringJob.CRON_MINUTELY);
                }
                else
                {
                    // Make sure that the recurring background job isn't running if the mode doesn't require it.
                    RecurringJob.RemoveIfExists(AUDITOR_MEMORY_QUEUE_FLUSH);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        public AuditorMode GetAuditorMode()
        {
            return auditorMode;
        }

        public static AuditEngine Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new AuditEngine();

                            instance.queuedEvents = new List<EventDetails>();

                            instance.AuditUserCache = new Dictionary<string, AuditUser>();
                            instance.AuditSourceCache = new Dictionary<string, AuditSource>();
                            instance.AuditUserAgentCache = new Dictionary<string, AuditUserAgent>();
                            instance.AuditModuleCache = new Dictionary<string, AuditModule>();
                            instance.AuditModuleEntityCache = new Dictionary<string, AuditModuleEntity>();
                            instance.AuditResourceCache = new Dictionary<string, AuditResource>();
                            instance.AuditHostSystemCache = new Dictionary<string, AuditHostSystem>();
                            instance.AuditSessionCache = new Dictionary<string, AuditSession>();
                        }
                    }
                }

                return instance;
            }
        }


        public void TriggerMemoryQueueFlush()
        {
            RecurringJob.TriggerJob(AUDITOR_MEMORY_QUEUE_FLUSH);
        }

        public void CreateExternalCommunication(DateTime timeStamp,
                                   bool completedSuccessfully,
                                   ExternalCommunicationType externalCommunicationType,
                                   string user,
                                   List<String> to,
                                   List<String> cc,
                                   List<String> bcc,
                                   string subject,
                                   string message,
                                   string responseMessage,
                                   Exception ex)
        {
            try
            {
                using (AuditorContext db = new AuditorContext())
                {
                    using (var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        ExternalCommunication externalComm = new ExternalCommunication();

                        if (user == null || user.Length == 0)
                        {
                            user = "Unknown";
                        }


                        AuditUser au = GetAuditUser(user, db);

                        if (au != null)
                        {
                            externalComm.auditUserId = au.id;
                        }


                        externalComm.communicationType = externalCommunicationType.ToString();
                        externalComm.timeStamp = timeStamp;
                        externalComm.completedSuccessfully = completedSuccessfully;
                        externalComm.message = message;
                        externalComm.subject = subject;

                        externalComm.responseMessage = responseMessage;

                        if (ex != null)
                        {
                            externalComm.exceptionText = ex.ToString();
                        }

                        // put in the new event
                        db.ExternalCommunications.Add(externalComm);

                        // save it
                        db.SaveChanges();

                        //
                        // Add in the to recipients
                        //
                        if (to != null)
                        {
                            for (int i = 0; i < to.Count; i++)
                            {
                                ExternalCommunicationRecipient ecr = new ExternalCommunicationRecipient();

                                ecr.externalCommunicationId = externalComm.id;

                                string recipient = to[i];

                                if (recipient != null)
                                {
                                    recipient = recipient.Trim();

                                    if (recipient.Length > 100)
                                    {
                                        recipient = recipient.Substring(0, 100);
                                    }

                                    ecr.recipient = recipient;
                                    ecr.type = ExternalCommunicationRecipientType.To.ToString();

                                    db.ExternalCommunicationRecipients.Add(ecr);
                                }
                            }
                        }


                        //
                        // Add in the cc recipients
                        //
                        if (cc != null)
                        {
                            for (int i = 0; i < cc.Count; i++)
                            {
                                ExternalCommunicationRecipient ecr = new ExternalCommunicationRecipient();

                                ecr.externalCommunicationId = externalComm.id;

                                string recipient = cc[i];

                                if (recipient != null)
                                {
                                    recipient = recipient.Trim();

                                    if (recipient.Length > 100)
                                    {
                                        recipient = recipient.Substring(0, 100);
                                    }

                                    ecr.recipient = recipient;
                                    ecr.type = ExternalCommunicationRecipientType.CC.ToString();

                                    db.ExternalCommunicationRecipients.Add(ecr);
                                }
                            }
                        }

                        //
                        // Add in the bcc recipients
                        //
                        if (bcc != null)
                        {
                            for (int i = 0; i < bcc.Count; i++)
                            {
                                ExternalCommunicationRecipient ecr = new ExternalCommunicationRecipient();

                                ecr.externalCommunicationId = externalComm.id;

                                string recipient = bcc[i];

                                if (recipient != null)
                                {
                                    recipient = recipient.Trim();

                                    if (recipient.Length > 100)
                                    {
                                        recipient = recipient.Substring(0, 100);
                                    }

                                    ecr.recipient = recipient;
                                    ecr.type = ExternalCommunicationRecipientType.BCC.ToString();

                                    db.ExternalCommunicationRecipients.Add(ecr);
                                }
                            }
                        }

                        db.SaveChanges();

                        transaction.Commit();
                    }
                }
            }
            catch (Exception createCommEx)
            {
                List<string> errors = null;

                if (createCommEx != null)
                {
                    errors = new List<string>();

                    //
                    // First put in the entire error written as a string
                    //
                    errors.Add(createCommEx.ToString());

                    Exception subEx = createCommEx;

                    while (subEx != null)
                    {
                        errors.Add(subEx.Message + " - " + subEx.ToString());
                        subEx = subEx.InnerException;
                    }
                }

                CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, false, AuditAccessType.Ambiguous, AuditType.Error, user, null, "CreateExternalCommunication", null, null, null, null, null, null, null, "Could not create External Communication.", null, null, errors);
            }
        }

        public async Task<bool> CreateAuditEventAsync(DateTime startTime,
                                   DateTime stopTime,
                                   bool completedSuccessfully,
                                   AuditAccessType accessType,
                                   AuditType auditType,
                                   string user,
                                   string session,
                                   string source,
                                   string userAgent,
                                   string module,
                                   string moduleEntity,
                                   string resource,
                                   string hostSystem,
                                   string primaryKey,
                                   int? threadId,
                                   string message,
                                   string entityBeforeState,
                                   string entityAfterState,
                                   List<string> errorMessages)
        {
            EventDetails e = new EventDetails(startTime, stopTime, completedSuccessfully, accessType, auditType, user, session, source, userAgent, module, moduleEntity, resource, hostSystem, primaryKey, threadId, message, entityBeforeState, entityAfterState, errorMessages);

            if (auditorMode == AuditorMode.MemoryQueueWithOneMinuteFlush)
            {
                //
                // A recurring job runs each minute to flush the memory queue to the database
                //
                CreateAuditEventInMemoryQueue(e);
            }
            else if (auditorMode == AuditorMode.DispatchToBackgroundImmediately)
            {
                //
                // use a background job to create this log message in the background.  This is at least 10 times faster than using the direct call to CreateAuditEvent because it pushes the expensive write to the heavily indexed log tables out of this process
                //
                try
                {
                    BackgroundJob.Enqueue(() => CreateAuditEvent(e));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Auditor background job queuing error.  Details are: " + ex.ToString());
                }

            }
            else if (auditorMode == AuditorMode.InProcess)
            {
                // run the audit event creation in the current thread
                await CreateAuditEventAsync(e);
            }

            return true;
        }


        public void CreateAuditEvent(DateTime startTime,
                                   DateTime stopTime,
                                   bool completedSuccessfully,
                                   AuditAccessType accessType,
                                   AuditType auditType,
                                   string user,
                                   string session,
                                   string source,
                                   string userAgent,
                                   string module,
                                   string moduleEntity,
                                   string resource,
                                   string hostSystem,
                                   string primaryKey,
                                   int? threadId,
                                   string message,
                                   string entityBeforeState,
                                   string entityAfterState,
                                   List<string> errorMessages)
        {
            EventDetails e = new EventDetails(startTime, stopTime, completedSuccessfully, accessType, auditType, user, session, source, userAgent, module, moduleEntity, resource, hostSystem, primaryKey, threadId, message, entityBeforeState, entityAfterState, errorMessages);

            if (auditorMode == AuditorMode.MemoryQueueWithOneMinuteFlush)
            {
                //
                // Runs a recurring job each minute to flush the memory queue to the database
                //
                CreateAuditEventInMemoryQueue(e);
            }
            else if (auditorMode == AuditorMode.DispatchToBackgroundImmediately)
            {
                //
                // Create this log message in the background.  This is at least 10 times faster than using the direct call to CreateAuditEvent because it pushes the expensive write to the heavily indexed log tables out of this process
                //
                try
                {
                    BackgroundJob.Enqueue(() => CreateAuditEvent(e));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Auditor background job queuing error.  Details are: " + ex.ToString());
                }

            }
            else if (auditorMode == AuditorMode.InProcess)
            {
                // run the audit event creation in the current thread
                CreateAuditEvent(e);
            }
        }

        private void CreateAuditEventInMemoryQueue(EventDetails e)
        {
            lock (syncRoot)
            {
                queuedEvents.Add(e);
            }
        }


        public static void FlushMemoryQueueToDatabase()
        {
            //
            // Copy all of the events over to a work list and then write them out one at a time
            //
            List<EventDetails> eventsToFlush = new List<EventDetails>();

            lock (AuditEngine.syncRoot)
            {
                if (AuditEngine.Instance.queuedEvents.Count > 0)
                {
                    eventsToFlush.AddRange(AuditEngine.Instance.queuedEvents);

                    AuditEngine.Instance.queuedEvents.Clear();
                }
            }

            foreach (EventDetails e in eventsToFlush)
            {
                // this makes a bunch of threads simulataneously attempt to write data at the same time.  That often leads to db insert conflicts which causes logging failures.  Instead of this, we're going to write one row at a time.
                //var jobId = BackgroundJob.Enqueue(() => CreateAuditEvent(e));

                //
                // This write the log messages sequentially, which keeps the order correct, rather than multiple threads writing together which change the order up
                //
                CreateAuditEvent(e);
            }

            return;
        }

        public void CreateAuditEvent(string message)
        {
            DateTime startTime = DateTime.UtcNow;
            DateTime stopTime = DateTime.UtcNow;

            bool completedSuccessfully = true;
            AuditAccessType accessType = AuditAccessType.Ambiguous;
            AuditType auditType = AuditType.Miscellaneous;

            string user = "System";
            string source = "System";
            string userAgent = "System";
            string module = "System";
            string moduleEntity = "System";
            string resource = "System";
            string hostSystem = "System";
            string primaryKey = "";
            string session = "System";
            int? threadId = null;
            string entityBeforeState = null;
            string entityAfterState = null;
            List<string> errorMessages = null;

            CreateAuditEvent(startTime, stopTime, completedSuccessfully, accessType, auditType, user, session, source, userAgent, module, moduleEntity, resource, hostSystem, primaryKey, threadId, message, entityBeforeState, entityAfterState, errorMessages);
        }

        public void CreateAuditEvent(string message, Exception ex)
        {
            DateTime startTime = DateTime.UtcNow;
            DateTime stopTime = DateTime.UtcNow;

            bool completedSuccessfully = false;
            AuditAccessType accessType = AuditAccessType.Ambiguous;
            AuditType auditType = AuditType.Miscellaneous;

            string user = "System";
            string source = "System";
            string userAgent = "System";
            string module = "System";
            string moduleEntity = "System";
            string resource = "System";
            string hostSystem = "System";
            string session = "System";
            string primaryKey = "";
            int? threadId = null;
            string entityBeforeState = null;
            string entityAfterState = null;


            List<string> errors = null;

            if (ex != null)
            {
                errors = new List<string>();

                System.Exception subEx = ex;


                //
                // First put in the entire error written as a string
                //
                errors.Add(ex.ToString());

                while (subEx != null)
                {
                    errors.Add(subEx.Message + " - " + subEx.ToString());
                    subEx = subEx.InnerException;
                }
            }

            CreateAuditEvent(startTime, stopTime, completedSuccessfully, accessType, auditType, user, session, source, userAgent, module, moduleEntity, resource, hostSystem, primaryKey, threadId, message, entityBeforeState, entityAfterState, errors);
        }


        public async static Task<bool> CreateAuditEventAsync(EventDetails e)
        {
            if (e == null)
            {
                return false;
            }

            using (AuditorContext db = new AuditorContext())
            {
                try
                {
                    if (e.user == null || e.user.Length == 0)
                    {
                        e.user = "Unknown";
                    }
                    if (e.session == null || e.session.Length == 0)
                    {
                        e.session = "Unknown";
                    }
                    if (e.source == null || e.source.Length == 0)
                    {
                        e.source = "Unknown";
                    }
                    if (e.userAgent == null || e.userAgent.Length == 0)
                    {
                        e.userAgent = "Unknown";
                    }
                    if (e.module == null || e.module.Length == 0)
                    {
                        e.module = "Unknown";
                    }
                    if (e.moduleEntity == null || e.moduleEntity.Length == 0)
                    {
                        e.moduleEntity = "Unknown";
                    }
                    if (e.resource == null || e.resource.Length == 0)
                    {
                        e.resource = "Unknown";
                    }
                    if (e.hostSystem == null || e.hostSystem.Length == 0)
                    {
                        e.hostSystem = "Unknown";
                    }

                    //
                    // Get the ancillaries first.
                    //
                    int repeatCounter = 0;
                    AuditUser au = null;
                    AuditSource _as = null;
                    AuditUserAgent aua = null;
                    AuditModule am = null;
                    AuditModuleEntity ame = null;
                    AuditResource ar = null;
                    AuditHostSystem ahs = null;
                    AuditSession asess = null;

                    //
                    // keep going while retry counter is less than 10 and we have any object that is still null
                    //        
                    while (repeatCounter < 10 && (au == null || _as == null || _as == null || aua == null || am == null || ame == null || ar == null || ahs == null || asess == null))
                    {
                        repeatCounter++;

                        try
                        {
                            if (au == null)
                            {
                                au = AuditEngine.GetAuditUser(e.user, db);
                            }

                            if (_as == null)
                            {
                                _as = AuditEngine.GetAuditSource(e.source, db);
                            }

                            if (aua == null)
                            {
                                aua = AuditEngine.GetAuditUserAgent(e.userAgent, db);
                            }

                            if (am == null)
                            {
                                am = AuditEngine.GetAuditModule(e.module, db);
                            }

                            if (ame == null)
                            {
                                ame = AuditEngine.GetAuditModuleEntity(e.module, e.moduleEntity, db);
                            }

                            if (ar == null)
                            {
                                ar = AuditEngine.GetAuditResource(e.resource, db);
                            }

                            if (ahs == null)
                            {
                                ahs = AuditEngine.GetAuditHostSystem(e.hostSystem, db);
                            }

                            if (asess == null)
                            {
                                asess = AuditEngine.GetAuditSession(e.session, db);
                            }

                        }
                        catch (Exception)
                        {
                            System.Threading.Thread.Sleep(100 * repeatCounter);
                            // do nothing.  Most likely cause is a SQL error on trying to insert something that is already there. In that case, a retry will probably fix it.  If that fails, then planb will catch it.
                        }
                    }

                    if (au != null && au != null && asess != null && _as != null && aua != null && am != null && ame != null && ar != null && ahs != null)
                    {

                        AuditEvent newEvent = new AuditEvent();

                        newEvent.startTime = e.startTime;
                        newEvent.stopTime = e.stopTime;

                        newEvent.completedSuccessfully = e.completedSuccessfully;
                        newEvent.auditAccessTypeId = (int)e.accessType;
                        newEvent.auditTypeId = (int)e.auditType;

                        newEvent.auditUserId = au.id;
                        newEvent.auditSessionId = asess.id;
                        newEvent.auditSourceId = _as.id;
                        newEvent.auditUserAgentId = aua.id;
                        newEvent.auditModuleId = am.id;
                        newEvent.auditModuleEntityId = ame.id;
                        newEvent.auditResourceId = ar.id;
                        newEvent.auditHostSystemId = ahs.id;
                        newEvent.threadId = e.threadId;


                        // primary key is limited to 250
                        string primaryKey = null;

                        if (e.primaryKey != null) {
                            if (e.primaryKey.Length <= 250)
                            {
                                primaryKey = e.primaryKey;
                            }
                            else
                            {
                                primaryKey = e.primaryKey.Substring(0, 250);
                            }
                        }

                        newEvent.primaryKey = primaryKey;

                        // Message is nvarchar(max).
                        newEvent.message = e.message;

                        // put in the new event
                        db.AuditEvents.Add(newEvent);

                        // save it
                        await db.SaveChangesAsync();

                        //
                        // If in debugger, put message into output window
                        //
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Debug.WriteLine($"Audit Event Written: {e.message}");
                        }

                        // add additional state info if it has been provided.
                        if (e.entityBeforeState != null && e.entityBeforeState.Length > 0 ||
                            e.entityAfterState != null && e.entityAfterState.Length > 0)
                        {
                            AuditEventEntityState newState = new AuditEventEntityState();

                            newState.auditEventId = newEvent.id;

                            newState.beforeState = e.entityBeforeState == null ? "" : e.entityBeforeState;
                            newState.afterState = e.entityAfterState == null ? "" : e.entityAfterState;

                            db.AuditEventEntityStates.Add(newState);

                            await db.SaveChangesAsync();
                        }

                        // add error messages if they have been provided.
                        if (e.errorMessages != null &&
                            e.errorMessages.Count > 0)
                        {
                            for (int i = 0; i < e.errorMessages.Count; i++)
                            {
                                if (e.errorMessages[i] != null)
                                {
                                    AuditEventErrorMessage newErrorMessage = new AuditEventErrorMessage();

                                    newErrorMessage.auditEventId = newEvent.id;
                                    newErrorMessage.errorMessage = e.errorMessages[i];

                                    db.AuditEventErrorMessages.Add(newErrorMessage);
                                }
                            }

                            // let this go off and do it's thing
                            await db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        throw new Exception("Could not get audit user or other audit object.  Reverting to PlanB");
                    }
                }
                catch (Exception ex)
                {
                    //
                    // Try writing to the fall back table here.  If that doesn't work, then simply stop.  We don't want the audit failure to stop the execution of the primary program.
                    //
                    try
                    {
                        ExecutePlanB(e, ex);
                    }
                    catch
                    {
                        // do nothing further here.
                    }
                }
            }

            return true;
        }


        public static void CreateAuditEvent(EventDetails e)
        {
            if (e == null)
            {
                return;
            }

            using (AuditorContext db = new AuditorContext())
            {
                try
                {
                    if (e.user == null || e.user.Length == 0)
                    {
                        e.user = "Unknown";
                    }
                    if (e.session == null || e.session.Length == 0)
                    {
                        e.session = "Unknown";
                    }
                    if (e.source == null || e.source.Length == 0)
                    {
                        e.source = "Unknown";
                    }
                    if (e.userAgent == null || e.userAgent.Length == 0)
                    {
                        e.userAgent = "Unknown";
                    }
                    if (e.module == null || e.module.Length == 0)
                    {
                        e.module = "Unknown";
                    }
                    if (e.moduleEntity == null || e.moduleEntity.Length == 0)
                    {
                        e.moduleEntity = "Unknown";
                    }
                    if (e.resource == null || e.resource.Length == 0)
                    {
                        e.resource = "Unknown";
                    }
                    if (e.hostSystem == null || e.hostSystem.Length == 0)
                    {
                        e.hostSystem = "Unknown";
                    }

                    //
                    // Get the ancillaries first.
                    //
                    int repeatCounter = 0;
                    AuditUser au = null;
                    AuditSource _as = null;
                    AuditUserAgent aua = null;
                    AuditModule am = null;
                    AuditModuleEntity ame = null;
                    AuditResource ar = null;
                    AuditHostSystem ahs = null;
                    AuditSession asess = null;

                    //
                    // keep going while retry counter is less than 10 and we have any object that is still null
                    //        
                    while (repeatCounter < 10 && (au == null || _as == null || _as == null || aua == null || am == null || ame == null || ar == null || ahs == null || asess == null))
                    {
                        repeatCounter++;

                        try
                        {
                            if (au == null)
                            {
                                au = AuditEngine.GetAuditUser(e.user, db);
                            }

                            if (_as == null)
                            {
                                _as = AuditEngine.GetAuditSource(e.source, db);
                            }

                            if (aua == null)
                            {
                                aua = AuditEngine.GetAuditUserAgent(e.userAgent, db);
                            }

                            if (am == null)
                            {
                                am = AuditEngine.GetAuditModule(e.module, db);
                            }

                            if (ame == null)
                            {
                                ame = AuditEngine.GetAuditModuleEntity(e.module, e.moduleEntity, db);
                            }

                            if (ar == null)
                            {
                                ar = AuditEngine.GetAuditResource(e.resource, db);
                            }

                            if (ahs == null)
                            {
                                ahs = AuditEngine.GetAuditHostSystem(e.hostSystem, db);
                            }

                            if (asess == null)
                            {
                                asess = AuditEngine.GetAuditSession(e.session, db);
                            }

                        }
                        catch (Exception ex)
                        {

                            System.Diagnostics.Debug.WriteLine("Caught error trying to get id for audit sub entity. Exception is " + ex.ToString());
                            System.Threading.Thread.Sleep(100 * repeatCounter);
                            // do nothing.  Most likely cause is a SQL error on trying to insert something that is already there. In that case, a retry will probably fix it.  If that fails, then planb will catch it.
                        }
                    }

                    if (au != null && au != null && asess != null && _as != null && aua != null && am != null && ame != null && ar != null && ahs != null)
                    {

                        AuditEvent newEvent = new AuditEvent();

                        newEvent.startTime = e.startTime;
                        newEvent.stopTime = e.stopTime;

                        newEvent.completedSuccessfully = e.completedSuccessfully;
                        newEvent.auditAccessTypeId = (int)e.accessType;
                        newEvent.auditTypeId = (int)e.auditType;

                        newEvent.auditUserId = au.id;
                        newEvent.auditSessionId = asess.id;
                        newEvent.auditSourceId = _as.id;
                        newEvent.auditUserAgentId = aua.id;
                        newEvent.auditModuleId = am.id;
                        newEvent.auditModuleEntityId = ame.id;
                        newEvent.auditResourceId = ar.id;
                        newEvent.auditHostSystemId = ahs.id;
                        newEvent.threadId = e.threadId;
                        newEvent.primaryKey = e.primaryKey;
                        newEvent.message = e.message;

                        // put in the new event
                        db.AuditEvents.Add(newEvent);

                        // save it
                        db.SaveChanges();

                        //
                        // If in debugger, put message into output window
                        //
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Debug.WriteLine(e.message);
                        }

                        // add additional state info if it has been provided.
                        if (e.entityBeforeState != null && e.entityBeforeState.Length > 0 ||
                            e.entityAfterState != null && e.entityAfterState.Length > 0)
                        {
                            AuditEventEntityState newState = new AuditEventEntityState();

                            newState.auditEventId = newEvent.id;
                            //newState.primaryKey7 = e.primaryKey;

                            newState.beforeState = e.entityBeforeState == null ? "" : e.entityBeforeState;
                            newState.afterState = e.entityAfterState == null ? "" : e.entityAfterState;

                            db.AuditEventEntityStates.Add(newState);

                            db.SaveChanges();
                        }

                        // add error messages if they have been provided.
                        if (e.errorMessages != null &&
                            e.errorMessages.Count > 0)
                        {
                            for (int i = 0; i < e.errorMessages.Count; i++)
                            {
                                if (e.errorMessages[i] != null)
                                {
                                    AuditEventErrorMessage newErrorMessage = new AuditEventErrorMessage();

                                    newErrorMessage.auditEventId = newEvent.id;
                                    newErrorMessage.errorMessage = e.errorMessages[i];

                                    db.AuditEventErrorMessages.Add(newErrorMessage);
                                }
                            }

                            // let this go off and do it's thing
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        throw new Exception("Could not get audit user or other audit object.  Reverting to PlanB");
                    }
                }
                catch (Exception ex)
                {
                    //
                    // Try writing to the fall back table here.  If that doesn't work, then simply stop.  We don't want the audit failure to stop the execution of the primary program.
                    //
                    try
                    {
                        ExecutePlanB(e, ex);
                    }
                    catch
                    {
                        // do nothing further here.
                    }
                }
            }
        }


        private static void ExecutePlanB(EventDetails e, System.Exception ex)
        {
            try
            {

                // we want to use a new DB each time we try to go with Plan B.
                AuditorContext db = new AuditorContext();


                AuditPlanB newEvent = new AuditPlanB();

                newEvent.startTime = e.startTime;
                newEvent.stopTime = e.stopTime;

                newEvent.completedSuccessfully = e.completedSuccessfully;
                newEvent.accessType = e.accessType.ToString();
                newEvent.type = e.auditType.ToString();


                if (e.user == null || e.user.Length == 0)
                {
                    e.user = "Unknown";
                }
                newEvent.user = e.user;

                if (e.session == null || e.session.Length == 0)
                {
                    e.session = "Unknown";
                }
                newEvent.session = e.session;

                if (e.source == null || e.source.Length == 0)
                {
                    e.source = "Unknown";
                }
                newEvent.source = e.source;

                if (e.userAgent == null || e.userAgent.Length == 0)
                {
                    e.userAgent = "Unknown";
                }
                newEvent.userAgent = e.userAgent;

                if (e.module == null || e.module.Length == 0)
                {
                    e.module = "Unknown";
                }
                newEvent.module = e.module;

                if (e.moduleEntity == null || e.moduleEntity.Length == 0)
                {
                    e.moduleEntity = "Unknown";
                }
                newEvent.moduleEntity = e.moduleEntity;

                if (e.resource == null || e.resource.Length == 0)
                {
                    e.resource = "Unknown";
                }
                newEvent.resource = e.resource;


                if (e.hostSystem == null || e.hostSystem.Length == 0)
                {
                    e.hostSystem = "Unknown";
                }
                newEvent.hostSystem = e.hostSystem;

                newEvent.threadId = e.threadId;
                newEvent.primaryKey = e.primaryKey;
                newEvent.message = e.message;

                // put in the new event
                db.AuditPlanBs.Add(newEvent);


                // add additional state info if it has been provided.
                if (e.entityBeforeState != null && e.entityBeforeState.Length > 0 ||
                    e.entityAfterState != null && e.entityAfterState.Length > 0)
                {
                    newEvent.beforeState = e.entityBeforeState == null ? "" : e.entityBeforeState;
                    newEvent.afterState = e.entityAfterState == null ? "" : e.entityAfterState;
                }

                // add error messages if they have been provided.
                if (e.errorMessages != null &&
                    e.errorMessages.Count > 0)
                {
                    String concatenatedErrorMessage = "";

                    for (int i = 0; i < e.errorMessages.Count; i++)
                    {
                        if (e.errorMessages[i] != null)
                        {
                            concatenatedErrorMessage += e.errorMessages[i] + "<br/>";
                        }
                    }

                    newEvent.errorMessage = concatenatedErrorMessage;
                }


                // add failure reason messages if they have been provided.
                if (ex != null)
                {
                    String concatenatedErrorMessage = "";

                    Exception workEx = ex;

                    int counter = 0;
                    while (workEx != null && counter < 100)
                    {
                        concatenatedErrorMessage += workEx.Message + " - " + workEx.ToString() + "<br/>";
                        workEx = workEx.InnerException;
                        counter++;
                    }

                    newEvent.exceptionText = concatenatedErrorMessage;
                }

                db.SaveChanges();
            }
            catch
            {
                // do not ever throw a failure from here.
            }

            return;
        }


        private static AuditUser GetAuditUser(string user, AuditorContext db)
        {
            if (Instance.AuditUserCache.ContainsKey(user) == true)
            {
                return Instance.AuditUserCache[user];
            }
            else
            {
                AuditUser au = null;

                lock (memoryCacheSyncRoot)
                {
                    au = (from x in db.AuditUsers where x.name == user select x).AsNoTracking().FirstOrDefault();

                    if (au == null)
                    {
                        au = new AuditUser();

                        if (user.Length > 500)
                        {
                            user = user.Substring(0, 500);
                        }

                        au.name = user;

                        au.firstAccess = DateTime.UtcNow;

                        db.AuditUsers.Add(au);

                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    if (Instance.AuditUserCache.ContainsKey(user) == false)
                    {
                        Instance.AuditUserCache.Add(user, au);
                    }
                }

                return au;
            }
        }


        private static AuditSource GetAuditSource(string source, AuditorContext db)
        {
            if (Instance.AuditSourceCache.ContainsKey(source) == true)
            {
                return Instance.AuditSourceCache[source];
            }
            else
            {
                AuditSource _as = null;

                lock (memoryCacheSyncRoot)
                {
                    _as = (from x in db.AuditSources where x.name == source select x).AsNoTracking().FirstOrDefault();

                    if (_as == null)
                    {
                        _as = new AuditSource();

                        if (source.Length > 500)
                        {
                            source = source.Substring(0, 500);
                        }

                        _as.name = source;

                        _as.firstAccess = DateTime.UtcNow;

                        db.AuditSources.Add(_as);

                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }


                    if (Instance.AuditSourceCache.ContainsKey(source) == false)
                    {
                        Instance.AuditSourceCache.Add(source, _as);
                    }
                }

                return _as;
            }
        }


        private static AuditUserAgent GetAuditUserAgent(string userAgent, AuditorContext db)
        {
            if (Instance.AuditUserAgentCache.ContainsKey(userAgent) == true)
            {
                return Instance.AuditUserAgentCache[userAgent];
            }
            else
            {
                AuditUserAgent aua = null;

                lock (memoryCacheSyncRoot)
                {
                    aua = (from x in db.AuditUserAgents where x.name == userAgent select x).AsNoTracking().FirstOrDefault();

                    if (aua == null)
                    {
                        aua = new AuditUserAgent();

                        if (userAgent.Length > 500)
                        {
                            userAgent = userAgent.Substring(0, 500);
                        }


                        aua.name = userAgent;

                        aua.firstAccess = DateTime.UtcNow;

                        db.AuditUserAgents.Add(aua);

                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    if (Instance.AuditUserAgentCache.ContainsKey(userAgent) == false)
                    {
                        Instance.AuditUserAgentCache.Add(userAgent, aua);
                    }
                }

                return aua;
            }
        }



        private static AuditModule GetAuditModule(string module, AuditorContext db)
        {
            if (Instance.AuditModuleCache.ContainsKey(module) == true)
            {
                return Instance.AuditModuleCache[module];
            }
            else
            {
                AuditModule _am;

                lock (memoryCacheSyncRoot)
                {
                    _am = (from x in db.AuditModules where x.name.ToUpper() == module.ToUpper() select x).AsNoTracking().FirstOrDefault();

                    if (_am == null)
                    {
                        _am = new AuditModule();

                        if (module.Length > 500)
                        {
                            module = module.Substring(0, 500);
                        }

                        _am.name = module;

                        _am.firstAccess = DateTime.UtcNow;

                        db.AuditModules.Add(_am);

                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    if (Instance.AuditModuleCache.ContainsKey(module) == false)
                    {
                        Instance.AuditModuleCache.Add(module, _am);
                    }
                }

                return _am;
            }
        }


        private static AuditModuleEntity GetAuditModuleEntity(string module, string entity, AuditorContext db)
        {
            if (Instance.AuditModuleEntityCache.ContainsKey(module + "_" + entity) == true)
            {
                return Instance.AuditModuleEntityCache[module + "_" + entity];
            }
            else
            {
                AuditModuleEntity _ame;

                lock (memoryCacheSyncRoot)
                {
                    _ame = (from x in db.AuditModuleEntities
                            join y in db.AuditModules on x.auditModuleId equals y.id
                            where x.name == entity
                            select x).AsNoTracking().FirstOrDefault();

                    if (_ame == null)
                    {
                        AuditModule am = GetAuditModule(module, db);

                        if (am != null)
                        {
                            _ame = new AuditModuleEntity();

                            _ame.auditModuleId = am.id;

                            if (entity.Length > 500)
                            {
                                entity = entity.Substring(0, 500);
                            }

                            _ame.name = entity;

                            _ame.firstAccess = DateTime.UtcNow;

                            db.AuditModuleEntities.Add(_ame);

                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }

                    if (Instance.AuditModuleEntityCache.ContainsKey(module + "_" + entity) == false)
                    {
                        Instance.AuditModuleEntityCache.Add(module + "_" + entity, _ame);
                    }
                }

                return _ame;
            }
        }


        private static AuditResource GetAuditResource(string resource, AuditorContext db)
        {
            if (Instance.AuditResourceCache.ContainsKey(resource) == true)
            {
                return Instance.AuditResourceCache[resource];
            }
            else
            {
                AuditResource ar = null;

                lock (memoryCacheSyncRoot)
                {
                    ar = (from x in db.AuditResources where x.name == resource select x).AsNoTracking().FirstOrDefault();

                    if (ar == null)
                    {
                        ar = new AuditResource();

                        if (resource.Length > 850)
                        {
                            resource = resource.Substring(0, 850);
                        }

                        ar.name = resource;

                        ar.firstAccess = DateTime.UtcNow;

                        db.AuditResources.Add(ar);

                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Error caught during getting audit resource string.  Details are: " + ex.ToString());
                            throw;
                        }
                    }

                    if (Instance.AuditResourceCache.ContainsKey(resource) == false)
                    {
                        Instance.AuditResourceCache.Add(resource, ar);
                    }
                }

                return ar;
            }
        }


        private static AuditHostSystem GetAuditHostSystem(string hostSystem, AuditorContext db)
        {
            if (Instance.AuditHostSystemCache.ContainsKey(hostSystem) == true)
            {
                return Instance.AuditHostSystemCache[hostSystem];
            }
            else
            {
                AuditHostSystem _ahs = null;

                lock (memoryCacheSyncRoot)
                {
                    _ahs = (from x in db.AuditHostSystems where x.name == hostSystem select x).AsNoTracking().FirstOrDefault();

                    if (_ahs == null)
                    {
                        _ahs = new AuditHostSystem();

                        if (hostSystem.Length > 500)
                        {
                            hostSystem = hostSystem.Substring(0, 500);
                        }


                        _ahs.name = hostSystem;

                        _ahs.firstAccess = DateTime.UtcNow;

                        db.AuditHostSystems.Add(_ahs);

                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    if (Instance.AuditHostSystemCache.ContainsKey(hostSystem) == false)
                    {
                        Instance.AuditHostSystemCache.Add(hostSystem, _ahs);
                    }
                }

                return _ahs;
            }
        }

        private static AuditSession GetAuditSession(string session, AuditorContext db)
        {
            if (Instance.AuditSessionCache.ContainsKey(session) == true)
            {
                return Instance.AuditSessionCache[session];
            }
            else
            {
                AuditSession _asess = null;

                lock (memoryCacheSyncRoot)
                {
                    _asess = (from x in db.AuditSessions where x.name.ToUpper() == session.ToUpper() select x).AsNoTracking().FirstOrDefault();

                    if (_asess == null)
                    {
                        _asess = new AuditSession();

                        if (session.Length > 500)
                        {
                            session = session.Substring(0, 500);
                        }

                        _asess.name = session;

                        _asess.firstAccess = DateTime.UtcNow;

                        db.AuditSessions.Add(_asess);

                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }


                    if (Instance.AuditSessionCache.ContainsKey(session) == false)
                    {
                        Instance.AuditSessionCache.Add(session, _asess);
                    }
                }

                return _asess;
            }
        }


        #region PurgingFunctions

        private const int ROWS_TO_DELETE_IN_EACH_PASS = 10000;


        public class idt_
        {
            public int id { get; set; }
        }

        public static void PurgeAuditEvents(int daysToKeep)
        {
            using (AuditorContext db = new AuditorContext())
            {

                if (daysToKeep <= 0)
                {
                    return;
                }

                Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Beginning audit log purge process.  The days to keep value is " + daysToKeep.ToString(), null, null, null);

                lock (AuditEngine.purgeSyncRoot)
                {
                    try
                    {
                        DateTime firstDayToKeep = DateTime.UtcNow.AddDays(-1 * daysToKeep);

                        // remove the error messages and entity state children as a first step

                        // first blow away the entity states linked to the events we want to remove
                        int removalCount = (from aees in db.AuditEventEntityStates
                                            join ae in db.AuditEvents on aees.auditEventId equals ae.id
                                            where ae.startTime <= firstDayToKeep
                                            select aees).Count();


                        Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Preparing to remove " + removalCount.ToString() + " entity state records.", null, null, null);

                        int rowsLeftToRemove = removalCount;

                        //
                        // Delete in blocks to allow large deletion sets to actually work without running out of memory
                        //
                        while (rowsLeftToRemove > 0)
                        {
                            using (AuditorContext iterationDB = new AuditorContext())
                            {
                                //iterationDB.Database.CommandTimeout = 60000;

                                // Get the count of rows to remove this pass.  All that is left, or the max, depending on the number of rows
                                int rowsToRemoveThisPass = (rowsLeftToRemove > ROWS_TO_DELETE_IN_EACH_PASS ? ROWS_TO_DELETE_IN_EACH_PASS : rowsLeftToRemove);

                                // Get a list of the rows to remove                        
                                var aeesToRemoveThisPass = (from aees in iterationDB.AuditEventEntityStates
                                                            join ae in iterationDB.AuditEvents on aees.auditEventId equals ae.id
                                                            where ae.startTime <= firstDayToKeep
                                                            select aees).Take(rowsToRemoveThisPass).ToList();

                                // Take them out of the EF context
                                iterationDB.AuditEventEntityStates.RemoveRange(aeesToRemoveThisPass);

                                // Save the changes to the DB
                                iterationDB.SaveChanges();

                                rowsLeftToRemove -= rowsToRemoveThisPass;

                                Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Removed " + rowsToRemoveThisPass.ToString() + " entity state records.  There are " + rowsLeftToRemove + " remaining to remove.", null, null, null);
                            }
                        }


                        //
                        // Now remove the error messages linked to the events to remove
                        //
                        removalCount = (from aeem in db.AuditEventErrorMessages
                                        join ae in db.AuditEvents on aeem.auditEventId equals ae.id
                                        where ae.startTime <= firstDayToKeep
                                        select aeem).Count();


                        Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Preparing to remove " + removalCount.ToString() + " error message records.", null, null, null);

                        rowsLeftToRemove = removalCount;

                        //
                        // Delete in blocks to allow large deletion sets to actually work without running out of memory
                        //
                        while (rowsLeftToRemove > 0)
                        {
                            using (AuditorContext iterationDB = new AuditorContext())
                            {
                                //iterationDB.Database.CommandTimeout = 60000;

                                // Get the count of rows to remove this pass.  All that is left, or the max, depending on the number of rows
                                int rowsToRemoveThisPass = (rowsLeftToRemove > ROWS_TO_DELETE_IN_EACH_PASS ? ROWS_TO_DELETE_IN_EACH_PASS : rowsLeftToRemove);

                                // Get a list of the rows to remove                        
                                var aeemToRemoveThisPass = (from aees in iterationDB.AuditEventErrorMessages
                                                            join ae in iterationDB.AuditEvents on aees.auditEventId equals ae.id
                                                            where ae.startTime <= firstDayToKeep
                                                            select aees).Take(rowsToRemoveThisPass).ToList();

                                // Take them out of the EF context
                                iterationDB.AuditEventErrorMessages.RemoveRange(aeemToRemoveThisPass);

                                // Save the changes to the DB
                                iterationDB.SaveChanges();

                                rowsLeftToRemove -= rowsToRemoveThisPass;

                                Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Removed " + rowsToRemoveThisPass.ToString() + " error message records.  There are " + rowsLeftToRemove + " remaining to remove.", null, null, null);
                            }
                        }


                        //
                        // now remove the audit events
                        //
                        removalCount = (from ae in db.AuditEvents
                                        where ae.startTime <= firstDayToKeep
                                        select ae).Count();


                        Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Preparing to remove " + removalCount.ToString() + " event records.", null, null, null);

                        rowsLeftToRemove = removalCount;

                        //
                        // Delete in blocks to allow large deletion sets to actually work without running out of memory
                        //
                        while (rowsLeftToRemove > 0)
                        {
                            using (AuditorContext iterationDB = new AuditorContext())
                            {
                                //iterationDB.Database.CommandTimeout = 60000;


                                // Get the count of rows to remove this pass.  All that is left, or the max, depending on the number of rows
                                int rowsToRemoveThisPass = (rowsLeftToRemove > ROWS_TO_DELETE_IN_EACH_PASS ? ROWS_TO_DELETE_IN_EACH_PASS : rowsLeftToRemove);

                                // Get a list of the rows to remove                        
                                var aeToRemoveThisPass = (from ae in iterationDB.AuditEvents
                                                          where ae.startTime <= firstDayToKeep
                                                          select ae).Take(rowsToRemoveThisPass).ToList();

                                //
                                // The EF RemoveRange and SaveChanges is unrealistically slow in deleting from the AuditEvent table.  Use SQL instead.
                                //
                                StringBuilder sb = new StringBuilder();
                                sb.Append("DELETE FROM AuditEvent WHERE id IN ( ");
                                bool first = true;
                                foreach (var ae in aeToRemoveThisPass)
                                {
                                    if (first == false)
                                    {
                                        sb.Append(", ");
                                    }

                                    sb.Append(ae.id.ToString());

                                    first = false;
                                }
                                sb.Append(" )");
                                var rowsDeleted = iterationDB.Database.ExecuteSqlRaw(sb.ToString());

                                rowsLeftToRemove -= rowsToRemoveThisPass;

                                Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Removed " + rowsToRemoveThisPass.ToString() + " event records.  There are " + rowsLeftToRemove + " remaining to remove.", null, null, null);
                            }
                        }

                        //
                        // Now remove any planBs from the time period
                        //
                        removalCount = (from ae in db.AuditPlanBs
                                        where ae.startTime <= firstDayToKeep
                                        select ae).Count();


                        Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Preparing to remove " + removalCount.ToString() + " plan B records.", null, null, null);

                        rowsLeftToRemove = removalCount;

                        //
                        // Delete in blocks to allow large deletion sets to actually work without running out of memory
                        //
                        while (rowsLeftToRemove > 0)
                        {
                            using (AuditorContext iterationDB = new AuditorContext())
                            {
                                //iterationDB.Database.CommandTimeout = 60000;

                                // Get the count of rows to remove this pass.  All that is left, or 10000, depending on the number of rows
                                int rowsToRemoveThisPass = (rowsLeftToRemove > ROWS_TO_DELETE_IN_EACH_PASS ? ROWS_TO_DELETE_IN_EACH_PASS : rowsLeftToRemove);

                                // Get a list of the rows to remove                        
                                var apbToRemoveThisPass = (from apb in iterationDB.AuditPlanBs
                                                           where apb.startTime <= firstDayToKeep
                                                           select apb).Take(rowsToRemoveThisPass).ToList();

                                // Take them out of the EF context
                                iterationDB.AuditPlanBs.RemoveRange(apbToRemoveThisPass);

                                // Save the changes to the DB
                                iterationDB.SaveChanges();

                                rowsLeftToRemove -= rowsToRemoveThisPass;

                                Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Removed " + rowsToRemoveThisPass.ToString() + " plan B records.  There are " + rowsLeftToRemove + " remaining to remove.", null, null, null);
                            }
                        }


                        //
                        // Now blow away any orphaned Audit Resources
                        //
                        object[] _params = new object[0];

                        //List<idt> orphanedResourceIds = db.Database.SqlQuery<idt>("SELECT ar.id FROM AuditResource ar LEFT OUTER JOIN AuditEvent ae ON ar.id = ae.auditResourceId WHERE ae.id IS NULL ORDER BY ar.id", _params).ToList();


                        List<idt_> orphanedResourceIds = (from ar in db.AuditResources
                                                          join ae in db.AuditEvents on ar.id equals ae.auditResourceId into aeJoin
                                                          from aeLeft in aeJoin.DefaultIfEmpty()
                                                          where aeLeft == null
                                                          select new idt_()
                                                          {
                                                              id = ar.id
                                                          }).ToList();



                        removalCount = orphanedResourceIds.Count();
                        Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Preparing to remove " + removalCount.ToString() + " resource records.", null, null, null);

                        rowsLeftToRemove = removalCount;

                        //
                        // Delete in blocks to allow large deletion sets to actually work without running out of memory
                        //
                        while (rowsLeftToRemove > 0)
                        {
                            using (AuditorContext iterationDB = new AuditorContext())
                            {
                                //iterationDB.Database.CommandTimeout = 60000;

                                // Get the count of rows to remove this pass.  All that is left, or the max, depending on the number of rows
                                int rowsToRemoveThisPass = (rowsLeftToRemove > ROWS_TO_DELETE_IN_EACH_PASS ? ROWS_TO_DELETE_IN_EACH_PASS : rowsLeftToRemove);

                                // Get a list of the rows to remove     
                                var arToRemoveThisPass = (from ar in orphanedResourceIds select ar).Take(rowsToRemoveThisPass).ToList();

                                //
                                // The EF RemoveRange and SaveChanges is unrealistically slow in deleting from the AuditEvent table.  Use SQL instead.
                                //
                                if (arToRemoveThisPass != null && arToRemoveThisPass.Count() > 0)
                                {
                                    StringBuilder sb = new StringBuilder();

                                    sb.Append("DELETE FROM AuditResource WHERE id IN ( ");
                                    bool first = true;
                                    foreach (var ar in arToRemoveThisPass)
                                    {
                                        if (first == false)
                                        {
                                            sb.Append(", ");
                                        }

                                        sb.Append(ar.id.ToString());

                                        first = false;
                                    }
                                    sb.Append(" )");

                                    var rowsDeleted = iterationDB.Database.ExecuteSqlRaw(sb.ToString());

                                    rowsLeftToRemove -= rowsDeleted;

                                    // take the ones we are deleting out of the work list
                                    orphanedResourceIds.RemoveRange(0, arToRemoveThisPass.Count());

                                    Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Removed " + rowsDeleted.ToString() + " resource records.  There are " + rowsLeftToRemove + " remaining to remove.", null, null, null);
                                }
                                else
                                {
                                    rowsLeftToRemove = 0;
                                }
                            }
                        }


                        //
                        // Now blow away any orphaned Audit sources
                        //
                        //List<idt> orphanedSourceIds = db.Database.FromSqlRaw("SELECT asrc.id FROM AuditSource asrc LEFT OUTER JOIN AuditEvent ae ON asrc.id = ae.auditSourceId WHERE ae.id IS NULL ORDER BY asrc.id", _params).ToList();

                        List<idt_> orphanedSourceIds = (from @as in db.AuditSources
                                                        join ae in db.AuditEvents on @as.id equals ae.auditResourceId into aeJoin
                                                        from aeLeft in aeJoin.DefaultIfEmpty()
                                                        where aeLeft == null
                                                        select new idt_()
                                                        {
                                                            id = @as.id
                                                        }).ToList();

                        removalCount = orphanedSourceIds.Count();
                        Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Preparing to remove " + removalCount.ToString() + " source records.", null, null, null);

                        rowsLeftToRemove = removalCount;

                        //
                        // Delete in blocks to allow large deletion sets to actually work without running out of memory
                        //
                        while (rowsLeftToRemove > 0)
                        {
                            using (AuditorContext iterationDB = new AuditorContext())
                            {
                                //iterationDB.Database.CommandTimeout = 60000;

                                // Get the count of rows to remove this pass.  All that is left, or the max, depending on the number of rows
                                int rowsToRemoveThisPass = (rowsLeftToRemove > ROWS_TO_DELETE_IN_EACH_PASS ? ROWS_TO_DELETE_IN_EACH_PASS : rowsLeftToRemove);

                                // Get a list of the rows to remove                        
                                var asrcToRemoveThisPass = (from _as in orphanedSourceIds select _as).Take(rowsToRemoveThisPass).ToList();

                                //
                                // The EF RemoveRange and SaveChanges is unrealistically slow in deleting from the AuditEvent table.  Use SQL instead.
                                //
                                if (asrcToRemoveThisPass != null && asrcToRemoveThisPass.Count() > 0)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append("DELETE FROM AuditSource WHERE id IN ( ");
                                    bool first = true;
                                    foreach (var asrc in asrcToRemoveThisPass)
                                    {
                                        if (first == false)
                                        {
                                            sb.Append(", ");
                                        }

                                        sb.Append(asrc.id.ToString());

                                        first = false;
                                    }
                                    sb.Append(" )");
                                    var rowsDeleted = iterationDB.Database.ExecuteSqlRaw(sb.ToString());

                                    rowsLeftToRemove -= rowsDeleted;

                                    // take the ones we are deleting out of the work list
                                    orphanedSourceIds.RemoveRange(0, asrcToRemoveThisPass.Count());

                                    Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Removed " + rowsDeleted.ToString() + " source records.  There are " + rowsLeftToRemove + " remaining to remove.", null, null, null);
                                }
                                else
                                {
                                    rowsLeftToRemove = 0;
                                }
                            }
                        }


                        //
                        // Now blow away any orphaned user agents
                        //
                        // List<idt> orphanedUserAgentIds = db.Database.SqlQuery<idt>("SELECT aua.id FROM AuditUserAgent aua LEFT OUTER JOIN AuditEvent ae ON aua.id = ae.auditUserAgentId WHERE ae.id IS NULL ORDER BY aua.id", _params).ToList();

                        List<idt_> orphanedUserAgentIds = (from aua in db.AuditUserAgents
                                                           join ae in db.AuditEvents on aua.id equals ae.auditResourceId into aeJoin
                                                           from aeLeft in aeJoin.DefaultIfEmpty()
                                                           where aeLeft == null
                                                           select new idt_()
                                                           {
                                                               id = aua.id
                                                           }).ToList();


                        removalCount = orphanedUserAgentIds.Count();
                        Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Preparing to remove " + removalCount.ToString() + " user agent records.", null, null, null);

                        rowsLeftToRemove = removalCount;

                        //
                        // Delete in blocks to allow large deletion sets to actually work without running out of memory
                        //
                        while (rowsLeftToRemove > 0)
                        {
                            using (AuditorContext iterationDB = new AuditorContext())
                            {
                                //iterationDB.Database.CommandTimeout = 60000;

                                // Get the count of rows to remove this pass.  All that is left, or the max, depending on the number of rows
                                int rowsToRemoveThisPass = (rowsLeftToRemove > ROWS_TO_DELETE_IN_EACH_PASS ? ROWS_TO_DELETE_IN_EACH_PASS : rowsLeftToRemove);

                                // Get a list of the rows to remove                        
                                var auaToRemoveThisPass = (from _as in orphanedUserAgentIds select _as).Take(rowsToRemoveThisPass);

                                //
                                // The EF RemoveRange and SaveChanges is unrealistically slow in deleting from the AuditEvent table.  Use SQL instead.
                                //
                                if (auaToRemoveThisPass != null && auaToRemoveThisPass.Count() > 0)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append("DELETE FROM AuditUserAgent WHERE id IN ( ");
                                    bool first = true;
                                    foreach (var aua in auaToRemoveThisPass)
                                    {
                                        if (first == false)
                                        {
                                            sb.Append(", ");
                                        }

                                        sb.Append(aua.id.ToString());

                                        first = false;
                                    }
                                    sb.Append(" )");
                                    var rowsDeleted = iterationDB.Database.ExecuteSqlRaw(sb.ToString());

                                    rowsLeftToRemove -= rowsDeleted;

                                    // take the ones we are deleting out of the work list
                                    orphanedUserAgentIds.RemoveRange(0, auaToRemoveThisPass.Count());



                                    Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Removed " + rowsDeleted.ToString() + " user agent records.  There are " + rowsLeftToRemove + " remaining to remove.", null, null, null);
                                }
                                else
                                {
                                    rowsLeftToRemove = 0;
                                }
                            }
                        }

                        //
                        // Now blow away any orphaned users
                        //
                        //List<idt> orphanedUserIds = db.Database.SqlQuery<idt>("SELECT au.id FROM AuditUser au LEFT OUTER JOIN AuditEvent ae ON au.id = ae.auditUserId WHERE ae.id IS NULL ORDER BY au.id", _params).ToList();

                        List<idt_> orphanedUserIds = (from au in db.AuditUsers
                                                      join ae in db.AuditEvents on au.id equals ae.auditResourceId into aeJoin
                                                      from aeLeft in aeJoin.DefaultIfEmpty()
                                                      where aeLeft == null
                                                      select new idt_()
                                                      {
                                                          id = au.id
                                                      }).ToList();

                        removalCount = orphanedUserIds.Count();
                        Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Preparing to remove " + removalCount.ToString() + " user records.", null, null, null);

                        rowsLeftToRemove = removalCount;

                        //
                        // Delete in blocks to allow large deletion sets to actually work without running out of memory
                        //
                        while (rowsLeftToRemove > 0)
                        {
                            using (AuditorContext iterationDB = new AuditorContext())
                            {
                                //iterationDB.Database.CommandTimeout = 60000;

                                // Get the count of rows to remove this pass.  All that is left, or the max, depending on the number of rows
                                int rowsToRemoveThisPass = (rowsLeftToRemove > ROWS_TO_DELETE_IN_EACH_PASS ? ROWS_TO_DELETE_IN_EACH_PASS : rowsLeftToRemove);

                                // Get a list of the rows to remove                        
                                var auToRemoveThisPass = (from _as in orphanedUserIds select _as).Take(rowsToRemoveThisPass);

                                //
                                // The EF RemoveRange and SaveChanges is unrealistically slow in deleting from the AuditEvent table.  Use SQL instead.
                                //
                                if (auToRemoveThisPass != null && auToRemoveThisPass.Count() > 0)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append("DELETE FROM AuditUser WHERE id IN ( ");
                                    bool first = true;
                                    foreach (var au in auToRemoveThisPass)
                                    {
                                        if (first == false)
                                        {
                                            sb.Append(", ");
                                        }

                                        sb.Append(au.id.ToString());

                                        first = false;
                                    }
                                    sb.Append(" )");
                                    var rowsDeleted = iterationDB.Database.ExecuteSqlRaw(sb.ToString());

                                    rowsLeftToRemove -= rowsDeleted;

                                    // take the ones we are deleting out of the work list
                                    orphanedUserIds.RemoveRange(0, auToRemoveThisPass.Count());


                                    Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Removed " + rowsDeleted.ToString() + " user records.  There are " + rowsLeftToRemove + " remaining to remove.", null, null, null);
                                }
                                else
                                {
                                    rowsLeftToRemove = 0;
                                }
                            }
                        }


                        //
                        // Now blow away any orphaned sessions
                        //
                        //List<idt> orphanedSessionIds = db.Database.SqlQuery<idt>("SELECT asess.id FROM AuditSession asess LEFT OUTER JOIN AuditEvent ae ON asess.id = ae.auditSessionId WHERE ae.id IS NULL ORDER BY asess.id", _params).ToList();

                        List<idt_> orphanedSessionIds = (from _as in db.AuditSessions
                                                         join ae in db.AuditEvents on _as.id equals ae.auditResourceId into aeJoin
                                                         from aeLeft in aeJoin.DefaultIfEmpty()
                                                         where aeLeft == null
                                                         select new idt_()
                                                         {
                                                             id = _as.id
                                                         }).ToList();

                        removalCount = orphanedSessionIds.Count();
                        Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Preparing to remove " + removalCount.ToString() + " session records.", null, null, null);

                        rowsLeftToRemove = removalCount;

                        //
                        // Delete in blocks to allow large deletion sets to actually work without running out of memory
                        //
                        while (rowsLeftToRemove > 0)
                        {
                            using (AuditorContext iterationDB = new AuditorContext())
                            {
                                //iterationDB.Database.CommandTimeout = 60000;

                                // Get the count of rows to remove this pass.  All that is left, or the max, depending on the number of rows
                                int rowsToRemoveThisPass = (rowsLeftToRemove > ROWS_TO_DELETE_IN_EACH_PASS ? ROWS_TO_DELETE_IN_EACH_PASS : rowsLeftToRemove);

                                // Get a list of the rows to remove                        
                                var asessToRemoveThisPass = (from _as in orphanedSessionIds select _as).Take(rowsToRemoveThisPass);

                                //
                                // The EF RemoveRange and SaveChanges is unrealistically slow in deleting from the AuditEvent table.  Use SQL instead.
                                //
                                if (asessToRemoveThisPass != null && asessToRemoveThisPass.Count() > 0)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append("DELETE FROM AuditSession WHERE id IN ( ");
                                    bool first = true;
                                    foreach (var asess in asessToRemoveThisPass)
                                    {
                                        if (first == false)
                                        {
                                            sb.Append(", ");
                                        }

                                        sb.Append(asess.id.ToString());

                                        first = false;
                                    }
                                    sb.Append(" )");
                                    var rowsDeleted = iterationDB.Database.ExecuteSqlRaw(sb.ToString());

                                    rowsLeftToRemove -= rowsDeleted;

                                    // take the ones we are deleting out of the work list
                                    orphanedSessionIds.RemoveRange(0, asessToRemoveThisPass.Count());


                                    Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Removed " + rowsDeleted.ToString() + " session records.  There are " + rowsLeftToRemove + " remaining to remove.", null, null, null);
                                }
                                else
                                {
                                    rowsLeftToRemove = 0;
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        List<string> errorMessages = new List<string>();

                        errorMessages.Add(ex.Message);

                        Exception recurseEx = ex;
                        while (recurseEx.InnerException != null)
                        {
                            recurseEx = recurseEx.InnerException;

                            errorMessages.Add(recurseEx.Message + " - " + recurseEx.ToString());
                        }

                        Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Error caught duruing audit purge process.", null, null, errorMessages);
                        throw;
                    }
                }

                Instance.CreateAuditEvent(DateTime.UtcNow, DateTime.UtcNow, true, AuditAccessType.Ambiguous, AuditType.Miscellaneous, null, null, null, null, null, null, null, null, null, System.Threading.Thread.CurrentThread.ManagedThreadId, "Completed audit log purge process.", null, null, null);
            }
            return;
        }

        #endregion
    }
}