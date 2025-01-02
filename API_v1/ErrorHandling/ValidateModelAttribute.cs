using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Linq;

namespace API.ErrorHandling
{
    public class ValidateModelAttribute : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context) {
            if (!context.ModelState.IsValid) {
                var errorMessage = context.ModelState.Values.SelectMany(p => p.Errors.Select(p => p.ErrorMessage));
                var message = "";
                errorMessage.ToList().ForEach(p => message = message + p + " ");
                context.Result = new UnprocessableEntityObjectResult(new ErrorDetails { 
                    StatusCode = (int) HttpStatusCode.BadRequest, 
                    Message = message
                });
            }
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
