using UniTask.ApplicationFulfillment.Domain.ValueObjects;

namespace UniTask.ApplicationFulfillment.Domain.Ports;

public interface IUniversityBillingPort
{
    Task<EscrowReceipt> HoldEscrowAsync(
        Guid applicationId,
        Guid requesterId,
        Money totalPrice,
        CancellationToken cancellationToken = default);
}
