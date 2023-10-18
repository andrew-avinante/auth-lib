using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthLib.Model.API.Response.Error
{
    public class TokenErrorResponse : IErrorResponse
    {
        public static TokenErrorResponse TokenRequired(string? message = "Token is required")
        {
            return new TokenErrorResponse()
            {
                Code = "TOKEN_REQUIRED",
                Message = message,
                Status = "ERROR"
            };
        }

        public static TokenErrorResponse TokenExpired(string? message = "Token is expired")
        {
            return new TokenErrorResponse()
            {
                Code = "TOKEN_EXPIRED",
                Message = message,
                Status = "ERROR"
            };
        }
    }
}
