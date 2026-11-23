namespace UniTask.ApplicationFulfillment.Domain.Exceptions;

public sealed class InsufficientStudentFundsException : DomainException
{
    public InsufficientStudentFundsException()
        : base("ERP_INSUFFICIENT_STUDENT_FUNDS", "The requester has insufficient funds for the escrow hold.")
    {
    }
}
