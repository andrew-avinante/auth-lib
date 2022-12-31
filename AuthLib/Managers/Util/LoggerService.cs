using AuthLib.Model.Db;
using AuthLib.Model.Db.Logger;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;

namespace AuthLib.Managers.Util
{
    public enum LogType
    {
        INFO,
        WARN,
        ERROR
    }

    public class LoggerService : ILoggerService
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoggerService(AuthDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task LogInfo(ControllerBase controller, string message, int? statusCode = null)
        {
            await InsertLog(LogType.INFO, controller, message, statusCode);
        }

        public async Task LogWarn(ControllerBase controller, string message, int? statusCode = null)
        {
            await InsertLog(LogType.WARN, controller, message, statusCode);
        }

        public async Task LogError(ControllerBase controller,string message, int? statusCode = null)
        {
            await InsertLog(LogType.ERROR, controller, message, statusCode);
        }

        private async Task InsertLog(LogType type, ControllerBase controller, string message, int? statusCode)
        {
            string? requestingUsername = controller.User.Identity?.Name;
            ApplicationUser? requestingUser = null;
            if (requestingUsername != null)
                requestingUser = await _userManager.FindByNameAsync(requestingUsername);

            Log log = new Log()
            {
                Endpoint = controller.HttpContext.Request.Path,
                HTTPMethod = controller.HttpContext.Request.Method,
                Type = type.GetDisplayName(),
                Message = message,
                ApplicationUserId = requestingUser?.Id,
                StatusCode = statusCode
            };

            await _context.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
