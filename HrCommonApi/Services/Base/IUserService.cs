using HrCommonApi.Database.Models;
using HrCommonApi.Enums;

namespace HrCommonApi.Services.Base;

public interface IUserService<TUser> : ICoreService<TUser> where TUser : User
{
    Task<ServiceResponse<List<Session>>> GetUserSessions(Guid userId);
    Task<ServiceResponse<TUser>> Login(string username, string password);
    Task<ServiceResponse<TUser>> UpdateRole(Guid userId, Role role);
}