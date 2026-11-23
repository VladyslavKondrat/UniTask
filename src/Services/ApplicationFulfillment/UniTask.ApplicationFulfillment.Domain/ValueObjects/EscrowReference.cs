namespace UniTask.ApplicationFulfillment.Domain.ValueObjects;

public sealed record EscrowReference
{
    public string Value { get; }

    public EscrowReference(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Escrow reference cannot be empty.", nameof(value));

        Value = value.Trim();
    }
}
