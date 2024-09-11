using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace NewFace.Filters;


// 1. ACTOR
public class AuthenticateAndValidateActorAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userIdString = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var actorIdString = context.HttpContext.User.FindFirst("ActorId")?.Value;

        if (userIdString == null || !int.TryParse(userIdString, out int userId) ||
            actorIdString == null || !int.TryParse(actorIdString, out int actorId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
            return;
        }

        // ActorId 파라미터 검증
        var actorIdParameter = context.ActionArguments.FirstOrDefault(p => p.Key.Equals("actorId", StringComparison.OrdinalIgnoreCase)).Value;
        if (actorIdParameter == null || !int.TryParse(actorIdParameter.ToString(), out int requestedActorId) || requestedActorId != actorId)
        {
            context.Result = new ForbidResult();
            return;
        }

        context.HttpContext.Items["UserId"] = userId;
        context.HttpContext.Items["ActorId"] = actorId;

        base.OnActionExecuting(context);
    }
}


// 2. ENTERTAINMENT

public class AuthenticateAndValidateEntertainmentAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userIdString = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var enterIdString = context.HttpContext.User.FindFirst("EnterId")?.Value;

        if (userIdString == null || !int.TryParse(userIdString, out int userId) ||
            enterIdString == null || !int.TryParse(enterIdString, out int enterId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
            return;
        }

        // EnterId 파라미터 검증
        var enterIdParameter = context.ActionArguments.FirstOrDefault(p => p.Key.Equals("enterId", StringComparison.OrdinalIgnoreCase)).Value;
        if (enterIdParameter == null || !int.TryParse(enterIdParameter.ToString(), out int requestedEnterId) || requestedEnterId != enterId)
        {
            context.Result = new ForbidResult();
            return;
        }

        context.HttpContext.Items["UserId"] = userId;
        context.HttpContext.Items["EnterId"] = enterId;

        base.OnActionExecuting(context);
    }
}


// 3. USER
public class AuthenticateAndValidateUserAttribute : ActionFilterAttribute
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