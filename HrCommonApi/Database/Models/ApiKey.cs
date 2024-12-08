using HrCommonApi.Database.Models.Base;
using HrCommonApi.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrCommonApi.Database.Models;

[Table("api_key")]
public class ApiKey : DbEntity
{
    [Column("key")]
    public string Key { get; set; } = string.Empty;

    [Column("enabled")]
    public bool Enabled { get; set; } = true;

    [Column("role")]
    public Role Role { get; set; } = Role.User;

    [Column("contact")]
    public string Contact { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<Right> Rights { get; set; } = new List<Right>();
}