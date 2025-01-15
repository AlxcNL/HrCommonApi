using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrCommonApi.Authorization;

public interface IApiKeyValidation
{
    bool IsValidApiKey(string apiKey);
}
