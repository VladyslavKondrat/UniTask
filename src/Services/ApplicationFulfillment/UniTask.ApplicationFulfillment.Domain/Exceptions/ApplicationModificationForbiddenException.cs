using UniTask.ApplicationFulfillment.Domain.Aggregates;

namespace UniTask.ApplicationFulfillment.Domain.Exceptions;

public sealed class ApplicationModificationForbiddenException : DomainException
{
    public ApplicationModificationForbiddenException(Guid applicationId, Application.ApplicationStatus status)
        : base("application.modification_forbidden",
            $"Application '{applicationId}' cannot be modified while it is in '{status}' status.")
    {
    }
}
