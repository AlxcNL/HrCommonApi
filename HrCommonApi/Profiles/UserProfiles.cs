using AutoMapper;
using HrCommonApi.Controllers.Requests.User;
using HrCommonApi.Controllers.Responses.User;
using HrCommonApi.Database.Models;

namespace HrCommonApi.Profiles;

public class UserProfiles : Profile
{
    public UserProfiles()
    {
        CreateMap<CreateUserRequest, User>();
        CreateMap<UpdateUserRequest, User>();
        CreateMap<User, SimpleUserResponse>();
        CreateMap<User, UserSessionsResponse>();
        CreateMap<Session, UserSessionResponse>();
    }
}
