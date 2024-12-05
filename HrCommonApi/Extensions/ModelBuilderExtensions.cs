using HrCommonApi.Database.Models;
using HrCommonApi.Database.Models.Base;
using HrCommonApi.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace HrCommonApi.Extensions;

/// <summary>
/// My advice is to not look at this extension in any way or shape.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Adds the entity models to the ModelBuilder.
    /// </summary>
    public static ModelBuilder AddEntityModels(this ModelBuilder modelBuilder, IConfiguration configuration)
    {
        var targetNamespace = configuration?["HrCommonApi:Namespaces:Models"] ?? null;
        if (string.IsNullOrEmpty(targetNamespace))
            throw new InvalidOperationException("The target namespace for the Models is not set. Expected configuration key: \"HrCommonApi:Namespaces:Models\"");

        modelBuilder.AddEntityModelsFromNamespace(targetNamespace);

        // Add this libraries models
        modelBuilder.MapCommonEntities(configuration!);

        return modelBuilder;
    }

    /// <summary>
    /// Adds the entity models in the target namespace to the ModelBuilder.
    /// </summary>
    public static ModelBuilder AddEntityModelsFromNamespace(this ModelBuilder modelBuilder, string targetNamespace)
    {
        foreach (var entityType in ReflectionUtils.GetTypesInNamespaceImplementing<DbEntity>(Assembly.GetExecutingAssembly(), targetNamespace))
        {
            if (typeof(IMappedEntity).IsAssignableFrom(entityType))
            {
                entityType.GetMethod(nameof(IMappedEntity.MapEntity), BindingFlags.Static | BindingFlags.Public)!.Invoke(null, [modelBuilder]);
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// 
    /// </summary>
    public static ModelBuilder MapCommonEntities(this ModelBuilder modelBuilder, IConfiguration configuration)
    {
        var jwtEnabled = configuration.GetValue<bool>("HrCommonApi:JwtAuthorization:Enabled");
        var keyEnabled = configuration.GetValue<bool>("HrCommonApi:ApiKeyAuthorization:Enabled");
        var simpleUserEnabled = configuration.GetValue<bool>("HrCommonApi:JwtAuthorization:SimpleUser");
        var simpleKeyEnabled = configuration.GetValue<bool>("HrCommonApi:ApiKeyAuthorization:SimpleKey");

        if (jwtEnabled)
        {
            if (simpleUserEnabled)
            {
                modelBuilder.Entity<User>().HasKey(q => q.Id);
                modelBuilder.Entity<User>().HasIndex(q => q.Username).IsUnique();
            }

            modelBuilder.Entity<Session>().HasKey(q => q.Id);
            modelBuilder.Entity<Session>().HasIndex(q => q.AccessToken).IsUnique();
        }

        if (keyEnabled)
        {
            if (simpleKeyEnabled)
            {
                modelBuilder.Entity<ApiKey>().HasKey(q => q.Id);
                modelBuilder.Entity<ApiKey>().HasIndex(q => q.Key).IsUnique();
            }

            modelBuilder.Entity<Right>().HasKey(q => q.Id);
        }

        return modelBuilder;
    }
}
