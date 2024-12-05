using HrCommonApi.Enums;

namespace HrCommonApi.Controllers.Responses.User;

public partial class SimpleUserResponse : SimpleResponse
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public Role Role { get; set; }
}