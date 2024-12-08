using System.ComponentModel.DataAnnotations.Schema;

namespace HrCommonApi.Database.Models.Base;

public abstract class DbEntity
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now.ToUniversalTime();
}
