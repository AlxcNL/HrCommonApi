using HrCommonApi.Database.Models;
using HrCommonApi.Enums;

namespace HrCommonApi.Services.Base;

public interface IUserService<TUser> : ICoreService<TUser> where TUser : User
{
    Task<ServiceResult<List<Session>>> GetUserSessions(Guid userId);
    Task<ServiceResult<TUser>> Login(string username, string password);
    Task<ServiceResult<TUser>> UpdateRole(Guid userId, Role role);
}