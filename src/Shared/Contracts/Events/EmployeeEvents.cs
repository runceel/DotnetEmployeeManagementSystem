namespace Shared.Contracts.Events;

public record EmployeeCreatedEvent
{
    public Guid EmployeeId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record EmployeeUpdatedEvent
{
    public Guid EmployeeId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}

public record EmployeeDeletedEvent
{
    public Guid EmployeeId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime DeletedAt { get; init; }
}
