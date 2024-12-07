using HrCommonApi.Controllers.Requests;
using HrCommonApi.Database;
using HrCommonApi.Database.Models.Base;
using HrCommonApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrCommonApi.Services.Base;

public abstract class CoreService<TEntity>(HrCommonDataContext context) : ICoreService<TEntity> where TEntity : DbEntity
{
    protected HrCommonDataContext Context { get; } = context;
    protected DbSet<TEntity> ServiceTable => Context.Set<TEntity>()!;

    public virtual async Task<ServiceResult<List<TEntity>>> Get(Guid[]? ids = null)
    {
        try
        {
            if (ids == null)
                return new ServiceResult<List<TEntity>>(ServiceResponse.Success, await ServiceTable.ToListAsync(), message: $"Result set contains {ServiceTable.Count()} items");

            var resultSet = await ServiceTable.Where(q => ids.Contains(q.Id)).ToListAsync();
            if (!resultSet.Any())
                return new ServiceResult<List<TEntity>>(ServiceResponse.NotFound, message: $"No results found for id's: {string.Join(',', ids)}");

            return new ServiceResult<List<TEntity>>(ServiceResponse.Success, resultSet, message: $"Result set contains {resultSet.Count} items");
        }
        catch (Exception exception)
        {
            return new ServiceResult<List<TEntity>>(ServiceResponse.Exception, exception: exception, message: exception.Message);
        }
    }

    public virtual async Task<ServiceResult<TEntity>> Get(Guid id)
    {
        try
        {
            var result = await ServiceTable.FindAsync(id);
            if (result == null)
                return new ServiceResult<TEntity>(ServiceResponse.NotFound, message: $"No results found for id: {id}");

            return new ServiceResult<TEntity>(ServiceResponse.Success, result, message: $"Result item with id: {id}");
        }
        catch (Exception exception)
        {
            return new ServiceResult<TEntity>(ServiceResponse.Exception, exception: exception, message: exception.Message);
        }
    }

    public virtual async Task<ServiceResult<TEntity>> Create(TEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.Now.ToUniversalTime();
            entity.UpdatedAt = DateTime.Now.ToUniversalTime();

            var result = await ServiceTable.AddAsync(entity);
            await Context.SaveChangesAsync();

            if (result == null)
                return new ServiceResult<TEntity>(ServiceResponse.BadRequest, message: "Could not create entity");

            return new ServiceResult<TEntity>(ServiceResponse.Success, result.Entity, message: "Entity created");
        }
        catch (Exception exception)
        {
            return new ServiceResult<TEntity>(ServiceResponse.Exception, exception: exception, message: exception.Message);
        }
    }

    public virtual async Task<ServiceResult<TEntity>> Update(Guid id, IRequest updateModel, bool isPatch = false)
    {
        try
        {
            var entity = await ServiceTable.FindAsync(id);
            if (entity == null)
                return new ServiceResult<TEntity>(ServiceResponse.NotFound, message: $"Could not find {typeof(TEntity)} with id: {id}");

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
                return new ServiceResult<TEntity>(ServiceResponse.BadRequest, message: $"No updates found for {typeof(TEntity)} with id: {id}");

            entity.UpdatedAt = DateTime.Now.ToUniversalTime();
            var changesSaved = await Context.SaveChangesAsync();

            return new ServiceResult<TEntity>(ServiceResponse.Success, entity, message: $"Updated {typeof(TEntity)} with id: {id}, saved {changesSaved} updates, {updates} expected");
        }
        catch (Exception exception)
        {
            return new ServiceResult<TEntity>(ServiceResponse.Exception, exception: exception, message: exception.Message);
        }
    }

    public virtual async Task<ServiceResult<TEntity>> Delete(Guid id)
    {
        try
        {
            var entity = await ServiceTable.FindAsync(id);
            if (entity == null)
                return new ServiceResult<TEntity>(ServiceResponse.NotFound, message: $"Could not find {typeof(TEntity)} with id: {id}");

            ServiceTable.Remove(entity);
            await Context.SaveChangesAsync();

            return new ServiceResult<TEntity>(ServiceResponse.Success, entity, message: $"Deleted {typeof(TEntity)} with id: {id}");
        }
        catch (Exception exception)
        {
            return new ServiceResult<TEntity>(ServiceResponse.Exception, exception: exception, message: exception.Message);
        }
    }
}
