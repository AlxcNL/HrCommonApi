using HrCommonApi.Database.Models.Base;
using HrCommonApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrCommonApi.Database.Models;

public partial class User : DbEntity, IMappedEntity
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Role Role { get; set; }

    // Navigation properties
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public static void MapEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            // Map relationships
            entity.HasMany(e => e.Sessions).WithOne(e => e.User).HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = new Guid(),
                Username = "admin",
                Password = "admin",
                Role = Role.Admin,
                Email = "",
            });
    }
}