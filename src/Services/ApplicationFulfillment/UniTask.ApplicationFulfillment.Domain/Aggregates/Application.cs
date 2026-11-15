using UniTask.ApplicationFulfillment.Domain.Entities;
using ApplicationTask = UniTask.ApplicationFulfillment.Domain.Entities.Task;

namespace UniTask.ApplicationFulfillment.Domain.Aggregates;

public sealed class Application
{
    private readonly List<ApplicationTask> _tasks = new();
    private readonly List<Payment> _payments = new();

    public enum ApplicationStatus
    {
        Draft,
        Open,
        InProgress,
        Completed,
        Cancelled
    }

    public Guid Id { get; }
    public Guid RequesterId { get; }
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Draft;
    public decimal TotalPrice => _tasks.Sum(task => task.Price);
    internal IReadOnlyCollection<ApplicationTask> Tasks => _tasks.AsReadOnly();
    internal IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    public Application(Guid id, Guid requesterId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Application ID cannot be empty.", nameof(id));
        if (requesterId == Guid.Empty)
            throw new ArgumentException("Requester ID cannot be empty.", nameof(requesterId));

        Id = id;
        RequesterId = requesterId;
    }

    public void AddTask(string title, string description, decimal price)
    {
        EnsureDraft();
        _tasks.Add(new ApplicationTask(Guid.NewGuid(), title, description, price));
    }

    public void Publish()
    {
        EnsureDraft();
        if (_tasks.Count == 0 || TotalPrice <= 0)
            throw new InvalidOperationException("Application must contain tasks with a positive total price.");

        Status = ApplicationStatus.Open;
    }

    public void AssignExecutor(Guid executorId)
    {
        if (Status != ApplicationStatus.Open)
            throw new InvalidOperationException("Only open applications can be assigned.");
        if (executorId == Guid.Empty || executorId == RequesterId)
            throw new ArgumentException("Executor must be a non-empty user different from the requester.", nameof(executorId));

        Status = ApplicationStatus.InProgress;
    }

    public void RecordPayment(Guid payerId, string cardNumber)
    {
        if (Status != ApplicationStatus.InProgress)
            throw new InvalidOperationException("Only in-progress applications can be paid.");
        var card = new Card();
        if (!card.ValidateCard(cardNumber))
            throw new ArgumentException("Card number is invalid.", nameof(cardNumber));

        _payments.Add(new Payment(Id, payerId, TotalPrice, card.GetCardType(cardNumber)));
    }

    public void ProcessPayments()
    {
        foreach (var payment in _payments.Where(payment => !payment.IsProcessed))
            payment.MarkAsProcessed();
    }

    private void EnsureDraft()
    {
        if (Status != ApplicationStatus.Draft)
            throw new InvalidOperationException("Application can only be modified in Draft status.");
    }
}
