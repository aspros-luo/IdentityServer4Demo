using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;

namespace Id4AuthorityCenter.Extensions.BaseExtensions
{
    public interface ILocalTokenEndpointHandler
    {
        Task<IEndpointResult> ProcessAsync(IDictionary<string, string> form);
    }
    public class LocalTokenEndpoint : ILocalTokenEndpointHandler
    {
        private readonly ICustomClientSecretValidator _clientValidator;
        private readonly ITokenRequestValidator _requestValidator;
        private readonly ITokenResponseGenerator _responseGenerator;
        private readonly IEventService _events;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenEndpoint" /> class.
        /// </summary>
        /// <param name="clientValidator">The client validator.</param>
        /// <param name="requestValidator">The request validator.</param>
        /// <param name="responseGenerator">The response generator.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        public LocalTokenEndpoint(
            ICustomClientSecretValidator clientValidator,
            ITokenRequestValidator requestValidator,
            ITokenResponseGenerator responseGenerator,
            IEventService events,
            ILogger<LocalTokenEndpoint> logger)
        {
            _clientValidator = clientValidator;
            _requestValidator = requestValidator;
            _responseGenerator = responseGenerator;
            _events = events;
            _logger = logger;
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="form">The HTTP context.</param>
        /// <returns></returns>
        public async Task<IEndpointResult> ProcessAsync(IDictionary<string, string> form)
        {
            _logger.LogTrace("Processing token request.");

            return await ProcessTokenRequestAsync(form);
        }

        private Task RaiseSuccessEventAsync(string clientId, string authMethod)
        {
            return _events.RaiseAsync(new ClientAuthenticationSuccessEvent(clientId, authMethod));
        }

        private async Task<IEndpointResult> ProcessTokenRequestAsync(IDictionary<string, string> form)
        {
            _logger.LogDebug("Start token request.");
            // validate client
            var clientResult = await _clientValidator.ValidateAsync(form);
            if (clientResult.Client == null)
            {
                return Error(OidcConstants.TokenErrors.InvalidClient);
            }

            // validate request
            var form2 = form.AsNameValueCollection();
            _logger.LogTrace("Calling into token request validator: {type}", _requestValidator.GetType().FullName);
            var requestResult = await _requestValidator.ValidateRequestAsync(form2, clientResult);

            if (requestResult.IsError)
            {
                await _events.RaiseAsync(new TokenIssuedFailureEvent(requestResult));
                return Error(requestResult.Error, requestResult.ErrorDescription, requestResult.CustomResponse);
            }

            // create response
            _logger.LogTrace("Calling into token request response generator: {type}", _responseGenerator.GetType().FullName);
            var response = await _responseGenerator.ProcessAsync(requestResult);

            await _events.RaiseAsync(new TokenIssuedSuccessEvent(response, requestResult));
            LogTokens(response, requestResult);

            // return result
            _logger.LogDebug("Token request success.");
            return new TokenResult(response);
        }

        private TokenErrorResult Error(string error, string errorDescription = null, Dictionary<string, object> custom = null)
        {
            var response = new TokenErrorResponse
            {
                Error = error,
                ErrorDescription = errorDescription,
                Custom = custom
            };

            return new TokenErrorResult(response);
        }

        private void LogTokens(TokenResponse response, TokenRequestValidationResult requestResult)
        {
            var clientId = $"{requestResult.ValidatedRequest.Client.ClientId} ({requestResult.ValidatedRequest.Client?.ClientName ?? "no name set"})";
            var subjectId = requestResult.ValidatedRequest.Subject?.GetSubjectId() ?? "no subject";

            if (response.IdentityToken != null)
            {
                _logger.LogTrace("Identity token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.IdentityToken);
            }
            if (response.RefreshToken != null)
            {
                _logger.LogTrace("Refresh token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.RefreshToken);
            }
            if (response.AccessToken != null)
            {
                _logger.LogTrace("Access token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.AccessToken);
            }
        }
    }

    public static class ReadableStringCollectionExtensions
    {
        public static NameValueCollection AsNameValueCollection(this IDictionary<string, string> collection)
        {
            var nv = new NameValueCollection();

            foreach (var field in collection)
            {
                nv.Add(field.Key, field.Value);
            }
            return nv;
        }
    }
}
