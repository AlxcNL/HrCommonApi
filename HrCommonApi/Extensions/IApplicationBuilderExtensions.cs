using HrCommonApi.Authorization;
using HrCommonApi.Database.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace HrCommonApi.Extensions;

/// <summary>
/// My advice is to not look at this extension in any way or shape.
/// </summary>
public static class IApplicationBuilderExtensions
{
    /// <summary>
    /// Adds common middleware and auth.
    /// </summary>
    public static IApplicationBuilder AddHrCommonApiMiddleware(this IApplicationBuilder app, IConfiguration configuration) => AddHrCommonApiMiddleware<ApiKey>(app, configuration);

    /// <summary>
    /// Adds common middleware and auth.
    /// </summary>
    public static IApplicationBuilder AddHrCommonApiMiddleware<TKey>(this IApplicationBuilder app, IConfiguration configuration) where TKey : ApiKey
    {
        var jwtEnabled = configuration?["HrCommonApi:JwtAuthorization:Enabled"] == "true";
        var keyEnabled = configuration?["HrCommonApi:ApiKeyAuthorization:Enabled"] == "true";

        if (keyEnabled)
        {
            app.UseMiddleware<ApiKeyAuthMiddleware<TKey>>();
        }

        if (jwtEnabled || keyEnabled)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        return app;
    }
}
