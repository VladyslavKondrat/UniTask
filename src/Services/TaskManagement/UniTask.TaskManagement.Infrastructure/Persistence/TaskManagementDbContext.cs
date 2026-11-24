using Microsoft.EntityFrameworkCore;
using TaskAggregate = UniTask.TaskManagement.Domain.Aggregates.Task;

namespace UniTask.TaskManagement.Infrastructure.Persistence;

internal sealed class TaskManagementDbContext : DbContext
{
    public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskAggregate> Tasks => Set<TaskAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskManagementDbContext).Assembly);
    }
}