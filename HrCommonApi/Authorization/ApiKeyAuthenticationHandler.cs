using HrCommonApi.Database.Models;
using HrCommonApi.Services.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
namespace HrCommonApi.Authorization;

public class ApiKeyAuthenticationHandler<TApiKey>(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IConfiguration configuration, IApiKeyService<TApiKey> apiKeyService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
     where TApiKey : ApiKey
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Retrieve the name of the API key header from the configuration
        var apiKeyHeaderName = configuration["HrCommonApi:ApiKeyAuthorization:ApiKeyName"];
        if (string.IsNullOrWhiteSpace(apiKeyHeaderName))
        {
            return AuthenticateResult.Fail("API Key header name is not configured.");
        }

        // Check if the API key is present in the request headers
        if (!Context.Request.Headers.TryGetValue(apiKeyHeaderName, out var extractedApiKey))
        {
            return AuthenticateResult.Fail("API Key was not provided.");
        }

        // Validate the API key by checking against accepted keys or a service
        var acceptedApiKeys = configuration.GetSection("HrCommonApi:ApiKeyAuthorization:AcceptedApiKeys").Get<List<string>>()!;
        if (!acceptedApiKeys.Contains(extractedApiKey))
        {
            return AuthenticateResult.Fail("Invalid API Key.");
        }

        // Call the API key service to authorize the key and retrieve claims
        var response = await apiKeyService.Authorize(extractedApiKey);
        if (response.Code != Enums.ServiceCode.Success)
        {
            return AuthenticateResult.Fail("Validation of API Key failed.");
        }

        if (response.Result != null && response.Result.Enabled)
        {
            // Create claims identity and principal
            var apiKeyIdentity = new ClaimsIdentity("ApiKey");
            apiKeyIdentity.AddClaim(new Claim(HrCommonApiKeyClaims.ApiKey, extractedApiKey));

            // Add additional claims from the response
            foreach (var claim in response.Result.Rights)
            {
                apiKeyIdentity.AddClaim(new Claim(claim.Name, claim.Value));
            }

            var claimsPrincipal = new ClaimsPrincipal(apiKeyIdentity);
            var ticket = new AuthenticationTicket(claimsPrincipal, "ApiKey");

            return AuthenticateResult.Success(ticket);
        }

        return AuthenticateResult.Fail("API Key is not enabled.");
    }
}