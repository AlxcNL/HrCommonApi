﻿using HrCommonApi.Database.Models;

namespace HrCommonApi.Services.Base;

public interface IApiKeyService<TApiKey> where TApiKey : ApiKey
{
    Task<ServiceResponse<TApiKey>> Authorize(string key);
}