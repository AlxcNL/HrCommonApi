using Microsoft.Extensions.Configuration;

namespace HrCommonApi.Authorization;

public class ApiKeyValidation(IConfiguration configuration) : IApiKeyValidation
{
    private readonly IConfiguration _configuration = configuration;

    public bool IsValidApiKey(string userApiKey)
    {
        var apiKeyHeaderName = _configuration["HrCommonApi:ApiKeyAuthorization:ApiKeyName"]!;
        string? apiKey = _configuration.GetValue<string>(apiKeyHeaderName);
        //var apiKeys = _configuration.GetSection("HrCommonApi:ApiKeyAuthorization:AcceptedApiKeys").Get<List<string>>()!;

        if (string.IsNullOrWhiteSpace(userApiKey))
        {
            return false;
        }

        if (apiKey == null || apiKey != userApiKey)
        {
            return false;
        }

        return true;
    }
}
