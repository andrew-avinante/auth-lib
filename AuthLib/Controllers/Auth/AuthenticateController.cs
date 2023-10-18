using AuthLib.Managers.Auth;
using AuthLib.Managers.Util;
using AuthLib.Model.API.Auth;
using AuthLib.Model.API.Response;
using AuthLib.Model.API.Response.Error;
using AuthLib.Model.Auth;
using AuthLib.Model.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthLib.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly AuthDbContext _dbContext;
        private readonly ILoggerService _loggerService;

        public AuthenticateController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            AuthDbContext dbContext,
            ILoggerService loggerService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Login endpoint
        /// </summary>
        /// <param name="model">Body of post request</param>
        /// <returns>HTTP Responses: <see cref="OkResult"/> or <see cref="UnauthorizedResult"/></returns>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(model.Username);
            bool passwordCorrect = await _userManager.CheckPasswordAsync(user, model.Password);
            if (user != null && passwordCorrect)
            {
                await _loggerService.LogInfo(this, $"User '{model.Username}' logged in successfully", StatusCodes.Status200OK);
                return Ok(await GetLoginTokensAsync(user));
            }

            if (passwordCorrect)
            {
                await _loggerService.LogInfo(this, $"Attempted login for unknown user '{model.Username}'", StatusCodes.Status401Unauthorized);
            }
            else
            {
                await _loggerService.LogInfo(this, $"Password incorrect for user '{model.Username}'", StatusCodes.Status401Unauthorized);
            }

            return Unauthorized(UserErrorResponse.UserAccessDenied());
        }

        /// <summary>
        /// Registration endpoint
        /// </summary>
        /// <param name="model">Body of post request</param>
        /// <returns>Http Responses: <see cref="ConflictResult"/>, <see cref="UnprocessableEntityResult"/>, and <see cref="OkResult"/></returns>
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            ApplicationUser userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                await _loggerService.LogInfo(this, $"Attempted creation of existing user '{model.Username}'", StatusCodes.Status409Conflict);
                return Conflict(UserErrorResponse.UserExists("User already exists"));
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            await _loggerService.LogInfo(this, $"Attempting to create user '{model.Username}'");
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                List<string> message = new List<string>();
                foreach (IdentityError error in result.Errors)
                {
                    message.Add(error.Description);
                }

                await _loggerService.LogWarn(this, $"Could not create user: ${message}", StatusCodes.Status422UnprocessableEntity);

                return UnprocessableEntity(GeneralErrorResponse.UnprocessableEntity());
            }

            await AddUserToRoleAsync(user, UserRoles.User);

            await _loggerService.LogInfo(this, $"Created new user '{model.Username}'", StatusCodes.Status201Created);
            return StatusCode(StatusCodes.Status201Created, await GetLoginTokensAsync(user));
        }

        /// <summary>
        /// Adds user to the provided role
        /// </summary>
        /// <param name="username">User to add the role to</param>
        /// <param name="roleId">Role to add the user to</param>
        /// <returns>No content</returns>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("role/{username}/{roleId}")]
        public async Task<IActionResult> AddUserToRole(string username, string roleId)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(username);
            if (user is null)
            {
                await _loggerService.LogInfo(this, $"No user '{username}' found", StatusCodes.Status400BadRequest);
                return BadRequest(UserErrorResponse.NoUserFound($"User `{username}` is not found!"));
            }

            if (!await _roleManager.RoleExistsAsync(roleId))
            {
                await _loggerService.LogInfo(this, $"No role with id '{roleId}' found", StatusCodes.Status400BadRequest);
                return BadRequest(UserErrorResponse.NoRoleFound($"Role `{roleId} is not found!"));
            }

            await _userManager.AddToRoleAsync(user, roleId);

            await _loggerService.LogInfo(this, $"User '{username}' added to role '{roleId}'", StatusCodes.Status204NoContent);
            return NoContent();
        }

        /// <summary>
        /// Generates access and refresh tokens for the provided user
        /// </summary>
        /// <param name="user"><see cref="ApplicationUser"/> to generate the access token for</param>
        /// <returns><see cref="TokenModel"/> containing the generated access and refresh tokens</returns>
        private async Task<TokenModel> GetLoginTokensAsync(ApplicationUser user)
        {
            IList<string> userRoles = await _userManager.GetRolesAsync(user);

            // Add required claims
            List<Claim> authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim("registrationCompleted", "false")
                };

            // Add role claims
            foreach (string userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            JwtSecurityToken accessToken = _tokenService.GenerateAccessToken(authClaims);
            RefreshToken refreshToken = _tokenService.AddRefreshTokenToUser(user, _dbContext);

            return new TokenModel()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                RefreshToken = refreshToken.Token,
                Expiration = accessToken.ValidTo,
                RefreshExpiration = refreshToken.RefreshTokenExpiryTime
            };
        }

        /// <summary>
        /// Adds user to provided role
        /// </summary>
        /// <param name="user"><see cref="ApplicationUser"/> to add the role to</param>
        /// <param name="role">Role to add to the user</param>
        private async Task AddUserToRoleAsync(ApplicationUser user, string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
                await _loggerService.LogInfo(this, $"Created new role {role}");
            }

            await _userManager.AddToRoleAsync(user, role);
        }

        private async Task<bool> IsUserSetup(ApplicationUser user)
        {
            Person person = Per
        }
    }
}
