namespace UniTask.ApplicationFulfillment.Domain.Entities;

internal sealed class Payment
{
    private Guid Id { get; } = Guid.NewGuid();
    private Guid ApplicationId { get; }
    private Guid PayerId { get; }
    private decimal Amount { get; }
    private string CardType { get; }
    private DateTime RecordedAt { get; } = DateTime.UtcNow;
    private bool IsProcessed { get; private set; }

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
