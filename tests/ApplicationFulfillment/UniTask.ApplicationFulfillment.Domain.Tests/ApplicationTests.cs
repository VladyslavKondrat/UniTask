using UniTask.ApplicationFulfillment.Domain.Aggregates;
using UniTask.ApplicationFulfillment.Domain.Entities;

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

    private static Application CreateApplication() =>
        new(Guid.NewGuid(), Guid.NewGuid());
}
