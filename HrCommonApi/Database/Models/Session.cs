using HrCommonApi.Database.Models.Base;
using Microsoft.EntityFrameworkCore;

namespace HrCommonApi.Database.Models;

public class Session : DbEntity
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime AccessExpiresAt { get; set; }
    public DateTime RefreshExpiresAt { get; set; }
    public Guid UserId { get; set; }
}