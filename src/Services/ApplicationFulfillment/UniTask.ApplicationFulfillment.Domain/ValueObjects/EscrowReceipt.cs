namespace UniTask.ApplicationFulfillment.Domain.ValueObjects;

public sealed record EscrowReceipt(EscrowReference Reference, EscrowStatus Status);
