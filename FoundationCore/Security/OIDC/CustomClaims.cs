namespace Foundation.Security.OIDC
{
    public static class CustomClaims
    {
        ///<summary>A claim that specifies the full name of an entity</summary>
        public const string FullName = "full_name";

        ///<summary>A claim that specifies the settings of an entity</summary>
        public const string Settings = "settings";

        ///<summary>A claim that specifies the read permission of an entity</summary>
        public const string ReadPermission = "read_permission";

        ///<summary>A claim that specifies the write permission of an entity</summary>
        public const string WritePermission = "write_permission";

        ///<summary>A claim that specifies the name of the tenant of an entity</summary>\
        public const string TenantName = "tenant_name";
    }
}
