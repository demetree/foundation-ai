namespace Foundation.Security.Configuration
{
    public class AppSettings
    {
        public required string DefaultUserRole { get; set; }
        public ExternalLoginConfig ExternalLogin { get; set; }
        public SmtpConfig SmtpConfig { get; set; }
    }

    public class ExternalLoginConfig
    {
        public OidcAuthConfig Google { get; set; }
        public OidcAuthConfig Facebook { get; set; }
        public OidcAuthConfig Microsoft { get; set; }
        public TwitterAuthConfig Twitter { get; set; }
    }

    public class OidcAuthConfig
    {
        public required string ClientId { get; set; }
        public required string Issuer { get; set; }
        public bool ValidateIssuer { get; set; }
    }

    public class TwitterAuthConfig
    {
        public required string ConsumerAPIKey { get; set; }
        public required string ConsumerSecret { get; set; }
    }

    public class SmtpConfig
    {
        public required string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }

        public required string EmailAddress { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
