// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Id4AuthorityCenter.Extensions.BaseExtensions;
using IdentityModel.Client;
using IdentityServer4;
using IdentityServer4.Models;

namespace Id4AuthorityCenter.Config
{
    public class Configs
    {
        /// <summary>
        /// accessToken 有效时间
        /// </summary>
        public static int AccessTokenLifetime { get; set; }
        /// <summary>
        /// refreshToken 有效时间
        /// </summary>
        public static int SlidingRefreshTokenLifetime { get; set; }

        public const string OpenApiClientName = "open_api";
        public const string OpenApiClientSecret = "shT5$Htd%!&hs*jcb1";

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static List<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("item", "商品"){ UserClaims = new List<string> {"role"}},
                new ApiResource("user", "用户"){ UserClaims = new List<string> {"role"}},
                new ApiResource("trade", "交易"){ UserClaims = new List<string> {"role"}},

            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients()
        {
            var apis = new List<string>();
            GetApiResources().ToList().ForEach(x => apis.Add(x.Name));
            apis.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
            apis.Add(IdentityServerConstants.StandardScopes.OpenId);
            apis.Add(IdentityServerConstants.StandardScopes.Profile);

            return new List<Client>
            {
                new Client
                {
                    ClientId = "external_customer",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AccessTokenLifetime =AccessTokenLifetime, //3600 * 24 * 15, //15 days
                    SlidingRefreshTokenLifetime=SlidingRefreshTokenLifetime,// 1296000, //15天
                    ClientSecrets =
                    {
                        new Secret("kfn&8Jgd#hwiJ143".Sha256())
                    },
                    AllowedScopes = new List<string>{ "trade_b2b" }
                }
            };
        }

        /// <summary>
        /// 获取tokenclien对象
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="endpointHandler"></param>
        /// <returns></returns>
        public static async Task<TokenClient> GetTokenClient(string clientId, string clientSecret, ILocalTokenEndpointHandler endpointHandler)
        {
            var tokenClient = new LocalTokenClient("", clientId, clientSecret, endpointHandler);
            return tokenClient;
        }
    }
}