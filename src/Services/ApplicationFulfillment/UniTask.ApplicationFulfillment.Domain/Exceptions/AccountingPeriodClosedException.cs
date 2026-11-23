namespace UniTask.ApplicationFulfillment.Domain.Exceptions;

public sealed class AccountingPeriodClosedException : DomainException
{
    public AccountingPeriodClosedException()
        : base("ERP_ACCOUNTING_PERIOD_CLOSED", "The university accounting period is closed.")
    {
    }
}
