using Application.Abstractions.Projections;
using Application.ReadModels.Payroll;
using Domain.EmployeeHistory;
using Domain.Employees;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Infrastructure.Projections.Payroll;

internal sealed class EmployeeProjection
    : IProjector<EmployeeCreatedDomainEvent>,
        IProjector<EmployeeUpdatedDomainEvent>,
        IProjector<EmployeeDeletedDomainEvent>,
        IProjector<EmployeeCheckInDomainEvent>,
        IProjector<EmployeeCheckOutDomainEvent>
{
    private readonly IMongoDatabase _db;

    public EmployeeProjection(IMongoDatabase db)
    {
        _db = db;
    }

    public async Task ProjectAsync(EmployeeCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<EmployeeReadModel>("employees");

        var branchName = await ResolveBranchNameAsync(e.BranchId, ct);

        var rm = new EmployeeReadModel
        {
            Id = $"employee:{e.EmployeeId}",
            EmployeeId = e.EmployeeId,
            UserId = e.UserId,
            BranchId = e.BranchId,
            BranchName = branchName,
            FullName = e.FullName,
            Email = e.Email,
            NationalId = e.NationalId,
            JobTitle = e.JobTitle,
            BaseSalary = e.BaseSalary,

            HireDate = new DateTime(
                e.HireDate.Year,
                e.HireDate.Month,
                e.HireDate.Day,
                0,
                0,
                0,
                DateTimeKind.Utc
            ),

            TerminationDate = e.TerminationDate.HasValue
                ? new DateTime(
                    e.TerminationDate.Value.Year,
                    e.TerminationDate.Value.Month,
                    e.TerminationDate.Value.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc
                )
                : (DateTime?)null,

            IsActive = e.IsActive,
            History = [],
        };

        await col.InsertOneAsync(rm, cancellationToken: ct);
    }

    public async Task ProjectAsync(EmployeeUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<EmployeeReadModel>("employees");

        var branchName = await ResolveBranchNameAsync(e.BranchId, ct);

        DateTime? terminationDate = e.TerminationDate.HasValue
            ? new DateTime(
                e.TerminationDate.Value.Year,
                e.TerminationDate.Value.Month,
                e.TerminationDate.Value.Day,
                0,
                0,
                0,
                DateTimeKind.Utc
            )
            : (DateTime?)null;

        var update = Builders<EmployeeReadModel>
            .Update.Set(x => x.UserId, e.UserId)
            .Set(x => x.FullName, e.FullName)
            .Set(x => x.Email, e.Email)
            .Set(x => x.NationalId, e.NationalId)
            .Set(x => x.JobTitle, e.JobTitle)
            .Set(x => x.BaseSalary, e.BaseSalary)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.BranchName, branchName)
            .Set(x => x.IsActive, e.IsActive)
            .Set(x => x.TerminationDate, terminationDate);

        await col.UpdateOneAsync(
            x => x.Id == $"employee:{e.EmployeeId}",
            update,
            cancellationToken: ct
        );
    }

    public async Task ProjectAsync(EmployeeDeletedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<EmployeeReadModel>("employees");

        await col.DeleteOneAsync(x => x.Id == $"employee:{e.EmployeeId}", ct);
    }

    public async Task ProjectAsync(EmployeeCheckInDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<EmployeeReadModel>("employees");

        var update = Builders<EmployeeReadModel>.Update.Push(
            x => x.History,
            new EmployeeHistoryReadModel
            {
                HistoryId = e.HistoryId,
                EventType = "CHECK_IN",
                Description = "Employee check-in",
                EventDate = e.CheckInAt,
            }
        );

        await col.UpdateOneAsync(
            x => x.Id == $"employee:{e.EmployeeId}",
            update,
            cancellationToken: ct
        );
    }

    public async Task ProjectAsync(EmployeeCheckOutDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<EmployeeReadModel>("employees");

        var update = Builders<EmployeeReadModel>.Update.Push(
            x => x.History,
            new EmployeeHistoryReadModel
            {
                HistoryId = e.HistoryId,
                EventType = "CHECK_OUT",
                Description = "Employee check-out",
                EventDate = e.CheckOutAt,
            }
        );

        await col.UpdateOneAsync(
            x => x.Id == $"employee:{e.EmployeeId}",
            update,
            cancellationToken: ct
        );
    }

    private async Task<string?> ResolveBranchNameAsync(Guid? branchId, CancellationToken ct)
    {
        if (!branchId.HasValue)
            return null;

        var branches = _db.GetCollection<BranchLookupDocument>("branches");

        var branchDoc = await branches
            .Find(x => x.Id == $"branch:{branchId.Value}")
            .FirstOrDefaultAsync(ct);

        return branchDoc?.Name;
    }

    [BsonIgnoreExtraElements]
    private sealed class BranchLookupDocument
    {
        public string Id { get; init; } = default!;
        public string? Name { get; init; }
    }
}
