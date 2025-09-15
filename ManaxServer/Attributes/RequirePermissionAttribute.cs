using ManaxLibrary.DTO.User;
using ManaxServer.Services.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ManaxServer.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RequirePermissionAttribute(params Permission[] permissions) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Get token from context items (set by BearerAuthenticationMiddleware)
        string? token = context.HttpContext.Items["BearerToken"] as string;
        if (string.IsNullOrEmpty(token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        ITokenService tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();

        bool hasAllPermissions = permissions.All(permission =>
            tokenService.TokenHasPermission(token, permission));

        if (!hasAllPermissions) context.Result = new ForbidResult();
    }
}