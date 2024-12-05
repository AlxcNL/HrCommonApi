using System.ComponentModel.DataAnnotations;

namespace HrCommonApi.Controllers.Requests.User;

public partial class LoginUserRequest : IRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}