using HrCommonApi.Controllers.Requests;
using HrCommonApi.Database.Models.Base;

namespace HrCommonApi.Services.Base;

public interface ICoreService<TEntity> where TEntity : DbEntity
{
    Task<ServiceResult<TEntity>> Create(TEntity entity);
    Task<ServiceResult<TEntity>> Delete(Guid id);
    Task<ServiceResult<List<TEntity>>> Get(Guid[]? ids = null);
    Task<ServiceResult<TEntity>> Get(Guid id);
    Task<ServiceResult<TEntity>> Update(Guid id, IRequest updateModel, bool isPatch = false);
}