using HrCommonApi.Database.Models;

namespace HrCommonApi.Services.Base;

public interface IUserService : ICoreService<User>
{
    Task<ServiceResult<List<Session>>> GetUserSessions(Guid userId);
    Task<ServiceResult<User>> Login(string username, string password);
}