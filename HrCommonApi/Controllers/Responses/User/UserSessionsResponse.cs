namespace HrCommonApi.Controllers.Responses.User;

public partial class UserSessionsResponse : IResponse
{
    public Guid Id { get; set; }

    public List<UserSessionResponse> Sessions { get; set; } = new List<UserSessionResponse>();
}