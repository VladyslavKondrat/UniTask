namespace UniTask.TaskManagement.Infrastructure.Persistence.Seeders;

public interface ITaskManagementDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}