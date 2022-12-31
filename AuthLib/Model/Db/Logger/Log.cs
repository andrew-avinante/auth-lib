using System.ComponentModel.DataAnnotations;

namespace AuthLib.Model.Db.Logger
{
    public class Log
    {
        [Required]
        public int LogId { get; set; }
        [Required]
        public string? Endpoint { get; set; }
        public string? HTTPMethod { get; set; }
        [Required]
        public string? Message { get; set; }
        public DateTime TimeLogged { get; set; }

        [Required]
        public string? Type { get; set; }
        public int? StatusCode { get; set; }
        public string? ApplicationUserId { get; set; }
        public List<ApplicationUser>? ApplicationUser { get; set; }

        public Log()
        {
            TimeLogged = DateTime.Now.ToUniversalTime();
        }
    }
}
