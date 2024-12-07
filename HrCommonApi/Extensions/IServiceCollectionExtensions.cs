using AutoMapper;
using HrCommonApi.Authorization;
using HrCommonApi.Database;
using HrCommonApi.Database.Models;
using HrCommonApi.Database.Models.Base;
using HrCommonApi.Profiles;
using HrCommonApi.Services;
using HrCommonApi.Services.Base;
using HrCommonApi.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HrCommonApi.Extensions;

/// <summary>
/// My advice is to not look at this extension in any way or shape.
/// </summary>
public static class IServiceCollectionExtensions
{

    public static IServiceCollection AddHrCommonApiServices<TDataContext>(this IServiceCollection services, IConfiguration configuration, Action<AuthorizationOptions>? configureCustomAuthorization = null)
        where TDataContext : HrCommonDataContext
        => services.AddHrCommonApiServices<TDataContext, User, ApiKey>(configuration, configureCustomAuthorization, false, false);

    public static IServiceCollection AddHrCommonJwtApiServices<TDataContext, TUser>(this IServiceCollection services, IConfiguration configuration, Action<AuthorizationOptions>? configureCustomAuthorization = null)
        where TDataContext : HrCommonDataContext
        where TUser : User
        => services.AddHrCommonApiServices<TDataContext, TUser, ApiKey>(configuration, configureCustomAuthorization, true, false);

    public static IServiceCollection AddHrCommonKeyApiServices<TDataContext, TKey>(this IServiceCollection services, IConfiguration configuration, Action<AuthorizationOptions>? configureCustomAuthorization = null)
        where TDataContext : HrCommonDataContext
        where TKey : ApiKey
        => services.AddHrCommonApiServices<TDataContext, User, TKey>(configuration, configureCustomAuthorization, false, true);

    /// <summary>
    /// Adds the services, profiles, and database context to the DI container.
    /// </summary>
    /// <exception cref="InvalidOperationException">Returns an InvalidOperationException if the configuration is improper.</exception>
    public static IServiceCollection AddHrCommonApiServices<TDataContext, TUser, TKey>(this IServiceCollection services, IConfiguration configuration, Action<AuthorizationOptions>? configureCustomAuthorization = null, bool jwtEnabled = true, bool keyEnabled = true)
        where TDataContext : HrCommonDataContext
        where TUser : User
        where TKey : ApiKey
    {
        var simpleUserEnabled = typeof(TUser) == typeof(User);
        var simpleKeyEnabled = typeof(TKey) == typeof(ApiKey);

        // JWT or API keys, probably both
        if (jwtEnabled)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false; // Set to true in production
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration?["JwtAuthorization:Jwt:Issuer"],
                    ValidAudience = configuration?["JwtAuthorization:Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtAuthorization:Jwt:Key"]!))
                };
            });
        }

        if (jwtEnabled || keyEnabled)
        {
            services.AddAuthorization(HrCommonApiPolicies.ConfigurePolicies);
            if (configureCustomAuthorization != null)
                services.AddAuthorization(configureCustomAuthorization);
        }

        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(q =>
        {
            if (!jwtEnabled && !keyEnabled)
                return;

            var securityRequirement = new OpenApiSecurityRequirement();

            if (jwtEnabled)
            {
                // Define the Bearer token security scheme
                q.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                securityRequirement.Add(new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }, new string[] { });
            }

            if (keyEnabled)
            {
                // Define the API key security scheme
                q.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "API key needed to access the endpoints using the ApiKey scheme. Example: \"Authorization: x-api-key {key}\"",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Name = configuration["HrCommonApi:ApiKeyAuthorization:ApiKeyName"],
                    Scheme = "ApiKey"
                });

                securityRequirement.Add(new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    }
                }, new string[] { });
            }

            // Add a security requirement that includes both schemes
            q.AddSecurityRequirement(securityRequirement);
        });

        var targetServicesNamespace = configuration?["HrCommonApi:Namespaces:Services"] ?? null;
        if (string.IsNullOrEmpty(targetServicesNamespace))
            throw new InvalidOperationException("The target namespace for the Services is not set. Expected configuration key: \"HrCommonApi:Namespaces:Services\"");

        // Services used by the frontend
        services.AddServicesFromNamespace(targetServicesNamespace);

        // Add this libraries services
        if (jwtEnabled && simpleUserEnabled)
            services.AddScoped<IUserService<User>, UserService<User>>();
        if (keyEnabled && simpleKeyEnabled)
            services.AddScoped<IApiKeyService<ApiKey>, ApiKeyService<ApiKey>>();

        var targetProfilesNamespace = configuration?["HrCommonApi:Namespaces:Profiles"] ?? null;
        if (string.IsNullOrEmpty(targetProfilesNamespace))
            throw new InvalidOperationException("The target namespace for the Profiles is not set. Expected configuration key: \"HrCommonApi:Namespaces:Profiles\"");

        // AutoMapper for mapping Entities to Responses and vice versa
        services.AddProfilesFromNamespace(targetProfilesNamespace);

        // Add this libraries profiles
        if (jwtEnabled && simpleUserEnabled)
            services.AddAutoMapper(typeof(UserProfiles));
        if (keyEnabled && simpleKeyEnabled)
            services.AddAutoMapper(typeof(ApiKeyProfiles));

        // Database context
        var targetConnectionString = configuration?["HrCommonApi:ConnectionString"] ?? null;
        if (string.IsNullOrEmpty(targetConnectionString))
            throw new InvalidOperationException("The target ConnectionString for the HrCommonApi is missing. Expected configuration key: \"HrCommonApi:ConnectionString\"");

        services.AddDbContext<TDataContext>(options =>
            options.UseNpgsql(configuration!.GetConnectionString(targetConnectionString))
            .UseLazyLoadingProxies()
        );

        return services;
    }


    /// <summary>
    /// Adds the services in the target namespace to the DI container.
    /// </summary>
    public static IServiceCollection AddServicesFromNamespace(this IServiceCollection services, string targetNamespace)
    {
        foreach (Type implementationType in ReflectionUtils.GetTypesInNamespaceImplementing<CoreService<DbEntity>>(Assembly.GetExecutingAssembly(), targetNamespace))
        {
            Type? serviceType = ReflectionUtils.TryGetInterfaceForType(implementationType);
            if (serviceType == null)
                continue;

            services.AddScoped(serviceType, implementationType);
        }

        return services;
    }

    /// <summary>
    /// Adds the profiles in the target namespace to the DI container.
    /// </summary>
    public static IServiceCollection AddProfilesFromNamespace(this IServiceCollection services, string targetNamespace)
    {
        foreach (var type in ReflectionUtils.GetTypesInNamespaceImplementing<Profile>(Assembly.GetExecutingAssembly(), targetNamespace))
        {
            services.AddAutoMapper(type);
        }

        return services;
    }
}
