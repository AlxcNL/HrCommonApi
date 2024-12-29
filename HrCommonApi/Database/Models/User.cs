using HrCommonApi.Database.Models.Base;
using HrCommonApi.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrCommonApi.Database.Models;

[Table("user")]
public class User : DbEntity
{
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("role")]
    public Role Role { get; set; }

    // Navigation properties
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}