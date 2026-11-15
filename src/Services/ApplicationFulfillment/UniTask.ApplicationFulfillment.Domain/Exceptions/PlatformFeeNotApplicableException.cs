namespace UniTask.ApplicationFulfillment.Domain.Exceptions;

public sealed class PlatformFeeNotApplicableException : DomainException
{
    public PlatformFeeNotApplicableException(Guid applicationId)
        : base("application.platform_fee_not_applicable",
            $"A platform fee is not applicable to application '{applicationId}'.")
    {
    }
}
