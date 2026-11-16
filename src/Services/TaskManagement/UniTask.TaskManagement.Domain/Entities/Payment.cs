using System;
using UniTask.TaskManagement.Domain.ValueObjects;

namespace UniTask.TaskManagement.Domain.Entities;


internal sealed class Payment
{
    public Guid Id { get; private set; }
    public TaskCost Cost { get; private set; } = null!;
    public Guid ExecutorId { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public bool IsProcessed { get; private set; }

    private Payment()
    {
        Cost = null!;
    }

    internal Payment(Guid executorId, TaskCost cost)
    {
        if (executorId == Guid.Empty)
            throw new ArgumentException("Executor ID cannot be empty.", nameof(executorId));

        if (cost is null)
            throw new ArgumentNullException(nameof(cost), "Cost cannot be null.");

        Id = Guid.NewGuid();
        ExecutorId = executorId;
        Cost = cost;
        RecordedAt = DateTime.UtcNow;
        IsProcessed = false;
    }

    internal void MarkAsProcessed()
    {
        IsProcessed = true;
    }
}