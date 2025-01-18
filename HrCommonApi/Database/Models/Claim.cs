using HrCommonApi.Database.Models.Base;
using HrCommonApi.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrCommonApi.Database.Models;

[Table("claim")]
public class Claim : DbEntity
{
    [Column("api_key_id")]
    public Guid ApiKeyId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("value")]
    public AccessType Value { get; set; } = AccessType.None;

    // Navigation properties
    public virtual ApiKey ApiKey { get; set; } = default!;
}