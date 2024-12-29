using HrCommonApi.Database.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrCommonApi.Database.Models;

[Table("right")]
public class Right : DbEntity
{
    [Column("api_key_id")]
    public Guid ApiKeyId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("value")]
    public string Value { get; set; } = string.Empty;

    // Navigation properties
    public virtual ApiKey ApiKey { get; set; } = default!;
}