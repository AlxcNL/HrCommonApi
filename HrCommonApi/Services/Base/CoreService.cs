using HrCommonApi.Controllers.Requests;
using HrCommonApi.Database.Models.Base;
using HrCommonApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrCommonApi.Services.Base;

public abstract class CoreService<TEntity, TDataContext>(TDataContext context) : ICoreService<TEntity>
    where TEntity : DbEntity
    where TDataContext : DbContext
{
    protected TDataContext Context { get; } = context;
    protected DbSet<TEntity> ServiceTable => Context.Set<TEntity>()!;

    public virtual async Task<ServiceResponse<List<TEntity>>> Get(Guid[]? ids = null)
    {
        try
        {
            if (ids == null)
                return new ServiceResponse<List<TEntity>>(ServiceCode.Success, await ServiceTable.ToListAsync(), message: $"Result set contains {ServiceTable.Count()} items");

            var resultSet = await ServiceTable.Where(q => ids.Contains(q.Id)).ToListAsync();
            if (!resultSet.Any())
                return new ServiceResponse<List<TEntity>>(ServiceCode.NotFound, message: $"No results found for id's: {string.Join(',', ids)}");

            return new ServiceResponse<List<TEntity>>(ServiceCode.Success, resultSet, message: $"Result set contains {resultSet.Count} items");
        }
        catch (Exception exception)
        {
            return new ServiceResponse<List<TEntity>>(ServiceCode.Exception, exception: exception, message: exception.Message);
        }
    }

    public virtual async Task<ServiceResponse<TEntity>> Get(Guid id)
    {
        try
        {
            var result = await ServiceTable.FindAsync(id);
            if (result == null)
                return new ServiceResponse<TEntity>(ServiceCode.NotFound, message: $"No results found for id: {id}");

            return new ServiceResponse<TEntity>(ServiceCode.Success, result, message: $"Result item with id: {id}");
        }
        catch (Exception exception)
        {
            return new ServiceResponse<TEntity>(ServiceCode.Exception, exception: exception, message: exception.Message);
        }
    }

    public virtual async Task<ServiceResponse<TEntity>> Create(TEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.Now.ToUniversalTime();
            entity.UpdatedAt = DateTime.Now.ToUniversalTime();

            var addedEntity = await ServiceTable.AddAsync(entity);
            await Context.SaveChangesAsync();

            if (addedEntity == null)
                return new ServiceResponse<TEntity>(ServiceCode.BadRequest, message: "Could not create entity");

            // Explicitly load navigation properties if needed
            foreach (var navigation in Context.Entry(addedEntity.Entity).Navigations)
                if (!navigation.IsLoaded)
                    await navigation.LoadAsync();

            return new ServiceResponse<TEntity>(ServiceCode.Success, addedEntity.Entity, message: "Entity created");
        }
        catch (Exception exception)
        {
            return new ServiceResponse<TEntity>(ServiceCode.Exception, exception: exception, message: exception.Message);
        }
    }

    public virtual async Task<ServiceResponse<TEntity>> Update(Guid id, IRequest updateModel, bool isPatch = false)
    {
        try
        {
            var entity = await ServiceTable.FindAsync(id);
            if (entity == null)
                return new ServiceResponse<TEntity>(ServiceCode.NotFound, message: $"Could not find {typeof(TEntity)} with id: {id}");

            var objType = updateModel.GetType();
            var updateProperties = objType.GetProperties();
            int updates = 0;

            foreach (var prop in updateProperties)
            {
                var propValue = prop.GetValue(updateModel);
                if (propValue == null && isPatch) // If the update model property is null && is a patch request
                    continue; // skip it

                var entityProp = entity.GetType().GetProperty(prop.Name);
                if (entityProp == null) // If the entity does not have an exact match for this update model property
                    continue; // Skip it

                var propType = entityProp.PropertyType; // Get the type of the target entity property
                if (propValue == null && (!propType.IsGenericType || propType.GetGenericTypeDefinition() != typeof(Nullable<>)))
                    continue; // Skip it, can't set null to a non nullable type

                var entityValue = entityProp.GetValue(entity);
                if (propValue == entityValue)
                    continue; // Skip it, no change

                entityProp.SetValue(entity, propValue);
                updates++;
            }

            if (updates == 0)
                return new ServiceResponse<TEntity>(ServiceCode.BadRequest, message: $"No updates found for {typeof(TEntity)} with id: {id}");

            entity.UpdatedAt = DateTime.Now.ToUniversalTime();
            var changesSaved = await Context.SaveChangesAsync();

            return new ServiceResponse<TEntity>(ServiceCode.Success, entity, message: $"Updated {typeof(TEntity)} with id: {id}, saved {changesSaved} updates, {updates} expected");
        }
        catch (Exception exception)
        {
            return new ServiceResponse<TEntity>(ServiceCode.Exception, exception: exception, message: exception.Message);
        }
    }

    public virtual async Task<ServiceResponse<TEntity>> Delete(Guid id)
    {
        try
        {
            var entity = await ServiceTable.FindAsync(id);
            if (entity == null)
                return new ServiceResponse<TEntity>(ServiceCode.NotFound, message: $"Could not find {typeof(TEntity)} with id: {id}");

            ServiceTable.Remove(entity);
            await Context.SaveChangesAsync();

            return new ServiceResponse<TEntity>(ServiceCode.Success, entity, message: $"Deleted {typeof(TEntity)} with id: {id}");
        }
        catch (Exception exception)
        {
            return new ServiceResponse<TEntity>(ServiceCode.Exception, exception: exception, message: exception.Message);
        }
    }
}
