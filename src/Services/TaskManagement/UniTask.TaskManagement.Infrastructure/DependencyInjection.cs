using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniTask.TaskManagement.Infrastructure.Persistence;

namespace UniTask.TaskManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTaskManagementInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var useInMemory = bool.TryParse(
            configuration["Persistence:UseInMemoryDatabase"],
            out var parsed) && parsed;

        services.AddDbContext<TaskManagementDbContext>(options =>
        {
            if (useInMemory)
            {
                var dbName = configuration["Persistence:InMemoryDatabaseName"]
                    ?? "UniTaskTaskManagementLabDb";
                options.UseInMemoryDatabase(dbName);
            }
            else
            {
                var connectionString = configuration.GetConnectionString("TaskManagementDb")
                    ?? throw new InvalidOperationException(
                        "Connection string 'TaskManagementDb' not found.");
                options.UseSqlServer(connectionString);
            }
        });

        return services;
    }
}