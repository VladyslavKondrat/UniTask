using UniTask.TaskManagement.Domain.Exceptions;
using UniTask.TaskManagement.Domain.ValueObjects;

namespace UniTask.TaskManagement.Domain.Aggregates;
public sealed class Task
{
    public enum TaskStatus
    {
        Draft = 0,
        Published = 1,
        Assigned = 2,
        Completed = 3,
        Cancelled = 4
    }

    private const int MaxHoursAfterAssignment = 24;
    public Guid Id { get; }
    public string Title { get; private set; }
    public string Description { get; private set; }

    public TaskCost Cost { get; private set; }

    public TaskStatus Status { get; private set; }
    public Guid? AssignedExecutorId { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? AssignedAt { get; private set; }
    public DateTime? TimeSubmitted { get; private set; }

    public Task(Guid id, string title, string description, decimal cost)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Task ID cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Task description cannot be empty.", nameof(description));

        Id = id;
        Title = title.Trim();
        Description = description.Trim();
        Cost = new TaskCost(cost);
        Status = TaskStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void PublishTask()
    {
        if (Status != TaskStatus.Draft)
            throw new InvalidOperationException($"Task can only be published from Draft status. Current status: {Status}");

        Status = TaskStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    public void AssignToExecutor(Guid executorId)
    {
        if (executorId == Guid.Empty)
            throw new ArgumentException("Executor ID cannot be empty.", nameof(executorId));

        if (Status != TaskStatus.Published)
            throw new InvalidOperationException($"Task can only be assigned from Published status. Current status: {Status}");

        AssignedExecutorId = executorId;
        Status = TaskStatus.Assigned;
        AssignedAt = DateTime.UtcNow;
    }

    public void SubmitCompletion()
    {
        if (Status != TaskStatus.Assigned)
            throw new InvalidOperationException($"Task must be in Assigned status to submit completion. Current status: {Status}");

        if (!AssignedAt.HasValue)
            throw new InvalidOperationException("Task assigned time is not set.");

        var timeSinceAssignment = DateTime.UtcNow - AssignedAt.Value;
        if (timeSinceAssignment.TotalHours > MaxHoursAfterAssignment)
        {
            throw new TimeForTaskSubmitionReachedTheLimitException(
                (decimal)timeSinceAssignment.TotalHours,
                MaxHoursAfterAssignment
            );
        }

        Status = TaskStatus.Completed;
        TimeSubmitted = DateTime.UtcNow;
    }

    public void CancelTask()
    {
        if (Status == TaskStatus.Cancelled || Status == TaskStatus.Completed)
            throw new InvalidOperationException($"Cannot cancel task in {Status} status.");

        Status = TaskStatus.Cancelled;
    }

    public void UpdateCost(decimal newCost)
    {
        Cost = new TaskCost(newCost);
    }
    public void SetAssignedTimeForTesting(DateTime assignedTime)
    {
        AssignedAt = assignedTime;
    }
}
