using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniTask.TaskManagement.Application.Contracts;
using DomainTask = UniTask.TaskManagement.Domain.Aggregates.Task;

namespace UniTask.TaskManagement.Infrastructure.Persistence.Repositories;

internal sealed class SqlTaskRepository : ITaskRepository
{
    private readonly TaskManagementDbContext _dbContext;

    public SqlTaskRepository(TaskManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DomainTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<DomainTask>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DomainTask task, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tasks.AddAsync(task, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DomainTask task, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(task).State == EntityState.Detached)
        {
            _dbContext.Tasks.Update(task);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}