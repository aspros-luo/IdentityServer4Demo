using System.Collections.Generic;
using System.Threading.Tasks;
using AQHG.WL.Authorization.Web.Extensions;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;

namespace Id4AuthorityCenter.Extensions.BaseExtensions
{
    /// <summary>
    /// A service for parsing secrets found on the request
    /// </summary>
    public interface ICustomSecretParser
    {
        /// <summary>
        /// Tries to find a secret on the context that can be used for authentication
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>
        /// A parsed secret
        /// </returns>
        Task<ParsedSecret> ParseAsync(IDictionary<string, string> context);

        /// <summary>
        /// Returns the authentication method name that this parser implements
        /// </summary>
        /// <value>
        /// The authentication method.
        /// </value>
        string AuthenticationMethod { get; }
    }

    /// <summary>
    /// Parses a POST body for secrets
    /// </summary>
    public class CustomPostBodySecretParser : ICustomSecretParser
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// Creates the parser with options
        /// </summary>
        /// <param name="options">IdentityServer options</param>
        /// <param name="logger">Logger</param>
        public CustomPostBodySecretParser(IdentityServerOptions options, ILogger<PostBodySecretParser> logger)
        {
            _logger = logger;
            _options = options;
        }

        /// <summary>
        /// Returns the authentication method name that this parser implements
        /// </summary>
        /// <value>
        /// The authentication method.
        /// </value>
        public string AuthenticationMethod => OidcConstants.EndpointAuthenticationMethods.PostBody;

        /// <summary>
        /// Tries to find a secret on the context that can be used for authentication
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>
        /// A parsed secret
        /// </returns>
        public async Task<ParsedSecret> ParseAsync(IDictionary<string, string> context)
        {
            _logger.LogDebug("Start parsing for secret in post body");

            var body = context;

            if (body != null)
            {
                var id = body["client_id"];
                var secret = body["client_secret"];

                // client id must be present
                if (id.IsPresent())
                {
                    if (id.Length > _options.InputLengthRestrictions.ClientId)
                    {
                        _logger.LogError("Client ID exceeds maximum length.");
                        return null;
                    }

                    if (secret.IsPresent())
                    {
                        if (secret.Length > _options.InputLengthRestrictions.ClientSecret)
                        {
                            _logger.LogError("Client secret exceeds maximum length.");
                            return null;
                        }

                        return new ParsedSecret
                        {
                            Id = id,
                            Credential = secret,
                            Type = IdentityServerConstants.ParsedSecretTypes.SharedSecret
                        };
                    }
                    else
                    {
                        // client secret is optional
                        _logger.LogDebug("client id without secret found");

                        return new ParsedSecret
                        {
                            Id = id,
                            Type = IdentityServerConstants.ParsedSecretTypes.NoSecret
                        };
                    }
                }
            }

            _logger.LogDebug("No secret in post body found");
            return null;
        }
    }
}
