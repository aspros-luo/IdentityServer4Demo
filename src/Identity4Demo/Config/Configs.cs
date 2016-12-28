using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
namespace Identity4Demo.Config
{
    public class Configs
    {
        public static IEnumerable<IdentityResource> GetyIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }
        public static IEnumerable<ApiResource> GeyApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("UserApi","用户API")
            };
        }
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "Client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = {"UserApi"}
                },
                new Client
                {
                    ClientId = "ro.Client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = {"UserApi"}
                },
                   // OpenID Connect implicit flow client (MVC)
                new Client
                {
                    ClientId = "MVC",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                     ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    RedirectUris = { "http://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:5002" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "UserApi"
                    },
                    AllowOfflineAccess = true
                }
            };
        }
        public static List<TestUser> GeTestUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "qwerty",
                    Password = "a123"
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "aspros",
                    Password = "b123"
                }
            };
        }
    }
}
