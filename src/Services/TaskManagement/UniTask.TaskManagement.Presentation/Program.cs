using UniTask.TaskManagement.Infrastructure;
using UniTask.TaskManagement.Application.Contracts;
using UniTask.TaskManagement.Application.Services;
using UniTask.TaskManagement.Infrastructure.Persistence.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddScoped<ITaskApplicationService, TaskApplicationService>();

builder.Services.AddTaskManagementInfrastructure(builder.Configuration);

builder.Services.AddTaskManagementInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var seeder = scope.ServiceProvider.GetRequiredService<ITaskManagementDataSeeder>();
    await seeder.SeedAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
