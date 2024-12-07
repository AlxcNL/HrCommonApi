using HrCommonApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HrCommonApi.Database;

public abstract class HrCommonDataContext(DbContextOptions options, IConfiguration configuration) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.AddEntityModels(configuration);
}
