using AutoMapper;
using HrCommonApi.Authorization;
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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace HrCommonApi.Extensions;

/// <summary>
/// My advice is to not look at this extension in any way or shape.
/// </summary>
public static class IServiceCollectionExtensions
{

    public static IServiceCollection AddHrCommonApiServices<TDataContext>(this IServiceCollection services, IConfiguration configuration, Action<AuthorizationOptions>? configureCustomAuthorization = null)
        where TDataContext : DbContext
        => services.AddHrCommonApiServices<TDataContext, User, ApiKey>(configuration, configureCustomAuthorization, false, false);

    public static IServiceCollection AddHrCommonJwtApiServices<TDataContext, TUser>(this IServiceCollection services, IConfiguration configuration, Action<AuthorizationOptions>? configureCustomAuthorization = null)
        where TDataContext : DbContext
        where TUser : User
        => services.AddHrCommonApiServices<TDataContext, TUser, ApiKey>(configuration, configureCustomAuthorization, true, false);

    public static IServiceCollection AddHrCommonKeyApiServices<TDataContext, TKey>(this IServiceCollection services, IConfiguration configuration, Action<AuthorizationOptions>? configureCustomAuthorization = null)
        where TDataContext : DbContext
        where TKey : ApiKey
        => services.AddHrCommonApiServices<TDataContext, User, TKey>(configuration, configureCustomAuthorization, false, true);

    /// <summary>
    /// Adds the services, profiles, and database context to the DI container.
    /// </summary>
    /// <exception cref="InvalidOperationException">Returns an InvalidOperationException if the configuration is improper.</exception>
    public static IServiceCollection AddHrCommonApiServices<TDataContext, TUser, TKey>(this IServiceCollection services, IConfiguration configuration, Action<AuthorizationOptions>? configureCustomAuthorization = null, bool jwtEnabled = true, bool keyEnabled = true)
        where TDataContext : DbContext
        where TUser : User
        where TKey : ApiKey
    {
        var simpleUserEnabled = typeof(TUser) == typeof(User);
        var simpleKeyEnabled = typeof(TKey) == typeof(ApiKey);

        var allowedOrigins = configuration.GetSection("HrCommonApi:CorsAllowOrigins").Get<List<string>>();
        if (allowedOrigins != null)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(allowedOrigins.ToArray())
                      .AllowAnyHeader()
                      .AllowAnyMethod();
                });
            });
        }

        // JWT or API keys, probably both
        if (jwtEnabled)
        {
            var issuer = configuration?["HrCommonApi:JwtAuthorization:Jwt:Issuer"];
            var audience = configuration?["HrCommonApi:JwtAuthorization:Jwt:Audience"];
            var jwtKey = configuration?["HrCommonApi:JwtAuthorization:Jwt:Key"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

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
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = key
                };
                x.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<IServiceCollection>>();
                        logger.LogError($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    }
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
                    In = ParameterLocation.Header,
                    Description = "Please enter your Bearer token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
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
                    Description = "Please enter your API Key",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = configuration["HrCommonApi:ApiKeyAuthorization:ApiKeyName"]
                });

                securityRequirement.Add(new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = configuration["HrCommonApi:ApiKeyAuthorization:ApiKeyName"]
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
        services.AddServicesFromNamespace<TDataContext>(targetServicesNamespace);

        // Add this libraries services
        if (jwtEnabled)
            services.AddScoped<IUserService<TUser>, UserService<TUser, TDataContext>>();
        if (keyEnabled)
            services.AddScoped<IApiKeyService<TKey>, ApiKeyService<TKey, TDataContext>>();

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
    public static IServiceCollection AddServicesFromNamespace<TDataContext>(this IServiceCollection services, string targetNamespace)
        where TDataContext : DbContext
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().First(q => q.FullName != null && q.FullName.Contains(targetNamespace.Split('.')[0]));
        foreach (Type implementationType in ReflectionUtils.GetTypesInNamespaceImplementing<CoreService<DbEntity, TDataContext>>(assembly, targetNamespace))
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
        var assembly = AppDomain.CurrentDomain.GetAssemblies().First(q => q.FullName != null && q.FullName.Contains(targetNamespace.Split('.')[0]));
        foreach (var type in ReflectionUtils.GetTypesInNamespaceImplementing<Profile>(assembly, targetNamespace))
        {
            services.AddAutoMapper(type);
        }

        return services;
    }
}
