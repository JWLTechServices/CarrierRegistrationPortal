using LoggerService.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Models;
using System;
using System.Net;

namespace JWLApplication
{
    public static class CustomExtension
    {
        public static IApplicationBuilder UseCustomRoute(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentException(nameof(app));
            }
            else
            {
                return app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "Default",
                        template: "{controller=Carrier}/{action=create}/{id:int?}"

                        );
                }
                );

            }
        }


        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILoggerManager loggerManager)
        {
            app.UseExceptionHandler(t =>
            {
                t.Run(async x =>
                {
                    x.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    x.Response.ContentType = "application/json";
                    var contextFeature = x.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        loggerManager.LogError($"Something went wrong: {contextFeature.Error}");



                        await x.Response.WriteAsync(new ErrorDetails()
                        {
                            StatusCode = x.Response.StatusCode,
                            Message = "Internal Server Error Credencys Generic."
                        }.ToString());

                    }

                });

            });
        }
    }
}
