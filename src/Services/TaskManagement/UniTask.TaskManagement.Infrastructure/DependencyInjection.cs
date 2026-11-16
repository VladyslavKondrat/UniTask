using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniTask.TaskManagement.Application.Contracts;
using UniTask.TaskManagement.Infrastructure.Persistence;
using UniTask.TaskManagement.Infrastructure.Persistence.Repositories;
using UniTask.TaskManagement.Infrastructure.Persistence.Seeders;

namespace UniTask.TaskManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTaskManagementInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue<bool>("Persistence:UseInMemoryDatabase");

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

                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "taskmanagement");
                });
            }
        });

        services.AddScoped<ITaskRepository, SqlTaskRepository>();
        services.AddScoped<ITaskManagementDataSeeder, TaskManagementDataSeeder>();

        return services;
    }
}