using HrCommonApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace HrCommonApi.Controllers.Requests.User;

public partial class CreateAdminRequest : CreateUserRequest
{
    [Required]
    public Role Role { get; set; } = Role.User;
}