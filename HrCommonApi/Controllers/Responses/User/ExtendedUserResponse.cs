namespace HrCommonApi.Controllers.Responses.User;

public partial class ExtendedUserResponse : SimpleUserResponse
{
    public ICollection<UserSessionResponse> Sessions { get; set; } = new List<UserSessionResponse>();
}
