using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HrCommonApi.Authorization
{
    public static class HrCommonApiPolicies
    {
        public static void ConfigurePolicies(AuthorizationOptions options)
        {
            options.AddPolicy("Admin", HasAdminRights);
        }

        private static void HasAdminRights(AuthorizationPolicyBuilder builder) => builder.RequireClaim(ClaimTypes.Role, "1");
    }
}
