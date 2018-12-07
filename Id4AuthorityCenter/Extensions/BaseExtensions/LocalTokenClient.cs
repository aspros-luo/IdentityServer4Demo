using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using IdentityServer4.Extensions;

namespace Id4AuthorityCenter.Extensions.BaseExtensions
{
    public class LocalTokenClient : TokenClient
    {
        private readonly ILocalTokenEndpointHandler _endpointHandler;

        public LocalTokenClient(string address, string clientId, string clientSecret, ILocalTokenEndpointHandler endpointHandler, HttpMessageHandler innerHttpMessageHandler = null, AuthenticationStyle style = AuthenticationStyle.BasicAuthentication) : base(address, clientId, clientSecret, innerHttpMessageHandler, style)
        {
            _endpointHandler = endpointHandler;
        }

        public override async Task<TokenResponse> RequestAsync(IDictionary<string, string> form, CancellationToken cancellationToken = new CancellationToken())
        {
            form.Add("client_id", ClientId);
            form.Add("client_secret", ClientSecret);

            try
            {
                var endpointResult = await _endpointHandler.ProcessAsync(form);
                if (endpointResult is TokenResult)
                {
                    var response = (TokenResult)endpointResult;
                    if (response.Response != null)
                    {
                        var result = new Dictionary<string, object>
                        {
                            {"id_token", response.Response.IdentityToken},
                            {"access_token", response.Response.AccessToken},
                            {"refresh_token", response.Response.RefreshToken},
                            {"expires_in", response.Response.AccessTokenLifetime},
                            {"token_type", OidcConstants.TokenResponse.BearerTokenType}
                        };

                        if (!response.Response.Custom.IsNullOrEmpty())
                        {
                            foreach (var item in response.Response.Custom)
                            {
                                result.Add(item.Key, item.Value);
                            }
                        }

                        return new TokenResponse(Newtonsoft.Json.JsonConvert.SerializeObject(result));
                    }
                }
                else
                {
                    var response = (TokenErrorResult)endpointResult;
                    var error = response.Response.Error;
                    if (response.Response.ErrorDescription != null)
                    {
                        error = response.Response.ErrorDescription;
                    }
                    return new TokenResponse(new Exception(error));
                }
            }
            catch (Exception ex)
            {
                return new TokenResponse(ex);
            }

            return new TokenResponse(new Exception("request token failed"));
        }
    }
}
