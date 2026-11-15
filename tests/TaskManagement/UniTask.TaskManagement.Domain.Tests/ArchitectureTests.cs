using Xunit;
using UniTask.TaskManagement.Domain.Aggregates;
using UniTask.TaskManagement.Domain.ValueObjects;
using DomainTask = UniTask.TaskManagement.Domain.Aggregates.Task;

namespace UniTask.TaskManagement.Domain.Tests;

public class ArchitectureTests
{
    [Fact]
    public void TaskCost_Should_Be_Sealed_ValueObject()
    {
        var taskCostType = typeof(TaskCost);

        Assert.True(taskCostType.IsSealed, 
            "Value Object should be sealed (prevent inheritance)");
        Assert.True(typeof(IEquatable<TaskCost>).IsAssignableFrom(taskCostType),
            "Value Object should implement IEquatable<T> for value comparison");
    }

    [Fact]
    public void Task_Should_Be_Sealed_AggregateRoot()
    {
        var taskType = typeof(DomainTask);

        Assert.True(taskType.IsSealed,
            "Aggregate Root should be sealed (prevent inheritance violations)");
    }

    [Fact]
    public void Task_Cost_Should_Be_TaskCost_ValueObject()
    {
        var costProp = typeof(DomainTask).GetProperty("Cost");

        Assert.NotNull(costProp);
        Assert.Equal(typeof(TaskCost), costProp.PropertyType);
    }

    [Fact]
    public void TaskCost_Should_Support_Implicit_Decimal_Conversion()
    {
        var implicitOp = typeof(TaskCost).GetMethod(
            "op_Implicit",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
            null,
            new[] { typeof(TaskCost) },
            null);

        Assert.NotNull(implicitOp);
        Assert.Equal(typeof(decimal), implicitOp.ReturnType);
    }
}
