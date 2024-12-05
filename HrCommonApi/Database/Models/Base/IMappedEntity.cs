using Microsoft.EntityFrameworkCore;

namespace HrCommonApi.Database.Models.Base;

public interface IMappedEntity
{
    static abstract void MapEntity(ModelBuilder modelBuilder);
}
