using HrCommonApi.Enums;

namespace HrCommonApi.Controllers.Responses.User;

public partial class SessionStateResponse : IResponse
{
    public Guid Id { get; set; }
    public bool HasSession { get; set; } = false;
    public string Username { get; set; } = string.Empty;
    public Role? Role { get; set; } = null;
    public List<UserSessionResponse> ActiveSessions { get; set; } = new List<UserSessionResponse>();
}
