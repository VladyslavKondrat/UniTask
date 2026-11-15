using UniTask.ApplicationFulfillment.Domain.Aggregates;
using UniTask.ApplicationFulfillment.Domain.Entities;
using UniTask.ApplicationFulfillment.Domain.Exceptions;
using UniTask.ApplicationFulfillment.Domain.ValueObjects;

namespace UniTask.ApplicationFulfillment.Domain.Tests;

public class ApplicationTests
{
    [Fact]
    public void Application_TotalPrice_IsSumOfTaskPrices()
    {
        var application = CreateApplication();

        application.AddTask("Repair", "Repair a phone", 50m);
        application.AddTask("Deliver", "Deliver the phone", 20m);

        Assert.Equal(70m, application.TotalPrice);
    }

    [Fact]
    public void Application_Payment_IsRecordedForWholeApplication()
    {
        var application = CreateApplication();
        application.AddTask("Repair", "Repair a phone", 50m);
        application.AddTask("Deliver", "Deliver the phone", 20m);
        application.Publish();
        application.AssignExecutor(Guid.NewGuid());

        application.RecordPayment(Guid.NewGuid(), "4111 1111 1111 1111");

        var payments = typeof(Application)
            .GetProperty("Payments", System.Reflection.BindingFlags.Instance |
                                      System.Reflection.BindingFlags.NonPublic)!
            .GetValue(application) as IReadOnlyCollection<Payment>;

        var payment = Assert.Single(payments!);
        Assert.Equal(application.Id, payment.ApplicationId);
        Assert.Equal(70m, payment.Amount);
    }

    [Fact]
    public void Application_CannotBePaidWithInvalidCard()
    {
        var application = CreateApplication();
        application.AddTask("Repair", "Repair a phone", 50m);
        application.Publish();
        application.AssignExecutor(Guid.NewGuid());

        Assert.Throws<ArgumentException>(() =>
            application.RecordPayment(Guid.NewGuid(), "4111 1111 1111 1112"));
    }

    [Fact]
    public void Application_ApplyPlatformFee_InDraft_SetsFee()
    {
        var application = CreateApplication();
        application.AddTask("Repair", "Repair a phone", 50m);
        var policy = new TestPlatformFeePolicy(true, new Money(5m));

        application.ApplyPlatformFee(policy, StudentTier.Standard);

        Assert.Equal(new Money(5m), application.ServiceFeeAmount);
    }

    [Fact]
    public void Application_ApplyPlatformFee_WhenNotDraft_ThrowsDomainException()
    {
        var application = CreateApplication();
        application.AddTask("Repair", "Repair a phone", 50m);
        application.Publish();

        Assert.Throws<ApplicationModificationForbiddenException>(() =>
            application.ApplyPlatformFee(new TestPlatformFeePolicy(true, new Money(5m)), StudentTier.Standard));
    }

    [Fact]
    public void Application_ApplyPlatformFee_WhenNotApplicable_ThrowsDomainException()
    {
        var application = CreateApplication();

        Assert.Throws<PlatformFeeNotApplicableException>(() =>
            application.ApplyPlatformFee(new TestPlatformFeePolicy(false, new Money(5m)), StudentTier.Standard));
    }

    [Fact]
    public void Application_ApplyPlatformFee_WhenPolicyReturnsNull_ThrowsDomainException()
    {
        var application = CreateApplication();

        Assert.Throws<InvalidFeeAmountException>(() =>
            application.ApplyPlatformFee(new TestPlatformFeePolicy(true, null), StudentTier.Standard));
    }

    private static Application CreateApplication() =>
        new(Guid.NewGuid(), Guid.NewGuid());

    private sealed class TestPlatformFeePolicy(bool applicable, Money? fee) : IPlatformFeePolicy
    {
        public bool IsApplicable(Application application, StudentTier requesterTier) => applicable;

        public Money CalculateFee(Application application, StudentTier requesterTier) => fee!;
    }
}
