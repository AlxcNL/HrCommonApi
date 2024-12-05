using HrCommonApi.Database.Models.Base;
using Microsoft.EntityFrameworkCore;

namespace HrCommonApi.Database.Models;

public partial class Session : DbEntity, IMappedEntity
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime AccessExpiresAt { get; set; }
    public DateTime RefreshExpiresAt { get; set; }
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;

    public static void MapEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Session>(entity =>
        {
            // Map relationships
            entity.HasOne(e => e.User).WithMany(e => e.Sessions).HasForeignKey(e => e.UserId);
        });
    }
}