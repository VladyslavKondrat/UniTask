using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniTask.TaskManagement.Domain.Entities;
using TaskAggregate = UniTask.TaskManagement.Domain.Aggregates.Task;

namespace UniTask.TaskManagement.Infrastructure.Persistence.Configurations;

internal sealed class TaskConfiguration : IEntityTypeConfiguration<TaskAggregate>
{
    public void Configure(EntityTypeBuilder<TaskAggregate> builder)
    {
        builder.ToTable("Tasks", schema: "taskmanagement");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.OwnsOne(t => t.Cost, costBuilder =>
        {
            costBuilder.Property(c => c.Amount)
                .HasColumnName("Cost")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(t => t.AssignedExecutorId);
        builder.HasIndex(t => t.AssignedExecutorId);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.PublishedAt);
        builder.Property(t => t.AssignedAt);
        builder.Property(t => t.TimeSubmitted);

        builder.Ignore(t => t.Payments);

        builder.OwnsMany<Payment>("_payments", paymentBuilder =>
        {
            paymentBuilder.ToTable("Payments", schema: "taskmanagement");

            paymentBuilder.WithOwner().HasForeignKey("TaskId");

            paymentBuilder.HasKey(p => p.Id);

            paymentBuilder.Property(p => p.Id)
                .ValueGeneratedNever();

            paymentBuilder.Property(p => p.ExecutorId)
                .IsRequired();

            paymentBuilder.OwnsOne(p => p.Cost, costBuilder =>
            {
                costBuilder.Property(c => c.Amount)
                    .HasColumnName("Cost")
                    .HasPrecision(18, 2)
                    .IsRequired();
            });

            paymentBuilder.Property(p => p.RecordedAt)
                .IsRequired();

            paymentBuilder.Property(p => p.IsProcessed)
                .IsRequired();
        });
    }
}