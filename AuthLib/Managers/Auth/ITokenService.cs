using AuthLib.Model.Db;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthLib.Managers.Auth
{
    public interface ITokenService
    {
        public string Issuer { get; set; }
        public string? Audience { get; set; }
        public string? Secret { get; set; }
        public int RefreshTokenValidityInDays { get; set; }
        public double AccessTokenValidityInHours { get; set; }
        JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token, bool validateLifetime = false);
        public RefreshToken AddRefreshTokenToUser(ApplicationUser user, AuthDbContext dbContex);
    }
}
