using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace API.ErrorHandling {
    public static class ExceptionMiddlewareExtensions {
        //Follow https://code-maze.com/global-error-handling-aspnetcore/ to create global exception handling
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger) {
            app.UseExceptionHandler(appError => {
                appError.Run(async context => {
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null) {
                        var a = int.Parse(contextFeature.Error.Message.Substring(0, 3));
                        context.Response.StatusCode = a;
                        string message;
                        if (a >= 500) {
                            message = "Internal server error";
                        } else {
                            message = $"{contextFeature.Error.Message.Substring(5)}";
                        }
                        logger.LogError($"Something went wrong: {contextFeature.Error}");
                        await context.Response.WriteAsync(new ErrorDetails() {
                            StatusCode = context.Response.StatusCode,
                            Message = message
                        }.ToString());
                    }
                });
            });
        }
    }
}
