using AuthLib.Model.Db.Logger;
using Microsoft.AspNetCore.Identity;

namespace AuthLib.Model.Db
{
    public class ApplicationUser : IdentityUser
    {
        public List<Log>? Log { get; set; }
        public List<RefreshToken>? RefreshTokens { get; set; }
        public List<ApiKeys>? ApiKeys { get; set; }
    }
}
