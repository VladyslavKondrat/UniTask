# Domain Model: Aggregates, Entities, Value Objects, and Invariants (UniTask)

This document describes the rich domain model of the **UniTask** student platform, designed following Domain-Driven Design (DDD) principles.

---

## 1. Domain Class Diagram (ERD with Cardinalities)

```mermaid
classDiagram
    direction TB

    class User {
        <<Abstract>>
        +UserId Id
        +EmailAddress Email
        +PasswordHash PasswordHash
        +FullName FullName
        +DateTime CreatedAt
        +UpdateProfile(FullName newFullName) void
    }

    class Requester {
        <<InternalEntity>>
        +List~ApplicationId~ PostedApplications
        +PublishApplication(ApplicationTitle title, Description desc, List~Task~ tasks) Application
    }

    class Executor {
        <<InternalEntity>>
        +List~ApplicationId~ SubmittedApplications
        +CartId CartId
        +ExecuteApplication(ApplicationId applicationId) void
    }

    class Admin {
        <<InternalEntity>>
        +ModerateApplication(ApplicationId applicationId) void
        +BanUser(UserId userId) void
    }

    User <|-- Requester : "is a (Role)"
    User <|-- Executor : "is a (Role)"
    User <|-- Admin : "is a (Role)"

    class Cart {
        <<AggregateRoot>>
        +CartId Id
        +UserId UserId
        +Money TotalCartPrice "Sum of all application prices in cart"
        -List~Application~ _selectedApplications "Cart items (Applications)"
        +AddApplication(Application application) void
        +RemoveApplication(ApplicationId applicationId) void
        +Clear() void
        +CalculateTotal() Money
    }

    class Application {
        <<AggregateRoot>>
        +ApplicationId Id
        +UserId RequesterId
        +ApplicationTitle Title
        +Description Description
        +Money TotalPrice "Sum of prices of all tasks inside"
        +ApplicationStatus Status "DRAFT, OPEN, IN_PROGRESS, COMPLETED"
        -List~Task~ _tasks "List of tasks inside application"
        +Create(UserId requesterId, ApplicationTitle title, Description description) Application
        +AddTask(TaskTitle taskTitle, Description taskDesc, Money taskPrice) void
        +RemoveTask(TaskId taskId) void
        +CalculateTotalPrice() Money
        +Publish() void
        +Cancel() void
    }

    class Task {
        <<InternalEntity>>
        +TaskId Id
        +ApplicationId ApplicationId
        +TaskTitle Title
        +Description Description
        +Money Price "Price of specific task"
        +TaskStatus Status
        ~UpdatePrice(Money newPrice) void
    }

    class EmailAddress {
        <<ValueObject>>
        +string Value
    }

    class PasswordHash {
        <<ValueObject>>
        +string Hash
    }

    class ApplicationTitle {
        <<ValueObject>>
        +string Value
    }

    class TaskTitle {
        <<ValueObject>>
        +string Value
    }

    class Description {
        <<ValueObject>>
        +string Text
    }

    class Money {
        <<ValueObject>>
        +decimal Amount
        +string Currency
    }

    User "1" --> "0..1" Cart : "owns / has"
    Cart "1" o-- "0..*" Application : "contains selected applications"
    Application "1" *-- "1..*" Task : "contains (TotalPrice = sum of Task prices)"
    Application "1" --> "1" ApplicationTitle : "has"
    Application "1" --> "1" Description : "has"
    Application "1" --> "1" Money : "Total Price"
    Task "1" --> "1" TaskTitle : "has"
    Task "1" --> "1" Money : "Task Price"
    User "1" --> "1" EmailAddress : "identified by"
    User "1" --> "1" PasswordHash : "secured by"