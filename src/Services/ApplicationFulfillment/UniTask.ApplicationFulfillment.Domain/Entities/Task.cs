namespace UniTask.ApplicationFulfillment.Domain.Entities;

internal sealed class Task
{
    public Guid Id { get; }
    public string Title { get; }
    public string Description { get; }
    public decimal Price { get; }

    internal Task(Guid id, string title, string description, decimal price)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Task ID cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Task description cannot be empty.", nameof(description));
        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Task price must be greater than zero.");

        Id = id;
        Title = title.Trim();
        Description = description.Trim();
        Price = price;
    }
}
