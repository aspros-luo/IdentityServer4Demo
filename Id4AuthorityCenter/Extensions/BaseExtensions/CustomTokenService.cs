using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Id4AuthorityCenter.Extensions.BaseExtensions
{
    public class CustomTokenService:DefaultTokenService,ITokenService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public CustomTokenService(IClaimsService claimsProvider, IReferenceTokenStore referenceTokenStore, ITokenCreationService creationService, IHttpContextAccessor contextAccessor, ISystemClock clock, ILogger<DefaultTokenService> logger) : base(claimsProvider, referenceTokenStore, creationService, contextAccessor, clock, logger)
        {
            _contextAccessor = contextAccessor;
        }
        /// <summary>
        /// Creates an access token.
        /// </summary>
        /// <param name="request">The token creation request.</param>
        /// <returns>
        /// An access token
        /// </returns>
        public new async Task<Token> CreateAccessTokenAsync(TokenCreationRequest request)
        {
            var token = await base.CreateAccessTokenAsync(request);
            var jti = _contextAccessor.HttpContext.Items[JwtClaimTypes.JwtId];
            if (jti != null)
            {
                var oldJtiClaim = token.Claims.FirstOrDefault(i => i.Type == JwtClaimTypes.JwtId);
                token.Claims.Remove(oldJtiClaim);
                token.Claims.Add(new Claim(JwtClaimTypes.JwtId, jti.ToString()));
            }
            return token;
        }
    }
}
