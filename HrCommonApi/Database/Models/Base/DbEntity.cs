namespace HrCommonApi.Database.Models.Base;

public abstract class DbEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
