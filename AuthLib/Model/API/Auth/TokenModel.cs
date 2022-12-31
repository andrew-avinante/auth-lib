using System.ComponentModel.DataAnnotations;

namespace AuthLib.Model.API.Auth
{
    public class TokenModel
    {
        [Required(ErrorMessage = "Access token is erquired")]
        public string? AccessToken { get; set; }
        [Required(ErrorMessage = "Refresh token is erquired")]
        public string? RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public DateTime RefreshExpiration { get; set; }
    }
}
