using HrCommonApi.Database.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrCommonApi.Database.Models;

[Table("session")]
public class Session : DbEntity
{
    [Column("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [Column("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [Column("access_expires_at")]
    public DateTime AccessExpiresAt { get; set; }

    [Column("refresh_expires_at")]
    public DateTime RefreshExpiresAt { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = default!;
}