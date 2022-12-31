using AuthLib.Model.Db;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AuthLib.Middleware
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly AuthDbContext _dbContext;

        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, AuthDbContext dbContext)
            : base(options, logger, encoder, clock)
        {
            _dbContext = dbContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string apiKey = ApiKeyMiddleware.GetApiKey(Request);

            if (string.IsNullOrEmpty(apiKey))
            {
                return AuthenticateResult.Fail("Missing API Key");
            }

            using (_dbContext)
            {
                using (var sha256 = SHA256.Create())
                {
                    var hashedApiKey = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey)));

                    var apiKeyEntity = await _dbContext.ApiKeys.Include(x => x.ApplicationUser).FirstOrDefaultAsync(x => x.Key == hashedApiKey);

                    if (apiKeyEntity == null || apiKeyEntity.ExpirationDate <= DateTime.UtcNow)
                    {
                        return AuthenticateResult.Fail("Invalid API Key");
                    }

                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, apiKeyEntity.ApplicationUser.UserName)
                };

                    var identity = new ClaimsIdentity(claims, "API Key");
                    var principal = new ClaimsPrincipal(identity);

                    var ticket = new AuthenticationTicket(principal, "API Key");

                    return AuthenticateResult.Success(ticket);
                }
            }
        }
    }
}
