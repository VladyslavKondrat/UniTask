using System;
using System.Collections.Generic;
using System.Text;
using UniTask.TaskManagement.Domain.Exceptions;
using UniTask.TaskManagement.Domain.ValueObjects;

namespace UniTask.TaskManagement.Domain.Entities
{
    internal class Task
    {
        public enum TaskStatus
        {
            Draft = 0,
            Published = 1,
            Assigned = 2,
            Completed = 3,
            Cancelled = 4
        }

        private const int MaxHoursAfterAssignment = 24;
        public Guid Id { get; }
        public string Title { get; private set; }
        public string Description { get; private set; }

        public TaskCost Cost { get; private set; }

        public TaskStatus Status { get; private set; }
        public Guid? AssignedExecutorId { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime? PublishedAt { get; private set; }
        public DateTime? AssignedAt { get; private set; }
        public DateTime? TimeSubmitted { get; private set; }

    }
}
