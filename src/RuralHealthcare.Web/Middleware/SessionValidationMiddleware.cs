namespace RuralHealthcare.Web.Middleware;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionValidationMiddleware> _logger;

    public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        // Public paths that don't require authentication
        var publicPaths = new[] { "/", "/index", "/login", "/emergency/create", "/accessdenied", "/logout", "/account/forgotpassword", "/account/resetpassword" };
        var isPublicPath = publicPaths.Any(p => path == p || path.StartsWith("/css") || path.StartsWith("/js"));

        if (!isPublicPath)
        {
            var userId = context.Session.GetString("UserId");
            
            if (string.IsNullOrEmpty(userId))
            {
                // Not authenticated - redirect to home
                context.Response.Redirect("/");
                return;
            }

            // Log access for audit trail
            var userEmail = context.Session.GetString("UserEmail");
            _logger.LogInformation("User {Email} accessed {Path}", userEmail, path);
        }

        await _next(context);
    }
}

public static class SessionValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionValidationMiddleware>();
    }
}
