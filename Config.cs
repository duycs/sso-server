using System.Collections.Generic;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;

namespace AuthServer
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("we-edit-api", "We Edit API")
                {
                    Scopes = {new Scope("api.read")}
                }
            };
        }

        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            var weeditAppUrl = configuration.GetValue<string>("AppSettings:weeditAppUrl");
            var ssoUrl = configuration.GetValue<string>("AppSettings:ssoUrl");

            return new[]
            {
                new Client {
                    RequireConsent = false,
                    ClientId = "we-edit-web-app",
                    ClientName = "We Edit Web App",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = { "openid", "email", "profile", "api.read"},
                    RedirectUris = {$"{weeditAppUrl}/authentication/callback", $"{weeditAppUrl}/silent-refresh.html" },
                    PostLogoutRedirectUris = {weeditAppUrl},
                    AllowedCorsOrigins = { weeditAppUrl, ssoUrl},
                    AllowAccessTokensViaBrowser = true,
                    AccessTokenLifetime = 86400 // one day
                }
            };
        }
    }
}
