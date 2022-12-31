using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthLib.Model.Db
{
    public class ApiKeys
    {
        public int ApiKeysId { get; set; }
        [Required]
        public string? Key { get; set; }
        [Required]
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public DateTime? ExpirationDate { get; set; }
        [Required]
        public string? Application { get; set; }
    }
}
