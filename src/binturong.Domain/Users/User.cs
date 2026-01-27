using SharedKernel;

namespace Domain.Users;

public sealed class User : Entity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool MustChangePassword { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public void RaiseRegistered() =>
        Raise(new UserRegisteredDomainEvent(Id, Username, Email, IsActive, CreatedAt, UpdatedAt));

    public void RaiseUpdated() =>
        Raise(
            new UserUpdatedDomainEvent(
                Id,
                Username,
                Email,
                IsActive,
                LastLogin,
                MustChangePassword,
                FailedAttempts,
                LockedUntil,
                UpdatedAt
            )
        );

    public void RaiseDeleted() => Raise(new UserDeletedDomainEvent(Id));

    // Navigation
    public ICollection<Domain.UserRoles.UserRole> UserRoles { get; set; } =
        new List<Domain.UserRoles.UserRole>();
    public ICollection<Domain.AuditLogs.AuditLog> AuditLogs { get; set; } =
        new List<Domain.AuditLogs.AuditLog>();
    public ICollection<Domain.MarketingAssetTracking.MarketingAssetTrackingEntry> MarketingTrackings { get; set; } =
        new List<Domain.MarketingAssetTracking.MarketingAssetTrackingEntry>();

    public ICollection<Domain.Employees.Employee> Employees { get; set; } =
        new List<Domain.Employees.Employee>();
}
