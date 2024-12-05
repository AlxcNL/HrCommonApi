using HrCommonApi.Database.Models;

namespace HrCommonApi.Services.Base;

public interface IUserService<TUser> : ICoreService<TUser> where TUser : User
{
    Task<ServiceResult<List<Session>>> GetUserSessions(Guid userId);
    Task<ServiceResult<TUser>> Login(string username, string password);
}