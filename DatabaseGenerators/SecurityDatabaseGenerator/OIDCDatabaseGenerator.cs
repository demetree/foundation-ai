using Foundation.CodeGeneration;
using System.Collections.Generic;

namespace Foundation.Security.Database
{
    public class OIDCDatabaseGenerator : DatabaseGenerator
    {
        /// <summary>
        /// 
        /// This extends the Foundation Security database with OIDC tables needed by the OIDC framework.  These table additions will be in the Security
        /// database, but in the dbo schema in order to allow them to work with the OIDC framework.
        /// 
        /// We will not generate custom code for any of these tables as they are part of the OIDC system, not our Security system.
        /// 
        /// </summary>
        public OIDCDatabaseGenerator() : base("Security", "Security")
        {
            //
            //
            //
            this.database.SetSchemaName("dbo");



            //
            // These are the tables needed for the OpenID library
            //
            Database.Table openIddictApplicationsTable = database.AddTable("OpenIddictApplications");
            openIddictApplicationsTable.excludeFromCodeGeneration = true;

            openIddictApplicationsTable.AddStringPrimaryKeyField("Id");
            openIddictApplicationsTable.AddString50Field("ApplicationType", true);
            openIddictApplicationsTable.AddString100Field("ClientId", true).CreateIndex(); ;
            openIddictApplicationsTable.AddTextField("ClientSecret", true);
            openIddictApplicationsTable.AddString50Field("ClientType", true);
            openIddictApplicationsTable.AddString500Field("ConcurrencyToken", true);
            openIddictApplicationsTable.AddString50Field("ConsentType", true);
            openIddictApplicationsTable.AddTextField("DisplayName", true);
            openIddictApplicationsTable.AddTextField("DisplayNames", true);
            openIddictApplicationsTable.AddTextField("JsonWebKeySet", true);
            openIddictApplicationsTable.AddTextField("Permissions", true);
            openIddictApplicationsTable.AddTextField("PostLogoutRedirectUris", true);
            openIddictApplicationsTable.AddTextField("Properties", true);
            openIddictApplicationsTable.AddTextField("RedirectUris", true);
            openIddictApplicationsTable.AddTextField("Requirements", true);
            openIddictApplicationsTable.AddTextField("Settings", true);



            Database.Table openIddictScopesTable = database.AddTable("OpenIddictScopes");
            openIddictScopesTable.AddStringPrimaryKeyField("Id");
            openIddictScopesTable.AddString50Field("ConcurrencyToken", true);
            openIddictScopesTable.AddTextField("Description", true);
            openIddictScopesTable.AddTextField("Descriptions", true);
            openIddictScopesTable.AddTextField("DisplayName", true);
            openIddictScopesTable.AddTextField("DisplayNames", true);
            openIddictScopesTable.AddString250Field("Name", true).CreateIndex();
            openIddictScopesTable.AddTextField("Properties", true);
            openIddictScopesTable.AddTextField("Resources", true);



            Database.Table openIddictAuthorizations = database.AddTable("OpenIddictAuthorizations");
            openIddictAuthorizations.AddStringPrimaryKeyField("Id");
            openIddictAuthorizations.AddForeignKeyStringField("ApplicationId", openIddictApplicationsTable, false);
            openIddictAuthorizations.AddString50Field("ConcurrencyToken", true);
            openIddictAuthorizations.AddDateTimeField("CreationDate", true);
            openIddictAuthorizations.AddTextField("Properties", true);
            openIddictAuthorizations.AddTextField("Scopes", true);
            openIddictAuthorizations.AddString50Field("Status", true);
            openIddictAuthorizations.AddString500Field("Subject", true);
            openIddictAuthorizations.AddString50Field("Type", true);

            List<Database.Table.Field> indexFieldsForAuthorizationsTable = new List<Database.Table.Field>();

            indexFieldsForAuthorizationsTable.Add(openIddictAuthorizations.GetFieldByName("ApplicationId"));
            indexFieldsForAuthorizationsTable.Add(openIddictAuthorizations.GetFieldByName("Status"));
            indexFieldsForAuthorizationsTable.Add(openIddictAuthorizations.GetFieldByName("Subject"));
            indexFieldsForAuthorizationsTable.Add(openIddictAuthorizations.GetFieldByName("Type"));
            openIddictAuthorizations.CreateIndexForFields(indexFieldsForAuthorizationsTable);



            Database.Table openIddictTokensTable = database.AddTable("OpenIddictTokens");
            openIddictTokensTable.AddStringPrimaryKeyField("Id");
            openIddictTokensTable.AddForeignKeyStringField("ApplicationId", openIddictApplicationsTable, false);
            openIddictTokensTable.AddForeignKeyStringField("AuthorizationId", openIddictAuthorizations, true).CreateIndex();
            openIddictTokensTable.AddString50Field("ConcurrencyToken", true);
            openIddictTokensTable.AddDateTimeField("CreationDate", true);
            openIddictTokensTable.AddDateTimeField("ExpirationDate", true);
            openIddictTokensTable.AddTextField("Payload", true);
            openIddictTokensTable.AddTextField("Properties", true);
            openIddictTokensTable.AddDateTimeField("RedemptionDate", true);
            openIddictTokensTable.AddString100Field("ReferenceId", true).CreateIndex();
            openIddictTokensTable.AddString50Field("Status", true);
            openIddictTokensTable.AddString500Field("Subject", true);
            openIddictTokensTable.AddString50Field("Type", true);

            List<Database.Table.Field> indexFieldsForTokensTable = new List<Database.Table.Field>();

            indexFieldsForTokensTable.Add(openIddictTokensTable.GetFieldByName("ApplicationId"));
            indexFieldsForTokensTable.Add(openIddictTokensTable.GetFieldByName("Status"));
            indexFieldsForTokensTable.Add(openIddictTokensTable.GetFieldByName("Subject"));
            indexFieldsForTokensTable.Add(openIddictTokensTable.GetFieldByName("Type"));
            openIddictTokensTable.CreateIndexForFields(indexFieldsForTokensTable);

        }
    }
}