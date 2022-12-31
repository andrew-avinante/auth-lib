using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthLib.Middleware
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string Header { get; set; } = "x-api-key";
        public string QueryString { get; set; } = "api_key";
    }
}
