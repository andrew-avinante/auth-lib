using AuthLib.Model.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthLib.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
            //_dbContext = dbContext;
        }

        public async Task Invoke(HttpContext context, AuthDbContext _dbContext)
        {
            string apiKey = GetApiKey(context.Request);

            if (!string.IsNullOrEmpty(apiKey))
            {
                using (_dbContext)
                {
                    using (var sha256 = SHA256.Create())
                    {
                        var hashedApiKey = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey)));

                        var apiKeyEntity = await _dbContext.ApiKeys.FirstOrDefaultAsync(x => x.Key == hashedApiKey);

                        if (apiKeyEntity != null && apiKeyEntity.ExpirationDate > DateTime.UtcNow)
                        {
                            var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, apiKeyEntity.ApplicationUser.UserName)
                        };

                            var identity = new ClaimsIdentity(claims, "API Key");
                            var principal = new ClaimsPrincipal(identity);

                            context.User = principal;
                        }
                    }
                }
            }

            await _next(context);
        }

        public static string GetApiKey(HttpRequest request)
        {
            // Check the header first
            var apiKey = request.Headers["x-api-key"].FirstOrDefault();

            // If the header is not present, check the query string
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = request.Query["api_key"].FirstOrDefault();
            }

            return apiKey;
        }
    }
}
