using AuthLib.Model.API.Auth;
using AuthLib.Model.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthLib.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class ApiKeyController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _dbContext;

        public ApiKeyController(AuthDbContext apiKeyDbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = apiKeyDbContext;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer,API Key")]
        public async Task<ActionResult<IssueApiKeyResponse>> IssueApiKey(IssueApiKeyRequest request)
        {
            string? requestingUsername = User.Identity?.Name;
            ApplicationUser? requestingUser = null;
            if (requestingUsername != null)
                requestingUser = await _userManager.FindByNameAsync(requestingUsername);

            string key = Guid.NewGuid().ToString("N");

            // Generate a unique random value
            var apiKey = new ApiKeys
            {
                Key = key,
                ApplicationUser = requestingUser,
                ExpirationDate = request.Expiration,
                Application = request.ApplicationName
            };

            // Hash the API key using a secure hashing algorithm
            using (var sha256 = SHA256.Create())
            {
                apiKey.Key = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey.Key)));
            }

            // Save the API key to the database
            _dbContext.ApiKeys?.Add(apiKey);
            await _dbContext.SaveChangesAsync();

            return Ok(new IssueApiKeyResponse { Key = key, Application = apiKey.Application });
        }
    }

}
