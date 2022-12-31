using NuGet.Protocol;

namespace AuthLib.Model.API
{

    public class ApiResponse
    {
        public string? Status { get; private set; }
        public string? Message { get; private set; }

        public static ApiResponse Error(string? message)
        {
            return new ApiResponse
            {
                Status = "Error",
                Message = message
            };
        }

        public static ApiResponse Error(List<string> message)
        {
            return Error(message.ToJson());
        }

        public static ApiResponse Success(string message)
        {
            return new ApiResponse
            {
                Status = "Success",
                Message = message
            };
        }
    }
}
