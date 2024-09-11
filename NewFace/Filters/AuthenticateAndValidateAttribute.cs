using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace NewFace.Filters;

public class AuthenticateAndValidateAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userIdString = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null || !int.TryParse(userIdString, out int userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
            return;
        }

        context.HttpContext.Items["UserId"] = userId;

        base.OnActionExecuting(context);
    }
}