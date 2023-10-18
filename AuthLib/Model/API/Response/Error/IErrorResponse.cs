using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthLib.Model.API.Response.Error
{
    public class IErrorResponse
    {
        public string? Status { get; protected set; }
        public string? Message { get; protected set; }
        public string? Code { get; protected set; }
    }
}
