using Foundation.CodeGeneration;

namespace Foundation.Hangfire.Database
{
    public class HangfireDatabaseScriptGenerator : DatabaseGenerator
    {
        public HangfireDatabaseScriptGenerator() : base("Hangfire", "Hangfire")
        {
            Database.Table aggregatedCounterTable = database.AddTable("AggregatedCounter");
            aggregatedCounterTable.AddString100Field("Key", false).primaryKey = true;
            aggregatedCounterTable.AddSingleField("Value");
            aggregatedCounterTable.AddLongField("ExpireAt").CreateIndex();


            Database.Table counterTable = database.AddTable("Counter");
            counterTable.AddString100Field("Id", false).primaryKey = true;
            counterTable.AddString100Field("Key");
            counterTable.AddSingleField("Value");
            counterTable.AddLongField("ExpireAt").CreateIndex();


            Database.Table distributedLockTable = database.AddTable("DistributedLock");
            distributedLockTable.AddString100Field("Id", false).primaryKey = true;
            distributedLockTable.AddTextField("Resource");
            distributedLockTable.AddTextField("ResourceKey");
            distributedLockTable.AddLongField("ExpireAt").CreateIndex();


            Database.Table hashTable = database.AddTable("Hash");
            var hashIdField = hashTable.AddIntField("Id", false);
            hashIdField.primaryKey = true;
            hashIdField.autoIncrement = true;

            hashTable.AddString100Field("Key");
            hashTable.AddString100Field("Field");
            hashTable.AddTextField("Value");
            hashTable.AddLongField("ExpireAt").CreateIndex();


            Database.Table jobTable = database.AddTable("Job");
            var jobIdField = jobTable.AddIntField("Id", false);
            jobIdField.primaryKey = true;
            jobIdField.autoIncrement = true;

            jobTable.AddIntField("StateId");
            jobTable.AddString100Field("StateName");
            jobTable.AddTextField("InvocationData");
            jobTable.AddTextField("Arguments");
            jobTable.AddLongField("CreatedAt");
            jobTable.AddLongField("ExpireAt").CreateIndex();


            Database.Table jobParameterTable = database.AddTable("JobParameter");
            var jpIdField = jobParameterTable.AddIntField("Id", false);
            jpIdField.primaryKey = true;
            jpIdField.autoIncrement = true;

            jobParameterTable.AddIntField("JobId");
            jobParameterTable.AddString100Field("name");
            jobParameterTable.AddTextField("Value");
            jobParameterTable.AddLongField("ExpireAt").CreateIndex();


            Database.Table jobQueueTable = database.AddTable("JobQueue");
            var jqIdField = jobQueueTable.AddIntField("Id", false);
            jqIdField.primaryKey = true;
            jqIdField.autoIncrement = true;

            jobQueueTable.AddIntField("JobId");
            jobQueueTable.AddString100Field("Queue").CreateIndex();
            jobQueueTable.AddLongField("FetchedAt");


            Database.Table listTable = database.AddTable("List");
            var listTableIdField = listTable.AddIntField("Id", false);
            listTableIdField.primaryKey = true;
            listTableIdField.autoIncrement = true;

            listTable.AddString100Field("Key");
            listTable.AddTextField("Value");
            listTable.AddLongField("ExpireAt");


            Database.Table serverTable = database.AddTable("Server");
            var serverTableIdField = serverTable.AddString100Field("Id", false);
            serverTableIdField.primaryKey = true;

            serverTable.AddTextField("Data");
            serverTable.AddLongField("LastHeartbeat");



            Database.Table setTable = database.AddTable("Set");
            var setTableIdField = setTable.AddIntField("Id", false);
            setTableIdField.primaryKey = true;
            setTableIdField.autoIncrement = true;

            setTable.AddString100Field("Key").CreateIndex();
            setTable.AddSingleField("Score");
            setTable.AddString500Field("Value").CreateIndex();
            setTable.AddLongField("ExpireAt").CreateIndex();


            Database.Table stateTable = database.AddTable("State");
            var stateTableIdField = stateTable.AddIntField("Id", false);
            stateTableIdField.primaryKey = true;
            stateTableIdField.autoIncrement = true;

            stateTable.AddIntField("JobId").CreateIndex();
            stateTable.AddString100Field("Name");
            stateTable.AddString100Field("Reason");
            stateTable.AddLongField("CreatedAt");
            stateTable.AddTextField("Data");
            stateTable.AddLongField("ExpireAt").CreateIndex();
        }
    }
}