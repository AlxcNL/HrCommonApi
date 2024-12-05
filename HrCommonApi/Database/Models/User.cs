using HrCommonApi.Database.Models.Base;
using HrCommonApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrCommonApi.Database.Models;

public class User : DbEntity
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Role Role { get; set; }

    // Navigation properties
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}