using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace CountryhouseService.API.Extensions
{
    public static class LoggerExtension
    {
        public static ILogger<T> LogControllerAction<T>(
            this ILogger<T> logger, 
            LogLevel level, 
            string message, 
            [CallerMemberName] string actionName = "") where T : ControllerBase
        {
            var date = DateTime.UtcNow.ToString("s");
            logger.Log(level, "{date} | {actionName}: {message}", date, actionName, message);
            return logger;
        }
    }
}
