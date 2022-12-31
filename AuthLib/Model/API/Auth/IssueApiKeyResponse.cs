using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthLib.Model.API.Auth
{
    public class IssueApiKeyResponse
    {
        public string? Key { get; set; }
        public string? Application { get; set; }
    }
}
