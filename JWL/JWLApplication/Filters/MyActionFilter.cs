using LoggerService.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JWLApplication.Filters
{
    public class MyActionFilter : IActionFilter
    {
        private readonly ILoggerManager _loggerManager;

        public MyActionFilter(ILoggerManager loggerManager)
        {
            _loggerManager = loggerManager;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            _loggerManager.LogInfo("After Method Execution" + context.ActionDescriptor.DisplayName);
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _loggerManager.LogInfo("Before  Method Execution" + context.ActionDescriptor.DisplayName);
        }
    }
}
