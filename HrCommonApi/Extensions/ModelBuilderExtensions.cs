﻿using HrCommonApi.Database.Models;
using HrCommonApi.Database.Models.Base;
using HrCommonApi.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HrCommonApi.Extensions;

/// <summary>
/// My advice is to not look at this extension in any way or shape.
/// </summary>
public static class ModelBuilderExtensions
{
    public static ModelBuilder AddEntityModels(this ModelBuilder modelBuilder, IConfiguration configuration)
        => modelBuilder.AddEntityModels<User, ApiKey>(configuration, false, false);

    public static ModelBuilder AddEntityModelsWithJwt<TUser>(this ModelBuilder modelBuilder, IConfiguration configuration)
        where TUser : User
        => modelBuilder.AddEntityModels<TUser, ApiKey>(configuration, true, false);

    public static ModelBuilder AddEntityModelsWithKey<TKey>(this ModelBuilder modelBuilder, IConfiguration configuration)
        where TKey : ApiKey
        => modelBuilder.AddEntityModels<User, TKey>(configuration, false, true);

    /// <summary>
    /// Adds the entity models to the ModelBuilder.
    /// </summary>
    public static ModelBuilder AddEntityModels<TUser, TKey>(this ModelBuilder modelBuilder, IConfiguration configuration, bool jwtEnabled = true, bool keyEnabled = true)
        where TUser : User
        where TKey : ApiKey
    {
        var targetNamespace = configuration?["HrCommonApi:Namespaces:Models"] ?? null;
        if (string.IsNullOrEmpty(targetNamespace))
            throw new InvalidOperationException("The target namespace for the Models is not set. Expected configuration key: \"HrCommonApi:Namespaces:Models\"");

        modelBuilder.AddEntityModelsFromNamespace(targetNamespace);

        // Add this libraries models
        modelBuilder.MapCommonEntities<TUser, TKey>(jwtEnabled, keyEnabled);

        return modelBuilder;
    }

    /// <summary>
    /// Adds the entity models in the target namespace to the ModelBuilder.
    /// </summary>
    public static ModelBuilder AddEntityModelsFromNamespace(this ModelBuilder modelBuilder, string targetNamespace)
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().First(q => q.FullName != null && q.FullName.Contains(targetNamespace.Split('.')[0]));
        foreach (var entityType in ReflectionUtils.GetTypesInNamespaceImplementing<DbEntity>(assembly, targetNamespace))
            modelBuilder.Entity(entityType);

        return modelBuilder;
    }

    public static ModelBuilder MapCommonEntities(this ModelBuilder modelBuilder)
        => modelBuilder.MapCommonEntities<User, ApiKey>(false, false);

    public static ModelBuilder MapCommonJwtEntities<TUser>(this ModelBuilder modelBuilder)
        where TUser : User
        => modelBuilder.MapCommonEntities<TUser, ApiKey>(true, false);

    public static ModelBuilder MapCommonKeyEntities<TKey>(this ModelBuilder modelBuilder)
        where TKey : ApiKey
        => modelBuilder.MapCommonEntities<User, TKey>(false, true);

    /// <summary>
    /// 
    /// </summary>
    public static ModelBuilder MapCommonEntities<TUser, TKey>(this ModelBuilder modelBuilder, bool jwtEnabled = true, bool keyEnabled = true)
        where TUser : User
        where TKey : ApiKey
    {
        var simpleUserEnabled = typeof(TUser) == typeof(User);
        var simpleKeyEnabled = typeof(TKey) == typeof(ApiKey);

        if (jwtEnabled)
        {
            if (simpleUserEnabled)
                modelBuilder.Entity<User>();

            modelBuilder.Entity<Session>();
        }

        if (keyEnabled)
        {
            if (simpleKeyEnabled)
                modelBuilder.Entity<ApiKey>();

            modelBuilder.Entity<Claim>();
        }

        return modelBuilder;
    }
}
