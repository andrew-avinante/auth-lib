using AuthLib.Model.Db;
using Microsoft.AspNetCore.Mvc;

namespace AuthLib.Managers.Util
{
    public interface ILoggerService
    {

        public Task LogInfo(ControllerBase controller, string message, int? statusCode = null);

        public Task LogWarn(ControllerBase controller, string message, int? statusCode = null);

        public Task LogError(ControllerBase controller,string message, int? statusCode = null);
    }
}
