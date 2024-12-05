using AutoMapper;
using HrCommonApi.Database;
using HrCommonApi.Database.Models.Base;
using HrCommonApi.Services.Base;
using HrCommonApi.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HrCommonApi.Extensions;

/// <summary>
/// My advice is to not look at this extension in any way or shape.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services, profiles, and database context to the DI container.
    /// </summary>
    /// <exception cref="InvalidOperationException">Returns an InvalidOperationException if the configuration is improper.</exception>
    public static IServiceCollection AddHrCommonApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var targetServicesNamespace = configuration?["HrCommonApi:Namespaces:Services"] ?? null;
        if (string.IsNullOrEmpty(targetServicesNamespace))
            throw new InvalidOperationException("The target namespace for the Services is not set. Expected configuration key: \"HrCommonApi:Namespaces:Services\"");

        // Services used by the frontend
        services.AddServicesFromNamespace(targetServicesNamespace);

        // Add this libraries services
        services.AddServicesFromNamespace("HrCommonApi.Services");

        var targetProfilesNamespace = configuration?["HrCommonApi:Namespaces:Profiles"] ?? null;
        if (string.IsNullOrEmpty(targetProfilesNamespace))
            throw new InvalidOperationException("The target namespace for the Profiles is not set. Expected configuration key: \"HrCommonApi:Namespaces:Profiles\"");

        // AutoMapper for mapping Entities to Responses and vice versa
        services.AddProfilesFromNamespace(targetProfilesNamespace);

        // Add this libraries profiles
        services.AddProfilesFromNamespace("HrCommonApi.Profiles");

        // Database context
        var targetConnectionString = configuration?["HrCommonApi:ConnectionString"] ?? null;
        if (string.IsNullOrEmpty(targetConnectionString))
            throw new InvalidOperationException("The target ConnectionString for the HrCommonApi is missing. Expected configuration key: \"HrCommonApi:ConnectionString\"");

        services.AddDbContext<HrDataContext>(options =>
            options.UseNpgsql(configuration!.GetConnectionString(targetConnectionString)).UseLazyLoadingProxies()
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
