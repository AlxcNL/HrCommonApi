using HrCommonApi.Database.Models.Base;
using HrCommonApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrCommonApi.Database.Models;

public class Right : DbEntity
{
    public Guid ApiKeyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    // Navigation properties
    public virtual ApiKey ApiKey { get; set; } = null!;
}