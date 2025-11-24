using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RuralHealthcare.Web.Filters;

public class AuthorizePageFilter : IPageFilter
{
    private readonly string[]? _allowedRoles;

    public AuthorizePageFilter(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles.Length > 0 ? allowedRoles : null;
    }

    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var httpContext = context.HttpContext;
        var userId = httpContext.Session.GetString("UserId");

        // Check if user is authenticated
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new RedirectToPageResult("/Index");
            return;
        }

        // Check role-based access if roles are specified
        if (_allowedRoles != null && _allowedRoles.Length > 0)
        {
            var userRole = httpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userRole) || !_allowedRoles.Contains(userRole))
            {
                context.Result = new RedirectToPageResult("/AccessDenied");
                return;
            }
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }
}
