using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireRoleAttribute : ActionFilterAttribute
{
    private readonly string[] _roles;
    public RequireRoleAttribute(params string[] roles) => _roles = roles;
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        if (session.GetInt32("UserID") is null)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }
        var role = session.GetString("Role");
        if (!_roles.Contains(role, StringComparer.Ordinal))
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
    }
}
