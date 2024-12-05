using HrCommonApi.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace HrCommonApi.Authorization;

/// <summary>
/// This middleware adds extra claims based on the API key header. It is only blocking is a claimed api key is not present or is invalid.
/// </summary>
public class ApiKeyAuthMiddleware(RequestDelegate _next)
{

    public async Task InvokeAsync(HttpContext context, HrDataContext dataContext, IConfiguration configuration)
    {
        var apiKeyHeaderName = configuration["HrCommonApi:ApiKeyName"]!;
        var apiKeys = configuration.GetSection("HrCommonApi:AcceptedApiKeys").Get<List<string>>()!;

        if (context.Request.Headers.TryGetValue(apiKeyHeaderName, out var extractedApiKey))
        {
            if (!apiKeys.Contains(extractedApiKey!))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API key");
                return;
            }

            var apiKeyIdentity = new ClaimsIdentity("ApiKey");
            apiKeyIdentity.AddClaim(new Claim(HrCommonApiKeyClaims.ApiKey, extractedApiKey!));
            context.User.AddIdentity(apiKeyIdentity);
        }

        await _next(context);
    }
}
