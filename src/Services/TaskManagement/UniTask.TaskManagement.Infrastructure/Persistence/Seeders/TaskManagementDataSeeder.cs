using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskAggregate = UniTask.TaskManagement.Domain.Aggregates.Task;

namespace UniTask.TaskManagement.Infrastructure.Persistence.Seeders;

internal sealed class TaskManagementDataSeeder : ITaskManagementDataSeeder
{
    public static readonly Guid DraftTaskId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid PublishedTaskId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly TaskManagementDbContext _dbContext;
    private readonly ILogger<TaskManagementDataSeeder> _logger;

    public TaskManagementDataSeeder(
        TaskManagementDbContext dbContext,
        ILogger<TaskManagementDataSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (_dbContext.Database.IsRelational())
        {
            await _dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        if (await _dbContext.Tasks.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Database already contains tasks. Skipping TaskManagement seeding.");
            return;
        }

        var draftTask = new TaskAggregate(
            DraftTaskId,
            "Configure CI/CD Pipeline",
            "Set up automated build and unit test workflows for UniTask.",
            450.00m);

        var publishedTask = new TaskAggregate(
            PublishedTaskId,
            "Implement SQL Server Persistence",
            "Configure EF Core DbContext, migrations, and SqlTaskRepository.",
            600.00m);
        publishedTask.PublishTask();

        await _dbContext.Tasks.AddRangeAsync([draftTask, publishedTask], cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seeded 2 initial tasks. DraftTaskId: {DraftTaskId}, PublishedTaskId: {PublishedTaskId}",
            draftTask.Id,
            publishedTask.Id);
    }
}