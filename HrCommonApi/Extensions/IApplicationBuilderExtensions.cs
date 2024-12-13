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
    public static IApplicationBuilder AddHrCommonApiMiddleware(this IApplicationBuilder app, IConfiguration configuration)
        => AddHrCommonApiMiddleware<User, ApiKey>(app, configuration, false, false);

    public static IApplicationBuilder AddHrCommonJwtApiMiddleware<TUser>(this IApplicationBuilder app, IConfiguration configuration)
        where TUser : User
        => AddHrCommonApiMiddleware<User, ApiKey>(app, configuration, true, false);

    public static IApplicationBuilder AddHrCommonKeyApiMiddleware<TKey>(this IApplicationBuilder app, IConfiguration configuration)
        where TKey : ApiKey
        => AddHrCommonApiMiddleware<User, ApiKey>(app, configuration, false, true);

    /// <summary>
    /// Adds common middleware and auth.
    /// </summary>
    public static IApplicationBuilder AddHrCommonApiMiddleware<TUser, TKey>(this IApplicationBuilder app, IConfiguration configuration, bool jwtEnabled = true, bool keyEnabled = true)
        where TKey : ApiKey
        where TUser : User
    {
        var simpleUserEnabled = typeof(TUser) == typeof(User);
        var simpleKeyEnabled = typeof(TKey) == typeof(ApiKey);

        if (keyEnabled)
        {
            app.UseMiddleware<ApiKeyAuthMiddleware<TKey>>();
        }

        app.UseCors();

        if (jwtEnabled || keyEnabled)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        return app;
    }
}
