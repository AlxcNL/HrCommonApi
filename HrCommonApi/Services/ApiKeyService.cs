using HrCommonApi.Database.Models;
using HrCommonApi.Enums;
using HrCommonApi.Services.Base;
using Microsoft.EntityFrameworkCore;

namespace HrCommonApi.Services;

public class ApiKeyService<TApiKey, TDataContext>(TDataContext context) : CoreService<TApiKey, TDataContext>(context), IApiKeyService<TApiKey> where TApiKey : ApiKey
    where TDataContext : DbContext
{
    public async Task<ServiceResponse<TApiKey>> Authorize(string key)
    {
        try
        {
            var apiKey = await ServiceTable.FirstOrDefaultAsync(q => q.Key == key);
            if (apiKey == null)
                return new ServiceResponse<TApiKey>(ServiceCode.NotFound, message: "Key not found");

            if (!apiKey.Enabled)
                return new ServiceResponse<TApiKey>(ServiceCode.NotFound, message: "Key not enabled.");

            return new ServiceResponse<TApiKey>(ServiceCode.Success, apiKey, message: "Successfully authorized");
        }
        catch (Exception exception)
        {
            return new ServiceResponse<TApiKey>(ServiceCode.Exception, exception: exception, message: exception.Message);
        }
    }
}