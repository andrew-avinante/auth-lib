using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthLib.Model.API.Auth
{
    public class IssueApiKeyRequest
    {
        public DateTime? Expiration { get; set; }
        [Required]
        public string? ApplicationName { get; set; }
    }
}
