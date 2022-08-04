using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Models;

namespace Interfaces
{
    public interface IAuditDbContext
    {
        DbSet<audit> audit { get; set; }
        ChangeTracker ChangeTracker { get; }
    }
}
