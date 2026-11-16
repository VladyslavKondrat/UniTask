namespace UniTask.ApplicationFulfillment.Domain.Entities;

internal sealed class Payment
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid ApplicationId { get; }
    public Guid PayerId { get; }
    public decimal Amount { get; }
    public string CardType { get; }
    public DateTime RecordedAt { get; } = DateTime.UtcNow;
    public bool IsProcessed { get; private set; }

    internal Payment(Guid applicationId, Guid payerId, decimal amount, string cardType)
    {
        if (applicationId == Guid.Empty)
            throw new ArgumentException("Application ID cannot be empty.", nameof(applicationId));
        if (payerId == Guid.Empty)
            throw new ArgumentException("Payer ID cannot be empty.", nameof(payerId));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(cardType))
            throw new ArgumentException("Card type cannot be empty.", nameof(cardType));

        ApplicationId = applicationId;
        PayerId = payerId;
        Amount = amount;
        CardType = cardType;
    }

    internal void MarkAsProcessed() => IsProcessed = true;
}
