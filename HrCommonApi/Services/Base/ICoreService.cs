using HrCommonApi.Controllers.Requests;
using HrCommonApi.Database.Models.Base;

namespace HrCommonApi.Services.Base;

public interface ICoreService<TEntity> where TEntity : DbEntity
{
    Task<ServiceResponse<TEntity>> Create(TEntity entity);
    Task<ServiceResponse<TEntity>> Delete(Guid id);
    Task<ServiceResponse<List<TEntity>>> Get(Guid[]? ids = null);
    Task<ServiceResponse<TEntity>> Get(Guid id);
    Task<ServiceResponse<TEntity>> Update(Guid id, IRequest updateModel, bool isPatch = false);
}