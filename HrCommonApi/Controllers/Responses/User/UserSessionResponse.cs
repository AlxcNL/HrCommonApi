namespace HrCommonApi.Controllers.Responses.User;

public partial class UserSessionResponse : IResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessExpiresAt { get; set; }
    public DateTime RefreshExpiresAt { get; set; }
}