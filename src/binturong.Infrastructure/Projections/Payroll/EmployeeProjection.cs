using Application.Abstractions.Projections;
using Application.ReadModels.Payroll;
using Domain.EmployeeHistory;
using Domain.Employees;
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

    public EmployeeProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(EmployeeCreatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<EmployeeReadModel>("employees");

        var rm = new EmployeeReadModel
        {
            Id = $"employee:{e.EmployeeId}",
            EmployeeId = e.EmployeeId,
            UserId = null,
            BranchId = e.BranchId,
            BranchName = null,
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
            TerminationDate = null,
            IsActive = e.IsActive,
            History = [],
        };

        await col.InsertOneAsync(rm, cancellationToken: ct);
    }

    public async Task ProjectAsync(EmployeeUpdatedDomainEvent e, CancellationToken ct)
    {
        var col = _db.GetCollection<EmployeeReadModel>("employees");

        var update = Builders<EmployeeReadModel>
            .Update.Set(x => x.FullName, e.FullName)
            .Set(x => x.JobTitle, e.JobTitle)
            .Set(x => x.BaseSalary, e.BaseSalary)
            .Set(x => x.BranchId, e.BranchId)
            .Set(x => x.IsActive, e.IsActive);

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
}
