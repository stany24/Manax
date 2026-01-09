using ManaxLibrary;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.User;
using ManaxServer.Services.Feature;
using ManaxServer.Services.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ManaxServer.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RequireFeatureAttribute(params FeatureType[] features) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        IFeatureService featureService = context.HttpContext.RequestServices.GetRequiredService<IFeatureService>();

        bool allFeatureEnabled = features.All(feature =>
            featureService.IsFeatureEnabled(feature));

        if (!allFeatureEnabled) context.Result = new ForbidResult(nameof(ErrorCode.FeatureDisabled));
    }
}