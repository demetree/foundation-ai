using Foundation.Security.OIDC.TokenValidators;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Foundation.Security.OIDC
{
    public static class Extensions
    {
        public static IServiceCollection AddExternalLoginValidators(this IServiceCollection services,
            Action<TokenValidatorOptions> configure = null)
        {
            if (configure != null)
                services.Configure(configure);

            services.AddScoped<GoogleTokenValidator>();
            services.AddScoped<FacebookTokenValidator>();
            services.AddScoped<MicrosoftTokenValidator>();
            services.AddHttpClient<TwitterTokenValidator>();
            services.AddTransient<OAuth1Helper>();

            return services;
        }
    }
}
