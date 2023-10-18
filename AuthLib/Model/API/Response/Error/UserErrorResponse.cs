using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthLib.Model.API.Response.Error
{
    public class UserErrorResponse : IErrorResponse
    {
        public static UserErrorResponse NoUserFound(string? message = "User was not found")
        {
            return new UserErrorResponse()
            {
                Code = "NO_USER_FOUND",
                Message = message,
                Status = "ERROR"
            };
        }

        public static UserErrorResponse NoRoleFound(string? message = "Role was not found")
        {
            return new UserErrorResponse()
            {
                Code = "NO_ROLE_FOUND",
                Message = message,
                Status = "ERROR"
            };
        }

        public static UserErrorResponse UserAccessDenied(string? message = "User is not authorized")
        {
            return new UserErrorResponse()
            {
                Code = "USER_UNAUTHORIZED",
                Message = message,
                Status = "ERROR"
            };
        }

        public static UserErrorResponse UserExists(string message)
        {
            return new UserErrorResponse()
            {
                Code = "USER_EXISTS",
                Message = message,
                Status = "ERROR"
            };
        }
    }
}
