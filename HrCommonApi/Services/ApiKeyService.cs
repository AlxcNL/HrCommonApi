using HrCommonApi.Authorization;
using HrCommonApi.Database;
using HrCommonApi.Database.Models;
using HrCommonApi.Enums;
using HrCommonApi.Services.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HrCommonApi.Services;

public class ApiKeyService<TApiKey>(HrDataContext context) : CoreService<TApiKey>(context), IApiKeyService<TApiKey> where TApiKey : ApiKey
{
    public async Task<ServiceResult<TApiKey>> Authorize(string key)
    {
        try
        {
            var apiKey = await ServiceTable.FirstOrDefaultAsync(q => q.Key == key);
            if (apiKey == null)
                return new ServiceResult<TApiKey>(ServiceResponse.NotFound, message: "Key not found");

            if (!apiKey.Enabled)
                return new ServiceResult<TApiKey>(ServiceResponse.NotFound, message: "Key not enabled.");

            return new ServiceResult<TApiKey>(ServiceResponse.Success, apiKey, message: "Successfully authorized");
        }
        catch (Exception exception)
        {
            return new ServiceResult<TApiKey>(ServiceResponse.Exception, exception: exception, message: exception.Message);
        }
    }
}