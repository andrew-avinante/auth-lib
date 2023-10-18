using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthLib.Model.API.Response.Error
{
    public class GeneralErrorResponse : IErrorResponse
    {
        public static GeneralErrorResponse UnprocessableEntity(string? message = "Error encountered while processing entity")
        {
            return new GeneralErrorResponse()
            {
                Code = "UNPROCESSABLE_ENTITY",
                Message = message,
                Status = "ERROR"
            };
        }
    }
}
