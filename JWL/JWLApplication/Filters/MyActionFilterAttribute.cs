using Data;
using LoggerService.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Models;
using System;

namespace JWLApplication.Filters
{
    public class MyActionFilterAttribute : Attribute, IActionFilter
    {
        private readonly ILoggerManager _loggerManager;
        public MyActionFilterAttribute(ILoggerManager loggerManager)
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
