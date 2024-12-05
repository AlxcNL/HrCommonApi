using HrCommonApi.Database.Models.Base;
using HrCommonApi.Enums;

namespace HrCommonApi.Database.Models;

public class ApiKey : DbEntity
{
    public string Key { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public Role Role { get; set; } = Role.User;
    public string Contact { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<Right> Rights { get; set; } = new List<Right>();
}