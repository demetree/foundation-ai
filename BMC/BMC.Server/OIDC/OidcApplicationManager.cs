using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Foundation.Security.OIDC
{
    /// <summary>
    /// 
    /// This class registers the BMC SPA and swagger UI applications in OpenID.
    /// 
    /// </summary>
    public static class OidcApplicationManager
    {
        public const string BMC_SERVER_NAME = "BMC API";
        public const string BMC_CLIENT_ID = "bmc_spa";

        public const string SWAGGER_SERVER_NAME = "Swagger SPA";
        public const string SWAGGER_CLIENT_ID = "swagger_ui";

        public static async Task RegisterClientApplicationsAsync(IServiceProvider provider)
        {
            var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

            //
            // Create the BMC Angular SPA Client in OpenID
            //
            if (await manager.FindByClientIdAsync(BMC_CLIENT_ID) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = BMC_CLIENT_ID,
                    ClientType = ClientTypes.Public,
                    DisplayName = BMC_SERVER_NAME,
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.Password,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.Prefixes.GrantType + Constants.ExtensionGrantType,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Phone,
                        Permissions.Scopes.Address,
                        Permissions.Scopes.Roles
                    }
                });
            }

            //
            // Create the Swagger UI Client record in OpenID
            //
            if (await manager.FindByClientIdAsync(SWAGGER_CLIENT_ID) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = SWAGGER_CLIENT_ID,
                    ClientType = ClientTypes.Public,
                    DisplayName = SWAGGER_SERVER_NAME,
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.Password
                    }
                });
            }
        }
    }
}
