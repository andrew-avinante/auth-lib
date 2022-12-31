using System.ComponentModel.DataAnnotations;

namespace AuthLib.Model.Db
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public string? Token { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        [Required]
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
