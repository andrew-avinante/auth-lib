using AuthLib.Model.Db;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthLib.Managers.Auth
{
    public class TokenService : ITokenService
    {
        public string Issuer { get; set; } = "";
        public string? Audience { get; set; }
        public string? Secret { get; set; }
        public int RefreshTokenValidityInDays { get; set; }
        public double AccessTokenValidityInHours { get; set; }

        /// <summary>
        /// Generates an accesstoken based on the provided <see cref="Claim"/>s
        /// </summary>
        /// <param name="authClaims"><see cref="Claim"/>s to add to the access token</param>
        /// <returns><see cref="JwtSecurityToken"/> representing the user</returns>
        public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> authClaims)
        {
            SymmetricSecurityKey authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret ?? throw new ArgumentNullException()));

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                expires: DateTime.Now.AddHours(AccessTokenValidityInHours),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        /// <summary>
        /// Generates a refresh token
        /// </summary>
        /// <returns></returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Retrieves <see cref="ClaimsPrincipal"/> from token
        /// </summary>
        /// <param name="accessToken">Access token to validate</param>
        /// <returns><see cref="ClaimsPrincipal"/> derived from access token</returns>
        /// <exception cref="SecurityTokenException">when token is invalid</exception>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken, bool validateLifetime = false)
        {
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret ?? throw new ArgumentNullException())),
                ValidateLifetime = validateLifetime //here we are saying that we don't care about the token's expiration date
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            ClaimsPrincipal principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }

        /// <summary>
        /// Adds a new refresh token to the provided <see cref="ApplicationUser"/>
        /// </summary>
        /// <param name="user"><see cref="ApplicationUser"/> to add the refresh token to</param>
        /// <param name="dbContext"><see cref="AuthDbContext"/> for the database</param>
        /// <returns>New refresh token</returns>
        public RefreshToken AddRefreshTokenToUser(ApplicationUser user, AuthDbContext dbContext)
        {
            RefreshToken token;
            token = new RefreshToken()
            {
                Token = GenerateRefreshToken(),
                RefreshTokenExpiryTime = DateTime.Now.AddDays(RefreshTokenValidityInDays).ToUniversalTime(),
                ApplicationUserId = user.Id
            };

            dbContext.Add(token);
            dbContext.SaveChanges();

            return token;
        }
    }
}
