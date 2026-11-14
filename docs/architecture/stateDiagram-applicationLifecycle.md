# Business Invariants, Lifecycle State Machine, and Domain Design Pattern (UniTask)

This document defines the lifecycle state machine, domain invariants, TDD test specifications, and GoF design pattern implementation for the **`Application`** aggregate root (a student's task announcement containing one or more sub-tasks) in the **UniTask** platform.

---

## 1. Lifecycle State Machine (`stateDiagram-applicationLifecycle`)

The **`Application`** aggregate moves through 5 distinct business states: `Draft`, `Open`, `InProgress`, `Completed`, and `Cancelled`.

```mermaid
stateDiagram-v2
    [*] --> Draft : Create(requesterId, title, description)

    Draft --> Draft : AddTask() / RemoveTask() [TaskPrice >= 0]
    Draft --> Open : Publish() [Tasks.Count >= 1 && TotalPrice > 0]
    Draft --> Cancelled : Cancel() [ByRequester]

    Open --> InProgress : AssignExecutor(executorId) [ExecutorId != RequesterId]
    Open --> Cancelled : Cancel() [ByRequester]

    InProgress --> Completed : CompleteExecution() [AllTasksCompleted]
    InProgress --> Cancelled : Cancel() [MutualAgreement]

    Completed --> [*]
    Cancelled --> [*]

    note right of Draft
        Forbidden operations in Draft:
        - AssignExecutor()
        - CompleteExecution()
    end note

    note left of Completed
        Forbidden operations in Completed:
        - AddTask() / RemoveTask()
        - AssignExecutor()
        - Cancel()
    end note