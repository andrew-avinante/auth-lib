using AuthLib.Managers.Auth;
using AuthLib.Managers.Util;
using AuthLib.Model.API.Auth;
using AuthLib.Model.API.Response;
using AuthLib.Model.API.Response.Error;
using AuthLib.Model.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthLib.Controllers.Auth
{
    [Route("api/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly AuthDbContext _dbContext;
        private readonly ILoggerService _loggerService;

        public TokenController(
            UserManager<ApplicationUser> userManager, 
            ITokenService tokenService, 
            AuthDbContext dbContext,
            ILoggerService loggerService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Refreshes the provided access token using the refersh token
        /// </summary>
        /// <param name="tokenModel">Body of post request</param>
        /// <returns>HTTP responses: <see cref="BadRequestResult"/>, <see cref="NotFoundResult"/>, <see cref="UnprocessableEntityResult"/>, and <see cref="OkResult"/></returns>
        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenModel? tokenModel)
        {
            if (tokenModel is null)
            {
                await _loggerService.LogInfo(this, $"User sent a null token model", StatusCodes.Status400BadRequest);
                return BadRequest(TokenErrorResponse.TokenRequired());
            }

            // Validates token
            ClaimsPrincipal? principal = null;
            try
            {
                principal = _tokenService.GetPrincipalFromExpiredToken(tokenModel.AccessToken ?? "");
            }
            catch(Exception e)
            {
                await _loggerService.LogError(this, $"Error while validating token: {e}", StatusCodes.Status422UnprocessableEntity);
                return UnprocessableEntity(TokenErrorResponse.TokenExpired());
            }

            string? username = principal.Identity?.Name;

            ApplicationUser user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                await _loggerService.LogInfo(this, $"No user with name claim `{username}` found", StatusCodes.Status400BadRequest);
                return BadRequest(UserErrorResponse.NoUserFound());
            }

            RefreshToken? refreshToken = _dbContext.
                RefreshToken?.
                Where(x =>
                x.ApplicationUserId == user.Id &&
                x.Token == tokenModel.RefreshToken
            ).FirstOrDefault();

            if (refreshToken is null)
            {
                await _loggerService.LogInfo(this, "Refresh token not found", StatusCodes.Status404NotFound);
                return NotFound(UserErrorResponse.NoUserFound());
            }
                
            if (refreshToken.RefreshTokenExpiryTime <= DateTime.Now)
            {
                await _loggerService.LogInfo(this, "User had expired refresh token", StatusCodes.Status422UnprocessableEntity);
                return UnprocessableEntity(TokenErrorResponse.TokenExpired());
            }

            JwtSecurityToken newAccessToken = _tokenService.GenerateAccessToken(principal.Claims.ToList());
            RefreshToken newRefreshToken = _tokenService.AddRefreshTokenToUser(user, _dbContext);

            await _loggerService.LogInfo(this, "Generated a new access token", StatusCodes.Status200OK);

            return Ok(new TokenModel()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                RefreshToken = newRefreshToken.Token,
                Expiration = newAccessToken.ValidTo,
                RefreshExpiration = newRefreshToken.RefreshTokenExpiryTime
            });
        }

        /// <summary>
        /// Revokes all of the current user's refresh tokens
        /// </summary>
        /// <param name="username">User to revoke</param>
        /// <returns>HTTP responses: <see cref="ForbidResult"/> or <see cref="NoContentResult"/></returns>
        [Authorize(AuthenticationSchemes = "Bearer,API Key")]
        [HttpPost]
        [Route("revoke/{username}")]
        public async Task<IActionResult> Revoke(string username)
        {      
            if (username != User.Identity?.Name)
            {
                await _loggerService.LogInfo(this, $"User attempted to revoke {username}'s tokens", StatusCodes.Status403Forbidden);
                return StatusCode(StatusCodes.Status403Forbidden, UserErrorResponse.UserAccessDenied());
            }

            ApplicationUser user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                await _loggerService.LogInfo(this, "User not found", StatusCodes.Status400BadRequest);
                return BadRequest(UserErrorResponse.NoUserFound());
            }

            _dbContext.RefreshToken?.RemoveRange(_dbContext.RefreshToken.Where(x => x.ApplicationUserId == user.Id));
            await _dbContext.SaveChangesAsync();

            await _loggerService.LogInfo(this, "Revoked all tokens", StatusCodes.Status204NoContent);
            return NoContent();
        }
    }
}
